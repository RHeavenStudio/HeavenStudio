using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class ntrMunchyMonkLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("munchyMonk", "Munchy Monk", "a9f6ff", false, false, new List<GameAction>()
            {
                new GameAction("One", "One")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MunchyMonk.instance.OneGoCue(e.beat);
                    },
                    defaultLength = 1.5f,
                },
                new GameAction("TwoTwo", "Two Two")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.instance.TwoTwoCue(e.beat); 
                    },
                    defaultLength = 2f,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.instance.PreTwoTwoCue(e.beat); 
                    },
                },
                new GameAction("Three", "Three")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.instance.ThreeGoCue(e.beat); 
                    },
                    defaultLength = 3.5f,
                },
                new GameAction("Bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.instance.Bop(e.beat, e["monkBop"]); 
                    },
                    parameters = new List<Param>()
                    {
                        new Param("monkBop", false, "Bop", "Does the Monk bop?"),
                    },
                    defaultLength = 0.5f,
                },
                // note: make the bg not scroll by default
                /*
                new GameAction("ScrollBackground", "Scroll Background")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.instance.ScrollBG(e.beat, true); 
                    },
                    defaultLength = 0.5f,
                },
                */
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MunchyMonk;
    public class MunchyMonk : Minigame
    {
        [Header("Objects")]
        [SerializeField] GameObject OneDumpling;
        [SerializeField] GameObject TwoDumpling1;
        [SerializeField] GameObject TwoDumpling2;
        [SerializeField] GameObject ThreeDumpling1;
        [SerializeField] GameObject ThreeDumpling2;
        [SerializeField] GameObject ThreeDumpling3;

        [Header("Animators")]
        [SerializeField] Animator OneGiverAnim;
        [SerializeField] Animator TwoGiverAnim;
        [SerializeField] Animator ThreeGiverAnim;
        public Animator MonkAnim;
        public Animator MonkArmsAnim;
        public Animator DumplingsAnim;

        [Header("Variables")]
        public float lastReportedBeat = 0f;
        public bool monkBop = true;
        public bool needBlush = false;
        float scrollModifier = 0f;
        const string sfxName = "munchyMonk/";

        public static MunchyMonk instance;
        
        private void Awake()
        {
            instance = this;
        }

        private void Update() 
        {
            if (needBlush && !MonkAnim.IsPlayingAnimationName("Eat")) {
                MonkAnim.DoScaledAnimationAsync("Blush", 0.5f);
                needBlush = false;
            }
            
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                MonkArmsAnim.DoScaledAnimationAsync("WristSlap", 0.5f);
                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound(sfxName+"slap", lastReportedBeat),
                    new MultiSound.Sound(sfxName+"slap_overlay", lastReportedBeat),
                });
            }

            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused) {
                MonkAnim.DoScaledAnimationAsync("Idle", 0.5f);
            }
        }

        private void LateUpdate() 
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat) 
                && !MonkAnim.IsPlayingAnimationName("Eat")
                && !MonkAnim.IsPlayingAnimationName("Blush")
                && monkBop)
            {
                MonkAnim.DoScaledAnimationAsync("Bop", 0.5f);
            };
        }

        public void Bop(float beat, bool doesBop)
        {
            monkBop = doesBop;
        }

        public void OneGoCue(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound(sfxName+"one", beat),
                    new MultiSound.Sound(sfxName+"one_go", beat + 1f),
            });

            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat     , delegate { OneGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); }),
                new BeatAction.Action(beat+0.5f, delegate { OneGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
            });
            
            Dumpling DumplingClone = Instantiate(OneDumpling).GetComponent<Dumpling>();
            DumplingClone.startBeat = beat;
        }

        public void PreTwoTwoCue(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound(sfxName+"two_1", beat - 0.5f),
                new MultiSound.Sound(sfxName+"two_2", beat), 
                new MultiSound.Sound(sfxName+"two_go_1", beat + 1f),
                new MultiSound.Sound(sfxName+"two_go_1", beat + 1.5f),
            });

            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat-0.5f, delegate { TwoGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); }),
                new BeatAction.Action(beat     , delegate { TwoGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
            });

            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat-0.5f, delegate { Dumpling DumplingClone1 = Instantiate(TwoDumpling1).GetComponent<Dumpling>(); DumplingClone1.startBeat = beat-0.5f; }),
                new BeatAction.Action(beat-0.5f, delegate { Dumpling DumplingClone2 = Instantiate(TwoDumpling2).GetComponent<Dumpling>(); DumplingClone2.startBeat = beat-0.5f; }),
            });
        }

        public void TwoTwoCue(float beat)
        {
            
        }

        public void ThreeGoCue(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound(sfxName+"three", beat),
                    new MultiSound.Sound(sfxName+"three_go_1", beat + 1f),
                    new MultiSound.Sound(sfxName+"three_go_2", beat + 2f),
                    new MultiSound.Sound(sfxName+"three_go_3", beat + 3f),
                });
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat     , delegate { Dumpling DumplingClone1 = Instantiate(ThreeDumpling1).GetComponent<Dumpling>(); DumplingClone1.startBeat = beat; }),
                new BeatAction.Action(beat     , delegate { ThreeGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); }),
                new BeatAction.Action(beat+0.5f, delegate { ThreeGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
                new BeatAction.Action(beat  +1f, delegate { Dumpling DumplingClone2 = Instantiate(ThreeDumpling2).GetComponent<Dumpling>(); DumplingClone2.startBeat = beat+1; }),
                new BeatAction.Action(beat  +1f, delegate { ThreeGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); }),
                new BeatAction.Action(beat+1.5f, delegate { ThreeGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
                new BeatAction.Action(beat  +2f, delegate { Dumpling DumplingClone3 = Instantiate(ThreeDumpling3).GetComponent<Dumpling>(); DumplingClone3.startBeat = beat+2; }),
                new BeatAction.Action(beat  +2f, delegate { ThreeGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); }),
                new BeatAction.Action(beat+2.5f, delegate { ThreeGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
            });
            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                
                
            });
        }
    }
}