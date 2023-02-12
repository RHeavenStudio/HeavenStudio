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
        public bool fromLeft;

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
            float flyPos = Conductor.instance.GetPositionFromBeat(startBeat, 0.5f);
            flyPos *= 0.22f;
            transform.position = curve.GetPoint(flyPos);
            
            // destroy object when it's off-screen
            if (flyPos > 1f) {
                GameObject.Destroy(gameObject);
            };
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            string Slice;
            if (fromLeft) {
                Slice = "SliceLeft";
            } else {
                Slice = "SliceRight";
            };

            DogAnim.DoScaledAnimationAsync(Slice, 0.5f);
            Jukebox.PlayOneShotGame(sfxNum+"2");

            SpawnHalves LeftHalf = Instantiate(game.HalvesLeftBase).GetComponent<SpawnHalves>();

            GameObject.Destroy(gameObject);
        }
            

        // miss and out are unused im pretty sure? when you miss in this game it just kinda flies by 
        private void Miss(PlayerActionEvent caller) {}

        private void Out(PlayerActionEvent caller) {}
    }
}
