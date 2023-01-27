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
        public bool fromLeft;

        string throwSfx;
        string sliceSfx;
        bool flying = true;
        float flyBeats;

        [Header("Animators")]
        public Animator DogAnim;

        [Header("References")]
        public GameObject ObjectLeftBase;

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
                case 1:
                    throwSfx = "dogNinja/bone1";
                    sliceSfx = "dogNinja/bone2";
                    break;
                case 5:
                    throwSfx = "dogNinja/pan1";
                    sliceSfx = "dogNinja/pan2";
                    break;
                case 8:
                    throwSfx = "dogNinja/tire1";
                    sliceSfx = "dogNinja/tire2";
                    break;
                case 9:
                    throwSfx = "dogNinja/pan1";
                    sliceSfx = "dogNinja/tacobell_combo";
                    break;
                default:
                    throwSfx = "dogNinja/fruit1";
                    sliceSfx = "dogNinja/fruit2";
                    break;
            }

            //diffObj.sprite("Broc");

            Jukebox.PlayOneShotGame(throwSfx);
        
            game.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN, Hit, Out, Miss);
            
            //DogNinja.dontPlay = true;

            //debug stuff below
            Debug.Log("it's a/an "+type);
        }

        private void Update(){}

        void CutObject()
        {
            DogAnim.Play("Slice", 0, 0);
            Jukebox.PlayOneShotGame(sliceSfx);

            //DogNinja.dontPlay = false;

            //SpawnHalves();

            GameObject.Destroy(gameObject);
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
