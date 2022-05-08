using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HeavenStudio
{
    public enum InputType : int {

        //General
        //-------
        //Down
        STANDARD_DOWN = 0,
        STANDARD_ALT_DOWN = 1,
        DIRECTION_DOWN = 2,
        //Up
        STANDARD_UP = 3,
        STANDARD_ALT_UP = 4,
        DIRECTION_UP = 5,

        //Specific
        //--------
        //Down
        DIRECTION_DOWN_DOWN = 6,
        DIRECTION_UP_DOWN = 7,
        DIRECTION_LEFT_DOWN = 8,
        DIRECTION_RIGHT_DOWN = 9,
        //Up
        DIRECTION_DOWN_UP = 10,
        DIRECTION_UP_UP = 11,
        DIRECTION_LEFT_UP = 12,
        DIRECTION_RIGHT_UP = 13
    }
}