using System.Text;
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
            return new Minigame("firstContact", "Second Contact", "1f3833", false, false, new List<GameAction>()
            {
                new GameAction("beat intervals", "Start Interval")
                {
                    function = delegate { var e = eventCaller.currentEntity; FirstContact.instance.SetIntervalStart(e.beat, e.length, e["dialogue"]);  },
                    parameters = new List<Param>()
                    {
                        new Param("dialogue", "REPLACE THIS", "Mistranslation Dialogue", "The line to use when messing up the translation")
                    },
                    defaultLength = 4f,
                    resizable = true,
                    priority = 2,
                },
                new GameAction("alien speak", "Bob Speak")
                {
                    function = delegate { var e = eventCaller.currentEntity; FirstContact.instance.AlienSpeak(e.beat, e["dialogue"], e["spaceNum"]);  },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("spaceNum", new EntityTypes.Integer(0, 12, 0), "Amount of spaces", "Spaces to add before the untranslated icon"),
                        new Param("dialogue", "", "Dialogue", "What should this sound translate to?")
                    },
                    priority = 1
                },
                new GameAction("alien turnover", "Pass Turn")
                {
                    function = delegate { FirstContact.instance.AlienTurnOver(eventCaller.currentEntity.beat, eventCaller.currentEntity.length);  },
                    resizable = true
                },
                new GameAction("alien success", "Success")
                {
                    function = delegate { FirstContact.instance.AlienSuccess(eventCaller.currentEntity.beat);  },
                },
                new GameAction("mission control", "Show Mission Control")
                {
                    function = delegate { var e = eventCaller.currentEntity; FirstContact.instance.MissionControlDisplay(e.beat, e["toggle"], e.length);  },
                    resizable = true,
                    parameters = new List<Param>
                    {
                        new Param("toggle", false, "Stay", "If it's the end of the remix/song")
                    }
                },
                new GameAction("look at", "Look At")
                {
                    function = delegate { FirstContact.instance.LookAtDirection(eventCaller.currentEntity["type"], eventCaller.currentEntity["type2"]);  },
                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", FirstContact.alienLookAt.lookAtAlien, "Bob look at what", "[Bob] will look at what"),
                        new Param("type2", FirstContact.translatorLookAt.lookAtBob, "Alien look at what", "[Alien] will look at what"),
                    }
                },
                new GameAction("live bar beat", "Live Bar Beat")
                {
                    function = delegate { FirstContact.instance.LiveBarBeat(eventCaller.currentEntity["toggle"]);  },
                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "On Beat", "If the live bar animation will be on beat or not")
                    }
                }
            },
            new List<string>() {"ctr", "repeat"},
            "ctrinterpreter", "en",
            new List<string>() {}
            );
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
        [SerializeField] GameObject translateFailTextbox;
        [SerializeField] TMP_Text translateFailText;

        [Header("Variables")]
        int currentVoicelineIndex = -1;
        public bool intervalStarted;
        float intervalStartBeat;
        public float beatInterval = 4f;
        public bool noHitOnce, isSpeaking;
        //public int version;
        public float lookAtLength = 1f;
        bool onBeat;
        float liveBarBeatOffset;

        string onOutDialogue = "YOU SUCK AT CHARTING";
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
            lookAtAlien,
            idle
        }

        public enum translatorLookAt
        {
            lookAtBob,
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

            alienTextbox.SetActive(false);
            translateTextbox.SetActive(false);
            translateFailTextbox.SetActive(false);
        }

        public void SetIntervalStart(float beat, float interval, string outDialogue)
        {
            translator.GetComponent<Animator>().Play("translator_lookAtAlien", 0, 0);
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
            translateFailText.text = "";

            alienTextbox.SetActive(false);
            translateTextbox.SetActive(false);
            translateFailTextbox.SetActive(false);
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
            else if (Conductor.instance.songPositionInBeats < lastReportedBeat)
            {
                lastReportedBeat = Mathf.Round(Conductor.instance.songPositionInBeats);
            }

            if (PlayerInput.Pressed(true) && !IsExpectingInputNow(InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN))
            {
                translator.GetComponent<Animator>().DoScaledAnimationAsync("translator_eh", 0.5f);
                if (isSpeaking)
                {
                    if (callDiagIndex == 0)
                        FailContact();
                    else
                        TrailingContact();
                }
                else if (!noHitOnce && !missionControl.activeInHierarchy)
                {
                    Jukebox.PlayOneShotGame("firstContact/ALIEN_PLAYER_MISS2_A", -1, Jukebox.GetPitchFromSemiTones(UnityEngine.Random.Range(-2, 1), false));
                }
            }
        }

        public void LiveBarBeat(bool onBeat)
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

        public void LookAtDirection(int alienLookAt, int translatorLookAt)
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

        public void AlienSpeak(float beat, string dialogue, int spaceNum)
        {
            queuedInputs.Add(new QueuedSecondContactInput()
            {
                beatAwayFromStart = beat - intervalStartBeat,
                dialogue = dialogue
            });
            int voiceline = UnityEngine.Random.Range(1, 11);
            if (voiceline == currentVoicelineIndex) voiceline++;
            if (voiceline > 10) voiceline = 1;
            currentVoicelineIndex = voiceline;
            Jukebox.PlayOneShotGame("firstContact/Bob" + voiceline, beat, Jukebox.GetPitchFromCents(UnityEngine.Random.Range(-100, 0), false));
            Jukebox.PlayOneShotGame("firstContact/BobB");
            alien.GetComponent<Animator>().DoScaledAnimationAsync("alien_talk", 0.5f);
            if (UnityEngine.Random.Range(0, 5) == 0) translator.GetComponent<Animator>().DoScaledAnimationAsync("translator_lookAtAlien_nod", 0.5f);
            callDiagList.Add(dialogue);

            alienTextbox.SetActive(true);
            for (int i = 0; i < spaceNum * 2; i++)
            {
                callDiagBuffer += " ";
            }
            callDiagBuffer += MSG_MAN;
            UpdateAlienTextbox();
        }

        public void AlienTurnOver(float beat, float length)
        {
            if (queuedInputs.Count == 0) return;
            Jukebox.PlayOneShotGame("firstContact/turnover");
            alienTextbox.SetActive(false);
            alien.GetComponent<Animator>().Play("alien_point", 0, 0);

            isSpeaking = true;
            intervalStarted = false;
            BeatAction.New(alien, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.5f, delegate { alien.GetComponent<Animator>().Play("alien_idle", 0, 0); })
            });
            if (!isSpeaking)
            {
                translator.GetComponent<Animator>().Play("translator_idle", 0, 0);
            }
            foreach (var input in queuedInputs)
            {
                ScheduleInput(beat, length + input.beatAwayFromStart, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, AlienTapping, AlienOnMiss, AlienEmpty);
            }
            queuedInputs.Clear();
        }

        public void AlienSuccess(float beat)
        {
            string animString = "";
            float secondSoundOffset = 0f;
            List<MultiSound.Sound> sound = new List<MultiSound.Sound>();
            if (!(hasMissed || noHitOnce))
            {
                sound = new List<MultiSound.Sound>()
                {
                    new MultiSound.Sound("firstContact/successCrowd", beat),
                    new MultiSound.Sound("firstContact/nod", beat),
                    new MultiSound.Sound("firstContact/nod", beat + 0.5f),
                    new MultiSound.Sound("firstContact/successExtra" + UnityEngine.Random.Range(1, 3), beat + 0.5f, Jukebox.GetPitchFromCents(UnityEngine.Random.Range(-50, 50), false)),
                    new MultiSound.Sound("firstContact/whistle", beat + UnityEngine.Random.Range(0.5f, 1.5f), Jukebox.GetPitchFromCents(UnityEngine.Random.Range(-50, 100), false), UnityEngine.Random.Range(0.4f, 1f)),
                };
                animString = "alien_success";
            }
            else
            {
                sound = new List<MultiSound.Sound>()
                {
                    new MultiSound.Sound("firstContact/fail", beat),
                    new MultiSound.Sound("firstContact/shakeHead", beat),
                    new MultiSound.Sound("firstContact/shakeHead", beat + 0.5f),
                };
                animString = "alien_fail";
            }


            MultiSound.Play(sound.ToArray());

            BeatAction.New(alien, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { alien.GetComponent<Animator>().Play(animString, 0, 0); }),
                new BeatAction.Action(beat + .5f, delegate { alien.GetComponent<Animator>().Play(animString, 0, 0); }),
                new BeatAction.Action(beat + 1, delegate { alienTextbox.SetActive(false); translateTextbox.SetActive(false); translateFailTextbox.SetActive(false); })
            });

            intervalStarted = false;
            isSpeaking = false;
            hasMissed = false;
            noHitOnce = false;
        }

        void UpdateAlienTextbox()
        {
            alienText.text = callDiagBuffer;
        }

        void UpdateTranslateTextbox()
        {
            if (callDiagIndex == 0 && !hasMissed && !noHitOnce)
            {
                // shift the textbox to centre the message
                StringBuilder sb = new StringBuilder();
                foreach (string s in callDiagList)
                {
                    sb.Append(s);
                }
                string fullMsg = sb.ToString();

                // many hardcoded values there'll be a better way to do this
                Vector2 size = translateText.GetPreferredValues(fullMsg, 10.95f, 2);
                translateText.rectTransform.anchoredPosition = new Vector2(Mathf.Max((10.95f/2f) + (-size.x / 2 - 0.25f), -0.25f), Mathf.Max((2.11f / 2f) + (-size.y / 2) + 0.2f, 0.2f));
            }
            translateText.text = respDiagBuffer;
            translateFailText.text = respDiagBuffer;
        }

        public void MissionControlDisplay(float beat, bool stay, float length)
        {
            missionControl.SetActive(true);

            alienTextbox.SetActive(false);
            translateTextbox.SetActive(false);
            translateFailTextbox.SetActive(false);

            string textToPut = "";

            if (!(hasMissed || noHitOnce))
            {
                textToPut = "missionControl_success";
            }
            else
            {
                textToPut = "missionControl_fail";
            }

            missionControl.GetComponentInParent<Animator>().Play(textToPut, 0, 0);
            alien.GetComponentInParent<Animator>().Play("alien_idle", 0, 0);
            translator.GetComponent<Animator>().Play("translator_idle", 0, 0);

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

        void FailContact()
        {
            Jukebox.PlayOneShotGame("firstContact/failContact");
            translator.GetComponent<Animator>().DoScaledAnimationAsync("translator_speak", 0.5f);
            if (!hasMissed && callDiagIndex == 0)
            {
                translateFailTextbox.SetActive(true);
                respDiagBuffer = onOutDialogue;
                UpdateTranslateTextbox();
                ScoreMiss();
            }
            hasMissed = true;
        }

        void TrailingContact()
        {
            Jukebox.PlayOneShotGame("firstContact/slightlyFail");
            translator.GetComponent<Animator>().Play("translator_eh", 0, 0);
            if (!hasMissed)
            {
                respDiagBuffer += MID_MSG_MISS;
                UpdateTranslateTextbox();
            }
            hasMissed = true;
        }

        public void AlienTapping(PlayerActionEvent caller, float state) //OnHit
        {
            if (hasMissed && callDiagIndex == 0)
            {
                caller.isEligible = false;
                ScoreMiss();
                return;
            };

            if (noHitOnce)
            {
                caller.isEligible = false;
                FailContact();
                return;
            }

            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("firstContact/ALIEN_PLAYER_A", -1, Jukebox.GetPitchFromSemiTones(UnityEngine.Random.Range(-3, 3), false));
                translator.GetComponent<Animator>().DoScaledAnimationAsync("translator_speak", 0.5f);
                if (callDiagIndex == 0) return;
                TrailingContact();
                return;
            }

            translator.GetComponent<Animator>().DoScaledAnimationAsync("translator_speak", 0.5f);
            Jukebox.PlayOneShotGame("firstContact/ALIEN_PLAYER_A", -1, Jukebox.GetPitchFromSemiTones(UnityEngine.Random.Range(-3, 3), false));
            Jukebox.PlayOneShotGame("firstContact/ALIEN_PLAYER_B");
            if (hasMissed)
            {
                caller.isEligible = false;
                return;
            }
            else
            {
                respDiagBuffer += callDiagList[callDiagIndex];
                translateTextbox.SetActive(true);
                UpdateTranslateTextbox();
                callDiagIndex++;
            }
        }

        public void AlienOnMiss(PlayerActionEvent caller) //OnMiss
        {
            if (!noHitOnce)
            {
                Jukebox.PlayOneShotGame("firstContact/alienNoHit");
                noHitOnce = true;
            }

            if (callDiagIndex > 0 && !hasMissed)
            {
                respDiagBuffer += MID_MSG_MISS;
                UpdateTranslateTextbox();
                hasMissed = true;
            }

            alien.GetComponent<Animator>().Play("alien_noHit", 0, 0);
        }

        public void AlienEmpty(PlayerActionEvent caller) { } //OnEmpty
    }
}