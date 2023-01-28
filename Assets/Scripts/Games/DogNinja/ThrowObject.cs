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
        const float rotSpeed = 360f;
        public float startBeat;
        public int type;
        public bool fromLeft;
        public bool needBop = true;

        string sfxNum = "dogNinja/";
        bool flying = true;
        float flyBeats;

        [Header("Animators")]
        public Animator DogAnim;

        [Header("References")]
        public GameObject ObjectBase;

        [Header("Curves")]
        public BezierCurve3D leftCurve;
        public BezierCurve3D rightCurve;

        private DogNinja game;
        
        private void Awake()
        {
            game = DogNinja.instance;

            
        }

        private void Start()
        {
            switch (type) {
                case 1:
                    sfxNum += "bone";
                    break;
                case 5:
                    sfxNum += "pan";
                    break;
                case 8:
                    sfxNum += "tire";
                    break;
                case 9:
                    sfxNum += "tacobell";
                    break;
                default:
                    sfxNum += "fruit";
                    break;
            }

            Jukebox.PlayOneShotGame(sfxNum+"1");
        
            game.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN, Hit, Out, Miss);
            
            

            //debug stuff below
            Debug.Log("it's a "+sfxNum);
        }

        private void Update()
        {
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
            needBop = false;
            DogAnim.Play("Slice", 0, 0);
            Jukebox.PlayOneShotGame(sfxNum+"2");

            //SpawnHalves();

            GameObject.Destroy(gameObject);
            needBop = true;
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
            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat+ 2.45f, delegate { 
                    Destroy(this.gameObject);
                }),
            });
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
