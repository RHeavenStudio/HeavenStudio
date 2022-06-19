using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class CtrFirstContact
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("firstContact", "First Contact", "008c97", false, false, new List<GameAction>()
            {
                new GameAction("beat intervals",                   delegate { FirstContact.instance.SetIntervalStart(eventCaller.currentEntity.beat, eventCaller.currentEntity.length);  }, 4f, true),
                new GameAction("alien speak",                   delegate { FirstContact.instance.alienSpeak(eventCaller.currentEntity.beat);  }, 0.5f, false),
                new GameAction("alien turnover",                   delegate { FirstContact.instance.alienTurnOver(eventCaller.currentEntity.beat);  }, 0.5f, false),
                new GameAction("alien success",                   delegate { FirstContact.instance.alienSuccess(eventCaller.currentEntity.beat);  }, 1f, false),
                new GameAction("mission control",                   delegate { FirstContact.instance.missionControlDisplay(eventCaller.currentEntity.beat, eventCaller.currentEntity.toggle);  }, 1f, false, new List<Param>
                {
                    new Param("toggle", false, "Stay", "If it's the end of the remix/song")
                }),
                new GameAction("look at",                   delegate { FirstContact.instance.lookAtDirection(eventCaller.currentEntity.type, eventCaller.currentEntity.type2);  }, .5f, false, new List<Param>()
                {
                    new Param("type", FirstContact.alienLookAt.lookAtTranslator, "alien look at what", "[Alien] will look at what"),
                    new Param("type", FirstContact.translatorLookAt.lookAtAlien, "translator look at what", "[Translator] will look at what"),
                }),
                
                //new GameAction("Version of First Contact",                   delegate { FirstContact.instance.versionOfFirstContact(eventCaller.currentEntity.type);  }, .5f, false, new List<Param>
                //{
                //    new Param("type", FirstContact.VersionOfContact.FirstContact, "Version", "Version of First Contact to play"),
                //}),
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_FirstContact;

    public class FirstContact : Minigame
    {
        public static FirstContact instance { get; private set; }

        [Header("Properties")]
        public int alienSpeakCount;
        public int translatorSpeakCount;
        public bool hasMissed;
        private float lastReportedBeat = 0f;

        [Header("Components")]
        [SerializeField] GameObject alien;
        [SerializeField] Translator translator;
        [SerializeField] GameObject alienSpeech;
        [SerializeField] GameObject dummyHolder;
        [SerializeField] GameObject missionControl;
        [SerializeField] GameObject liveBar;

        [Header("Variables")]
        public bool intervalStarted;
        //float intervalStartBeat;
        public float beatInterval = 4f;
        public bool isCorrect, noHitOnce, isSpeaking;
        //public int version;
        public float lookAtLength = 1f;

        //public enum VersionOfContact
        //{
        //    FirstContact,
        //    CitrusRemix,
        //    SecondContact
        //}

        public enum alienLookAt
        {
            lookAtTranslator,
            idle
        }

        public enum translatorLookAt
        {
            lookAtAlien,
            idle
        }

        private void Awake()
        {
            instance = this;
            translator.Init();
        }


        private void Update()
        {
        //This is taken from the conductor script
            if (Conductor.instance.ReportBeat(ref lastReportedBeat))
            {
                liveBar.GetComponent<Animator>().Play("liveBar", 0, 0);          
            }
            else if(Conductor.instance.songPositionInBeats < lastReportedBeat)
            {
                lastReportedBeat = Mathf.Round(Conductor.instance.songPositionInBeats);
            }
        }

        //public void versionOfFirstContact(int type)
        //{
        //    version = type;
        //}

        public void lookAtDirection(int alienLookAt, int translatorLookAt)
        {
            switch (alienLookAt)
            {
                case 0:
                    alien.GetComponent<Animator>().Play("alien_lookAt", 0, 0);
                    break;
                case 1:
                    alien.GetComponent<Animator>().Play("alien_idle", 0, 0);
                    break;
            }

            switch (translatorLookAt)
            {
                case 0:
                    translator.GetComponent<Animator>().Play("translator_lookAtAlien", 0, 0);
                    break;
                case 1:
                    translator.GetComponent<Animator>().Play("translator_idle", 0, 0);
                    break;
            }
  
        }

        public void alienSpeak(float beat)
        {
            //if (!intervalStarted)
            //{
            //    SetIntervalStart(beat, beatInterval);
            //}
            //missionControl.SetActive(false);
            Jukebox.PlayOneShotGame("firstContact/alien");
            
            var random = Random.Range(0, 2);
            string textToPut = "";
            if(random == 0)
            {
                textToPut = "translator_lookAtAlien";
            }
            else
            {
                textToPut = "translator_lookAtAlien_nod";
            }

            
            BeatAction.New(alien, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { alien.GetComponent<Animator>().Play("alien_talk", 0, 0); }),
                new BeatAction.Action(beat, delegate { translator.GetComponent<Animator>().Play(textToPut, 0, 0); }),
            });

            AlienFirstContact a = Instantiate(alienSpeech, dummyHolder.transform).GetComponent<AlienFirstContact>();
            a.GetComponent<AlienFirstContact>().createBeat = beat;
            alienSpeakCount++;
        }

        public void alienTurnOver(float beat)
        {
            //if (!intervalStarted)
            //{
            //    SetIntervalStart(beat, beatInterval);
            //}

            Jukebox.PlayOneShotGame("firstContact/turnover");

            BeatAction.New(alien, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { alien.GetComponent<Animator>().Play("alien_point", 0, 0); }),
                new BeatAction.Action(beat + .5f, delegate { alien.GetComponent<Animator>().Play("alien_idle", 0, 0); }),
                new BeatAction.Action(beat + .5f, delegate { translator.GetComponent<Animator>().Play("translator_idle", 0, 0); })
            });

            isSpeaking = true;
        }

        public void SetIntervalStart(float beat, float interval = 4f)
        {
            if (!intervalStarted)
            {
                alienSpeakCount = 0;
                translatorSpeakCount = 0;
                intervalStarted = true;
            }

            //intervalStartBeat = beat;
            beatInterval = interval;
        }



        public void alienSuccess(float beat)
        {
            //Make this codeblock smaller
            if (alienSpeakCount == translatorSpeakCount)
            {
                string[] sounds = new string[] { "firstContact/success_1", "firstContact/success_2" };
                var sound = new MultiSound.Sound[]
                {
                    new MultiSound.Sound(sounds[0], beat),
                    new MultiSound.Sound(sounds[1], beat + .5f, offset: .15f)
                };

                MultiSound.Play(sound);

                BeatAction.New(alien, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { alien.GetComponent<Animator>().Play("alien_success", 0, 0); }),
                    new BeatAction.Action(beat + .5f, delegate { alien.GetComponent<Animator>().Play("alien_success", 0, 0); })
                });

                BeatAction.New(translator.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { translator.GetComponent<Animator>().Play("translator_idle", 0, 0); }),
                });
            }
            else if (alienSpeakCount != translatorSpeakCount)
            {
                string[] sounds = new string[] { "firstContact/failAlien_1", "firstContact/failAlien_2" };
                var sound = new MultiSound.Sound[]
                {
                    new MultiSound.Sound(sounds[0], beat),
                    new MultiSound.Sound(sounds[1], beat + .5f)
                };

                MultiSound.Play(sound);

                BeatAction.New(alien, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { alien.GetComponent<Animator>().Play("alien_fail", 0, 0); }),
                    new BeatAction.Action(beat + .5f, delegate { alien.GetComponent<Animator>().Play("alien_fail", 0, 0); })
                });

                BeatAction.New(translator.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { translator.GetComponent<Animator>().Play("translator_idle", 0, 0); }),
                });
            }

            alienSpeakCount = 0;
            translatorSpeakCount = 0;
            isSpeaking = false;
            hasMissed = false;
            noHitOnce = false;
        }

        public void alienNoHit()
        {
            if (!noHitOnce)
            {
                Jukebox.PlayOneShotGame("firstContact/alienNoHit");
                noHitOnce = true;
            }
            BeatAction.New(alien, new List<BeatAction.Action>()
            {
                new BeatAction.Action(.5f, delegate { alien.GetComponent<Animator>().Play("alien_noHit", 0, 0); }),
            });

        }

        public void missionControlDisplay(float beat, bool stay)
        {
            missionControl.SetActive(true);
            string textToPut = "";

            if (alienSpeakCount == translatorSpeakCount)
            {
                textToPut = "missionControl_success";
            }
            else
            {
                textToPut = "missionControl_fail";
            }

            BeatAction.New(missionControl, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { missionControl.GetComponentInParent<Animator>().Play(textToPut, 0, 0); }),
                new BeatAction.Action(beat, delegate { alien.GetComponentInParent<Animator>().Play("alien_idle", 0, 0); }),
                new BeatAction.Action(beat, delegate { translator.GetComponent<Animator>().Play("translator_idle", 0, 0); }),

            });

            if (!stay)
            {
                BeatAction.New(missionControl, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 1f, delegate { missionControl.SetActive(false); }),
                    });
            }
            else
            {
                missionControl.SetActive(true);
            }

            alienSpeakCount = 0;
            translatorSpeakCount = 0;
            isSpeaking = false;
        }
    }
}

