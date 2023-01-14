using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_WorkingDough
{
    public class NPCDoughBall : PlayerActionObject
    {
        private WorkingDough game;

        private void Awake()
        {
            game = WorkingDough.instance;
        }
    }
}
