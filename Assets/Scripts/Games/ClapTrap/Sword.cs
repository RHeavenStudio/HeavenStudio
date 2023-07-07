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
        public int cueType;

        private Animator dollHead;
        private Animator dollArms;

        private ClapTrap game;

        // Start is called before the first frame update
        void Awake()
        {
            game = ClapTrap.instance;
            dollHead = game.dollHead;
            dollArms = game.dollArms;
            
            game.ScheduleInput((float)cueStart, cueLength, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, Hit, Miss, Out);
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
            else if (state == 0f)
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

            GetComponent<Animator>().DoScaledAnimationAsync("swordHandHit", 0.5f);
        }

        private void Miss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame($"clapTrap/miss");
            dollHead.DoScaledAnimationAsync("HeadMiss", 0.5f);
            dollArms.DoScaledAnimationAsync("ArmsMiss", 0.5f);
        }

        private void Out(PlayerActionEvent caller)
        {

        }
    }
}
