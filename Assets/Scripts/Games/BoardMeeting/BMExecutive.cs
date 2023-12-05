using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_BoardMeeting
{
    public class BMExecutive : MonoBehaviour
    {
        public BoardMeeting game;
        public bool player;
        public Animator anim;
        bool canBop = true;
        int smileCounter = 0;
        public bool spinning;
        bool preparing;
        Sound rollLoop = null;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            game = BoardMeeting.instance;
        }

        private void OnDestroy()
        {
            if (rollLoop != null)
            {
                rollLoop.Stop();
            }
        }

        public void Prepare()
        {
            if (spinning) return;
            preparing = true;
            anim.DoScaledAnimationAsync("Prepare", 0.5f);
            canBop = false;
        }

        public void Spin(string soundToPlay = "A", bool forceStart = false)
        {
            if (spinning) return;
            spinning = true;
            preparing = false;
            string animToPlay = game.firstSpinner.anim.GetCurrentAnimatorStateInfo(0).IsName("Spin") ? "Spin" : "LoopSpin";
            if (this == game.firstSpinner) anim.DoUnscaledAnimation("Spin", 0);
            else anim.DoUnscaledAnimation(forceStart ? "Spin" : animToPlay, forceStart ? 0 : game.firstSpinner.anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
            canBop = false;
            SoundByte.PlayOneShotGame("boardMeeting/rollPrepare" + soundToPlay);
            float offset = 0;
            switch (soundToPlay)
            {
                case "A":
                case "B":
                    offset = 0.01041666666f;
                    break;
                case "C":
                case "Player":
                    offset = 0.02083333333f;
                    break;
                default:
                    offset = 0;
                    break;
            }
            rollLoop = SoundByte.PlayOneShotGame("boardMeeting/roll" + soundToPlay, Conductor.instance.songPositionInBeatsAsDouble + 0.5f - Conductor.instance.GetRestFromRealTime(offset), 1, 1, true);
        }

        public void Stop(bool hit = true)
        {
            if (!spinning) return;
            spinning = false;
            anim.DoScaledAnimationAsync(hit ? "Stop" : "Miss", hit ? 0.5f : 0.25f);
            if (rollLoop != null)
            {
                rollLoop.KillLoop(0);
                rollLoop = null;
            }
            game.StopChairLoopSoundIfLastToStop();

            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(Conductor.instance.songPositionInBeatsAsDouble + 1.5f, delegate { canBop = true; })
            });
        }

        public void Bop()
        {
            if (!canBop || spinning || preparing) return;
            if (smileCounter > 0)
            {
                anim.DoScaledAnimationAsync("SmileBop", 0.5f);
                smileCounter--;
            }
            else
            {
                anim.DoScaledAnimationAsync("Bop", 0.5f);
            }

        }

        public void Smile()
        {
            if (spinning) return;
            if (!preparing) anim.Play("SmileIdle");
            smileCounter = 2;
        }
    }
}

