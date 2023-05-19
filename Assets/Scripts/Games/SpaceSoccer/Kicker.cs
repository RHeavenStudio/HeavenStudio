using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_SpaceSoccer
{
    public class Kicker : MonoBehaviour
    {
        SpaceSoccer game;

        [Header("Properties")]
        public bool canKick = true; //why was this false by default???
        public bool canHighKick;
        private bool kickPrepare = false;
        public bool kickLeft;
        bool kickLeftWhiff;
        public float dispenserBeat; //unused
        public int kickTimes = 0;
        public bool player;
        private string animName = "Enter";
        private float animLength;
        private float animStartBeat;
        private EasingFunction.Ease ease;
        bool stopBall;

        [Header("Components")]
        private Animator anim;
        public Ball ball;
        [SerializeField] private Animator enterExitAnim;

        PlayerActionEvent nextHit;
        PlayerActionEvent nextAutoKick;

        private void Awake()
        {
            game = SpaceSoccer.instance;
            anim = GetComponent<Animator>();
        }

        public void SetAnimParams(float beat, float length, string anim, int easeToPut)
        {
            animStartBeat = beat;
            animLength = length;
            animName = anim;
            ease = (EasingFunction.Ease)easeToPut;
            EasingFunction.Function func = EasingFunction.GetEasingFunction(ease);
            float newAnimPos = func(0, 1, 0);
            enterExitAnim.DoNormalizedAnimation(animName, newAnimPos);
        }

        public void DispenseBall(float beat)
        {
            if (player)
            {
                nextHit = game.ScheduleInput(beat, ball.GetAnimLength(Ball.State.Dispensing), InputType.STANDARD_DOWN, KickJust, Miss, Out);
            }
            else
            {
                float beatToKick = beat + ball.GetAnimLength(Ball.State.Dispensing);
                if (beatToKick < Conductor.instance.songPositionInBeats) beatToKick = ball.nextAnimBeat;
                if (ball.state == Ball.State.HighKicked)
                {
                    BeatAction.New(this.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beatToKick - 0.5f, delegate { Kick(true, true); }),
                        new BeatAction.Action(beatToKick, delegate { Toe(true); }),
                        new BeatAction.Action(beatToKick + ball.GetAnimLength(Ball.State.Toe), delegate { KickCheck(true, false, beatToKick + ball.GetAnimLength(Ball.State.Toe)); }),
                    });
                }
                else
                {
                    BeatAction.New(this.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beatToKick, delegate { KickCheck(true, false, beatToKick); }),
                    });
                }
            }
        }

        public void Kick(bool hit, bool highKick = false)
        {
            if (stopBall) return;

            if (player)
            {
                Jukebox.PlayOneShotGame("spaceSoccer/kick");
            }

            if (hit)
            {
                if (highKick)
                {
                    if (kickLeft)
                    {
                        anim.DoScaledAnimationAsync("HighKickLeft_0", 0.5f);
                    }
                    else
                    {
                        anim.DoScaledAnimationAsync("HighKickRight_0", 0.5f);
                    }
                }
                else
                {
                    if (kickLeft)
                    {
                        anim.DoScaledAnimationAsync("KickLeft", 0.5f);
                    }
                    else
                    {
                        anim.DoScaledAnimationAsync("KickRight", 0.5f);
                    }
                }
            }
            else
            {
                if (highKick)
                {
                    if (kickLeftWhiff)
                    {
                        anim.DoScaledAnimationAsync("HighKickLeft_0", 0.5f);
                    }
                    else
                    {
                        anim.DoScaledAnimationAsync("HighKickRight_0", 0.5f);
                    }
                }
                else
                {
                    if (kickLeftWhiff)
                    {
                        anim.DoScaledAnimationAsync("KickLeft", 0.5f);
                    }
                    else
                    {
                        anim.DoScaledAnimationAsync("KickRight", 0.5f);
                    }
                }
                kickLeftWhiff = !kickLeftWhiff;
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
            if (stopBall) return;
            kickTimes++;
            if (hit)
            {
                if (kickLeft)
                {
                    anim.DoScaledAnimationAsync("HighKickLeft_0", 0.5f);
                }
                else
                {
                    anim.DoScaledAnimationAsync("HighKickRight_0", 0.5f);
                }
            }
            else
            {
                if (kickLeftWhiff)
                {
                    anim.DoScaledAnimationAsync("HighKickLeft_0", 0.5f);
                }
                else
                {
                    anim.DoScaledAnimationAsync("HighKickRight_0", 0.5f);
                }
                kickLeftWhiff = !kickLeftWhiff;
            }

            if (player) Jukebox.PlayOneShotGame("spaceSoccer/highkicktoe1");
            if (hit && ball)
            {
                ball.HighKick();

                if (player) Jukebox.PlayOneShotGame("spaceSoccer/highkicktoe1_hit");
            }

        }

        public void Toe(bool hit)
        {
            if (stopBall) return;
            if (kickLeft)
            {
                anim.DoScaledAnimationAsync("ToeLeft", 0.5f);
            }
            else
            {
                anim.DoScaledAnimationAsync("ToeRight", 0.5f);
            }

            if (player)
            {
                Jukebox.PlayOneShotGame("spaceSoccer/highkicktoe3");
                if (hit && ball)
                {
                    Jukebox.PlayOneShotGame("spaceSoccer/highkicktoe3_hit");
                }
            }

            if (hit && ball)
                ball.Toe();

            kickTimes++;
            kickPrepare = false;
        }

        private void Update()
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(animStartBeat, animLength);
            if (normalizedBeat >= 0 && normalizedBeat <= 1)
            {
                EasingFunction.Function func = EasingFunction.GetEasingFunction(ease);
                float newAnimPos = func(0, 1, normalizedBeat);
                enterExitAnim.DoNormalizedAnimation(animName, newAnimPos);
            }
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
                if (stopBall) return;
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
            if (stopBall) return;
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

        public void StopBall(bool stop)
        {
            stopBall = stop;
            if (ball != null && stop) Destroy(ball.gameObject);
        }

        void MissBall(float targetBeat)
        {
            if (stopBall) return;

            var cond = Conductor.instance;
            ball = null;
            // queue the miss sound
            MultiSound.Play(new MultiSound.Sound[] { new MultiSound.Sound("spaceSoccer/missNeutral", targetBeat + (float)cond.SecsToBeats(Minigame.EndTime()-1, 
                cond.GetBpmAtBeat(targetBeat)), Jukebox.GetPitchFromCents(UnityEngine.Random.Range(-75, 75), false)) });
        }

        private void KickJust(PlayerActionEvent caller, float state)
        {
            if (stopBall) return;
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
            if (stopBall) return;
            if (ball != null)
                MissBall(caller.startBeat + caller.timer);
            
            // if this were any other keep the beat game you'd cue the next input here
        }

        private void ToeJust(PlayerActionEvent caller, float state)
        {
            if (stopBall) return;

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
            if (stopBall) return;

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
            if (ball != null) Destroy(ball.gameObject);
        }
    }
}