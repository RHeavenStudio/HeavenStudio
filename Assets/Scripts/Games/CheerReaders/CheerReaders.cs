using HeavenStudio.Util;
using JetBrains.Annotations;
using Starpelly.Transformer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using static HeavenStudio.EntityTypes;
using static HeavenStudio.Games.CheerReaders;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlBookLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("cheerReaders", "Cheer Readers", "ffffde", false, false, new List<GameAction>()
            {
                new GameAction("oneTwoThree", "One Two Three!")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.OneTwoThree(e.beat, e["solo"]); CheerReaders.instance.SetIsDoingCue(e.beat, e.length);},
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("solo", CheerReaders.WhoSpeaks.Both, "Who Speaks", "Who should say the voice line?")
                    }
                },
                new GameAction("itsUpToYou", "It's Up To You!")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.ItsUpToYou(e.beat, e["solo"]); CheerReaders.instance.SetIsDoingCue(e.beat, e.length);},
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("solo", CheerReaders.WhoSpeaks.Both, "Who Speaks", "Who should say the voice line?")
                    }
                },
                new GameAction("letsGoReadABunchaBooks", "Let's Go Read A Buncha Books!")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.LetsGoReadABunchaBooks(e.beat, e["solo"]); CheerReaders.instance.SetIsDoingCue(e.beat, e.length);},
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("solo", CheerReaders.WhoSpeaks.Both, "Who Speaks", "Who should say the voice line?")
                    }
                },
                new GameAction("rahRahSisBoomBaBoom", "Rah-Rah Sis Boom Bah-Boom!")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.RahRahSisBoomBaBoom(e.beat, e["solo"], e["consecutive"]); CheerReaders.instance.SetIsDoingCue(e.beat, e.length);},
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("solo", CheerReaders.WhoSpeaks.Both, "Who Speaks", "Who should say the voice line?"),
                        new Param("consecutive", false, "Consecutive", "Is this cue using the alternate consecutive version?")
                    }
                },
                new GameAction("okItsOn", "OK It's On!")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.OkItsOnStretchable(e.beat, e.length, e["solo"], e["toggle"], e["poster"], e["happy"]); CheerReaders.instance.SetIsDoingCue(e.beat, e.length, false);},
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("solo", CheerReaders.WhoSpeaks.Both, "Who Speaks", "Who should say the voice line?"),
                        new Param("toggle", true, "Whistle", "Should the whistle sound play?"),
                        new Param("poster", CheerReaders.PosterToChoose.Random, "Poster", "Which image should the cheer readers display?"),
                        new Param("happy", true, "Do Happy Face?", "Will the cheer readers do a happy smile 2 beats after showing off their books?")
                    }
                },
                new GameAction("okItsOnStretch", "OK It's On! (Stretchable)")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.OkItsOnStretchable(e.beat, e.length, e["solo"], e["toggle"], e["poster"], e["happy"]); CheerReaders.instance.SetIsDoingCue(e.beat, e.length, false); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("solo", CheerReaders.WhoSpeaks.Both, "Who Speaks", "Who should say the voice line?"),
                        new Param("toggle", true, "Whistle", "Should the whistle sound play?"),
                        new Param("poster", CheerReaders.PosterToChoose.Random, "Poster", "Which image should the cheer readers display?"),
                        new Param("happy", true, "Do Happy Face?", "Will the cheer readers do a happy smile 2 beats after showing off their books?")
                    }
                },
                new GameAction("yay", "Yay")
                {
                    function = delegate {CheerReaders.instance.Yay(eventCaller.currentEntity["solo"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("solo", CheerReaders.WhoSpeaks.Both, "Who Speaks", "Who should say the voice line?"),
                    }
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.BopToggle(e.beat, e.length, e["toggle"], e["toggle2"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Should bop?", "Should the nerds bop?"),
                        new Param("toggle2", false, "Should auto bop?", "Should the nerds auto bop?")
                    }
                },
                new GameAction("resetPose", "Reset Pose")
                {
                    function = delegate {CheerReaders.instance.ResetPose(); },
                    defaultLength = 0.5f
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
        public enum WhoSpeaks
        {
            Solo = 0,
            Girls = 1,
            Both = 2
        }
        public enum PosterToChoose
        {
            DJSchool = 0,
            Lockstep = 1,
            RhythmTweezers = 2,
            Random = 14
        }
        [System.Serializable]
        public struct PosterImages
        {
            public Sprite topPart;
            public Sprite middlePart;
            public Sprite bottomPart;
            public Sprite miss;
        }
        [Header("Components")]
        //Doing this because unity doesn't expose multidimensional/jagged arrays in the inspector - Rasmus
        [SerializeField] List<RvlCharacter> firstRow = new List<RvlCharacter>();
        [SerializeField] List<RvlCharacter> secondRow = new List<RvlCharacter>();
        [SerializeField] List<RvlCharacter> thirdRow = new List<RvlCharacter>();
        List<RvlCharacter> allGirls = new List<RvlCharacter>();
        [SerializeField] List<GameObject> topMasks = new List<GameObject>();
        [SerializeField] List<GameObject> middleMasks = new List<GameObject>();
        [SerializeField] List<GameObject> bottomMasks = new List<GameObject>();
        [SerializeField] GameObject playerMask;
        [SerializeField] GameObject missPoster;
        [SerializeField] SpriteRenderer topPoster;
        [SerializeField] SpriteRenderer middlePoster;
        [SerializeField] SpriteRenderer bottomPoster;
        [SerializeField] ParticleSystem whiteYayParticle;
        [SerializeField] ParticleSystem blackYayParticle;

        [SerializeField] RvlCharacter player;
        Sound SpinningLoop;
        [Header("Variables")]
        [SerializeField] List<PosterImages> posters = new List<PosterImages>();
        bool shouldBop = true;
        bool canBop = true;
        public bool doingCue;
        float cueLength;
        float cueBeat;
        bool shouldYay;
        bool shouldDoSuccessZoom;
        public bool shouldBeBlack = false;
        public GameEvent bop = new GameEvent();
        int currentZoomIndex;
        float currentZoomCamBeat;
        float currentZoomCamLength;
        private List<DynamicBeatmap.DynamicEntity> allCameraEvents = new List<DynamicBeatmap.DynamicEntity>();

        void OnDestroy()
        {
            Jukebox.KillLoop(SpinningLoop, 0.5f);
        }

        public override void OnTimeChange()
        {
            UpdateCameraZoom();
        }

        void Awake()
        {
            instance = this;
            for (int i = 0; i < topMasks.Count; i++)
            {
                firstRow[i].posterBook = topMasks[i];
            }
            for (int i = 0; i < middleMasks.Count; i++)
            {
                secondRow[i].posterBook = middleMasks[i];
            }
            for (int i = 0; i < bottomMasks.Count; i++)
            {
                thirdRow[i].posterBook = bottomMasks[i];
            }
            player.posterBook = playerMask;
            allGirls.AddRange(firstRow);
            allGirls.AddRange(secondRow);
            allGirls.AddRange(thirdRow);
            var camEvents = EventCaller.GetAllInGameManagerList("cheerReaders", new string[] { "okItsOn" });
            List<DynamicBeatmap.DynamicEntity> tempEvents = new List<DynamicBeatmap.DynamicEntity>();
            for (int i = 0; i < camEvents.Count; i++)
            {
                if (camEvents[i].beat + camEvents[i].beat >= Conductor.instance.songPositionInBeats)
                {
                    tempEvents.Add(camEvents[i]);
                }
            }

            allCameraEvents = tempEvents;

            UpdateCameraZoom();
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1) && shouldBop)
            {
                BopSingle();
            }

            if (cond.isPlaying && !cond.isPaused)
            {
                if (allCameraEvents.Count > 0)
                {
                    if (currentZoomIndex < allCameraEvents.Count && currentZoomIndex >= 0)
                    {
                        if (Conductor.instance.songPositionInBeats >= allCameraEvents[currentZoomIndex].beat)
                        {
                            UpdateCameraZoom();
                            currentZoomIndex++;
                        }
                    }

                    float normalizedZoomOutBeat = cond.GetPositionFromBeat(currentZoomCamBeat + 2 * (currentZoomCamLength * 0.25f), 1 * (currentZoomCamLength * 0.25f));
                    float normalizedZoomInBeat = cond.GetPositionFromBeat(currentZoomCamBeat + 3 * (currentZoomCamLength * 0.25f), 0.1f);
                    float normalizedZoomOutAgainBeat = cond.GetPositionFromBeat(currentZoomCamBeat + 3 * (currentZoomCamLength * 0.25f) + 0.1f, 1.1f);
                    if (normalizedZoomOutAgainBeat >= 0)
                    {
                        if (normalizedZoomOutAgainBeat > 1)
                        {
                            GameCamera.additionalPosition = new Vector3(0, 0, 0);
                        }
                        else
                        {
                            EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInOutQuint);
                            float newZoom = func(shouldDoSuccessZoom ? 4f : 1.5f, 0, normalizedZoomOutAgainBeat);
                            GameCamera.additionalPosition = new Vector3(0, 0, newZoom);
                        }
                    }
                    else if (normalizedZoomInBeat >= 0)
                    {
                        if (normalizedZoomInBeat > 1)
                        {
                            GameCamera.additionalPosition = new Vector3(0, 0, shouldDoSuccessZoom ? 4f : 1.5f);
                        }
                        else
                        {
                            EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutQuint);
                            float newZoom = func(-1, shouldDoSuccessZoom ? 4f : 1.5f, normalizedZoomInBeat);
                            GameCamera.additionalPosition = new Vector3(0, 0, newZoom);
                        }
                    }
                    else if (normalizedZoomOutBeat >= 0)
                    {
                        if (normalizedZoomOutBeat > 1)
                        {
                            GameCamera.additionalPosition = new Vector3(0, 0, -1);
                        }
                        else
                        {
                            EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutQuint);
                            float newZoom = func(0f, 1f, normalizedZoomOutBeat);
                            GameCamera.additionalPosition = new Vector3(0, 0, newZoom * -1);
                        }
                    }
                }
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    player.FlipBook(false);
                    missPoster.SetActive(false);
                    Jukebox.PlayOneShotGame("cheerReaders/miss");
                    ScoreMiss(1f);
                }
                if (PlayerInput.AltPressed() && !IsExpectingInputNow(InputType.STANDARD_ALT_DOWN)) 
                {
                    Jukebox.PlayOneShotGame("cheerReaders/doingoing");
                    player.StartSpinBook();
                    missPoster.SetActive(false);
                    SpinningLoop = Jukebox.PlayOneShotGame("cheerReaders/bookSpinLoop", -1, 1, 1, true);
                    ScoreMiss(1f);
                }
                if (PlayerInput.AltPressedUp() && !IsExpectingInputNow(InputType.STANDARD_ALT_UP))
                {
                    Jukebox.PlayOneShotGame("cheerReaders/doingoing");
                    player.StopSpinBook();
                    Jukebox.KillLoop(SpinningLoop, 0f);
                    ScoreMiss(1f);
                    missPoster.SetActive(true);
                }
                float normalizedBeat = cond.GetPositionFromBeat(cueBeat, cueLength);
                if (normalizedBeat >= 0)
                {
                    if (normalizedBeat > 1f)
                    {
                        doingCue = false;
                    }
                    else
                    {
                        doingCue = true;
                    }
                }
            }
            else if (!cond.isPlaying)
            {
                Jukebox.KillLoop(SpinningLoop, 0.5f);
            }
        }

        void SetPosterImage(int posterToChoose)
        {
            if (posterToChoose >= posters.Count || posterToChoose < 0) posterToChoose = UnityEngine.Random.Range(0, 3);
            topPoster.sprite = posters[posterToChoose].topPart;
            middlePoster.sprite = posters[posterToChoose].middlePart;
            bottomPoster.sprite = posters[posterToChoose].bottomPart;
            missPoster.GetComponent<SpriteRenderer>().sprite = posters[posterToChoose].miss;
        }

        public void ResetPose()
        {
            canBop = true;
            player.ResetPose();
            foreach (var nerd in allGirls)
            {
                nerd.ResetPose();
            }
            foreach (var mask in topMasks)
            {
                mask.SetActive(false);
            }
            foreach (var mask in middleMasks)
            {
                mask.SetActive(false);
            }
            foreach (var mask in bottomMasks)
            {
                mask.SetActive(false);
            }
            playerMask.SetActive(false);
        }

        void UpdateCameraZoom()
        {
            if (currentZoomIndex < allCameraEvents.Count && currentZoomIndex >= 0)
            {
                currentZoomCamBeat = allCameraEvents[currentZoomIndex].beat;
                currentZoomCamLength = allCameraEvents[currentZoomIndex].length;
            }
        }

        public void Yay(int whoSpeaks)
        {
            if (!shouldYay) return;
            if (shouldBeBlack)
            {
                blackYayParticle.Play();
            }
            else
            {
                whiteYayParticle.Play();
            }
            playerMask.SetActive(false);
            missPoster.SetActive(false);
            foreach (var mask in topMasks)
            {
                mask.SetActive(false);
            }
            foreach (var mask in middleMasks)
            {
                mask.SetActive(false);
            }
            foreach (var mask in bottomMasks)
            {
                mask.SetActive(false);
            }
            switch (whoSpeaks)
            {
                case (int)WhoSpeaks.Solo:
                    Jukebox.PlayOneShotGame("cheerReaders/Solo/yayS");
                    player.Yay(true);
                    foreach (var nerd in allGirls)
                    {
                        nerd.Yay(true);
                    }
                    break;
                case (int)WhoSpeaks.Girls:
                    Jukebox.PlayOneShotGame("cheerReaders/Girls/yayGirls");
                    foreach (var nerd in allGirls)
                    {
                        nerd.Yay(true);
                    }
                    player.Yay(false);
                    break;
                default:
                    Jukebox.PlayOneShotGame("cheerReaders/All/yay");
                    foreach (var nerd in allGirls)
                    {
                        nerd.Yay(true);
                    }
                    player.Yay(true);
                    break;
            }
        }

        public void BopToggle(float beat, float length, bool startBop, bool bopAuto)
        {
            shouldBop = bopAuto;
            if (startBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            BopSingle();
                        })
                    });
                }
            }
        }

        void BopSingle()
        {
            if (canBop)
            {
                foreach (var nerd in firstRow)
                {
                    nerd.Bop();
                }
                foreach (var nerd in secondRow)
                {
                    nerd.Bop();
                }
                foreach (var nerd in thirdRow)
                {
                    nerd.Bop();
                }
                player.Bop();
            }
        }

        public void SetIsDoingCue(float beat, float length, bool shouldSwitchColor = true)
        {
            if (!doingCue) shouldYay = false;
            foreach (var girl in allGirls)
            {
                girl.ResetFace();
            }
            player.ResetFace();
            doingCue = true;
            cueBeat = beat;
            cueLength = length - 1f;
            if (!shouldSwitchColor) return;
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length * 0.5f, delegate { shouldBeBlack = !shouldBeBlack; })
            });
        }

        public void OneTwoThree(float beat, int whoSpeaks)
        {
            canBop = false;
            ScheduleInput(beat, 2, InputType.STANDARD_DOWN, JustFlip, MissFlip, Nothing);
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("cheerReaders/bookHorizontal", beat),
                new MultiSound.Sound("cheerReaders/bookHorizontal", beat + 1),
            };
            switch (whoSpeaks)
            {
                case (int)WhoSpeaks.Solo:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Solo/123/oneTwoThreeS1", beat),
                        new MultiSound.Sound("cheerReaders/Solo/123/oneTwoThreeS2", beat + 1),
                        new MultiSound.Sound("cheerReaders/Solo/123/oneTwoThreeS3", beat + 2),
                    });
                    break;
                case (int)WhoSpeaks.Girls:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Girls/123/onegirls", beat),
                        new MultiSound.Sound("cheerReaders/Girls/123/twogirls", beat + 1),
                        new MultiSound.Sound("cheerReaders/Girls/123/threegirls", beat + 2),
                    });
                    break;
                case (int)WhoSpeaks.Both:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Girls/123/onegirls", beat),
                        new MultiSound.Sound("cheerReaders/Girls/123/twogirls", beat + 1),
                        new MultiSound.Sound("cheerReaders/Girls/123/threegirls", beat + 2),
                        new MultiSound.Sound("cheerReaders/Solo/123/oneTwoThreeS1", beat),
                        new MultiSound.Sound("cheerReaders/Solo/123/oneTwoThreeS2", beat + 1),
                        new MultiSound.Sound("cheerReaders/Solo/123/oneTwoThreeS3", beat + 2),
                    });
                    break;
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
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.OneTwoThree(1);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.OneTwoThree(1);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.OneTwoThree(1);
                            foreach (var nerd in allGirls)
                            {
                                nerd.OneTwoThree(1);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    foreach (var nerd in secondRow)
                    {
                        nerd.FlipBook();
                    }
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.OneTwoThree(2);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.OneTwoThree(2);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.OneTwoThree(2);
                            foreach (var nerd in allGirls)
                            {
                                nerd.OneTwoThree(2);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 2, delegate
                {
                    foreach (var nerd in thirdRow)
                    {
                        nerd.FlipBook();
                    }
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.OneTwoThree(3);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.OneTwoThree(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.OneTwoThree(3);
                            foreach (var nerd in allGirls)
                            {
                                nerd.OneTwoThree(3);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 2.5f, delegate
                {
                    if (!doingCue) canBop = true;
                })
            });
        }

        public void ItsUpToYou(float beat, int whoSpeaks)
        {
            canBop = false;
            ScheduleInput(beat, 2, InputType.STANDARD_DOWN, JustFlip, MissFlip, Nothing);
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("cheerReaders/bookVertical", beat),
                new MultiSound.Sound("cheerReaders/bookVertical", beat + 0.75f),
                new MultiSound.Sound("cheerReaders/bookVertical", beat + 1.5f),
            };
            switch (whoSpeaks)
            {
                case (int)WhoSpeaks.Solo:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS1", beat),
                        new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS2", beat + 0.5f),
                        new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS3", beat + 0.75f),
                        new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS4", beat + 1.5f),
                        new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS5", beat + 2f),
                    });
                    break;
                case (int)WhoSpeaks.Girls:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Girls/UpToYou/itgirls", beat),
                        new MultiSound.Sound("cheerReaders/Girls/UpToYou/sgirls", beat + 0.5f),
                        new MultiSound.Sound("cheerReaders/Girls/UpToYou/upgirls", beat + 0.75f),
                        new MultiSound.Sound("cheerReaders/Girls/UpToYou/togirls", beat + 1.5f),
                        new MultiSound.Sound("cheerReaders/Girls/UpToYou/yougirls", beat + 2f),
                    });
                    break;
                case (int)WhoSpeaks.Both:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Girls/UpToYou/itgirls", beat),
                        new MultiSound.Sound("cheerReaders/Girls/UpToYou/sgirls", beat + 0.5f),
                        new MultiSound.Sound("cheerReaders/Girls/UpToYou/upgirls", beat + 0.75f),
                        new MultiSound.Sound("cheerReaders/Girls/UpToYou/togirls", beat + 1.5f),
                        new MultiSound.Sound("cheerReaders/Girls/UpToYou/yougirls", beat + 2f),
                        new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS1", beat),
                        new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS2", beat + 0.5f),
                        new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS3", beat + 0.75f),
                        new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS4", beat + 1.5f),
                        new MultiSound.Sound("cheerReaders/Solo/UpToYou/itsUpToYouS5", beat + 2f),
                    });
                    break;
            }
            MultiSound.Play(soundsToPlay.ToArray(), forcePlay: true);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    firstRow[0].FlipBook();
                    secondRow[0].FlipBook();
                    thirdRow[0].FlipBook();
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(1);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(1);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(1);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(1);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 0.75f, delegate
                {
                    firstRow[1].FlipBook();
                    secondRow[1].FlipBook();
                    thirdRow[1].FlipBook();
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(2);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(2);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(2);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(2);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 1.5f, delegate
                {
                    firstRow[2].FlipBook();
                    secondRow[2].FlipBook();
                    thirdRow[2].FlipBook();
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(3);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 2f, delegate
                {
                    firstRow[3].FlipBook();
                    secondRow[3].FlipBook();
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(4);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(4);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(4);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(4);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 2.5f, delegate
                {
                    if (!doingCue) canBop = true;
                })
            });
        }

        public void LetsGoReadABunchaBooks(float beat, int whoSpeaks)
        {
            canBop = false;
            ScheduleInput(beat, 2, InputType.STANDARD_DOWN, JustFlip, MissFlip, Nothing);
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("cheerReaders/letsGoRead", beat),
                new MultiSound.Sound("cheerReaders/bookDiagonal", beat + 0.75f),
                new MultiSound.Sound("cheerReaders/bookDiagonal", beat + 1f),
                new MultiSound.Sound("cheerReaders/bookDiagonal", beat + 1.25f),
                new MultiSound.Sound("cheerReaders/bookDiagonal", beat + 1.5f),
                new MultiSound.Sound("cheerReaders/bookDiagonal", beat + 1.75f),
            };
            switch (whoSpeaks)
            {
                case (int)WhoSpeaks.Solo:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS1", beat + 0.25f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS2", beat + 0.5f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS3", beat + 0.75f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS4", beat + 1f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS5", beat + 1.25f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS6", beat + 1.5f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS7", beat + 1.75f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS8", beat + 2f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS9", beat + 2.5f),
                    });
                    break;
                case (int)WhoSpeaks.Girls:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls1", beat + 0.25f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls2", beat + 0.5f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls3", beat + 0.75f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls4", beat + 1f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls5", beat + 1.25f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls6", beat + 1.5f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls7", beat + 1.75f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls8", beat + 2f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls9", beat + 2.5f),
                    });
                    break;
                case (int)WhoSpeaks.Both:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls1", beat + 0.25f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls2", beat + 0.5f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls3", beat + 0.75f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls4", beat + 1f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls5", beat + 1.25f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls6", beat + 1.5f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls7", beat + 1.75f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls8", beat + 2f),
                        new MultiSound.Sound("cheerReaders/Girls/LetsGoRead/bunchaBooksgirls9", beat + 2.5f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS1", beat + 0.25f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS2", beat + 0.5f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS3", beat + 0.75f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS4", beat + 1f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS5", beat + 1.25f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS6", beat + 1.5f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS7", beat + 1.75f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS8", beat + 2f),
                        new MultiSound.Sound("cheerReaders/Solo/LetsGoRead/bunchaBooksS9", beat + 2.5f),
                    });
                    break;
            }
            MultiSound.Play(soundsToPlay.ToArray(), forcePlay: true);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(1);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(1);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(1);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(1);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 0.75f, delegate
                {
                    firstRow[0].FlipBook();
                }),
                new BeatAction.Action(beat + 1f, delegate
                {
                    firstRow[1].FlipBook();
                    secondRow[0].FlipBook();
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(1);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(1);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(1);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(1);
                            }
                            break;
                    }

                }),
                new BeatAction.Action(beat + 1.25f, delegate
                {
                    firstRow[2].FlipBook();
                    secondRow[1].FlipBook();
                    thirdRow[0].FlipBook();
                }),
                new BeatAction.Action(beat + 1.5f, delegate
                {
                    firstRow[3].FlipBook();
                    secondRow[2].FlipBook();
                    thirdRow[1].FlipBook();
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(3);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 1.75f, delegate
                {
                    secondRow[3].FlipBook();
                    thirdRow[2].FlipBook();
                }),
                new BeatAction.Action(beat + 2f, delegate
                {
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(3);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 2.5f, delegate
                {
                    if (!doingCue) canBop = true;
                })
            });
        }

        public void RahRahSisBoomBaBoom(float beat, int whoSpeaks, bool consecutive)
        {
            canBop = false;
            ScheduleInput(beat, 2.5f, InputType.STANDARD_DOWN, JustFlipBoom, MissFlip, Nothing);
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("cheerReaders/bookDiagonal", beat + 0.5f),
                new MultiSound.Sound("cheerReaders/bookDiagonal", beat + 1f),
                new MultiSound.Sound("cheerReaders/bookDiagonal", beat + 1.5f),
                new MultiSound.Sound("cheerReaders/bookDiagonal", beat + 2f),
            };
            switch (whoSpeaks)
            {
                case (int)WhoSpeaks.Solo:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Solo/RRSBBB/rahRahSisBoomBaBoomS1", beat),
                        new MultiSound.Sound("cheerReaders/Solo/RRSBBB/rahRahSisBoomBaBoomS2", beat + 0.5f),
                        new MultiSound.Sound("cheerReaders/Solo/RRSBBB/rahRahSisBoomBaBoomS3", beat + 1f, 1, 1, false, 0.081f),
                        new MultiSound.Sound("cheerReaders/Solo/RRSBBB/rahRahSisBoomBaBoomS4", beat + 1.5f),
                        new MultiSound.Sound("cheerReaders/Solo/RRSBBB/rahRahSisBoomBaBoomS5", beat + 2f),
                        new MultiSound.Sound("cheerReaders/Solo/RRSBBB/rahRahSisBoomBaBoomS6", beat + 2.5f),
                    });
                    break;
                case (int)WhoSpeaks.Girls:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Girls/RRSBBB/rahRahSisBoomBaBoomgirls1", beat),
                        new MultiSound.Sound("cheerReaders/Girls/RRSBBB/rahRahSisBoomBaBoomgirls2", beat + 0.5f),
                        new MultiSound.Sound("cheerReaders/Girls/RRSBBB/rahRahSisBoomBaBoomgirls3", beat + 1f, 1, 1, false, 0.116f),
                        new MultiSound.Sound("cheerReaders/Girls/RRSBBB/rahRahSisBoomBaBoomgirls4", beat + 1.5f),
                        new MultiSound.Sound("cheerReaders/Girls/RRSBBB/rahRahSisBoomBaBoomgirls5", beat + 2f),
                        new MultiSound.Sound("cheerReaders/Girls/RRSBBB/rahRahSisBoomBaBoomgirls6", beat + 2.5f),
                    });
                    break;
                case (int)WhoSpeaks.Both:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Girls/RRSBBB/rahRahSisBoomBaBoomgirls1", beat),
                        new MultiSound.Sound("cheerReaders/Girls/RRSBBB/rahRahSisBoomBaBoomgirls2", beat + 0.5f),
                        new MultiSound.Sound("cheerReaders/Girls/RRSBBB/rahRahSisBoomBaBoomgirls3", beat + 1f, 1, 1, false, 0.116f),
                        new MultiSound.Sound("cheerReaders/Girls/RRSBBB/rahRahSisBoomBaBoomgirls4", beat + 1.5f),
                        new MultiSound.Sound("cheerReaders/Girls/RRSBBB/rahRahSisBoomBaBoomgirls5", beat + 2f),
                        new MultiSound.Sound("cheerReaders/Girls/RRSBBB/rahRahSisBoomBaBoomgirls6", beat + 2.5f),
                        new MultiSound.Sound("cheerReaders/Solo/RRSBBB/rahRahSisBoomBaBoomS1", beat),
                        new MultiSound.Sound("cheerReaders/Solo/RRSBBB/rahRahSisBoomBaBoomS2", beat + 0.5f),
                        new MultiSound.Sound("cheerReaders/Solo/RRSBBB/rahRahSisBoomBaBoomS3", beat + 1f, 1, 1, false, 0.081f),
                        new MultiSound.Sound("cheerReaders/Solo/RRSBBB/rahRahSisBoomBaBoomS4", beat + 1.5f),
                        new MultiSound.Sound("cheerReaders/Solo/RRSBBB/rahRahSisBoomBaBoomS5", beat + 2f),
                        new MultiSound.Sound("cheerReaders/Solo/RRSBBB/rahRahSisBoomBaBoomS6", beat + 2.5f),
                    });
                    break;
            }
            if (!consecutive)
            {
                soundsToPlay.Add(new MultiSound.Sound("cheerReaders/bookDiagonal", beat));
            }
            MultiSound.Play(soundsToPlay.ToArray(), forcePlay: true);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    firstRow[0].FlipBook();
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(1);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(1);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(1);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(1);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 0.5f, delegate
                {
                    firstRow[1].FlipBook();
                    secondRow[0].FlipBook();
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(3);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 1f, delegate
                {
                    firstRow[2].FlipBook();
                    secondRow[1].FlipBook();
                    thirdRow[0].FlipBook();
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(1);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(1);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(1);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(1);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 1.5f, delegate
                {
                    firstRow[3].FlipBook();
                    secondRow[2].FlipBook();
                    thirdRow[1].FlipBook();
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.OneTwoThree(2);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.OneTwoThree(2);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.OneTwoThree(2);
                            foreach (var nerd in allGirls)
                            {
                                nerd.OneTwoThree(2);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 2f, delegate
                {
                    secondRow[3].FlipBook();
                    thirdRow[2].FlipBook();
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(3);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 2.5f, delegate
                {
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.Boom();
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.Boom();
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.Boom();
                            foreach (var nerd in allGirls)
                            {
                                nerd.Boom();
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 3.5f, delegate
                {
                    if (!doingCue) canBop = true;
                })
            });
        }

        public void OkItsOnStretchable(float beat, float length, int whoSpeaks, bool whistle, int posterToChoose, bool shouldHappyFace)
        {
            canBop = false;
            float actualLength = length * 0.25f;
            ScheduleInput(beat, 2 * actualLength, InputType.STANDARD_ALT_DOWN, JustHoldSpin, MissFlip, Nothing);
            ScheduleInput(beat, 3 * actualLength, InputType.STANDARD_ALT_UP, JustReleaseSpin, MissFlip, Nothing);
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>();
            if (whistle)
            {
                soundsToPlay.AddRange(new List<MultiSound.Sound>()
                {
                    new MultiSound.Sound("cheerReaders/whistle1", beat),
                    new MultiSound.Sound("cheerReaders/whistle2", beat + 1 * actualLength),
                });
            }
            switch (whoSpeaks)
            {
                case (int)WhoSpeaks.Solo:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Solo/OKItsOn/okItsOnS1", beat),
                        new MultiSound.Sound("cheerReaders/Solo/OKItsOn/okItsOnS2", beat + 1f * actualLength),
                        new MultiSound.Sound("cheerReaders/Solo/OKItsOn/okItsOnS3", beat + 2f * actualLength),
                        new MultiSound.Sound("cheerReaders/Solo/OKItsOn/okItsOnS4", beat + 2f * actualLength + 0.75f),
                        new MultiSound.Sound("cheerReaders/Solo/OKItsOn/okItsOnS5", beat + 3f * actualLength),
                    });
                    break;
                case (int)WhoSpeaks.Girls:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Girls/OKItsOn/okItsOngirls1", beat),
                        new MultiSound.Sound("cheerReaders/Girls/OKItsOn/okItsOngirls2", beat + 1f * actualLength),
                        new MultiSound.Sound("cheerReaders/Girls/OKItsOn/okItsOngirls3", beat + 2f * actualLength),
                        new MultiSound.Sound("cheerReaders/Girls/OKItsOn/okItsOngirls4", beat + 2f * actualLength + 0.75f),
                        new MultiSound.Sound("cheerReaders/Girls/OKItsOn/okItsOngirls5", beat + 3f * actualLength),
                    });
                    break;
                case (int)WhoSpeaks.Both:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("cheerReaders/Solo/OKItsOn/okItsOnS1", beat),
                        new MultiSound.Sound("cheerReaders/Solo/OKItsOn/okItsOnS2", beat + 1f * actualLength),
                        new MultiSound.Sound("cheerReaders/Solo/OKItsOn/okItsOnS3", beat + 2f * actualLength),
                        new MultiSound.Sound("cheerReaders/Solo/OKItsOn/okItsOnS4", beat + 2f * actualLength + 0.75f),
                        new MultiSound.Sound("cheerReaders/Solo/OKItsOn/okItsOnS5", beat + 3f * actualLength),
                        new MultiSound.Sound("cheerReaders/Girls/OKItsOn/okItsOngirls1", beat),
                        new MultiSound.Sound("cheerReaders/Girls/OKItsOn/okItsOngirls2", beat + 1f * actualLength),
                        new MultiSound.Sound("cheerReaders/Girls/OKItsOn/okItsOngirls3", beat + 2f * actualLength),
                        new MultiSound.Sound("cheerReaders/Girls/OKItsOn/okItsOngirls4", beat + 2f * actualLength + 0.75f),
                        new MultiSound.Sound("cheerReaders/Girls/OKItsOn/okItsOngirls5", beat + 3f * actualLength),
                    });
                    break;
            }
            MultiSound.Play(soundsToPlay.ToArray(), forcePlay: true);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(3);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 1f * actualLength, delegate
                {
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(1);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(1);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(1);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(1);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 2f * actualLength, delegate
                {
                    foreach (var nerd in firstRow)
                    {
                        nerd.StartSpinBook();
                    }
                    foreach (var nerd in secondRow)
                    {
                        nerd.StartSpinBook();
                    }
                    foreach (var nerd in thirdRow)
                    {
                        nerd.StartSpinBook();
                    }
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(3);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 3f * actualLength, delegate
                {
                    SetPosterImage(posterToChoose);
                    foreach (var nerd in firstRow)
                    {
                        nerd.StopSpinBook();
                    }
                    foreach (var nerd in secondRow)
                    {
                        nerd.StopSpinBook();
                    }
                    foreach (var nerd in thirdRow)
                    {
                        nerd.StopSpinBook();
                    }
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(3);
                            if (shouldHappyFace)
                            {
                                foreach (var girl in allGirls)
                                {
                                    girl.HappyFace(true);
                                }
                            }
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            if (shouldHappyFace)
                            {
                                player.HappyFace(true);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var nerd in allGirls)
                            {
                                nerd.ItsUpToYou(3);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 3f * actualLength + 2f, delegate
                {
                    if (!shouldHappyFace) return;
                    foreach (var girl in allGirls)
                    {
                        girl.HappyFace();
                    }
                    player.HappyFace();
                })
            });
        }

        void JustFlip(PlayerActionEvent caller, float state)
        {
            missPoster.SetActive(false);
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("cheerReaders/doingoing");
                player.FlipBook(); //Need near miss anims
                return;
            }
            SuccessFlip();
        }

        void JustFlipBoom(PlayerActionEvent caller, float state)
        {
            missPoster.SetActive(false);
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("cheerReaders/doingoing");
                player.FlipBook(); //Need near miss anims
                return;
            }
            SuccessFlip(true);
        }

        void SuccessFlip(bool boom = false)
        {
            player.FlipBook();
            shouldYay = true;
            if (boom)
            {
                Jukebox.PlayOneShotGame("cheerReaders/bookBoom");
            }
            else
            {
                Jukebox.PlayOneShotGame("cheerReaders/bookPlayer");
            }
        }

        void JustHoldSpin(PlayerActionEvent caller, float state)
        {
            missPoster.SetActive(false);
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("cheerReaders/doingoing");
                player.StartSpinBook();
                SpinningLoop = Jukebox.PlayOneShotGame("cheerReaders/bookSpinLoop", -1, 1, 1, true);
                return;
            }
            SuccessHoldSpin();
        }

        void SuccessHoldSpin()
        {
            player.StartSpinBook();
            Jukebox.PlayOneShotGame("cheerReaders/bookSpin");
            SpinningLoop = Jukebox.PlayOneShotScheduledGame("cheerReaders/bookSpinLoop", Jukebox.GetClipLengthGame("cheerReaders/bookSpin"), 1, 1, true);
        }

        void JustReleaseSpin(PlayerActionEvent caller, float state)
        {
            Jukebox.KillLoop(SpinningLoop, 0f);
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("cheerReaders/doingoing");
                player.StopSpinBook();
                shouldDoSuccessZoom = false;
                missPoster.SetActive(true);
                return;
            }
            SuccessReleaseSpin();
        }

        void SuccessReleaseSpin()
        {
            Jukebox.PlayOneShotGame("cheerReaders/bookOpen");
            player.StopSpinBook();
            shouldYay = true;
            shouldDoSuccessZoom = true;
            missPoster.SetActive(false);
        }

        void MissFlip(PlayerActionEvent caller)
        {
            playerMask.SetActive(false);
            missPoster.SetActive(false);
            Jukebox.PlayOneShotGame("cheerReaders/doingoing");
            player.Miss();
            shouldDoSuccessZoom = false;
            foreach (var girl in allGirls)
            {
                girl.Stare();
            }
        }

        void Nothing(PlayerActionEvent caller) {}
    }
}
