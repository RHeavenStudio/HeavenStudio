using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TapTrial
{
    public class TapTrialPlayer : MonoBehaviour
    {
        private enum TapState
        {
            Tap,
            DoubleTap,
            TripleTap
        }
        private TapState state = TapState.Tap;
        private int tripleTaps = 0;
        private Animator anim;
        [SerializeField] private ParticleSystem tapEffectLeft;
        [SerializeField] private ParticleSystem tapEffectRight;

        private TapTrial game;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            game = TapTrial.instance;
        }

        private void Update()
        {
            if (PlayerInput.Pressed() && !game.IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                WhiffTap();
            }
        }

        public void Bop()
        {
            anim.DoScaledAnimationAsync("Bop", 0.5f);
        }

        private void WhiffTap()
        {
            switch (state)
            {
                case TapState.Tap:
                    game.ScoreMiss();
                    Tap(false, false);
                    break;
                case TapState.DoubleTap:
                    game.ScoreMiss();
                    Tap(false, true);
                    break;
                case TapState.TripleTap:
                    game.ScoreMiss();
                    break;
            }
        }

        public void PrepareTap(bool doubleTap = false)
        {
            anim.DoScaledAnimationAsync(doubleTap ? "DoubleTapPrepare" : "TapPrepare", 0.5f);
            state = doubleTap ? TapState.DoubleTap : TapState.Tap;
        }

        public void Tap(bool ace, bool doubleTap = false)
        {
            if (ace)
            {
                SoundByte.PlayOneShotGame("tapTrial/tap");
                SpawnTapEffect(!doubleTap);
            }
            else
            {
                SoundByte.PlayOneShot("nearMiss");
            }
            anim.DoScaledAnimationAsync(doubleTap ? "DoubleTap" : "Tap", 0.5f);
        }

        public void PrepareTripleTap()
        {
            anim.DoScaledAnimationAsync("Pose", 0.5f);
            state = TapState.TripleTap;
            tripleTaps = 0;
        }

        public void TripleTap(bool ace)
        {
            bool tapLeft = tripleTaps % 2 == 0;
            tripleTaps++;

            if (ace)
            {
                SoundByte.PlayOneShotGame("tapTrial/tap");
                SpawnTapEffect(tapLeft);
            }
            else
            {
                SoundByte.PlayOneShot("nearMiss");
            }

            anim.DoScaledAnimationAsync(tapLeft ? "PoseTap_L" : "PoseTap_R", 0.5f);
        }



        private void SpawnTapEffect(bool left)
        {
            ParticleSystem spawnedTap = Instantiate(left ? tapEffectLeft : tapEffectRight, game.transform);
            spawnedTap.Play();
        }
    }
}