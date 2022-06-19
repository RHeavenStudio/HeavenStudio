using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using HeavenStudio.Util;
using Starpelly;

namespace HeavenStudio.Games.Scripts_FirstContact
{
    public class Translator : PlayerActionObject
    {
        public Animator anim;

        FirstContact game;

        public void Init()
        {
            game = FirstContact.instance;
            //anim = GetComponent<Animator>();
        }

        private void Update()
        {
            ////IF YOU WANT TO PLAY NOTES ANYTIME W/O CONSTRAINTS
            //if (PlayerInput.Pressed(true) && !game.isSpeaking)
            //{
            //    successTranslation(true);
            //}
        }

  


        public void successTranslation(bool ace)
        {
            if (ace)
            {
                //if(game.version == 1)
                //{
                //    Jukebox.PlayOneShotGame("firstContact/citrusRemix/1_r");
                //}
                Jukebox.PlayOneShotGame("firstContact/" + randomizerLines());
            }
            else
            {
                Jukebox.PlayOneShotGame("firstContact/failContact");
            }

            BeatAction.New(this.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(.5f, delegate { anim.Play("translator_speak", 0, 0);}),
            });
        }

        public void ehTranslation()
        {
            Jukebox.PlayOneShotGame("firstContact/slightlyFail");
            BeatAction.New(this.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(.5f, delegate { anim.Play("translator_eh", 0, 0);}),
            });
        }

        public int randomizerLines()
        {
            return Random.Range(1, 11);     
        }
    }
}
