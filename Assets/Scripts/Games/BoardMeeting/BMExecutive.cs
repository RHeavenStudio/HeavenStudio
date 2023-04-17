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
        Animator anim;
        bool canBop = true;
        bool smiling;
        bool spinning;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            game = BoardMeeting.instance;
        }

        public void Prepare()
        {
            if (spinning) return;
            anim.DoScaledAnimationAsync("Prepare", 0.5f);
            canBop = false;
        }

        public void Spin()
        {
            spinning = true;
            anim.DoScaledAnimationAsync("Spin", 0.5f);
            canBop = false;
        }

        public void Stop()
        {
            if (!spinning) return;
            spinning = false;
            anim.DoScaledAnimationAsync("Stop", 0.5f);
            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(Conductor.instance.songPositionInBeats + 1.5f, delegate { canBop = true; })
            });
        }

        public void Bop()
        {
            if (!canBop || spinning) return;
            if (smiling)
            {
                anim.DoScaledAnimationAsync("SmileBop", 0.5f);
            }
            else
            {
                anim.DoScaledAnimationAsync("Bop", 0.5f);
            }

        }

        public void Smile()
        {
            if (spinning) return;
            anim.Play("SmileIdle");
            smiling = true;
        }
    }
}

