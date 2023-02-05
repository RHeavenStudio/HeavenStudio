using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_SpaceSoccer
{
    public class Kicker : PlayerActionObject
    {
        SpaceSoccer game;

        [Header("Properties")]
        public bool canKick = true; //why was this false by default???
        public bool canHighKick;
        private bool kickPrepare = false;
        public bool kickLeft;
        public float dispenserBeat; //unused
        public int kickTimes = 0;
        public bool player;
        public float zValue;

        [Header("Components")]
        private Animator anim;
        public Ball ball;

        PlayerActionEvent nextHit;
        PlayerActionEvent nextAutoKick;

        private void Awake()
        {
            game = SpaceSoccer.instance;
            anim = GetComponent<Animator>();
        }

        public void DispenseBall(float beat)
        {
            if (player)
            {
                nextHit = game.ScheduleInput(beat, ball.GetAnimLength(Ball.State.Dispensing), InputType.STANDARD_DOWN, KickJust, Miss, Out);
            }
            else
            {
                BeatAction.New(this.gameObject, new List<BeatAction.Action>(){
                    new BeatAction.Action(beat + ball.GetAnimLength(Ball.State.Dispensing), delegate { KickCheck(true, false, beat + ball.GetAnimLength(Ball.State.Dispensing)); }),
                });
            }
        }

        public void Kick(bool hit, bool highKick = false)
        {
            aceTimes = 0;

            if (player)
            {
                Jukebox.PlayOneShotGame("spaceSoccer/kick");
            }

            if (highKick)
            {
                if (kickLeft)
                {
                    anim.Play("HighKickLeft_0", 0, 0);
                }
                else
                {
                    anim.Play("HighKickRight_0", 0, 0);
                }
            }
            else
            {
                if (kickLeft)
                {
                    anim.Play("KickLeft", 0, 0);
                }
                else
                {
                    anim.Play("KickRight", 0, 0);
                }
            }

            if (ball == null) return;

            if (highKick == false)
            {
                kickTimes++;
                if (ball != null && hit)
                    ball.Kick(player);
            }
            else
            {
                kickPrepare = true;
            }
        }

        public void HighKick(bool hit)
        {
            kickTimes++;

            if (kickLeft)
            {
                anim.Play("HighKickLeft_0", 0, 0);
            }
            else
            {
                anim.Play("HighKickRight_0", 0, 0);
            }

            if (hit && ball)
            {
                ball.HighKick();

                if (player)
                Jukebox.PlayOneShotGame("spaceSoccer/highkicktoe1_hit");
            }
            else
            {
                if (player)
                    Jukebox.PlayOneShotGame("spaceSoccer/highkicktoe1");
            }
        }

        public void Toe(bool hit)
        {
            if (kickLeft)
            {
                anim.Play("ToeLeft", 0, 0);
            }
            else
            {
                anim.Play("ToeRight", 0, 0);
            }

            if (player)
            {
                if (hit && ball)
                {
                    Jukebox.PlayOneShotGame("spaceSoccer/highkicktoe3_hit");
                }
                else
                {
                    Jukebox.PlayOneShotGame("spaceSoccer/highkicktoe3");
                }
            }

            if (hit && ball)
                ball.Toe();

            kickTimes++;
            kickPrepare = false;
        }

        private void Update()
        {
            if (kickTimes % 2 == 0)
            {
                kickLeft = false;
            }
            else
            {
                kickLeft = true;
            }

            var highKicks = GameManager.instance.Beatmap.entities.FindAll(c => c.datamodel == "spaceSoccer/high kick-toe!");
            for (int i = 0; i < highKicks.Count; i++)
            {
                if ((highKicks[i].beat - 0.15f) <= Conductor.instance.songPositionInBeats && highKicks[i].beat + 1f > Conductor.instance.songPositionInBeats)
                {
                    canHighKick = true;
                    canKick = false;

                    if (ball)
                    {
                        ball.highKickSwing = highKicks[i].swing;
                        if (ball.highKickSwing == 0f)
                            ball.highKickSwing = 0.5f;
                    }
                    break;
                }
                else
                {
                    canKick = true;
                    canHighKick = false;
                }
            }

            if (player)
            {
                if (PlayerInput.Pressed() && !game.IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    if (ball == null)
                        KickCheck(false, true);
                    else
                        Kick(false, ball.canKick);

                }
                if (PlayerInput.PressedUp() && ball != null)
                {
                    if (ball.waitKickRelease)
                    {
                        ball.waitKickRelease = false;
                    }
                    else if (ball.canKick && !game.IsExpectingInputNow(InputType.STANDARD_UP))
                    {
                        ball.canKick = false;
                        Kick(false);
                    }
                }
            }
        }

        private void KickCheck(bool hit,  bool overrideState = false, float beat = 0f)
        {
            if (canHighKick)
            {
                HighKick(hit);
                if (!player)
                {
                    BeatAction.New(this.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + ball.GetAnimLength(Ball.State.Kicked), delegate { Kick(true, true); }),
                        new BeatAction.Action(beat + ball.GetAnimLength(Ball.State.Toe), delegate { Toe(true); }),
                        new BeatAction.Action(beat + ball.GetAnimLength(Ball.State.Toe) + 1.5f, delegate { KickCheck(true, false, beat + ball.GetAnimLength(Ball.State.Toe) + 1.5f); }),
                    });
                }
            }
            else if (canKick)
            {
                Kick(hit);
                if (!player)
                {
                    BeatAction.New(this.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + ball.GetAnimLength(Ball.State.Kicked), delegate { KickCheck(true, false, beat + ball.GetAnimLength(Ball.State.Kicked)); }),
                    });
                }
            }
            else if (!canKick && !canHighKick && overrideState)
            {
                Kick(hit);
                if (!player)
                {
                    BeatAction.New(this.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + ball.GetAnimLength(Ball.State.Kicked), delegate { KickCheck(true, false, beat + ball.GetAnimLength(Ball.State.Kicked)); }),
                    });
                }
            }


        }

        void MissBall(float targetBeat)
        {
            var cond = Conductor.instance;
            ball = null;
            // queue the miss sound
            MultiSound.Play(new MultiSound.Sound[] { new MultiSound.Sound("spaceSoccer/missNeutral", targetBeat + (float)cond.SecsToBeats(Minigame.EndTime()-1, cond.GetBpmAtBeat(targetBeat))) });
        }

        private void KickJust(PlayerActionEvent caller, float state)
        {
            if (ball == null || state >= 1f || state <= -1f) {  //todo: proper near miss feedback
                KickCheck(false, true);
                MissBall(caller.startBeat + caller.timer);
                return;
            }
            KickCheck(true);
            if (canHighKick)
            {
                // queue high kick inputs
                nextHit = game.ScheduleInput(caller.startBeat + caller.timer, ball.GetAnimLength(Ball.State.Toe), InputType.STANDARD_UP, ToeJust, Miss, Out);
                nextAutoKick = game.ScheduleAutoplayInput(caller.startBeat + caller.timer, ball.GetAnimLength(Ball.State.Kicked), InputType.STANDARD_DOWN, ToePrepareJust, Out, Out);
                ball.canKick = true;
                ball.waitKickRelease = true;
            }
            else
            {
                // queue normal kick input
                nextHit = game.ScheduleInput(caller.startBeat + caller.timer, ball.GetAnimLength(Ball.State.Kicked), InputType.STANDARD_DOWN, KickJust, Miss, Out);
            }
        }

        private void Miss(PlayerActionEvent caller) 
        {
            if (ball != null)
                MissBall(caller.startBeat + caller.timer);
            
            // if this were any other keep the beat game you'd cue the next input here
        }

        private void ToeJust(PlayerActionEvent caller, float state)
        {
            if (ball == null || (!ball.canKick) || state >= 1f || state <= -1f) {  //todo: proper near miss feedback
                Toe(false);
                MissBall(caller.startBeat + caller.timer);
                return;
            }
            Toe(true);
            nextHit = game.ScheduleInput(caller.startBeat, 3f, InputType.STANDARD_DOWN, KickJust, Miss, Out);
            ball.canKick = false;
        }

        private void ToePrepareJust(PlayerActionEvent caller, float state)
        {
            //autoplay only
            Kick(true, true);
        }

        private void Out(PlayerActionEvent caller) {}

        void OnDestroy()
        {
            if (nextHit != null)
                nextHit.Disable();
            if (nextAutoKick != null)
                nextAutoKick.Disable();
        }
    }
}