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
            game.ScheduleInput((float)cueStart, cueLength, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, Hit, Miss, Out);
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
                Jukebox.PlayOneShotGame($"clapTrap/barely{UnityEngine.Random.Range(1, 2)}");
                dollHead.DoScaledAnimationAsync("HeadBarely", 0.5f);
            }
            else if (state >= -0.01f && state <= 0.01f)
            {
                Jukebox.PlayOneShotGame($"clapTrap/aceClap{UnityEngine.Random.Range(1, 4)}");
                dollHead.DoScaledAnimationAsync("HeadHit", 0.5f);
            }
            else
            {
                Jukebox.PlayOneShotGame($"clapTrap/goodClap{UnityEngine.Random.Range(1, 4)}");
                dollHead.DoScaledAnimationAsync("HeadHit", 0.5f);
            }

            dollArms.DoScaledAnimationAsync("ArmsHit", 0.5f);
            game.clapEffect.DoScaledAnimationAsync("ClapEffect", 0.5f);

            gameObject.SetActive(true);
            GetComponent<Animator>().DoScaledAnimationAsync("sword" + cueType + "Hit", 0.5f);

            if (spotlightToggle) { game.currentSpotlightClaps -= 1; }

            Debug.Log(state);
        }

        private void Miss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame($"clapTrap/miss");
            dollHead.DoScaledAnimationAsync("HeadMiss", 0.5f);
            dollArms.DoScaledAnimationAsync("ArmsMiss", 0.5f);

            gameObject.SetActive(true);
            GetComponent<Animator>().DoScaledAnimationAsync("sword" + cueType + "Miss", 0.5f);

            if (spotlightToggle) { game.currentSpotlightClaps -= 1; }
        }

        private void Out(PlayerActionEvent caller)
        {

        }
    }
}
