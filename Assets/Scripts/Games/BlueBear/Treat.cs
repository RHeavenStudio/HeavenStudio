using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_BlueBear
{
    public class Treat : SuperCurveObject
    {
        const float barelyDistX = 1.5f;
        const float barelyDistY = -6f;
        const float barelyHeight = 4f;
        const float rotSpeed = 360f * 3;

        public bool isCake;
        public double startBeat;
        double flyBeats;

        private Path path;

        private BlueBear game;

        private void Awake()
        {
            game = BlueBear.instance;
        }

        private void Start()
        {
            flyBeats = isCake ? 3f : 2f;
            Path pathToCopy = isCake ? game.GetPath("Cake") : game.GetPath("Donut");
            path = new();
            path.positions = new PathPos[2];
            path.positions[0].pos = pathToCopy.positions[0].pos;
            path.positions[0].duration = pathToCopy.positions[0].duration;
            path.positions[0].height = pathToCopy.positions[0].height;
            path.positions[1].pos = pathToCopy.positions[1].pos;
            game.ScheduleInput(startBeat, flyBeats, isCake ? BlueBear.InputAction_Left : BlueBear.InputAction_Right, Just, Out, Out);
            Update();
        }

        private void Update()
        {
            var cond = Conductor.instance;
            transform.localPosition = GetPathPositionFromBeat(path, cond.songPositionInBeatsAsDouble, startBeat);

            float flyPos = cond.GetPositionFromBeat(startBeat, flyBeats);
            if (flyPos > 2f)
            {
                Destroy(gameObject);
                return;
            }

            float rot = isCake ? rotSpeed : -rotSpeed;
            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (rot * Time.deltaTime * cond.pitchedSecPerBeat));
        }
        void EatFood()
        {
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

            Destroy(gameObject);
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
                if (isCake)
                {
                    game.headAndBodyAnim.DoScaledAnimationAsync("BiteL", 0, 0);
                }
                else
                {
                    game.headAndBodyAnim.DoScaledAnimationAsync("BiteR", 0, 0);
                }
                path.positions[0].pos = transform.localPosition;
                path.positions[0].height = barelyHeight;
                path.positions[0].duration = 1;
                path.positions[1].pos = new Vector3(path.positions[0].pos.x + (isCake ? -barelyDistX : barelyDistX), path.positions[0].pos.y + barelyDistY);
                startBeat = Conductor.instance.songPositionInBeatsAsDouble;
                Update();
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
            ps.PlayScaledAsync(1);
        }
    }
}
