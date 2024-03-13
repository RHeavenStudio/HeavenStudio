using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_ShootEmUp
{
    public class Enemy : MonoBehaviour
    {
        [NonSerialized] public double createBeat;
        public Transform SpawnEffect;

        public float scaleSpeed;
        Vector3 scaleRate => new Vector3(scaleSpeed, scaleSpeed, scaleSpeed) / (Conductor.instance.pitchedSecPerBeat * 2f);

        private ShootEmUp game;

        public void Init()
        {
            game = ShootEmUp.instance;
        }

        public void StartInput(double beat, double length)
        {
            game = ShootEmUp.instance;
            game.ScheduleInput(beat, length, ShootEmUp.InputAction_Press, Just, Miss, Empty);
        }
        private void Just(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("shootEmUp/shoot");
            game.shipAnim.Play("shipShoot");
            game.hitEffect.Play();
            Destroy(gameObject);
        }

        private void Miss(PlayerActionEvent caller) 
        {
            // this is where perfect challenge breaks
            game.Damage();
            Destroy(gameObject);
        }

        private void Empty(PlayerActionEvent caller) {}

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                var enemyScale = transform.localScale;
                transform.localScale = enemyScale + (scaleRate * Time.deltaTime);
            }
        }
    }
}