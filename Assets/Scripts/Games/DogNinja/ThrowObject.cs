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
        [SerializeField] Transform ObjectParent;
        public Sprite[] objectLeftHalves;
        public Sprite[] objectRightHalves;

        private DogNinja game;
        
        private void Awake()
        {
            game = DogNinja.instance;
            DogAnim = game.DogAnim;
        }

        private void Start()
        {
            barelyCurve = fromLeft ? BarelyRightCurve : BarelyLeftCurve;
            
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
            float flyPosBarely = Conductor.instance.GetPositionFromBeat(barelyTime, 1f)+1f;
            if (isActive) {
                flyPos *= 0.31f;
                transform.position = curve.GetPoint(flyPos);
                objPos = curve.GetPoint(flyPos);
                // destroy object when it's off-screen
                if (flyPos > 1f) {
                    GameObject.Destroy(gameObject);
                };
            } else {
                flyPosBarely *= 0.3f;
                transform.position = barelyCurve.GetPoint(flyPosBarely) + (objPos/3);
                float rot = fromLeft ? 200f : -200f;
                transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (rot * Time.deltaTime));
                if (flyPosBarely > 1f) {
                    GameObject.Destroy(gameObject);
                };
            }

            if ((!Conductor.instance.isPlaying && !Conductor.instance.isPaused) 
                || GameManager.instance.currentGame != "dogNinja") {
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
            barelyTime = Conductor.instance.songPositionInBeats;

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
            Jukebox.PlayOneShotGame("dogNinja/barely");
            
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
            if (!DogAnim.GetBool("needPrepare")) ;
            DogAnim.DoScaledAnimationAsync("UnPrepare", 0.5f);
            DogAnim.SetBool("needPrepare", false);
        }

        private void Out(PlayerActionEvent caller) 
        {
            DogAnim.SetBool("needPrepare", false);
        }
    }
}
