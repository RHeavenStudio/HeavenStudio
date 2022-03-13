using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TapTrial
{
    public class Tap : PlayerActionObject
    {
        public float startBeat;

        public int type;

        // Start is called before the first frame update
        void Start()
        {
            PlayerActionInit(this.gameObject, startBeat);
        }

        public override void OnAce()
        {
            Hit(true);
        }

        // Update is called once per frame
        void Update()
        {
            if (Conductor.instance.GetPositionFromBeat(startBeat, 2) >= 1)
                CleanUp();

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, 1f);
            StateCheck(normalizedBeat);

            if(PlayerInput.Pressed())
            {
                if(state.perfect)
                {
                    Hit(true);
                }
                else if(state.notPerfect())
                {
                    Hit(false);
                }
            }

        }

        public void Hit(bool hit)
        {
            TapTrial.instance.player.Tap(hit, type);

            if (hit)
                CleanUp();
        }

        public void CleanUp()
        {
            Destroy(this.gameObject);
        }
    }
}