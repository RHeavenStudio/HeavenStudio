using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DogNinja
{
    public class ThrowObject : MonoBehaviour
    {
        [SerializeField] DogNinja game;

        public double startBeat;
        public int type;
        public bool fromLeft;
        public bool shouldSfx = true;
        public int direction;
        public string sfxNum;
        
        private Vector3 objPos;
        private bool isActive = true;
        private double barelyTime;
        
        [Header("Animators")]
        Animator DogAnim;

        [Header("References")]
        public BezierCurve3D curve;
        [SerializeField] BezierCurve3D LeftCurve;
        [SerializeField] BezierCurve3D RightCurve;
        private BezierCurve3D barelyCurve;
        [SerializeField] BezierCurve3D BarelyLeftCurve;
        [SerializeField] BezierCurve3D BarelyRightCurve;
        [SerializeField] SpawnHalves HalvesLeftBase;
        [SerializeField] SpawnHalves HalvesRightBase;
        public Sprite[] objectLeftHalves;
        public Sprite[] objectRightHalves;

        private void Start()
        {
            DogAnim = game.DogAnim;
            curve = fromLeft ? LeftCurve : RightCurve;
            barelyCurve = fromLeft ? BarelyRightCurve : BarelyLeftCurve;

            game.ScheduleInput(startBeat, 1f, DogNinja.InputAction_Press, Hit, Miss, null);
        }

        private void Update()
        {
            if (isActive) {
                float flyPos = game.conductor.GetPositionFromBeat(startBeat, 1f)+1.1f;
                flyPos *= 0.31f;
                transform.position = curve.GetPoint(flyPos);
                objPos = curve.GetPoint(flyPos);
                // destroy object when it's off-screen
                if (flyPos > 1f) {
                    Destroy(gameObject);
                }
            } else {
                float flyPosBarely = game.conductor.GetPositionFromBeat(barelyTime, 1f)+1f;
                flyPosBarely *= 0.3f;
                transform.position = barelyCurve.GetPoint(flyPosBarely) + objPos;
                float rot = fromLeft ? 200f : -200f;
                transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (rot * Time.deltaTime));
                if (flyPosBarely > 1f) {
                    Destroy(gameObject);
                }
            }
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            game.StopPrepare();
            string dir = direction switch {
                0 => "Left",
                1 => "Right",
                _ => "Both",
            };
            if (state >= 1f || state <= -1f) {
                isActive = false;
                barelyTime = game.conductor.songPositionInBeatsAsDouble;

                DogAnim.DoScaledAnimationAsync("Barely" + dir, 0.5f);
                if (shouldSfx) SoundByte.PlayOneShotGame("dogNinja/barely");
            } else {
                DogAnim.DoScaledAnimationAsync("Slice" + dir, 0.5f);
                if (shouldSfx) SoundByte.PlayOneShotGame(sfxNum + "2");

                HalvesLeftBase.sr.sprite = objectLeftHalves[type - 1];
                HalvesRightBase.sr.sprite = objectRightHalves[type - 1];
                for (int i = 0; i < 2; i++) {
                    SpawnHalves half = Instantiate(i == 0 ? HalvesLeftBase : HalvesRightBase, game.transform);
                    half.startBeat = startBeat;
                    half.lefty = fromLeft;
                    half.objPos = objPos;
                }

                Destroy(gameObject);
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            if (!game.preparing) return;
            DogAnim.DoScaledAnimationAsync("Unprepare", 0.5f);
            game.StopPrepare();
        }
    }
}
