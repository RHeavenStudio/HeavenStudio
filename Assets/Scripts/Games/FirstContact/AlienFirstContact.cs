using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HeavenStudio.Games.Scripts_FirstContact
{
    public class AlienFirstContact : PlayerActionObject
    {
        public float createBeat;
        FirstContact game;
        Translator translator;
        bool hasSpoke;
        public float stateBeat;
        public bool prefabHolder;
        bool missed;


        private void Awake()
        {
            game = FirstContact.instance;
            translator = GameObject.Find("Games/firstContact/Translator").GetComponent<Translator>();
        }

        private void Start()
        {

        }

        private void Update()
        {
            if (hasSpoke)
            {
                return;
            }
            stateBeat = Conductor.instance.GetPositionFromMargin(createBeat + game.beatInterval, 1f);
            StateCheck(stateBeat);
            if (PlayerInput.Pressed(true))
            {
                if (state.eligible())
                {
                    if (!game.hasMissed)
                    {
                        Ace();                      
                    }
                    else
                    {
                        Eh();
                    }
                    
                }
                else if (state.notPerfect() && game.translatorSpeakCount > 0)
                {
                    Eh();
                }
                //else if (stateBeat > Minigame.LateTime() && game.translatorSpeakCount == 0)
                //{
                //    //Debug.Log("OW");
                //    Miss();
                //}

            }

            if (stateBeat > Minigame.LateTime())
            {
                if (!missed)
                {
                    MissNoHit();
                }  
            }


        }

        public void Ace()
        {
            translator.successTranslation(true);
            game.isCorrect = true;
            game.translatorSpeakCount++;
            hasSpoke = true;
            missed = false;
        }

        public void Miss()
        {
            translator.successTranslation(false);
            game.isCorrect = false;
            hasSpoke = true;
            missed = false;
        }


        public void MissNoHit()
        {
            game.alienNoHit();
            game.isCorrect = false;
            missed = true;
            game.hasMissed = true;
        }

        public void Eh()
        {
            translator.ehTranslation();
            hasSpoke = true;
        }

        public override void OnAce()
        {
            Ace();
        }
    }
}

