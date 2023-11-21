using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_BlueBear
{
    public class Treat : MonoBehaviour
    {
        const float rotSpeed = 360f;

        public bool isCake;
        public double startBeat;

        bool flying = true;
        double flyBeats;

        [NonSerialized] public BezierCurve3D curve;

        private BlueBear game;

        private void Awake()
        {
            game = BlueBear.instance;
        }

        private void Start()
        {
            flyBeats = isCake ? 3f : 2f;
            game.ScheduleInput(startBeat, flyBeats, isCake ? BlueBear.InputAction_Left : BlueBear.InputAction_Right, Just, Out, Out);
        }

        private void Update()
        {
            if (flying)
            {
                var cond = Conductor.instance;

                float flyPos = cond.GetPositionFromBeat(startBeat, flyBeats);
                flyPos *= isCake ? 0.75f : 0.6f;
                transform.position = curve.GetPoint(flyPos);

                if (flyPos > 1f)
                {
                    Destroy(gameObject);
                    return;
                }

                float rot = isCake ? rotSpeed : -rotSpeed;
                transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (rot * Time.deltaTime));
            }
        }
        void EatFood()
        {
            flying = false;

            if (isCake)
            {
                SoundByte.PlayOneShotGame("blueBear/chompCake");
            }
            else
            {
                SoundByte.PlayOneShotGame("blueBear/chompDonut");
            }

            game.Bite(isCake);
            game.EatTreat();

            SpawnCrumbs();

            GameObject.Destroy(gameObject);
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {  //todo: proper near miss feedback
                if (isCake)
                {
                    game.headAndBodyAnim.Play("BiteL", 0, 0);
                }
                else
                {
                    game.headAndBodyAnim.Play("BiteR", 0, 0);
                }
                return;
            }
            EatFood();
        }

        private void Out(PlayerActionEvent caller) { }

        void SpawnCrumbs()
        {
            var crumbsGO = GameObject.Instantiate(game.crumbsBase, game.crumbsHolder);
            crumbsGO.SetActive(true);
            crumbsGO.transform.position = transform.position;

            var ps = crumbsGO.GetComponent<ParticleSystem>();
            var main = ps.main;
            var newGradient = new ParticleSystem.MinMaxGradient(isCake ? game.cakeGradient : game.donutGradient);
            newGradient.mode = ParticleSystemGradientMode.RandomColor;
            main.startColor = newGradient;
            ps.Play();
        }
    }
}
