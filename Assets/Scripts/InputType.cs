using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HeavenStudio
{
    [System.Flags]
    public enum InputType : int {
        // Shouldn't be used by minigame scripts
        ANY = -1,

        //General
        //-------
        //Down
        STANDARD_DOWN = 1<<0,
        STANDARD_ALT_DOWN = 1<<1,
        DIRECTION_DOWN = 1<<2,
        //Up
        STANDARD_UP = 1<<3,
        STANDARD_ALT_UP = 1<<4,
        DIRECTION_UP = 1<<5,

        //Specific
        //--------
        //Down
        DIRECTION_DOWN_DOWN = 1<<6,
        DIRECTION_UP_DOWN = 1<<7,
        DIRECTION_LEFT_DOWN = 1<<8,
        DIRECTION_RIGHT_DOWN = 1<<9,
        //Up
        DIRECTION_DOWN_UP = 1<<10,
        DIRECTION_UP_UP = 1<<11,
        DIRECTION_LEFT_UP = 1<<12,
        DIRECTION_RIGHT_UP = 1<<13
    }
}