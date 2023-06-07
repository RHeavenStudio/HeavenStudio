using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TapTrial
{
    public class TapTrialPlayer : MonoBehaviour
    {
        [Header("References")]
        [System.NonSerialized] public Animator anim;

        public int tripleOffset = 0;

        private void Awake()
        {
            anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (PlayerInput.Pressed())
            {
                Tap(false, 0);
            }
        }

        public void Tap(bool hit, int type)
        {
            if (hit)
                SoundByte.PlayOneShotGame("tapTrial/tap");
            else
                SoundByte.PlayOneShotGame("tapTrial/tonk");


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