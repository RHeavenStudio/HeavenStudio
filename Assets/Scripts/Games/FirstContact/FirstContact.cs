using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HeavenStudio.Util;
using TMPro;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrFirstContact
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("firstContact", "First Contact", "008c97", false, false, new List<GameAction>()
            {
                new GameAction("beat intervals", "Start Interval")
                {
                    function = delegate { var e = eventCaller.currentEntity; FirstContact.instance.SetIntervalStart(e.beat, e.length, e["dialogue"]);  }, 
                    parameters = new List<Param>()
                    {
                        new Param("dialogue", "Yo, hairless apes!", "Mistranslation Dialogue", "The line to use when messing up the translation")
                    },
                    defaultLength = 4f, 
                    resizable = true,
                    priority = 1,
                },
                new GameAction("alien speak", "Bob Speak")
                {
                    function = delegate { var e = eventCaller.currentEntity; FirstContact.instance.AlienSpeak(e.beat, e["valA"], e["dialogue"], e["spaceNum"]);  }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("valA", new EntityTypes.Float(.8f, 1.5f, 1f), "Pitch"),
                        new Param("spaceNum", new EntityTypes.Integer(0, 12, 0), "Amount of spaces", "Spaces to add before the untranslated icon"),
                        new Param("dialogue", "", "Dialogue", "What should this sound translate to?")
                    }
                },
                new GameAction("alien turnover", "Pass Turn")
                {
                    function = delegate { FirstContact.instance.alienTurnOver(eventCaller.currentEntity.beat, eventCaller.currentEntity.length);  }, 
                    defaultLength = 0.5f,
                    resizable = true
                },
                new GameAction("alien success", "Success")
                {
                    function = delegate { FirstContact.instance.alienSuccess(eventCaller.currentEntity.beat);  }, 
                },
                new GameAction("mission control", "Show Mission Control")
                {
                    function = delegate { var e = eventCaller.currentEntity; FirstContact.instance.missionControlDisplay(e.beat, e["toggle"], e.length);  }, 
                    resizable = true, 
                    parameters = new List<Param>
                    {
                        new Param("toggle", false, "Stay", "If it's the end of the remix/song")
                    }
                },
                new GameAction("look at", "Look At")
                {
                    function = delegate { FirstContact.instance.lookAtDirection(eventCaller.currentEntity["type"], eventCaller.currentEntity["type"]);  }, 
                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", FirstContact.alienLookAt.lookAtTranslator, "alien look at what", "[Alien] will look at what"),
                        new Param("type2", FirstContact.translatorLookAt.lookAtAlien, "translator look at what", "[Translator] will look at what"),
                    }
                },
                new GameAction("live bar beat", "Live Bar Beat")
                {
                    function = delegate { FirstContact.instance.liveBarBeat(eventCaller.currentEntity["toggle"]);  }, 
                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "On Beat", "If the live bar animation will be on beat or not")
                    }
                },
                
                //new GameAction("Version of First Contact",                   delegate { FirstContact.instance.versionOfFirstContact(eventCaller.currentEntity["type"]);  }, .5f, false, new List<Param>
                //{
                //    new Param("type", FirstContact.VersionOfContact.FirstContact, "Version", "Version of First Contact to play"),
                //}),
            });
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_FirstContact;

    public class FirstContact : Minigame
    {
        const string MID_MSG_MISS = "<color=\"red\"> ..? </color>";
        const string MSG_ALIEN = "<sprite name=\"AlienIcn\">";
        const string MSG_MAN = "<sprite name=\"ManIcn\">";
        // I should add a DonkTroll sprite 🫰🫰🫰🫰🫰

        public static FirstContact instance { get; private set; }

        [Header("Properties")]
        public int alienSpeakCount;
        public int translatorSpeakCount;
        public bool hasMissed;
        private float lastReportedBeat = 0;

        [Header("Components")]
        [SerializeField] GameObject alien;
        [SerializeField] GameObject translator;
        //[SerializeField] GameObject alienSpeech;
        [SerializeField] GameObject dummyHolder;
        [SerializeField] GameObject missionControl;
        [SerializeField] GameObject liveBar;

        [SerializeField] GameObject alienTextbox;
        [SerializeField] TMP_Text alienText;
        [SerializeField] GameObject translateTextbox;
        [SerializeField] TMP_Text translateText;

        [Header("Variables")]
        public bool intervalStarted;
        float intervalStartBeat;
        public float beatInterval = 4f;
        public bool noHitOnce, isSpeaking;
        //public int version;
        public float lookAtLength = 1f;
        bool onBeat;
        float liveBarBeatOffset;

        string onOutDialogue = "I come form Mars!";
        string callDiagBuffer = "";
        string respDiagBuffer = "";
        List<string> callDiagList = new List<string>();
        int callDiagIndex = 0;


        static List<QueuedSecondContactInput> queuedInputs = new List<QueuedSecondContactInput>();
        struct QueuedSecondContactInput
        {
            public float beatAwayFromStart;
            public string dialogue;
        }

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

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            callDiagBuffer = "";
            respDiagBuffer = "";
            callDiagList.Clear();
            callDiagIndex = 0;
        }

        public void SetIntervalStart(float beat, float interval, string outDialogue)
        {
            if (!intervalStarted)
            {
                //alienSpeakCount = 0;
                //translatorSpeakCount = 0;
                intervalStarted = true;
            }

            intervalStartBeat = beat;
            beatInterval = interval;

            onOutDialogue = outDialogue;
            callDiagBuffer = "";
            respDiagBuffer = "";
            callDiagList.Clear();
            callDiagIndex = 0;

            alienText.text = "";
            translateText.text = "";

            alienTextbox.SetActive(false);
            translateTextbox.SetActive(false);
        }

        private void Update()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
            }
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused && intervalStarted)
            {
                intervalStarted = false;
            }
            if (Conductor.instance.ReportBeat(ref lastReportedBeat, offset: liveBarBeatOffset))
            {
                liveBar.GetComponent<Animator>().Play("liveBar", 0, 0);          
            }
            else if(Conductor.instance.songPositionInBeats < lastReportedBeat)
            {
                lastReportedBeat = Mathf.Round(Conductor.instance.songPositionInBeats);
            }

            if (PlayerInput.Pressed() && !IsExpectingInputNow() && !noHitOnce && !isSpeaking && !missionControl.activeInHierarchy)
            {
                Jukebox.PlayOneShotGame("firstContact/" + randomizerLines());
                BeatAction.New(this.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(.5f, delegate { translator.GetComponent<Animator>().Play("translator_speak", 0, 0);}),
                });
            }
            if ((PlayerInput.Pressed(true) && !IsExpectingInputNow(InputType.STANDARD_DOWN|InputType.DIRECTION_DOWN) && isSpeaking))
            {
                hasMissed = true;
            }
        }

        //public void versionOfFirstContact(int type)
        //{
        //    version = type;
        //}

        public void liveBarBeat(bool onBeat)
        {
            if (onBeat)
            {
                liveBarBeatOffset = 0;
            }
            else
            {
                liveBarBeatOffset = .5f;
            }
        }

        public void lookAtDirection(int alienLookAt, int translatorLookAt)
        {
            Debug.Log(alienLookAt);
            Debug.Log(translatorLookAt);
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

        public void AlienSpeak(float beat, float pitch, string dialogue, int spaceNum)
        {
            queuedInputs.Add(new QueuedSecondContactInput()
            {
                beatAwayFromStart = beat - intervalStartBeat,
                dialogue = dialogue
            });
            Jukebox.PlayOneShotGame("firstContact/Bob" + randomizerLines().ToString(), beat, pitch);
            alien.GetComponent<Animator>().DoScaledAnimationAsync("alien_talk", 0.5f);
            callDiagList.Add(dialogue);

            alienTextbox.SetActive(true);
            for (int i = 0; i < spaceNum*2; i++)
            {
                callDiagBuffer += " ";
            }
            callDiagBuffer += MSG_MAN;
            UpdateAlienTextbox();
        }

        public void alienTurnOver(float beat, float length)
        {
            if (queuedInputs.Count == 0) return;
            Jukebox.PlayOneShotGame("firstContact/turnover");
            alienTextbox.SetActive(false);
            alien.GetComponent<Animator>().Play("alien_point", 0, 0);

            isSpeaking = true;
            intervalStarted = false;
            BeatAction.New(alien, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + .5f, delegate { 
                    alien.GetComponent<Animator>().Play("alien_idle", 0, 0); 
                    if (!isSpeaking)
                    {
                        translator.GetComponent<Animator>().Play("translator_idle", 0, 0);
                    }
                }),
            });
            foreach (var input in queuedInputs)
            {
                ScheduleInput(beat, length + input.beatAwayFromStart, InputType.STANDARD_DOWN|InputType.DIRECTION_DOWN, alienTapping, alienOnMiss, AlienEmpty);
            }
            queuedInputs.Clear();
        }

        public void alienSuccess(float beat)
        {
            string[] sfxStrings = { "", "" };
            string animString = "";
            float secondSoundOffset = 0f;

            if (!hasMissed)
            {
                sfxStrings[0] = "firstContact/success_1";
                sfxStrings[1] = "firstContact/success_2";
                animString = "alien_success";
                secondSoundOffset = 0.1f;
            }
            else
            {
                sfxStrings[0] = "firstContact/failAlien_1";
                sfxStrings[1] = "firstContact/failAlien_2";
                animString = "alien_fail";
            }

            string[] sounds = new string[] { sfxStrings[0], sfxStrings[1] };
            var sound = new MultiSound.Sound[]
            {
                new MultiSound.Sound(sounds[0], beat),
                new MultiSound.Sound(sounds[1], beat + .5f, 1f, 1f, false, secondSoundOffset)
            };

            MultiSound.Play(sound);

            BeatAction.New(alien, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { alien.GetComponent<Animator>().Play(animString, 0, 0); }),
                new BeatAction.Action(beat + .5f, delegate { alien.GetComponent<Animator>().Play(animString, 0, 0); })
            });

            BeatAction.New(translator.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { translator.GetComponent<Animator>().Play("translator_idle", 0, 0); }),
            });
            intervalStarted = false;
            isSpeaking = false;
            hasMissed = false;
            noHitOnce = false;
        }

        void UpdateAlienTextbox()
        {
            Debug.Log(callDiagBuffer);
            alienText.text = callDiagBuffer;
        }

        void UpdateTranslateTextbox()
        {
            Debug.Log(respDiagBuffer);
        }

        public void missionControlDisplay(float beat, bool stay, float length)
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
                new BeatAction.Action(length, delegate { missionControl.GetComponentInParent<Animator>().Play(textToPut, 0, 0); }),
                new BeatAction.Action(length, delegate { alien.GetComponentInParent<Animator>().Play("alien_idle", 0, 0); }),
                new BeatAction.Action(length, delegate { translator.GetComponent<Animator>().Play("translator_idle", 0, 0); }),

            });

            if (!stay)
            {
                BeatAction.New(missionControl, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + length, delegate { missionControl.SetActive(false); }),
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

        public void alienTapping(PlayerActionEvent caller, float state) //OnHit
        {
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("firstContact/slightlyFail");
                BeatAction.New(this.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(.5f, delegate { translator.GetComponent<Animator>().Play("translator_eh", 0, 0);}),
                });
                hasMissed = true;
                respDiagBuffer += MID_MSG_MISS;
                UpdateTranslateTextbox();
                return;
            }

            BeatAction.New(this.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(.5f, delegate { translator.GetComponent<Animator>().Play("translator_speak", 0, 0);}),
            });
            Jukebox.PlayOneShotGame("firstContact/alien");
            if (!hasMissed)
            {
                respDiagBuffer += callDiagList[callDiagIndex];
                callDiagIndex++;
                UpdateTranslateTextbox();
            }
        }

        public void alienOnMiss(PlayerActionEvent caller) //OnMiss
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
            hasMissed = true;
        }

        public void AlienEmpty(PlayerActionEvent caller) //OnEmpty
        {
            //empty
        }

        public int randomizerLines()
        {
            return Random.Range(1, 11);
        }
    }
}