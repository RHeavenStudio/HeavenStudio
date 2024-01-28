using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using HeavenStudio.Util;
using Starpelly;

namespace HeavenStudio.Games.Scripts_FirstContact
{
    public class Translator : MonoBehaviour
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
        }

        public void SuccessTranslation(bool ace)
        {
            if (ace)
            {
                SoundByte.PlayOneShotGame("firstContact/" + RandomizerLines());
            }
            else
            {
                SoundByte.PlayOneShotGame("firstContact/failContact");
            }

            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(.5f, delegate { anim.Play("translator_speak", 0, 0);}),
            });
        }

        public void EhTranslation()
        {
            SoundByte.PlayOneShotGame("firstContact/slightlyFail");
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(.5f, delegate { anim.Play("translator_eh", 0, 0);}),
            });
        }

        public int RandomizerLines()
        {
            return Random.Range(1, 11);
        }
    }
}
