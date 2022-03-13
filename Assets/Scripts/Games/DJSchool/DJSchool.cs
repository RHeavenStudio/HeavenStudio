using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games
{
    using Scripts_DJSchool;

    public class DJSchool : Minigame
    {
        public enum DJVoice
        {
            Standard,
            Cool,
            Hyped
        }

        [Header("Components")]
        [SerializeField] private Student student;
        [SerializeField] private GameObject djYellow;
        private Animator djYellowAnim;
        [SerializeField] private SpriteRenderer headSprite;
        [SerializeField] private Sprite[] headSprites;

        [Header("Properties")]
        public GameEvent bop = new GameEvent();
        private bool djYellowHolding;

        public static DJSchool instance { get; private set; }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            djYellowAnim = djYellow.GetComponent<Animator>();
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                if (cond.songPositionInBeats >= bop.startBeat && cond.songPositionInBeats < bop.startBeat + bop.length)
                {
                    if (student.anim.IsAnimationNotPlaying())
                    {
                        if (student.isHolding)
                        {
                            student.anim.Play("HoldBop", 0, 0);
                        }
                        else
                        {
                            student.anim.Play("IdleBop", 0, 0);
                        }
                    }
                    if (djYellowAnim.IsAnimationNotPlaying())
                    {
                        var yellowState = djYellowAnim.GetCurrentAnimatorStateInfo(0);
                        if (yellowState.IsName("Hey"))
                        {
                            PostScratchoFace();
                        }

                        if (djYellowHolding)
                        {
                            djYellowAnim.Play("HoldBop", 0, 0);
                        }
                        else
                        {
                            // todo: split between left and right bop based on beat
                            djYellowAnim.Play("IdleBop", 0, 0);
                        }
                    }
                }
            }
        }

        public void Bop(float beat, float length)
        {
            bop.startBeat = beat;
            bop.length = length;
        }

        public void BreakCmon(float beat, int type, bool ooh)
        {
            if (djYellowHolding) return;

            string[] sounds = new string[] { };

            switch (type)
            {
                case 0:
                    sounds = new string[] { "djSchool/breakCmon1", "djSchool/breakCmon2", "djSchool/ooh" };
                    break;
                case 1:
                    sounds = new string[] { "djSchool/breakCmonAlt1", "djSchool/breakCmonAlt2", "djSchool/oohAlt" };
                    break;
                case 2:
                    sounds = new string[] { "djSchool/breakCmonLoud1", "djSchool/breakCmonLoud2", "djSchool/oohLoud" };
                    break;
            }

            var sound = new MultiSound.Sound[]
            {
                new MultiSound.Sound(sounds[0],   beat),
                new MultiSound.Sound(sounds[1], beat + 1f - (0.030f/Conductor.instance.secPerBeat)*Conductor.instance.musicSource.pitch),
                new MultiSound.Sound("", beat + 2f)
            };

            if (ooh)
                sound[2] = new MultiSound.Sound(sounds[2], beat + 2f);

            MultiSound.Play(sound);

            BeatAction.New(djYellow, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { djYellow.GetComponent<Animator>().Play("BreakCmon", 0, 0); }),
                new BeatAction.Action(beat + 1f, delegate { djYellow.GetComponent<Animator>().Play("BreakCmon", 0, 0); SetupCue(beat, false); }),
                new BeatAction.Action(beat + 2f, delegate 
                { 
                    djYellow.GetComponent<Animator>().Play("Hold", 0, 0); 
                    djYellowHolding = true;
                }),
            });
        }

        public void AndStop(float beat, bool ooh)
        {
            if (djYellowHolding) return;

            var sound = new MultiSound.Sound[]
            {
                new MultiSound.Sound("djSchool/andStop1",   beat),
                new MultiSound.Sound("djSchool/andStop2",   beat + .5f - (0.1200f/Conductor.instance.secPerBeat)*Conductor.instance.musicSource.pitch),
                new MultiSound.Sound("", beat + 1.5f)
            };

            if (ooh)
                sound[2] = new MultiSound.Sound("djSchool/oohAlt", beat + 1.5f);

            MultiSound.Play(sound);

            BeatAction.New(djYellow, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.5f, delegate { djYellow.GetComponent<Animator>().Play("BreakCmon", 0, 0); SetupCue(beat - 0.5f, false); }),
                new BeatAction.Action(beat + 1.5f, delegate
                {
                    djYellow.GetComponent<Animator>().Play("Hold", 0, 0);
                    djYellowHolding = true;
                }),
            });
        }

        public void ScratchoHey(float beat, int type)
        {
            string[] sounds = new string[] { };

            if (type == 0)
            {
                sounds = new string[] { "djSchool/scratchoHey1", "djSchool/scratchoHey2", "djSchool/scratchoHey3", "djSchool/scratchoHey4", "djSchool/hey" };
            }
            else if (type == 1)
            {
                sounds = new string[] { "djSchool/scratchoHeyAlt1", "djSchool/scratchoHeyAlt2", "djSchool/scratchoHeyAlt3", "djSchool/scratchoHeyAlt4", "djSchool/heyAlt" };
            }
            else if (type == 2)
            {
                sounds = new string[] { "djSchool/scratchoHeyLoud1", "djSchool/scratchoHeyLoud2", "djSchool/scratchoHeyLoud3", "djSchool/scratchoHeyLoud4", "djSchool/heyLoud" };
            }

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound(sounds[0],   beat),
                new MultiSound.Sound(sounds[1], beat + .25f),
                new MultiSound.Sound(sounds[2], beat + .5f),
                new MultiSound.Sound(sounds[3], beat + 1f - (0.0500f/Conductor.instance.secPerBeat)*Conductor.instance.musicSource.pitch),
                new MultiSound.Sound(sounds[4], beat + 2f - (0.070f/Conductor.instance.secPerBeat)*Conductor.instance.musicSource.pitch),
            });

            BeatAction.New(djYellow, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { djYellow.GetComponent<Animator>().Play("Scratcho", 0, 0); }),
                new BeatAction.Action(beat + 1f, delegate { djYellow.GetComponent<Animator>().Play("Scratcho2", 0, 0); SetupCue(beat, true); }),
                new BeatAction.Action(beat + 2.05f, delegate 
                { 
                    djYellow.GetComponent<Animator>().Play("Hey", 0, 0);
                    djYellowHolding = false;
                }),
            });
        }

        void SetupCue(float beat, bool swipe)
        {
            if (swipe)
                student.swipeBeat = beat;
            else
                student.holdBeat = beat;
            
            student.eligible = true;
            student.ResetState();
        }

        public void SetDJYellowHead(int type, bool resetAfterBeats = false)
        {
            headSprite.sprite = headSprites[type];

            if (resetAfterBeats)
            {
                BeatAction.New(djYellow, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(Mathf.Floor(Conductor.instance.songPositionInBeats) + 2f, delegate 
                    { 
                        var yellowState = djYellowAnim.GetCurrentAnimatorStateInfo(0);
                        if (yellowState.IsName("Idle")
                            || yellowState.IsName("IdleBop")
                            || yellowState.IsName("IdleBop2")
                            || yellowState.IsName("BreakCmon"))
                        {
                            SetDJYellowHead(0);
                        }
                    })
                });
            }
        }

        public void PostScratchoFace()
        {
            if (student.missed)
            {
                student.missed = false;
                SetDJYellowHead(3, true);
            }
            else
            {
                SetDJYellowHead(2, true);
            }
        }
    }
}