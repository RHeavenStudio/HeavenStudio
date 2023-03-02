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
        public Vector3 objPos;
        public string sfxNum = "dogNinja/";
        

        [Header("Animators")]
        public Animator DogAnim;

        [Header("References")]
        public BezierCurve3D curve;
        public Sprite[] objectLeftHalves;
        public Sprite[] objectRightHalves;

        private DogNinja game;
        
        private void Awake()
        {
            game = DogNinja.instance;
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
            
            game.DogAnim.SetBool("needPrepare", true);
        }

        private void Update()
        {
            float flyPos = Conductor.instance.GetPositionFromBeat(startBeat, 1f)+1.1f;
            flyPos *= 0.31f;
            transform.position = curve.GetPoint(flyPos);
            objPos = curve.GetPoint(flyPos);
            
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

            Debug.Log(spriteInt);
            
            game.WhichLeftHalf.sprite = objectLeftHalves[spriteInt-1];
            game.WhichRightHalf.sprite = objectRightHalves[spriteInt-1];

            Debug.Log(objectLeftHalves[spriteInt-1]);

            SpawnHalves LeftHalf = Instantiate(game.HalvesLeftBase).GetComponent<SpawnHalves>();
            LeftHalf.startBeat = startBeat;

            SpawnHalves RightHalf = Instantiate(game.HalvesRightBase).GetComponent<SpawnHalves>();
            RightHalf.startBeat = startBeat;

            GameObject.Destroy(gameObject);
        }

        private void JustSlice()
        {

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
            DogAnim.Play("UnPrepare", 0, 0);
            game.DogAnim.SetBool("needPrepare", false);
        }

        private void Out(PlayerActionEvent caller) 
        {
            game.DogAnim.SetBool("needPrepare", false);
        }
    }
}
