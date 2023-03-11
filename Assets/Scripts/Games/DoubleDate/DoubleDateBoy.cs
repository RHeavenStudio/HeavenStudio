using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DoubleDate
{
    public class DoubleDateBoy : MonoBehaviour
    {
        private DoubleDate game;

        void Awake()
        {
            game = DoubleDate.instance;
        }

        //Animation events are silly
        public void DisableBop()
        {
            game.ToggleBop(false);
        }

        public void EnableBop()
        {
            game.ToggleBop(true);
        }
    }

}
