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

        bool flying = true;
        float flyBeats;
        public int ObjType;
        bool fromLeft;

        //public ObjectType type;

        [Header("Curves")]
        public BezierCurve3D CurveFromLeft;
        public BezierCurve3D CurveFromRight;

        private DogNinja game;
        
        private void Awake()
        {
            game = DogNinja.instance;

            
        }

        private void Start()
        {
            game.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN, Hit, Out, Miss);

            //debug stuff below
            Debug.Log("it's a/an "+ObjType);
        }

        private void Update()
        {
            if (flying)
            {
                var cond = Conductor.instance;

                float flyPos = cond.GetPositionFromBeat(startBeat, flyBeats);
                flyPos *=  0.6f;
                
                if (fromLeft)
                {
                    transform.position = CurveFromLeft.GetPoint(flyPos);
                } else {
                    transform.position = CurveFromRight.GetPoint(flyPos);
                }
                
                // DESTROYS GAME OBJECT! UNINTENTIONALLY!
                /* if (flyPos > 1f)
                {
                    GameObject.Destroy(gameObject);
                    return;
                } */
            }
        }
        
        void CutObject()
        {
            //flying = false;

            //game.headAndBodyAnim.Play("BiteR", 0, 0);
            Jukebox.PlayOneShotGame("dogNinja/fruit2");

            //SpawnHalves();

            //GameObject.Destroy(gameObject);
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
            /* BeatAction.New(game.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat+ 2.45f, delegate { 
                    Destroy(this.gameObject);
                }),
            }); */
        }

        private void Out(PlayerActionEvent caller) {}

        // WILL USE FOR SPAWNING HALVES

        /* void SpawnHalves()
        {
            var HalvesGO = GameObject.Instantiate(game.HalvesBase, game.HalvesHolder);
            HalvesGO.SetActive(true);
            HalvesGO.transform.position = transform.position;

            var ps = HalvesGO.GetComponent<ParticleSystem>();
            var main = ps.main;
            var newGradient = new ParticleSystem.MinMaxGradient();
            newGradient.mode = ParticleSystemGradientMode.RandomColor;
            main.startColor = newGradient;
            ps.Play();
        } */
    }
}
