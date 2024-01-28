using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using JetBrains.Annotations;
using Starpelly.Transformer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using static HeavenStudio.EntityTypes;
using static HeavenStudio.Games.CheerReaders;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlBookLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("cheerReaders", "Cheer Readers", "ffffde", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.BopToggle(e.beat, e.length, e["toggle"], e["toggle2"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Toggle if the girls should bop for the duration of this event."),
                        new Param("toggle2", false, "Bop (Auto)", "Toggle if the girls should automatically bop until another Bop event is reached.")
                    }
                },
                new GameAction("oneTwoThree", "One! Two! Three!")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.OneTwoThree(e.beat, e["solo"]); CheerReaders.instance.SetIsDoingCue(e.beat, e.length);},
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("solo", CheerReaders.WhoSpeaks.Both, "Speaker", "Choose who says the voice line.")
                    }
                },
                new GameAction("itsUpToYou", "It's Up To You!")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.ItsUpToYou(e.beat, e["solo"]); CheerReaders.instance.SetIsDoingCue(e.beat, e.length);},
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("solo", CheerReaders.WhoSpeaks.Both, "Speaker", "Choose who says the voice line.")
                    }
                },
                new GameAction("letsGoReadABunchaBooks", "Let's Go Read A Buncha Books!")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.LetsGoReadABunchaBooks(e.beat, e["solo"]); CheerReaders.instance.SetIsDoingCue(e.beat, e.length);},
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("solo", CheerReaders.WhoSpeaks.Both, "Speaker", "Choose who says the voice line.")
                    }
                },
                new GameAction("rahRahSisBoomBaBoom", "Rah-Rah Sis Boom Bah-BOOM!")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.RahRahSisBoomBaBoom(e.beat, e["solo"], e["consecutive"]); CheerReaders.instance.SetIsDoingCue(e.beat, e.length);},
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("solo", CheerReaders.WhoSpeaks.Both, "Speaker", "Choose who says the voice line."),
                        new Param("consecutive", false, "Consecutive Version", "Toggle if this cue should use the consecutive version of the cue. This mutes the first book flip so it doesn't overlap with a previous cue.")
                    }
                },
                new GameAction("okItsOn", "OK, It's On!")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.OkItsOnStretchable(e.beat, e.length, e["solo"], e["toggle"], e["poster"], e["happy"]); CheerReaders.instance.SetIsDoingCue(e.beat, e.length, false);},
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("solo", CheerReaders.WhoSpeaks.Both, "Speaker", "Choose who says the voice line."),
                        new Param("toggle", true, "Whistle", "Toggle if the whistle sounds should play."),
                        new Param("poster", CheerReaders.PosterToChoose.Random, "Image", "Choose the image to display inside the books."),
                        new Param("happy", true, "Smile", "Toggle if the girls will smile two beats after the input.")
                    }
                },
                new GameAction("okItsOnStretch", "OK, It's On! (Stretchable)")
                {
                    function = delegate {var e = eventCaller.currentEntity; CheerReaders.instance.OkItsOnStretchable(e.beat, e.length, e["solo"], e["toggle"], e["poster"], e["happy"]); CheerReaders.instance.SetIsDoingCue(e.beat, e.length, false); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("solo", CheerReaders.WhoSpeaks.Both, "Speaker", "Choose who says the voice line."),
                        new Param("toggle", true, "Whistle", "Toggle if the whistle sounds should play."),
                        new Param("poster", CheerReaders.PosterToChoose.Random, "Image", "Choose the image to display inside the books."),
                        new Param("happy", true, "Smile", "Toggle if the girls should smile two beats after the input.")
                    }
                },
                new GameAction("yay", "Yay!")
                {
                    function = delegate {CheerReaders.instance.Yay(eventCaller.currentEntity["solo"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("solo", CheerReaders.WhoSpeaks.Both, "Speaker", "Choose who says the voice line."),
                    }
                },
                new GameAction("resetPose", "Reset Pose")
                {
                    function = delegate {CheerReaders.instance.ResetPose(); },
                    defaultLength = 0.5f
                }
            },
            new List<string>() { "rvl", "normal" },
            "rvlbooks", "en",
             new List<string>() { "en" }
            );
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
        bool canBop = true;
        public bool doingCue;
        double cueLength;
        double cueBeat;
        bool shouldYay;
        bool shouldDoSuccessZoom;
        public bool shouldBeBlack = false;
        int currentZoomIndex;
        double currentZoomCamBeat;
        float currentZoomCamLength;
        private List<RiqEntity> allCameraEvents = new List<RiqEntity>();

        const int IAAltDownCat = IAMAXCAT;
        const int IAAltUpCat = IAMAXCAT + 1;

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }
        protected static bool IA_TouchAltPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_AltStart);
        }

        protected static bool IA_PadAltRelease(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltRelease(out double dt)
        {
            return PlayerInput.GetSqueezeUp(out dt);
        }

        public static PlayerInput.InputAction InputAction_AltStart =
            new("RvlBookAltStart", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);
        public static PlayerInput.InputAction InputAction_AltFinish =
            new("RvlBookAltFinish", new int[] { IAAltUpCat, IAFlickCat, IAAltUpCat },
            IA_PadAltRelease, IA_TouchFlick, IA_BatonAltRelease);
        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("RvlBookTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);

        void OnDestroy()
        {
            SoundByte.KillLoop(SpinningLoop, 0.5f);
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        public override void OnTimeChange()
        {
            UpdateCameraZoom();
        }

        void Awake()
        {
            instance = this;
            SetupBopRegion("cheerReaders", "bop", "toggle2");
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
            camEvents.AddRange(EventCaller.GetAllInGameManagerList("cheerReaders", new string[] { "okItsOnStretch" }));
            List<RiqEntity> tempEvents = new List<RiqEntity>();
            for (int i = 0; i < camEvents.Count; i++)
            {
                if (camEvents[i].beat + camEvents[i].beat >= Conductor.instance.songPositionInBeatsAsDouble)
                {
                    tempEvents.Add(camEvents[i]);
                }
            }

            allCameraEvents = tempEvents;

            UpdateCameraZoom();
        }

        public override void OnBeatPulse(double beat)
        {
            if (!BeatIsInBopRegion(beat)) return;
            BopSingle();
        }

        void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (allCameraEvents.Count > 0)
                {
                    if (currentZoomIndex < allCameraEvents.Count && currentZoomIndex >= 0)
                    {
                        if (Conductor.instance.songPositionInBeatsAsDouble >= allCameraEvents[currentZoomIndex].beat)
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
                            GameCamera.AdditionalPosition = new Vector3(0, 0, 0);
                        }
                        else
                        {
                            Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseInOutQuint);
                            float newZoom = func(shouldDoSuccessZoom ? 4f : 1.5f, 0, normalizedZoomOutAgainBeat);
                            GameCamera.AdditionalPosition = new Vector3(0, 0, newZoom);
                        }
                    }
                    else if (normalizedZoomInBeat >= 0)
                    {
                        if (normalizedZoomInBeat > 1)
                        {
                            GameCamera.AdditionalPosition = new Vector3(0, 0, shouldDoSuccessZoom ? 4f : 1.5f);
                        }
                        else
                        {
                            Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);
                            float newZoom = func(-1, shouldDoSuccessZoom ? 4f : 1.5f, normalizedZoomInBeat);
                            GameCamera.AdditionalPosition = new Vector3(0, 0, newZoom);
                        }
                    }
                    else if (normalizedZoomOutBeat >= 0)
                    {
                        if (normalizedZoomOutBeat > 1)
                        {
                            GameCamera.AdditionalPosition = new Vector3(0, 0, -1);
                        }
                        else
                        {
                            Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuint);
                            float newZoom = func(0f, 1f, normalizedZoomOutBeat);
                            GameCamera.AdditionalPosition = new Vector3(0, 0, newZoom * -1);
                        }
                    }
                }
                if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
                {
                    if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch
                        || (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && !IsExpectingInputNow(InputAction_AltStart)))
                    {
                        player.FlipBook(false);
                        missPoster.SetActive(false);
                        SoundByte.PlayOneShotGame("cheerReaders/miss");
                        ScoreMiss(1f);
                    }
                }
                if (PlayerInput.GetIsAction(InputAction_TouchRelease) && player.isSpinning)
                {
                    if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && !IsExpectingInputNow(InputAction_AltFinish))
                    {
                        player.FlipBook(false);
                        missPoster.SetActive(false);
                        SoundByte.PlayOneShotGame("cheerReaders/miss");
                        SoundByte.KillLoop(SpinningLoop, 0f);
                        ScoreMiss(1f);
                    }
                }
                if (PlayerInput.GetIsAction(InputAction_AltStart) && !IsExpectingInputNow(InputAction_AltStart))
                {
                    SoundByte.PlayOneShotGame("cheerReaders/doingoing");
                    player.StartSpinBook();
                    missPoster.SetActive(false);
                    SpinningLoop = SoundByte.PlayOneShotGame("cheerReaders/bookSpinLoop", -1, 1, 1, true);
                    ScoreMiss(1f);
                }
                if (PlayerInput.GetIsAction(InputAction_AltFinish) && !IsExpectingInputNow(InputAction_AltFinish) && player.isSpinning)
                {
                    SoundByte.PlayOneShotGame("cheerReaders/doingoing");
                    player.StopSpinBook();
                    SoundByte.KillLoop(SpinningLoop, 0f);
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
                SoundByte.KillLoop(SpinningLoop, 0.5f);
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
            foreach (var girl in allGirls)
            {
                girl.ResetPose();
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
                    SoundByte.PlayOneShotGame("cheerReaders/Solo/yayS");
                    player.Yay(true);
                    foreach (var girl in allGirls)
                    {
                        girl.Yay(true);
                    }
                    break;
                case (int)WhoSpeaks.Girls:
                    SoundByte.PlayOneShotGame("cheerReaders/Girls/yayGirls");
                    foreach (var girl in allGirls)
                    {
                        girl.Yay(true);
                    }
                    player.Yay(false);
                    break;
                default:
                    SoundByte.PlayOneShotGame("cheerReaders/All/yay");
                    foreach (var girl in allGirls)
                    {
                        girl.Yay(true);
                    }
                    player.Yay(true);
                    break;
            }
        }

        public void BopToggle(double beat, float length, bool startBop, bool bopAuto)
        {
            if (startBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
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
                foreach (var girl in firstRow)
                {
                    girl.Bop();
                }
                foreach (var girl in secondRow)
                {
                    girl.Bop();
                }
                foreach (var girl in thirdRow)
                {
                    girl.Bop();
                }
                player.Bop();
            }
        }

        public void SetIsDoingCue(double beat, float length, bool shouldSwitchColor = true)
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
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length * 0.5f, delegate { shouldBeBlack = !shouldBeBlack; })
            });
        }

        public void OneTwoThree(double beat, int whoSpeaks)
        {
            canBop = false;
            ScheduleInput(beat, 2, InputAction_BasicPress, JustFlip, MissFlip, Nothing);
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

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    foreach (var girl in firstRow)
                    {
                        girl.FlipBook();
                    }
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.OneTwoThree(1);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var girl in allGirls)
                            {
                                girl.OneTwoThree(1);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.OneTwoThree(1);
                            foreach (var girl in allGirls)
                            {
                                girl.OneTwoThree(1);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    foreach (var girl in secondRow)
                    {
                        girl.FlipBook();
                    }
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.OneTwoThree(2);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var girl in allGirls)
                            {
                                girl.OneTwoThree(2);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.OneTwoThree(2);
                            foreach (var girl in allGirls)
                            {
                                girl.OneTwoThree(2);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 2, delegate
                {
                    foreach (var girl in thirdRow)
                    {
                        girl.FlipBook();
                    }
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.OneTwoThree(3);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var girl in allGirls)
                            {
                                girl.OneTwoThree(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.OneTwoThree(3);
                            foreach (var girl in allGirls)
                            {
                                girl.OneTwoThree(3);
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

        public void ItsUpToYou(double beat, int whoSpeaks)
        {
            canBop = false;
            ScheduleInput(beat, 2, InputAction_BasicPress, JustFlip, MissFlip, Nothing);
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

            BeatAction.New(instance, new List<BeatAction.Action>()
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
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(1);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(1);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(1);
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
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(2);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(2);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(2);
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
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
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
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(4);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(4);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(4);
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

        public void LetsGoReadABunchaBooks(double beat, int whoSpeaks)
        {
            canBop = false;
            ScheduleInput(beat, 2, InputAction_BasicPress, JustFlip, MissFlip, Nothing);
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
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(1);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(1);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(1);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(1);
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
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(1);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(1);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(1);
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
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
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
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
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

        public void RahRahSisBoomBaBoom(double beat, int whoSpeaks, bool consecutive)
        {
            canBop = false;
            ScheduleInput(beat, 2.5f, InputAction_BasicPress, JustFlipBoom, MissFlip, Nothing);
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
            BeatAction.New(instance, new List<BeatAction.Action>()
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
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(1);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(1);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(1);
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
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
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
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(1);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(1);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(1);
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
                            foreach (var girl in allGirls)
                            {
                                girl.OneTwoThree(2);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.OneTwoThree(2);
                            foreach (var girl in allGirls)
                            {
                                girl.OneTwoThree(2);
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
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
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
                            foreach (var girl in allGirls)
                            {
                                girl.Boom();
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.Boom();
                            foreach (var girl in allGirls)
                            {
                                girl.Boom();
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

        public void OkItsOnStretchable(double beat, float length, int whoSpeaks, bool whistle, int posterToChoose, bool shouldHappyFace)
        {
            canBop = false;
            float actualLength = length * 0.25f;
            ScheduleInput(beat, 2 * actualLength, InputAction_AltStart, JustHoldSpin, MissFlip, Nothing);
            ScheduleInput(beat, 3 * actualLength, InputAction_AltFinish, JustReleaseSpin, MissFlip, Nothing).IsHittable = IsReleaseSpinHittable;
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
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(3);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
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
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(1);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(1);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(1);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 2f * actualLength, delegate
                {
                    foreach (var girl in firstRow)
                    {
                        girl.StartSpinBook();
                    }
                    foreach (var girl in secondRow)
                    {
                        girl.StartSpinBook();
                    }
                    foreach (var girl in thirdRow)
                    {
                        girl.StartSpinBook();
                    }
                    switch (whoSpeaks)
                    {
                        case (int)WhoSpeaks.Solo:
                            player.ItsUpToYou(3);
                            break;
                        case (int)WhoSpeaks.Girls:
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
                            }
                            break;
                    }
                }),
                new BeatAction.Action(beat + 3f * actualLength, delegate
                {
                    SetPosterImage(posterToChoose);
                    foreach (var girl in firstRow)
                    {
                        girl.StopSpinBook();
                    }
                    foreach (var girl in secondRow)
                    {
                        girl.StopSpinBook();
                    }
                    foreach (var girl in thirdRow)
                    {
                        girl.StopSpinBook();
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
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
                            }
                            if (shouldHappyFace)
                            {
                                player.HappyFace(true);
                            }
                            break;
                        case (int)WhoSpeaks.Both:
                            player.ItsUpToYou(3);
                            foreach (var girl in allGirls)
                            {
                                girl.ItsUpToYou(3);
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
                SoundByte.PlayOneShotGame("cheerReaders/doingoing");
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
                SoundByte.PlayOneShotGame("cheerReaders/doingoing");
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
                SoundByte.PlayOneShotGame("cheerReaders/bookBoom");
            }
            else
            {
                SoundByte.PlayOneShotGame("cheerReaders/bookPlayer");
            }
        }

        void JustHoldSpin(PlayerActionEvent caller, float state)
        {
            missPoster.SetActive(false);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("cheerReaders/doingoing");
                player.StartSpinBook();
                SpinningLoop = SoundByte.PlayOneShotGame("cheerReaders/bookSpinLoop", -1, 1, 1, true);
                return;
            }
            SuccessHoldSpin();
        }

        void SuccessHoldSpin()
        {
            player.StartSpinBook();
            SoundByte.PlayOneShotGame("cheerReaders/bookSpin");
            SpinningLoop = SoundByte.PlayOneShotScheduledGame("cheerReaders/bookSpinLoop", SoundByte.GetClipLengthGame("cheerReaders/bookSpin"), 1, 1, true);
        }

        void JustReleaseSpin(PlayerActionEvent caller, float state)
        {
            SoundByte.KillLoop(SpinningLoop, 0f);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("cheerReaders/doingoing");
                player.StopSpinBook();
                shouldDoSuccessZoom = false;
                missPoster.SetActive(true);
                return;
            }
            SuccessReleaseSpin();
        }

        void SuccessReleaseSpin()
        {
            SoundByte.PlayOneShotGame("cheerReaders/bookOpen");
            player.StopSpinBook();
            shouldYay = true;
            shouldDoSuccessZoom = true;
            missPoster.SetActive(false);
        }

        void MissFlip(PlayerActionEvent caller)
        {
            playerMask.SetActive(false);
            missPoster.SetActive(false);
            SoundByte.PlayOneShotGame("cheerReaders/doingoing");

            if (SpinningLoop != null)
                SoundByte.KillLoop(SpinningLoop, 0f);

            player.Miss();
            shouldDoSuccessZoom = false;
            foreach (var girl in allGirls)
            {
                girl.Stare();
            }
        }

        void Nothing(PlayerActionEvent caller) { }

        bool IsReleaseSpinHittable()
        {
            return player.isSpinning;
        }
    }
}
