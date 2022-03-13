using UnityEngine;

using HeavenStudio.Util;

<<<<<<< Updated upstream
namespace RhythmHeavenMania.Games.Scripts_TapTrial
=======
namespace HeavenStudio.Games.TapTrial
>>>>>>> Stashed changes
{
    public class TapTrialPlayer : MonoBehaviour
    {
        [Header("References")]
        [System.NonSerialized] public Animator anim;

        public float nextBeat;
        public int tripleOffset = 0;

        private void Start()
        {
            anim = GetComponent<Animator>();
        }

        private void Update()
        {
            float normalizedBeat = Conductor.instance.GetPositionFromMargin(nextBeat, 1f);


            if (PlayerInput.Pressed())
            {
                Tap(false, 0);
            }
        }

        public void Tap(bool hit, int type)
        {
            if (hit)
                Jukebox.PlayOneShotGame("tapTrial/tap");
            else
                Jukebox.PlayOneShotGame("tapTrial/tonk");


            switch (type)
            {
                case 0:
                    anim.Play("Tap", 0, 0);
                    break;
                case 1:
                    anim.Play("DoubleTap", 0, 0);
                    break;
                case 2:
                    if(tripleOffset % 2 == 0)
                        anim.Play("DoubleTap", 0, 0);
                    else
                        anim.Play("Tap", 0, 0);
                    tripleOffset++;
                    break;
            }
        }
    }
}