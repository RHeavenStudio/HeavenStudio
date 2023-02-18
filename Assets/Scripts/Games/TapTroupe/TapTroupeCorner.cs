using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TapTroupe
{
    public class TapTroupeCorner : MonoBehaviour
    {
        private Animator anim;
        [SerializeField] Animator expressionAnim;
        [SerializeField] Animator bodyAnim;
        [SerializeField] ParticleSystem popperEffect;

        private TapTroupe game;

        void Awake()
        {
            game = TapTroupe.instance;
            anim = GetComponent<Animator>();
        }

        public void Bop()
        {
            anim.DoScaledAnimationAsync("Bop", 0.3f);
        }

        public void Okay()
        {
            expressionAnim.DoScaledAnimationAsync("Okay", 0.25f);
        }

        public void PartyPopper(float beat)
        {
            bodyAnim.Play("PartyPopperReady", 0, 0);
            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { bodyAnim.Play("PartyPopper", 0, 0); }),
                new BeatAction.Action(beat + 1f, delegate { bodyAnim.DoScaledAnimationAsync("PartyPopperPop", 0.25f); Jukebox.PlayOneShotGame("tapTroupe/popper"); popperEffect.Play(); }),
                new BeatAction.Action(beat + 3f, delegate { bodyAnim.Play("IdleBody", 0, 0); })
            });
        }
    }
}


