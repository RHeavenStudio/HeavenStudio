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
        [NonSerialized] public Vector2 pos;
        [Header("References")]
        public Transform effectHolder;
        public GameObject trajectoryEffect;
        public GameObject originEffect;
        public GameObject hitEffect;

        [NonSerialized] public float scaleSpeed;
        Vector3 scaleRate => new Vector3(scaleSpeed, scaleSpeed, scaleSpeed) / (Conductor.instance.pitchedSecPerBeat * 2f);
        bool isScale;

        private ShootEmUp game;

        public void Init()
        {
            game = ShootEmUp.instance;
            transform.localPosition = new Vector3(5.05f/3*pos.x, 2.5f/3*pos.y + 1.25f, 0);
            isScale = true;
        }

        public void StartInput(double beat, double length)
        {
            game = ShootEmUp.instance;
            game.ScheduleInput(beat, length, ShootEmUp.InputAction_Press, Just, Miss, Empty);
        }
        private void Just(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("shootEmUp/shoot");
            game.shipAnim.Play("shipShoot", 0, 0);
            game.hitEffect.Play();
            JustAnim();
        }

        private void Miss(PlayerActionEvent caller) 
        {
            // this is where perfect challenge breaks
            game.Damage();
            AttackAnim();
        }

        private void Empty(PlayerActionEvent caller) {}

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (isScale)
                {
                    var enemyScale = transform.localScale;
                    transform.localScale = enemyScale + (scaleRate * Time.deltaTime);
                }

            }
        }

        public void SpawnAnim()
        {
            this.GetComponent<Animator>().Play("enemySpawn", 0, 0);
            
            var trajectory = Instantiate(trajectoryEffect, effectHolder);
            trajectory.transform.localPosition = this.transform.localPosition;

            Vector3 angle = new Vector3(0, 0, 0);
            if (pos.x > 0 && pos.y > 0) {
                angle = new Vector3(0, 0, -70);
            } else if (pos.x < 0 && pos.y > 0) {
                angle = new Vector3(0, 0, 70);
            } else if (pos.x > 0 && pos.y < 0) {
                angle = new Vector3(0, 0, -110);
            } else if (pos.x < 0 && pos.y < 0) {
                angle = new Vector3(0, 0, 110);
            }
            trajectory.transform.eulerAngles = angle;
            trajectory.gameObject.SetActive(true);
        }

        public void AttackAnim()
        {
            var origin = Instantiate(originEffect, effectHolder);
            origin.transform.localPosition = this.transform.localPosition;
            origin.gameObject.SetActive(true);

            this.GetComponent<Animator>().Play("enemyAttack", 0, 0);
            isScale = false;
            transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

            var trajectory = Instantiate(trajectoryEffect, effectHolder);
            var hit = Instantiate(hitEffect, effectHolder);

            Vector3 attackPos = new Vector3(0, 0, 0);
            Vector3 angle = new Vector3(0, 0, 0);
            if (pos.x > 0) {
                attackPos = new Vector3(-5, -3, 0);
                angle = new Vector3(0, 0, -70);
            } else if (pos.x < 0) {
                attackPos = new Vector3(5, -3, 0);
                angle = new Vector3(0, 0, 70);
            }

            transform.localPosition = attackPos;
            trajectory.transform.localPosition = attackPos;
            trajectory.transform.eulerAngles = angle;
            trajectory.gameObject.SetActive(true);
            hit.transform.localPosition = attackPos;
            hit.gameObject.SetActive(true);
        }

        public void JustAnim()
        {
            this.GetComponent<Animator>().Play("enemyAttack", 0, 0);
            isScale = false;
            transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            
            var hit = Instantiate(hitEffect, effectHolder);

            Vector3 attackPos = new Vector3(0, 0.29f, 0);
            transform.localPosition = attackPos;
            hit.transform.localPosition = attackPos;
            hit.gameObject.SetActive(true);
        }

        void End()
        {
            Destroy(gameObject);
        }
    }
}