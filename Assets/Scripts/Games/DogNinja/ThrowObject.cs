using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DogNinja
{
    public class ThrowObject : PlayerActionObject
    {
        public float startBeat;
        public int type;
        public int spriteInt;
        public string textObj;
        public bool fromLeft;
        public bool fromBoth = false;
        private Vector3 objPos;
        private bool isActive = true;
        private float barelyTime;
        string sfxNum = "dogNinja/";
        
        [Header("Animators")]
        Animator DogAnim;

        [Header("References")]
        public BezierCurve3D curve;
        [SerializeField] BezierCurve3D barelyCurve;
        [SerializeField] BezierCurve3D BarelyLeftCurve;
        [SerializeField] BezierCurve3D BarelyRightCurve;
        [SerializeField] GameObject HalvesLeftBase;
        [SerializeField] GameObject HalvesRightBase;
        public Sprite[] objectLeftHalves;
        public Sprite[] objectRightHalves;

        private DogNinja game;
        
        private void Awake()
        {
            game = DogNinja.instance;
            DogAnim = game.DogAnim;
            barelyCurve = fromLeft ? BarelyLeftCurve : BarelyRightCurve;
        }

        private void Start()
        {
            switch (type) {
                case 7:
                    sfxNum += "bone";
                    break;
                case 8:
                    sfxNum += "pan";
                    break;
                case 9:
                    sfxNum += "tire";
                    break;
                case 10:
                    sfxNum += textObj;
                    break;
                default:
                    sfxNum += "fruit";
                    break;
            };
            
            if (fromLeft && fromBoth) {} else { Jukebox.PlayOneShotGame(sfxNum+"1"); }
            
            game.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN, Hit, Miss, Out);
            
            DogAnim.SetBool("needPrepare", true);
        }

        private void Update()
        {
            float flyPos = Conductor.instance.GetPositionFromBeat(startBeat, 1f)+1.1f;
            if (isActive) {
                flyPos *= 0.31f;
                transform.position = curve.GetPoint(flyPos);
                objPos = curve.GetPoint(flyPos);
            } else {
                Debug.Log("brake point before big");
                float flyPosBarely = Conductor.instance.GetPositionFromBeat(barelyTime, 1f);
                flyPos *= 0.31f;
                transform.position = barelyCurve.GetPoint(flyPosBarely);
                Debug.Log("brake point after big");
            }
            
            
            // destroy object when it's off-screen
            if (flyPos > 1f) {
                GameObject.Destroy(gameObject);
            };

            // destroy object when game is stopped, but not when it's paused. 
            // no other game has this??? am i missing something here -AJ
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused) {
                GameObject.Destroy(gameObject);
            };
        }

        private void SuccessSlice() 
        {
            string Slice = "Slice";
            if (!fromBoth && fromLeft) {
                Slice += "Left";
            } else if (!fromBoth && !fromLeft) {
                Slice += "Right";
            } else {
                Slice += "Both";
            };

            DogAnim.DoScaledAnimationAsync(Slice, 0.5f);
            if (fromLeft && fromBoth) {} else { Jukebox.PlayOneShotGame(sfxNum+"2"); }

            game.WhichLeftHalf.sprite = objectLeftHalves[spriteInt-1];
            game.WhichRightHalf.sprite = objectRightHalves[spriteInt-1];

            SpawnHalves LeftHalf = Instantiate(HalvesLeftBase).GetComponent<SpawnHalves>();
            LeftHalf.startBeat = startBeat;
            LeftHalf.lefty = fromLeft;
            LeftHalf.objPos = objPos;

            SpawnHalves RightHalf = Instantiate(HalvesRightBase).GetComponent<SpawnHalves>();
            RightHalf.startBeat = startBeat;
            RightHalf.lefty = fromLeft;
            RightHalf.objPos = objPos;

            GameObject.Destroy(gameObject);
        }

        private void JustSlice()
        {
            Debug.Log("brake point before small");
            isActive = false;
            barelyTime = Conductor.instance.songBpm;

            Debug.Log("brake point middle 1 small");

            string Barely = "Barely";
            if (!fromBoth && fromLeft) {
                Barely += "Left";
            } else if (!fromBoth && !fromLeft) {
                Barely += "Right";
            } else {
                Barely += "Both";
            };

            Debug.Log("brake point middle 2 small");

            DogAnim.DoScaledAnimationAsync(Barely, 0.5f);
            Jukebox.PlayOneShotGame(sfxNum+"barely");
            
            Debug.Log("brake point end small");
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            game.DogAnim.SetBool("needPrepare", false);
            if (state >= 1f || state <= -1f) {
                JustSlice();
            } else {
                SuccessSlice();
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            if (!DogAnim.GetBool("needPrepare")) DogAnim.DoScaledAnimationAsync("UnPrepare", 0.5f);
            DogAnim.SetBool("needPrepare", false);
        }

        private void Out(PlayerActionEvent caller) 
        {
            DogAnim.SetBool("needPrepare", false);
        }
    }
}
