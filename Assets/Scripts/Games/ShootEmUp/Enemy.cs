using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_ShootEmUp
{
    public class Enemy : MonoBehaviour
    {
        public double targetBeat;
        public Transform SpawnEffect;

        private ShootEmUp game;

        public void Init()
        {
            game = ShootEmUp.instance;
        }
    }
}