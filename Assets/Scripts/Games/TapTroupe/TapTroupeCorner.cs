using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TapTroupe
{
    public class TapTroupeCorner : MonoBehaviour
    {
        private Animator anim;
        public Animator expressionAnim;
        [SerializeField] Animator bodyAnim;
        [SerializeField] ParticleSystem popperEffect;
        public enum MissFace
        {
            Sad = 0,
            Spit = 1,
            LOL = 2
        }

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

        public void ResetFace()
        {
            if (expressionAnim.IsPlayingAnimationName("Okay")) return;
            expressionAnim.Play("NoExpression", 0, 0);
        }

        public void SetMissFace(MissFace missFace)
        {
            switch (missFace)
            {
                case MissFace.Sad:
                    expressionAnim.Play("Sad", 0, 0);
                    break;
                case MissFace.Spit:
                    expressionAnim.Play("Spit", 0, 0);
                    break;
                case MissFace.LOL:
                    expressionAnim.Play("LOL", 0, 0);
                    break;
            }
        }

        public void OkaySign()
        {
            bodyAnim.DoScaledAnimationAsync("OkaySign", 0.25f);
        }

        public void PartyPopper(double beat)
        {
            bodyAnim.Play("PartyPopperReady", 0, 0);
            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { bodyAnim.Play("PartyPopper", 0, 0); }),
                new BeatAction.Action(beat + 1f, delegate { bodyAnim.DoScaledAnimationAsync("PartyPopperPop", 0.25f); SoundByte.PlayOneShotGame("tapTroupe/popper"); popperEffect.Play(); }),
                new BeatAction.Action(beat + 3f, delegate { bodyAnim.Play("IdleBody", 0, 0); })
            });
        }
    }
}


