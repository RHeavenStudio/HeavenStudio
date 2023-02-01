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
        public bool leftie;
        public string textObj;

        public string sfxNum = "dogNinja/";
        bool flying = true;
        float flyBeats;

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
                case 6:
                    sfxNum += "bone";
                    break;
                case 7:
                    sfxNum += "pan";
                    break;
                case 8:
                    sfxNum += "tire";
                    break;
                case 9:
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
            flyPos *= 1f;
            transform.position = curve.GetPoint(flyPos);
            
            /* if (flying)
            {
                var cond = Conductor.instance;

                float flyPos = cond.GetPositionFromBeat(startBeat, flyBeats);
                flyPos *= 1f;
                transform.position = leftCurve.GetPoint(flyPos);

                /* if (flyPos > 1f)
                {
                    GameObject.Destroy(gameObject);
                    return;
                } *
            } */
        }

        void CutObject()
        {
            DogAnim.Play("Slice", 0, 0);
            Jukebox.PlayOneShotGame(sfxNum+"2");

            // ABOUT TO BE IN USE
            //SpawnHalves();

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

        private void Miss(PlayerActionEvent caller) 
        {
            // i want this to work.
            /* new BeatAction.Action(startBeat+ 2.45f, delegate { 
                    Destroy(this.gameObject);
            }); */
        }

        private void Out(PlayerActionEvent caller) {}

        
    }
}
