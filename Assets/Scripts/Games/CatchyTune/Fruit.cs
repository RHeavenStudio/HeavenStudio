using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_CatchyTune
{
    public class Fruit : PlayerActionObject
    {
        const float rotSpeed = 360f;
        const float sizeSpeed = 2f;

        public bool isPineapple;
        public float startBeat;

        bool flying = true;
        float flyBeats;

        public Animator anim;

        public bool side;

        private CatchyTune game;
        
        private void Awake()
        {
            game = CatchyTune.instance;

            var cond = Conductor.instance;
            var tempo = cond.songBpm;

            float speedmult = isPineapple ? 0.5f : 1f;
            anim.SetFloat("speed", (speedmult * tempo / 60f) * 0.2f);

            if (side)
            {
                anim.Play("fruit bounce right", 0, 0f);
            }
            else {
                anim.Play("fruit bounce", 0, 0f);
            }

            

        }
    }
}
