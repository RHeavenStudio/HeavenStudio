using HeavenStudio.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_ClapTrap
{
    public class Sword : MonoBehaviour
    {
        public double cueStart;
        public float cueLength;
        public string cueType;
        public bool spotlightToggle;

        private Animator dollHead;
        private Animator dollArms;
        

        private ClapTrap game;

        // Start is called before the first frame update
        void Awake()
        {
            

            game = ClapTrap.instance;
            dollHead = game.dollHead;
            dollArms = game.dollArms;
        }    

        void Start()
        {
            game.ScheduleInput((float)cueStart, cueLength, ClapTrap.InputAction_BasicPress, Hit, Miss, Out);
            gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void KillYourselfNow()
        {
            GameObject.Destroy(gameObject);
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame($"clapTrap/barely{UnityEngine.Random.Range(1, 3)}");
                dollHead.DoScaledAnimationAsync("HeadBarely", 0.5f);
            }
            else
            {
                SoundByte.PlayOneShotGame($"clapTrap/goodClap{UnityEngine.Random.Range(1, 5)}");
                dollHead.DoScaledAnimationAsync("HeadHit", 0.5f);
                if (state >= -0.2 && state <= 0.2)
                {
                    SoundByte.PlayOneShotGame($"clapTrap/clapAce");
                }
                else
                {
                    SoundByte.PlayOneShotGame($"clapTrap/clapGood");
                }
            }

            dollArms.DoScaledAnimationAsync("ArmsHit", 0.5f);
            game.doll.DoScaledAnimationAsync("DollHit", 0.5f);
            game.clapEffect.DoScaledAnimationAsync("ClapEffect", 0.5f);

            gameObject.SetActive(true);
            GetComponent<Animator>().DoScaledAnimationAsync("sword" + cueType + "Hit", 0.5f);

            if (spotlightToggle) { game.currentSpotlightClaps -= 1; }

            Debug.Log(state);
        }

        private void Miss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame($"clapTrap/miss");
            dollHead.DoScaledAnimationAsync("HeadMiss", 0.5f);
            dollArms.DoScaledAnimationAsync("ArmsMiss", 0.5f);
            game.doll.DoScaledAnimationAsync("DollMiss", 0.5f);

            gameObject.SetActive(true);
            GetComponent<Animator>().DoScaledAnimationAsync("sword" + cueType + "Miss", 0.5f);

            if (spotlightToggle) { game.currentSpotlightClaps -= 1; }
        }

        private void Out(PlayerActionEvent caller)
        {

        }
    }
}
