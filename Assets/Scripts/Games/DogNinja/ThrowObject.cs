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
        public string textObj;

        // this condenses the sfx code so much 
        public string sfxNum = "dogNinja/";

        [Header("Animators")]
        public Animator DogAnim;

        [Header("References")]
        public GameObject ObjectBase;

        [Header("Curves")]
        public BezierCurve3D curve;

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
            }

            Jukebox.PlayOneShotGame(sfxNum+"1");
        
            game.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN, Hit, Out, Miss);
        }

        private void Update()
        {
            float flyPos = Conductor.instance.GetPositionFromBeat(startBeat, 1);
            // apparently the og uses exponentiality. im just gonna mimic it (?)
            flyPos *= 0.46f;
            transform.position = curve.GetPoint(flyPos);
            
            // destroys object when it's off-screen
            if (flyPos > 1f) {
                GameObject.Destroy(gameObject);
            };
        }

        void CutObject()
        {
            DogAnim.Play("SliceRight", 0, 0);
            Jukebox.PlayOneShotGame(sfxNum+"2");

            SpawnHalves();

            GameObject.Destroy(gameObject);
        }

        void SpawnHalves()
        {
            var HalvesGO = GameObject.Instantiate(game.HalvesLeftBase);
            HalvesGO.transform.position = transform.position;

            
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            /* if (state >= 1f || state <= -1f) {  //todo: proper near miss feedback
                //game.headAndBodyAnim.Play("BiteR", 0, 0);
            } */
            CutObject();
        }

        // miss and out are unused im pretty sure? when you miss in this game it just kinda flies by 
        private void Miss(PlayerActionEvent caller) {}

        private void Out(PlayerActionEvent caller) {}
    }
}
