using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_ShootEmUp
{
    public class Enemy : MonoBehaviour
    {
        [Header("References")]
        public Animator enemyAnim;
        public Transform effectHolder;
        public GameObject trajectoryEffect;
        public GameObject originEffect;
        public GameObject impactEffect;
        public GameObject missimpactEffect;
        public ParticleSystem smokeEffect;

        [Header("Parameters")]
        [NonSerialized] public double createBeat;
        [NonSerialized] public int type;
        [NonSerialized] public Vector2 pos;

        [NonSerialized] public float scaleSpeed;
        Vector3 scaleRate => new Vector3(scaleSpeed, scaleSpeed, scaleSpeed) / (Conductor.instance.pitchedSecPerBeat * 2f);
        bool isScale;

        private ShootEmUp game;

        public void Init()
        {
            transform.localPosition = new Vector3(5.05f/3*pos.x, 2.5f/3*pos.y + 1.25f, 0);
            enemyAnim = GetComponent<Animator>();
            enemyAnim.Play(Enum.GetName(typeof(ShootEmUp.EnemyType), type));
            isScale = true;
        }

        public void StartInput(double beat, double length)
        {
            game = ShootEmUp.instance;
            game.ScheduleInput(beat, length, ShootEmUp.InputAction_Press, Just, Miss, Empty);
            // (type == (int)ShootEmUp.EnemyType.Endless ?  : )
        }
        private void Just(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("shootEmUp/shoot");
            game.playerShip.Shoot();
            if (state <= -1f || state >= 1f)
            {
                JudgeAnim("miss");

                ParticleSystem.MainModule main = smokeEffect.main;
                float startLifetime = main.startLifetimeMultiplier;
                main.startLifetimeMultiplier = startLifetime * (Conductor.instance.pitchedSecPerBeat * 2f);
                smokeEffect.Play();
                return;
            }
            ParticleSystem spawnedParticle = Instantiate(game.hitEffect, effectHolder);
            spawnedParticle.PlayScaledAsyncAllChildren(0.45f);
            JudgeAnim("just");
        }

        private void Miss(PlayerActionEvent caller) 
        {
            SoundByte.PlayOneShotGame("shootEmUp/15");
            game.playerShip.Damage();
            JudgeAnim("attack");
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
            enemyAnim.DoScaledAnimationAsync("enemySpawn", 1f);
            
            var trajectory = Instantiate(trajectoryEffect, effectHolder);
            trajectory.transform.localPosition = this.transform.localPosition;

            Vector3 angle = new Vector3(0, 0, 0);
            if (pos.x > 0 && pos.y >= 0) {
                angle = new Vector3(0, 0, -70);
            } else if (pos.x < 0 && pos.y >= 0) {
                angle = new Vector3(0, 0, 70);
            } else if (pos.x > 0 && pos.y <= 0) {
                angle = new Vector3(0, 0, -110);
            } else if (pos.x < 0 && pos.y <= 0) {
                angle = new Vector3(0, 0, 110);
            }
            trajectory.transform.eulerAngles = angle;
            trajectory.gameObject.SetActive(true);
            trajectory.GetComponent<Animator>().DoScaledAnimationAsync("trajectory", 1f);
        }

        public void JudgeAnim(string type)
        {
            Vector3 currentPos = this.transform.localPosition;
            Vector3 nextPos = new Vector3(0, 0.29f, 0);

            GameObject origin = Instantiate(originEffect, effectHolder);
            origin.transform.localPosition = currentPos;
            origin.gameObject.SetActive(true);
            origin.GetComponent<Animator>().DoScaledAnimationAsync("origin", 1f);

            isScale = false;

            GameObject trajectory = Instantiate(trajectoryEffect, effectHolder);
            GameObject impact, missimpact;

            switch (type)
            {
                case "just":
                    enemyAnim.DoScaledAnimationAsync("enemyAttack", 1f);
                    transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                    impact = Instantiate(impactEffect, effectHolder);
                    impact.transform.localPosition = nextPos;
                    impact.gameObject.SetActive(true);
                    impact.GetComponent<Animator>().DoScaledAnimationAsync("impact", 1f);
                    break;
                case "attack":
                    enemyAnim.DoScaledAnimationAsync("enemyAttack", 1f);
                    transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                    if (pos.x > 0) {
                        nextPos = new Vector3(-5, -3, 0);
                    } else if (pos.x < 0) {
                        nextPos = new Vector3(5, -3, 0);
                    } else {
                        nextPos = new Vector3(0, -1.25f, 0);
                    }
                    impact = Instantiate(impactEffect, effectHolder);
                    impact.transform.localPosition = nextPos;
                    impact.gameObject.SetActive(true);
                    impact.GetComponent<Animator>().DoScaledAnimationAsync("impact", 1f);
                    break;
                case "miss":
                    if (pos.x <= 0) {
                        enemyAnim.DoScaledAnimationAsync("enemyMissRight", 1f);
                    } else {
                        enemyAnim.DoScaledAnimationAsync("enemyMissLeft", 1f);
                    }
                    
                    missimpact = Instantiate(missimpactEffect, effectHolder);
                    missimpact.gameObject.SetActive(true);
                    missimpact.GetComponent<Animator>().DoScaledAnimationAsync("missimpact", 1f);
                    break;
                default:
                    break;
            }

            float angleDegrees = 180 - Mathf.Atan2(nextPos.x - currentPos.x, nextPos.y - currentPos.y) * Mathf.Rad2Deg;
            Vector3 angle = new Vector3(0, 0, angleDegrees);
            Vector3 scale = new Vector3(1, Vector3.Distance(nextPos, currentPos)*0.16f, 1);

            this.transform.localPosition = nextPos;
            trajectory.transform.localPosition = nextPos;
            trajectory.transform.eulerAngles = angle;
            trajectory.transform.localScale = scale;
            trajectory.gameObject.SetActive(true);
            trajectory.GetComponent<Animator>().DoScaledAnimationAsync("trajectory_damage", 1f);
        }

        void End()
        {
            Destroy(gameObject);
        }
    }
}