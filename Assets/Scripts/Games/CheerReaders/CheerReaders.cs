using HeavenStudio.Util;
using JetBrains.Annotations;
using Starpelly.Transformer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using static HeavenStudio.EntityTypes;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlBookLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("cheerReaders", "Cheer Readers \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
            {
                new GameAction("oneTwoThree", "One Two Three!")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.OneTwoThree(e.beat, e["solo"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("solo", false, "Solo", "Should only the player say the voice line?")
                    }
                },
                new GameAction("itsUpToYou", "It's Up To You!")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.ItsUpToYou(e.beat, e["solo"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("solo", false, "Solo", "Should only the player say the voice line?")
                    }
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_CheerReaders;
    public class CheerReaders : Minigame
    {
        public static CheerReaders instance;
        [Header("Components")]
        //Doing this because unity doesn't expose multidimensional/jagged arrays in the inspector - Rasmus
        [SerializeField] List<RvlCharacter> firstRow = new List<RvlCharacter>();
        [SerializeField] List<RvlCharacter> secondRow = new List<RvlCharacter>();
        [SerializeField] List<RvlCharacter> thirdRow = new List<RvlCharacter>();

        [SerializeField] RvlCharacter player;

        void Awake()
        {
            instance = this;
        }

        public void OneTwoThree(float beat, bool solo)
        {
            ScheduleInput(beat, 2, InputType.STANDARD_DOWN, JustFlip, MissFlip, Nothing);
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("cheerReaders/bookHorizontal", beat),
                new MultiSound.Sound("cheerReaders/Solo/123/oneTwoThreeS1", beat),
                new MultiSound.Sound("cheerReaders/bookHorizontal", beat + 1),
                new MultiSound.Sound("cheerReaders/Solo/123/oneTwoThreeS2", beat + 1),
                new MultiSound.Sound("cheerReaders/Solo/123/oneTwoThreeS3", beat + 2),
            };
            if (!solo)
            {
                soundsToPlay.AddRange(new List<MultiSound.Sound>()
                {
                    new MultiSound.Sound("cheerReaders/Girls/123/onegirls", beat),
                    new MultiSound.Sound("cheerReaders/Girls/123/twogirls", beat + 1),
                    new MultiSound.Sound("cheerReaders/Girls/123/threegirls", beat + 2),
                });
            }
            MultiSound.Play(soundsToPlay.ToArray(), forcePlay: true);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    foreach (var nerd in firstRow)
                    {
                        nerd.FlipBook();
                    }
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    foreach (var nerd in secondRow)
                    {
                        nerd.FlipBook();
                    }
                }),
                new BeatAction.Action(beat + 2, delegate
                {
                    foreach (var nerd in thirdRow)
                    {
                        nerd.FlipBook();
                    }
                }),
            });
        }

        public void ItsUpToYou(float beat, bool solo)
        {
            ScheduleInput(beat, 2, InputType.STANDARD_DOWN, JustFlip, MissFlip, Nothing);
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("cheerReaders/bookVertical", beat),
                new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS1", beat),
                new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS2", beat + 0.5f),
                new MultiSound.Sound("cheerReaders/bookVertical", beat + 0.75f),
                new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS3", beat + 0.75f),
                new MultiSound.Sound("cheerReaders/bookVertical", beat + 1.5f),
                new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS4", beat + 1.5f),
                new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS5", beat + 2f),
            };
            if (!solo)
            {
                soundsToPlay.AddRange(new List<MultiSound.Sound>()
                {
                    new MultiSound.Sound("cheerReaders/Girls/UpToYou/itgirls", beat),
                    new MultiSound.Sound("cheerReaders/Girls/UpToYou/sgirls", beat + 0.5f),
                    new MultiSound.Sound("cheerReaders/Girls/UpToYou/upgirls", beat + 0.75f),
                    new MultiSound.Sound("cheerReaders/Girls/UpToYou/togirls", beat + 1.5f),
                    new MultiSound.Sound("cheerReaders/Girls/UpToYou/yougirls", beat + 2f),
                });
            }
            MultiSound.Play(soundsToPlay.ToArray(), forcePlay: true);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    firstRow[0].FlipBook();
                    secondRow[0].FlipBook();
                    thirdRow[0].FlipBook();
                }),
                new BeatAction.Action(beat + 0.75f, delegate
                {
                    firstRow[1].FlipBook();
                    secondRow[1].FlipBook();
                    thirdRow[1].FlipBook();
                }),
                new BeatAction.Action(beat + 1.5f, delegate
                {
                    firstRow[2].FlipBook();
                    secondRow[2].FlipBook();
                    thirdRow[2].FlipBook();
                }),
                new BeatAction.Action(beat + 2f, delegate
                {
                    firstRow[3].FlipBook();
                    secondRow[3].FlipBook();
                }),
            });
        }

        public void LetsGoReadABunchaBooks(float beat, bool solo)
        {
            ScheduleInput(beat, 2, InputType.STANDARD_DOWN, JustFlip, MissFlip, Nothing);
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("cheerReaders/letsGoRead", beat),
                new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS1", beat + 0.25f),
            };
            if (!solo)
            {
                soundsToPlay.AddRange(new List<MultiSound.Sound>()
                {

                });
            }
            MultiSound.Play(soundsToPlay.ToArray(), forcePlay: true);
        }

        void JustFlip(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("cheerReaders/doingong");
                player.FlipBook(); //Need near miss anims
                return;
            }
            SuccessFlip();
        }

        void SuccessFlip()
        {
            player.FlipBook();
            Jukebox.PlayOneShotGame("cheerReaders/bookPlayer");
        }

        void MissFlip(PlayerActionEvent caller)
        {

        }

        void Nothing(PlayerActionEvent caller) {}
    }
}
