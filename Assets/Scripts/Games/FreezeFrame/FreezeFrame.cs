using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrFreezeFrameLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("freezeFrame", "Freeze Frame", "8b93b4", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; FreezeFrame.SetBopping(e.beat, e.length, e["bop"], e["autoBop"], e["blink"], e["autoBlink"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", false, "Bop", "Set the type of photo to use."),
                        new Param("autoBop", true, "Bop (Auto)", "Set the type of photo to use."),
                        new Param("blink", false, "Crosshair Blink", "Set the type of photo to use."),
                        new Param("autoBlink", true, "Crosshair Blink (Auto)", "Set the type of photo to use."),
                    }
                },
                // cues
                new GameAction("slowCar", "Slow Car")
                {
                    function = delegate { var e = eventCaller.currentEntity; FreezeFrame.SlowCarCue(e.beat, e["variant"]); },
                    defaultLength = 3f,
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; if (!(bool)e["mute"]) FreezeFrame.SlowCarSFX(); },
                    parameters = new List<Param>()
                    {
                        new Param("variant", FreezeFrame.PhotoType.Random, "Photo Variant", "Set the type of photo to use."),
                        new Param("mute", false, "Mute", "Mute the sound of the cue."),
                        new Param("autoShowPhotos", true, "Auto Show Photos", "Automagically show the photos after they're taken.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "gradeType", "audience" })
                        }),
                        new Param("gradeType", FreezeFrame.GradeType.Symbols, "Rating Type", "Choose whether to use the English or Japanese variant of the grading screen."),
                        new Param("audience", true, "Crowd Cheer", "Set whether or not the audience should cheer when the photos are shown."),
                    }
                },
                new GameAction("fastCar", "Fast Car")
                {
                    function = delegate { var e = eventCaller.currentEntity; FreezeFrame.FastCarCue(e.beat, e["variant"]); },
                    defaultLength = 3f,
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; if (!(bool)e["mute"]) FreezeFrame.FastCarSFX(); },
                    parameters = new List<Param>()
                    {
                        new Param("variant", FreezeFrame.PhotoType.Random, "Photo Variant", "Set the type of photo to use."),
                        new Param("mute", false, "Mute", "Mute the sound of the cue."),
                        new Param("autoShowPhotos", true, "Auto Show Photos", "Automagically show the photos after they're taken.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "gradeType", "audience" })
                        }),
                        new Param("gradeType", FreezeFrame.GradeType.Symbols, "Rating Type", "Choose whether to use the English or Japanese variant of the grading screen."),
                        new Param("audience", true, "Crowd Cheer", "Set whether or not the audience should cheer when the photos are shown."),
                    }
                },
                new GameAction("showPhotos", "Show Photos")
                {
                    function = delegate { var e = eventCaller.currentEntity; FreezeFrame.ShowPhotos(e.beat, e.length, e["gradeType"], e["audience"], e["clearCache"]); },
                    defaultLength = 1,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("gradeType", FreezeFrame.GradeType.Symbols, "Rating Type", "Choose whether to use the English or Japanese variant of the grading screen."),
                        new Param("audience", true, "Crowd Cheer", "Set whether or not the audience should cheer when the photos are shown."),
                        new Param("clearCache", true, "Clear Photos", "Clears the photo cache after the photos are shown."),
                    }
                },
                new GameAction("clearPhotos", "Clear Photo Cache")
                {
                    function = delegate { var e = eventCaller.currentEntity; FreezeFrame.ClearPhotos(); },
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; FreezeFrame.ClearPhotos(); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                    }
                },
                // distractions
                new GameAction("spawnPerson", "Spawn Walker")
                {
                    function = delegate { var e = eventCaller.currentEntity; FreezeFrame.SummonWalker(e); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("personType", FreezeFrame.PersonType.Dude1, "Walker Type", "Choose the type of walker to spawn."),
                        new Param("direction", FreezeFrame.PersonDirection.Random, "Direction", "Choose the direction from which to spawn the walker."),
                        new Param("layer", new EntityTypes.Integer(-10, 10, 0), "Layer", "The layer on which this walker should spawn (higher numbers are shown in front)."),
                    }
                },
                new GameAction("spawnCrowd", "Show/Hide Crowd")
                {
                    function = delegate { var e = eventCaller.currentEntity; FreezeFrame.ToggleCrowd(e.beat, e["crowd"], e["customCrowd"], e["crowdFarLeft"], e["crowdLeft"], e["crowdRight"], e["crowdFarRight"], e["billboard"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("crowd", true, "Show Crowd", "Choose whether to spawn or despawn the crowd."),
                        new Param("customCrowd", false, "Custom Crowd", "Select to customize the crowd.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "crowdFarLeft", "crowdLeft", "crowdRight", "crowdFarRight" })
                        }),
                        new Param("crowdFarLeft" , FreezeFrame.CustomCrowdType.PinkDancers,   "Far Left"    , "Select the graphic to display on the far left."),
                        new Param("crowdLeft"    , FreezeFrame.CustomCrowdType.YellowDancers, "Center Left" , "Select the graphic to display on the near left."),
                        new Param("crowdRight"   , FreezeFrame.CustomCrowdType.TealDancers,   "Center Right", "Select the graphic to display on the near right."),
                        new Param("crowdFarRight", FreezeFrame.CustomCrowdType.PinkDancers,   "Far Right"   , "Select the graphic to display on the far right."),
                        new Param("billboard", false, "Show Billboard", "Choose whether to show or hide the billboards."),
                    }
                },
                new GameAction("introSign", "Intro Sign")
                {
                    function = delegate { var e = eventCaller.currentEntity; FreezeFrame.DoIntroSign(e.beat, e.length, e["enter"], e["ease"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("enter", true, "Enter", "Choose the sign should enter or exit."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action."),
                    }
                },
                new GameAction("introLights", "Intro Lights")
                {
                    function = delegate { var e = eventCaller.currentEntity; FreezeFrame.IntroLightsAnim(e.beat, e.length, e["lightsOn"]); FreezeFrame.IntroLightsSound(e.beat, e.length, e["lightsOn"]); },
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; FreezeFrame.IntroLightsSound(e.beat, e.length, e["lightsOn"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("lightsOn", true, "Turn Lights On", "Choose whether to turn the lights on or off."),
                    }
                },
                // settings
                new GameAction("toggleOverlay", "Toggle Overlay")
                {
                    function = delegate { var e = eventCaller.currentEntity; FreezeFrame.ToggleOverlay(e.beat, e["showOverlay"], e["showCameraMan"], e["followCamera"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("showOverlay", true, "Show Overlay", "Choose whether or not to show the camera overlay."),
                        new Param("showCameraMan", true, "Show T.J.", "Choose whether or not to show the box containing T.J. Snapper."),
                        new Param("followCamera", true, "Follow Camera", "Choose whether or not the overlay should follow the camera."),
                    }
                },
                new GameAction("moveCameraMan", "Move T.J.")
                {
                    function = delegate { var e = eventCaller.currentEntity; FreezeFrame.SetMoveCameraMan(e.beat, e.length, e["startPosX"], e["startPosY"], e["endPosX"], e["endPosY"], e["ease"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("startPosX", new EntityTypes.Float(-5.0f, 5.0f, 0.0f), "Start X Position", "X position at which to start."),
                        new Param("startPosY", new EntityTypes.Float(-5.0f, 5.0f, 0.0f), "Start Y Position", "Y position at which to start."),
                        new Param("endPosX"  , new EntityTypes.Float(-5.0f, 5.0f, 0.0f), "End X Position"  , "X position at which to end."),
                        new Param("endPosY"  , new EntityTypes.Float(-5.0f, 5.0f, 0.0f), "End Y Position"  , "Y position at which to end."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (Util.EasingFunction.Ease)x != Util.EasingFunction.Ease.Instant, new string[] { "startPosX", "startPosY" })
                        }),
                        //new Param("flipX", false, "Flip", "Set whether or not to flip T.J. horizontally."),
                    }
                },
                new GameAction("rotateCameraMan", "Rotate T.J.")
                {
                    function = delegate { var e = eventCaller.currentEntity; FreezeFrame.SetRotateCameraMan(e.beat, e.length, e["startRot"], e["endRot"], e["ease"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("startRot", new EntityTypes.Float(-360.0f, 360.0f, 0.0f), "Start Rotation", "Rotation degrees at which to start."),
                        new Param("endRot"  , new EntityTypes.Float(-360.0f, 360.0f, 0.0f), "End Rotation"  , "Rotation degrees position at which to end."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (Util.EasingFunction.Ease)x != Util.EasingFunction.Ease.Instant, new string[] { "startRot" })
                        }),
                    }
                },
                new GameAction("scaleCameraMan", "Scale T.J.")
                {
                    function = delegate { var e = eventCaller.currentEntity; FreezeFrame.SetScaleCameraMan(e.beat, e.length, e["startSizeX"], e["startSizeY"], e["endSizeX"], e["endSizeY"], e["ease"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("startSizeX", new EntityTypes.Float(-5.0f, 5.0f, 1.0f), "Start Scale X", "Horizontal scale at which to start."),
                        new Param("startSizeY", new EntityTypes.Float(-5.0f, 5.0f, 1.0f), "Start Scale Y", "Vertical scale at which to start."),
                        new Param("endSizeX",   new EntityTypes.Float(-5.0f, 5.0f, 1.0f), "End Scale X"  , "Horizontal scale at which to end."),
                        new Param("endSizeY",   new EntityTypes.Float(-5.0f, 5.0f, 1.0f), "End Scale Y"  , "Vertical scale at which to end."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (Util.EasingFunction.Ease)x != Util.EasingFunction.Ease.Instant, new string[] { "startSizeX", "startSizeY" })
                        }),
                    }
                },
            },
            new List<string>() { "ntr", "normal" },
            "ntrcameraman", "en"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using HeavenStudio.Common;
    using Scripts_FreezeFrame;
    using UnityEngine.Rendering;

    public class FreezeFrame : Minigame
    {
        /*
        BIG LIST OF TODOS
        - finish sounds
        - wait for upscale
        - make particles random sprites

        - REAL icon
        */

        public static FreezeFrame Instance
        {
            get
            {
                if (GameManager.instance.minigame is FreezeFrame instance)
                    return instance;
                return null;
            }
        }

        [SerializeField] Animator CameraMan;
        [SerializeField] Photograph[] Photographs;
        [SerializeField] Photograph Photograph1;
        [SerializeField] Photograph Photograph2;
        [SerializeField] Photograph Photograph3;
        [SerializeField] Animator Results;

        [SerializeField] Animator IntroSign;

        [SerializeField] GameObject Overlay;
        [SerializeField] GameObject Crosshair;
        [SerializeField] Animator Shutter;
        [SerializeField] GameObject DimRect;

        [SerializeField] StickyCanvas StickyLayer;

        [SerializeField] Transform FarCarSpawn;
        [SerializeField] GameObject FarCarPrefab;

        [SerializeField] Transform NearCarSpawn;
        [SerializeField] GameObject NearCarPrefab;

        [SerializeField] Transform WalkerSpawn;
        [SerializeField] GameObject WalkerPrefab;

        [SerializeField] Animator Crowd;
        [SerializeField] SpriteRenderer CrowdFarLeft;
        [SerializeField] SpriteRenderer CrowdLeft;
        [SerializeField] SpriteRenderer CrowdRight;
        [SerializeField] SpriteRenderer CrowdFarRight;
        [SerializeField] Sprite[] CrowdSprites;
        [SerializeField] GameObject Billboards;

        public bool DoAutoBop { get; set; } = true;
        public bool DoAutoCrosshairBlink { get; set; } = true;
        public bool ShowOverlay { get; set; } = true;
        public bool ShowCameraMan { get; set; } = true;
        public bool OverlayFollowCamera { get; set; } = true;
        public bool ShowCrowd { get; set; } = false;
        public bool ShowBillboard { get; set; } = false;

        public List<WalkerArgs> Walkers { get; set; } = new();

        public bool SignIsMoving { get; set; } = false;
        public SignMoveArgs CurrentSignArgs { get; set; }

        public static Vector3 CameraManStartPos { get; private set; }
        public bool CameraManMoving { get; set; } = false;
        public CameraManMoveArgs CurrentCameraManMoveArgs;
        public bool CameraManRotating { get; set; } = false;
        public CameraManRotateArgs CurrentCameraManRotateArgs;
        public bool CameraManScaling { get; set; } = false;
        public CameraManScaleArgs CurrentCameraManScaleArgs;

        public static List<PhotoArgs> PhotoList = new();
        public Dictionary<PlayerActionEvent, PhotoArgs> EventArgs = new();
        public bool IsShowingPhotos { get; set; } = false;

        public List<SpawnCarArgs> QueuedCars { get; set; } = new();

        public static Dictionary<RiqEntity, PersonDirection> WalkerDirections = new();

        //protected static int? SuperSeed { get; set; }

        // UNITY BUILTIN METHODS
        void Awake()
        {
            CameraManStartPos = CameraMan.transform.localPosition;
            //if (SuperSeed is null)
            //    SuperSeed = new System.Random().Next();
        }
        void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress) && !IsShowingPhotos)
            {
                CameraFlash();
                //ScoreMiss();
            }

            // sign
            if (SignIsMoving)
            {
                float normalizedBeat = conductor.GetPositionFromBeat(CurrentSignArgs.StartTime, CurrentSignArgs.Length);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(CurrentSignArgs.Ease);
                float newPos = func(0f, 1f, normalizedBeat);
                IntroSign.DoNormalizedAnimation(CurrentSignArgs.AnimName, newPos, animLayer: 0);
                if (normalizedBeat >= 1f)
                    SignIsMoving = false;
            }

            // move TJ
            if (CameraManMoving)
            {
                float normalizedBeat = conductor.GetPositionFromBeat(CurrentCameraManMoveArgs.StartBeat, CurrentCameraManMoveArgs.Length);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(CurrentCameraManMoveArgs.Ease);
                float newPos = func(0f, 1f, normalizedBeat);

                Vector3 diff = CurrentCameraManMoveArgs.EndPos - CurrentCameraManMoveArgs.StartPos;
                Vector3 diffPos = newPos * diff;
                CameraMan.transform.localPosition = CurrentCameraManMoveArgs.StartPos + diffPos;

                if (normalizedBeat >= 1f)
                {
                    CameraMan.transform.localPosition = CurrentCameraManMoveArgs.EndPos;
                    CameraManMoving = false;
                }
            }
            if (CameraManRotating)
            {
                float normalizedBeat = conductor.GetPositionFromBeat(CurrentCameraManRotateArgs.StartBeat, CurrentCameraManRotateArgs.Length);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(CurrentCameraManRotateArgs.Ease);
                float newPos = func(0f, 1f, normalizedBeat);

                float diff = CurrentCameraManRotateArgs.EndRot - CurrentCameraManRotateArgs.StartRot;
                float diffPos = newPos * diff;
                CameraMan.transform.localEulerAngles = new Vector3(0, 0, CurrentCameraManRotateArgs.StartRot + diffPos);

                if (normalizedBeat >= 1f)
                {
                    CameraMan.transform.localEulerAngles = new Vector3(0, 0, CurrentCameraManRotateArgs.EndRot);
                    CameraManRotating = false;
                }
            }
            if (CameraManScaling)
            {
                float normalizedBeat = conductor.GetPositionFromBeat(CurrentCameraManScaleArgs.StartBeat, CurrentCameraManScaleArgs.Length);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(CurrentCameraManScaleArgs.Ease);
                float newPos = func(0f, 1f, normalizedBeat);

                Vector3 diff = CurrentCameraManScaleArgs.EndScale - CurrentCameraManScaleArgs.StartScale;
                Vector3 diffPos = newPos * diff;
                CameraMan.transform.localScale = CurrentCameraManScaleArgs.StartScale + diffPos;

                if (normalizedBeat >= 1f)
                {
                    CameraMan.transform.localScale = CurrentCameraManScaleArgs.EndScale;
                    CameraManScaling = false;
                }
            }

            // boppers
            if (Walkers.Count > 0)
            {
                Walkers.RemoveAll(w => w.Walker == null || w.Walker.gameObject == null);
                foreach (WalkerArgs args in Walkers)
                {
                    float normalizedBeat = conductor.GetPositionFromBeat(args.StartTime, args.Length);
                    args.Walker.DoNormalizedAnimation(args.AnimName, normalizedBeat, animLayer: 1);
                    if (normalizedBeat >= 1f)
                        Destroy(args.Walker.gameObject);
                }
            }
            
            // car animations
            if (QueuedCars.Count > 0)
            {
                var beat = conductor.songPositionInBeats;
                if (beat >= 0)
                {
                    QueuedCars.RemoveAll(e => e.Beat < beat - 5); // could probably be smaller. 5 just to be safe
                    foreach (SpawnCarArgs args in QueuedCars.Where(e => e.Beat <= beat))
                    {
                        SpawnCar(args);
                    }
                    QueuedCars.RemoveAll(e => e.Beat <= beat);
                }
            }

            if (!IsShowingPhotos)
                Instance.Overlay.SetActive(ShowOverlay);

            Billboards.SetActive(ShowBillboard);
            Instance.CameraMan.gameObject.SetActive(ShowCameraMan);
            StickyLayer.Sticky = OverlayFollowCamera;
        }
        private void OnDestroy()
        {
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
            //if (PhotoList.Count > 0) PhotoList.Clear();
        }

        // MINIGAME METHODS
        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat))
            {
                if (DoAutoBop && !IsShowingPhotos)
                    Bop();

                if (DoAutoCrosshairBlink)
                    CrosshairBlink();
            }
        }
        public override void OnPlay(double beat)
        {
            if (PhotoList.Count > 0) PhotoList.Clear();
            CarbageCollection();
            OnGameSwitch(beat);
        }
        public override void OnGameSwitch(double beat)
        {
            Instance.Overlay.SetActive(ShowOverlay);
            Instance.CameraMan.gameObject.SetActive(ShowCameraMan);
            Instance.StickyLayer.Sticky = OverlayFollowCamera;

            // calculation
            CalculateAutoShowPhotos();
            CalculateCarSpawns();
            PreRandomizeWalkers();
            
            // setting local variables
            RiqEntity e = GetLastEntityOfType(beat, "bop");
            if (e is not null)
            {
                DoAutoBop = e["autoBop"];
                DoAutoCrosshairBlink = e["autoBlink"];
            }
            e = GetLastEntityOfType(beat, "toggleOverlay");
            if (e is not null)
            {
                ShowOverlay = e["showOverlay"];
                ShowCameraMan = e["showCameraMan"];
                OverlayFollowCamera = e["followCamera"];
            }
            e = GetLastEntityOfType(beat, "spawnCrowd");
            if (e is not null)
            {
                ToggleCrowd(e.beat, e["crowd"], e["customCrowd"], e["crowdFarLeft"], e["crowdLeft"], e["crowdRight"], e["crowdFarRight"], e["billboard"]);
            }

            //  walkers
            List<RiqEntity> eList = GetCurrentlyActiveEntities(beat, "spawnPerson");
            if (eList.Count > 0)
            {
                foreach (RiqEntity entity in eList)
                {
                    SummonWalker(entity);
                }
            }

            // bop entities
            eList = GetCurrentlyActiveEntities(beat, "bop");
            if (eList.Count > 0)
            {
                foreach (RiqEntity entity in eList)
                {
                    SetBopping(entity.beat, entity.length, entity["bop"], entity["autoBop"], entity["blink"], entity["autoBlink"], beat);
                }
            }

            // Intro
            e = GetLastEntityOfType(beat, "introSign");
            if (e is not null)
            {
                DoIntroSign(e.beat, e.length, e["enter"], e["ease"]);
            }
            e = GetLastEntityOfType(beat, "introLights");
            if (e is not null)
            {
                IntroLightsAnim(e.beat, e.length, e["lightsOn"]);
            }

            // Camera Man Movement
            e = GetLastEntityOfType(beat, "moveCameraMan");
            if (e is not null)
            {
                SetMoveCameraMan(e.beat, e.length, e["startPosX"], e["startPosY"], e["endPosX"], e["endPosY"], e["ease"]);
            }
            e = GetLastEntityOfType(beat, "rotateCameraMan");
            if (e is not null)
            {
                SetRotateCameraMan(e.beat, e.length, e["startRot"], e["endRot"], e["ease"]);
            }
            e = GetLastEntityOfType(beat, "scaleCameraMan");
            if (e is not null)
            {
                SetScaleCameraMan(e.beat, e.length, e["startSizeX"], e["startSizeY"], e["endSizeX"], e["endSizeY"], e["ease"]);
            }

            // cues
            eList = GetCurrentlyActiveEntities(beat, new string[] { "slowCar", "fastCar" });
            if (eList.Count > 0)
            {
                foreach (RiqEntity entity in eList)
                {
                    if (beat > entity.beat + 2 || beat <= entity.beat)
                        continue;
                    
                    if (entity.datamodel == "freezeFrame/slowCar")
                    {
                        SlowCarCue(entity.beat, entity["variant"], true);
                    }
                    if (entity.datamodel == "freezeFrame/fastCar")
                    {
                        FastCarCue(entity.beat, entity["variant"], true);
                    }
                }
            }
            //if (QueuedCues.Count > 0)
            //{
            //    QueuedCues.RemoveAll(e => e.beat < beat - 2);
            //    foreach (RiqEntity cue in QueuedCues)
            //    {
            //        if (cue.datamodel == "freezeFrame/slowCar")
            //        {
            //            SlowCarCue(cue.beat, cue["variant"], true);
            //            continue;
            //        }
            //        if (cue.datamodel == "freezeFrame/fastCar")
            //        {
            //            FastCarCue(cue.beat, cue["variant"], true);
            //        }
            //    }
            //}
        }

        // CUE FUNCTIONS
        public static void SetBopping(double beat, float length, bool bop, bool autoBop, bool blink, bool autoBlink, double currentBeat = -1)
        {
            if (Instance == null) return;
            
            Instance.DoAutoBop = autoBop;
            Instance.DoAutoCrosshairBlink = autoBlink;
            

            if (bop || blink)
            {
                List<BeatAction.Action> actions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++) {
                    if (beat + i < currentBeat)
                        continue;
                    if (bop)
                        actions.Add(new(beat + i, delegate { Instance.Bop(); }));
                    if (blink)
                        actions.Add(new(beat + i, delegate { Instance.CrosshairBlink(); }));
                }
                BeatAction.New(Instance, actions);
            }
        }
        public static void SlowCarCue(double beat, int photoType, bool mute = false)
        {
            if (!mute) SlowCarSFX();

            if (Instance == null) return;

            Instance.EventArgs.Add(
                Instance.ScheduleInput(beat, 2f, InputAction_BasicPress, Instance.PhotoSuccess, Instance.PhotoMiss, Instance.PhotoEmpty),
                new PhotoArgs(CarType.SlowCar, (PhotoType)photoType, 0f)
            );
        }
        public static void SlowCarSFX()
        {
            SoundByte.PlayOneShotGame("freezeFrame/slowCarFar", forcePlay: true);
        }
        public static void FastCarCue(double beat, int photoType, bool mute = false)
        {
            if (!mute) FastCarSFX();

            if (Instance == null) return;

            Instance.EventArgs.Add(
                Instance.ScheduleInput(beat, 2f, InputAction_BasicPress, Instance.PhotoSuccess, Instance.PhotoMiss, Instance.PhotoEmpty),
                new PhotoArgs(CarType.FastCar, (PhotoType)photoType, 0f)
            );
            SoundByte.PlayOneShotGame("freezeFrame/fastCarNear", beat + 2);
        }
        public static void FastCarSFX()
        {
            SoundByte.PlayOneShotGame("freezeFrame/fastCarFar", forcePlay: true);
        }
        public static void ShowPhotos(double beat, float length, int gradeTypeI, bool audience, bool clearCache)
        {
            if (Instance == null) return;

            GradeType gradeType = (GradeType)gradeTypeI;

            if (PhotoList.Count <= 0)
                return;
            
            SoundByte.PlayOneShotGame("freezeFrame/pictureShow");

            // 2 = Hi
            // 1 = OK
            // 0 = Ng
            int goodScore = 2;
            foreach (PhotoArgs photo in PhotoList)
            {
                if (photo.State <= -2)
                {
                    goodScore = 0;
                    break;
                }
                if (goodScore == 2 && photo.State != 0)
                    goodScore = 1;
            }

            if (gradeType == GradeType.Symbols)
            {
                switch (goodScore)
                {
                    case 0:
                        Instance.Results.DoScaledAnimationAsync("Batsu", 0.5f);
                        break;

                    case 1:
                        Instance.Results.DoScaledAnimationAsync("Sankaku", 0.5f);
                        break;

                    case 2:
                    default:
                        Instance.Results.DoScaledAnimationAsync("Maru", 0.5f);
                        break;
                }
            }
            if (gradeType == GradeType.Thumbs)
            {
                switch (goodScore)
                {
                    case 0:
                        Instance.Results.DoScaledAnimationAsync("ThumbsDown", 0.5f);
                        break;

                    case 1:
                        Instance.Results.DoScaledAnimationAsync("ThumbsSide", 0.5f);
                        break;

                    case 2:
                    default:
                        Instance.Results.DoScaledAnimationAsync("ThumbsUp", 0.5f);
                        break;
                }
            }

            for (int i = 0; i < PhotoList.Count && i < Instance.Photographs.Length; i++)
            {
                Instance.Photographs[i].ShowPhoto(PhotoList[i]);
            }
            
            if (clearCache)
                PhotoList.Clear();

            Instance.Overlay.SetActive(false);
            Instance.DimRect.SetActive(true);
            Instance.IsShowingPhotos = true;

            // reactions sounds
            switch (goodScore)
            {
                case 2:
                    Instance.CameraMan.DoScaledAnimationAsync("Happy", 0.5f);
                    SoundByte.PlayOneShotGame("freezeFrame/result_Hi");
                    if (audience)
                        break;
                    break;
                case 1:
                    Instance.CameraMan.DoScaledAnimationAsync("Oops", 0.5f);
                    SoundByte.PlayOneShotGame("freezeFrame/result_Ok");
                    if (audience)
                        SoundByte.PlayOneShot("applause");
                    break;
                case 0:
                    Instance.CameraMan.DoScaledAnimationAsync("Cry", 0.5f);
                    SoundByte.PlayOneShotGame("freezeFrame/result_Ng");
                    if (audience)
                        break;
                    break;
                default:
                    break;
            }

            BeatAction.New(Instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate { Instance.HidePhotos(beat + length); })
            });
        }
        public static void ClearPhotos()
        {
            PhotoList.Clear();
        }
        public static void SummonWalker(RiqEntity e/*double beat, double length, int walkerType, int direction, int layer = 0*/)
        {
            if (Instance == null) return;

            double beat = e.beat;
            double length = e.length;
            PersonType walkerType = (PersonType)e["personType"];
            PersonDirection direction = (PersonDirection)e["direction"];
            int layer = e["layer"];

            GameObject walker = Instantiate(Instance.WalkerPrefab, Instance.WalkerSpawn.transform);
            Animator animator = walker.GetComponent<Animator>();
            walker.GetComponent<SortingGroup>().sortingOrder = layer;

            switch (walkerType)
            {
                case PersonType.Girlfriend:
                    animator.DoScaledAnimationAsync("Girlfriend", animLayer: 2);
                    break;
                case PersonType.Dude2:
                    animator.DoScaledAnimationAsync("Dude2", animLayer: 2);
                    break;
                case PersonType.Dude1:
                default:
                    animator.DoScaledAnimationAsync("Dude1", animLayer: 2);
                    break;
            }

            /*if (direction == (int)PersonDirection.Random)
            {
                int seed = BitConverter.ToInt32(BitConverter.GetBytes((float)beat));
                direction = new System.Random(seed).Next(1, 3);
            }*/
            if (direction == PersonDirection.Random)
            {
                if (WalkerDirections.ContainsKey(e))
                    direction = WalkerDirections[e];
                else
                    direction = PersonDirection.Right;
            }
            
            if (direction == PersonDirection.Left)
                Instance.Walkers.Add(new WalkerArgs(animator, beat, length, "EnterLeft"));
            else
                Instance.Walkers.Add(new WalkerArgs(animator, beat, length, "EnterRight"));

            double nextBeat = Math.Ceiling(beat);
            List<BeatAction.Action> actions = new();
            for (double i = nextBeat; i < beat + length; i += 1)
            {
                actions.Add(new(i, delegate { animator.DoScaledAnimationAsync("Bop", timeScale: 0.5f, animLayer: 0); }));
            }
            BeatAction.New(Instance, actions);
        }
        public static void ToggleCrowd(double beat, bool showCrowd, bool customCrowd, int crowdFarLeft, int crowdLeft, int crowdRight, int crowdFarRight, bool billboard)
        {
            if (Instance == null) return;

            Instance.ShowCrowd = showCrowd;
            if (Instance.ShowCrowd)
                Instance.Crowd.DoScaledAnimationAsync("Show", 0.5f);
            else
                Instance.Crowd.DoScaledAnimationAsync("Hide", 0.5f);
            
            if (customCrowd)
            {
                Instance.CrowdFarLeft.sprite  = Instance.CrowdSprites[crowdFarLeft];
                Instance.CrowdLeft.sprite     = Instance.CrowdSprites[crowdLeft];
                Instance.CrowdRight.sprite    = Instance.CrowdSprites[crowdRight];
                Instance.CrowdFarRight.sprite = Instance.CrowdSprites[crowdFarRight];
            }
            else
            {
                Instance.CrowdFarLeft.sprite  = Instance.CrowdSprites[2];
                Instance.CrowdLeft.sprite     = Instance.CrowdSprites[1];
                Instance.CrowdRight.sprite    = Instance.CrowdSprites[0];
                Instance.CrowdFarRight.sprite = Instance.CrowdSprites[2];
            }

            Instance.ShowBillboard = billboard;
        }
        public static void DoIntroSign(double beat, double length, bool enter, int easeIndex)
        {
            Util.EasingFunction.Ease ease = (Util.EasingFunction.Ease)easeIndex;
            Instance.SignIsMoving = true;
            if (enter)
                Instance.CurrentSignArgs = new("Enter", beat, length, ease);
            else
                Instance.CurrentSignArgs = new("Exit", beat, length, ease);
        }
        public static void IntroLightsAnim(double beat, double length, bool lightsOn)
        {
            if (!lightsOn)
            {
                Instance.IntroSign.DoScaledAnimationAsync("LightsOff", timeScale: 0.5f, animLayer: 1);
                return;
            }

            Instance.IntroSign.DoScaledAnimationFromBeatAsync("Light01", startBeat: beat, timeScale: 0.5f, animLayer: 1);
            BeatAction.New(Instance, new List<BeatAction.Action>
                {
                    new(beat + length, delegate { Instance.IntroSign.DoScaledAnimationFromBeatAsync("Light02", startBeat: beat + length, timeScale: 0.5f, animLayer: 1); }),
                    new(beat + (length * 2), delegate { Instance.IntroSign.DoScaledAnimationFromBeatAsync("Light03", startBeat: beat + (length * 2), timeScale: 0.5f, animLayer: 1); }),
                }
            );
        }
        public static void IntroLightsSound(double beat, double length, bool lightsOn)
        {
            if (!lightsOn)
                return;
            
            SoundByte.PlayOneShotGame("freezeFrame/beginningSignal1", forcePlay: true);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("freezeFrame/beginningSignal1", beat + length),
                new MultiSound.Sound("freezeFrame/beginningSignal2", beat + (length * 2)),
            }, forcePlay: true);
        }
        public static void ToggleOverlay(double beat, bool showOverlay, bool showCameraMan, bool followCamera)
        {
            Instance.ShowOverlay = showOverlay;
            Instance.ShowCameraMan = showCameraMan;
            Instance.OverlayFollowCamera = followCamera;
        }
        public static void SetMoveCameraMan(double beat, double length, float startX, float startY, float endX, float endY, int easeIndex)
        {
            Instance.CurrentCameraManMoveArgs = new(
                beat,
                length,
                new Vector3(startX, startY, 0) + CameraManStartPos,
                new Vector3(endX, endY, 0) + CameraManStartPos,
                (Util.EasingFunction.Ease)easeIndex
            );
            Instance.CameraManMoving = true;
        }
        public static void SetRotateCameraMan(double beat, double length, float startRot, float endRot, int easeIndex)
        {
            Instance.CurrentCameraManRotateArgs = new(
                beat,
                length,
                startRot,
                endRot,
                (Util.EasingFunction.Ease)easeIndex
            );
            Instance.CameraManRotating = true;
        }
        public static void SetScaleCameraMan(double beat, double length, float startX, float startY, float endX, float endY, int easeIndex)
        {
            Instance.CurrentCameraManScaleArgs = new(
                beat,
                length,
                new Vector3(startX, startY, 1),
                new Vector3(endX, endY, 1),
                (Util.EasingFunction.Ease)easeIndex
            );
            Instance.CameraManScaling = true;
        }

        // PRE-FUNCTIONS
        public void SpawnCar(SpawnCarArgs args)
        {
            if (args.Near)
            {
                if (NearCarSpawn == null)
                {
                    UnityEngine.Debug.LogError($"Failed to spawn a car at beat {args.Beat}.");
                    return;
                }
                Animator car = Instantiate(NearCarPrefab, NearCarSpawn).GetComponent<Animator>();
                car.DoScaledAnimationFromBeatAsync("Idle", startBeat: 0f, timeScale: 0.5f, animLayer: 0);
                
                if (args.Fast)
                    car.DoScaledAnimationFromBeatAsync("FastCarGo", startBeat: args.Beat, timeScale: 2.666666666666667f, animLayer: 1);
                else
                    car.DoScaledAnimationFromBeatAsync("SlowCarGo", startBeat: args.Beat, timeScale: 1.5f, animLayer: 1);

                //BeatAction.New(Instance, new List<BeatAction.Action>() { new BeatAction.Action(args.Beat + 3, delegate { Destroy(car); }) });
            }
            else
            {
                if (FarCarSpawn == null)
                {
                    UnityEngine.Debug.LogError($"Failed to spawn a car at beat {args.Beat}.");
                    return;
                }
                Animator car = Instantiate(FarCarPrefab, FarCarSpawn).GetComponent<Animator>();
                car.DoScaledAnimationFromBeatAsync("Idle", startBeat: 0f, timeScale: 0.5f, animLayer: 0);

                if (args.Fast)
                    car.DoScaledAnimationFromBeatAsync("FastCarGo", startBeat: args.Beat, timeScale: 0.5f, animLayer: 1);
                else
                    car.DoScaledAnimationFromBeatAsync("SlowCarGo", startBeat: args.Beat, timeScale: 0.16666666666666666666666666666667f, animLayer: 1);

                //BeatAction.New(Instance, new List<BeatAction.Action>() { new BeatAction.Action(args.Beat + 8, delegate { Destroy(car); }) });
            }
        }
        /*public void SpawnSlowCarNear(double beat)
        {
            if (NearCarSpawn == null) return;
            Animator nearCar = Instantiate(NearCarPrefab, NearCarSpawn).GetComponent<Animator>();
            nearCar.DoScaledAnimationFromBeatAsync("Idle", startBeat: 0f, timeScale: 0.5f, animLayer: 0);

            double startBeat = beat + 2 - 0.16666666666666666666666666666667;
            BeatAction.New(Instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat, delegate { nearCar.DoScaledAnimationFromBeatAsync("SlowCarGo", startBeat: startBeat, timeScale: 1.5f, animLayer: 1); }),
                new BeatAction.Action(beat + 3, delegate { Destroy(nearCar); } )
            });
        }
        public void SpawnSlowCarFar(double beat)
        {
            //if (beat < 0) return;
            if (FarCarSpawn == null) return;
            Animator farCar = Instantiate(FarCarPrefab, FarCarSpawn).GetComponent<Animator>();
            farCar.DoScaledAnimationFromBeatAsync("Idle", startBeat: 0f, timeScale: 0.5f, animLayer: 0);
            farCar.DoScaledAnimationFromBeatAsync("SlowCarGo", startBeat: beat, timeScale: 0.16666666666666666666666666666667f, animLayer: 1);

            BeatAction.New(Instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 7, delegate { Destroy(farCar); } )
            });
        }
        public void SpawnFastCar(double beat)
        {
            if (FarCarSpawn != null)
            {
                UnityEngine.Debug.Log("farcar");
                Animator farCar = Instantiate(FarCarPrefab, FarCarSpawn).GetComponent<Animator>();
                farCar.DoScaledAnimationFromBeatAsync("Idle", startBeat: 0f, timeScale: 0.5f, animLayer: 0);
                
                BeatAction.New(Instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - 0.5, delegate { if (beat >= 0.5) farCar.DoScaledAnimationFromBeatAsync("FastCarGo", startBeat: beat - 0.5, timeScale: 0.5f, animLayer: 1); }),
                    new BeatAction.Action(beat + 3, delegate { Destroy(farCar); } )
                });
            }
            if (NearCarSpawn != null)
            {
                UnityEngine.Debug.Log("fartcar");
                Animator nearCar = Instantiate(NearCarPrefab, NearCarSpawn).GetComponent<Animator>();
                nearCar.DoScaledAnimationFromBeatAsync("Idle", startBeat: 0f, timeScale: 0.5f, animLayer: 0);
                
                BeatAction.New(Instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 2 - 0.09375, delegate { nearCar.DoScaledAnimationFromBeatAsync("FastCarGo", startBeat: beat - 0.09375, timeScale: 2.666666666666667f, animLayer: 1); }),
                    new BeatAction.Action(beat + 3, delegate { Destroy(nearCar); } )
                });
            }
        }*/

        // INPUT RESULTS
        public void PhotoSuccess(PlayerActionEvent caller, float state)
        {
            PhotoArgs args;
            bool hasArgs = EventArgs.TryGetValue(caller, out args);
            EventArgs.Remove(caller);

            // passing the args for the photos
            if (hasArgs)
            {
                if (state >= 1f)
                {
                    args.State = 1;
                }
                else if (state <= -1f)
                {
                    args.State = -1;
                }
                else
                {
                    args.State = 0;
                }

                PushPhoto(args);
            }
            CameraFlash();
        }
        public void PhotoMiss(PlayerActionEvent caller)
        {
            PhotoArgs args;
            bool hasArgs = EventArgs.TryGetValue(caller, out args);
            EventArgs.Remove(caller);

            if (hasArgs)
            {
                args.State = -2;
                PushPhoto(args);
            }
        }
        public void PhotoEmpty(PlayerActionEvent caller)
        {
            
        }

        // GENERAL METHODS
        public void Bop()
        {
            CameraMan.DoScaledAnimationAsync("Bop", 0.5f);
        }
        public void CrosshairBlink()
        {
            Crosshair.SetActive(!Crosshair.activeSelf);
        }
        public void CameraFlash()
        {
            Shutter.DoScaledAnimationAsync("Shut", 0.5f);
            CameraMan.DoScaledAnimationAsync("Flash", 0.5f);
            SoundByte.PlayOneShotGame("freezeFrame/shutter"/*, pitch: (float)new System.Random().NextDouble() + 0.5f*/);
        }
        public void PushPhoto(PhotoArgs args)
        {
            //while (PhotoList.Count >= MAX_PHOTOS)
            //    PhotoList.RemoveAt(0);
            if (args.PhotoType == PhotoType.Random)
            {
                if (UnityEngine.Random.Range(0, 8) >= 7)
                {
                    switch (UnityEngine.Random.Range(0, 3))
                    {
                        case 0:
                            args.PhotoType = PhotoType.Ninja;
                            break;
                        case 1:
                            args.PhotoType = PhotoType.Ghost;
                            break;
                        case 2:
                            args.PhotoType = PhotoType.Rats;
                            break;
                        default:
                            args.PhotoType = PhotoType.Default;
                            break;
                    }
                }
                else
                    args.PhotoType = PhotoType.Default;
            }

            PhotoList.Add(args);
        }
        public void HidePhotos(double beat)
        {
            Overlay.SetActive(ShowOverlay);
            DimRect.SetActive(false);
            foreach (Photograph photo in Photographs)
                photo.HideAll();
            Results.DoScaledAnimationAsync("None", 0.5f);
            IsShowingPhotos = false;
            if (beat % 1 == 0 && DoAutoBop)
                CameraMan.DoScaledAnimationAsync("Bop", 0.5f);
            else
                CameraMan.DoScaledAnimationAsync("Idle", 0.5f);
        }
        /*public static void QueueCue(RiqEntity e)
        {
            if (e.datamodel == "freezeFrame/slowCar")
            {
                SoundByte.PlayOneShotGame("freezeFrame/smallCarZoom1a", forcePlay: true);
            }
            if (e.datamodel == "freezeFrame/fastCar")
            {
                SoundByte.PlayOneShotGame("freezeFrame/fastCarZoom", forcePlay: true);
            }
            QueuedCues.Add(e);
        }*/
        public void CalculateAutoShowPhotos()
        {
            List<RiqEntity> allCars = EventCaller.GetAllInGameManagerList("freezeFrame", new string[] { "slowCar", "fastCar" });

            List<BeatAction.Action> actions = new();
            List<double> beats = new(); // so you don't double up

            foreach (RiqEntity entity in allCars.Where(car => (bool)car["autoShowPhotos"]))
            {
                double showBeat = 3f;
                if (allCars.Any(car => car.beat == entity.beat - 1f))
                    showBeat = 3.5f;
                if (entity.datamodel == "freezeFrame/fastCar")
                    showBeat = 4f;
                if (allCars.Any(car => car.beat == entity.beat - 2f))
                    showBeat = 4.5f;
                showBeat = entity.beat + showBeat;
                
                if (!allCars.Any(car => car.beat > entity.beat && car.beat <= showBeat))
                {
                    if (!beats.Any(b => showBeat >= b && showBeat < b + 2))
                    {
                        beats.Add(showBeat);
                        actions.Add(new BeatAction.Action(showBeat, delegate { ShowPhotos(showBeat, 2, (int)entity["gradeType"], (bool)entity["audience"], false); }));
                    }
                }
            }

            if (actions.Count > 0)
            {
                BeatAction.New(Instance, actions);
            }
        }
        public void CalculateCarSpawns()
        {
            List<RiqEntity> fastCars = EventCaller.GetAllInGameManagerList("freezeFrame", new string[] { "fastCar" });
            foreach (RiqEntity e in fastCars)
            {
                QueuedCars.Add(new SpawnCarArgs(e.beat - 0.5, true, false));
                QueuedCars.Add(new SpawnCarArgs(e.beat + 2 - 0.09375, true, true));
            }

            List<RiqEntity> slowCars = EventCaller.GetAllInGameManagerList("freezeFrame", new string[] { "slowCar" });
            foreach (RiqEntity e in slowCars)
            {
                QueuedCars.Add(new SpawnCarArgs(e.beat + 2 - 0.16666666666666666666666666666667, false, true));
            }

            List<List<double>> clusters = new();

            while (slowCars.Count > 0)
            {
                double minBeat = slowCars[0].beat;
                double maxBeat = minBeat + 2;

                clusters.Add(slowCars.Where(car => car.beat >= minBeat && car.beat < maxBeat).Select(car => car.beat).ToList());
                slowCars.RemoveAll(car => car.beat >= minBeat && car.beat < maxBeat);
            }

            foreach (List<double> cluster in clusters)
            {
                double midBeat = cluster.Min() + ((cluster.Max() - cluster.Min()) / 2);

                foreach (double beat in cluster)
                {
                    double diff = midBeat - beat;
                    double modifiedBeat = midBeat + (diff / 4);

                    //BeatAction.New(Instance, new List<BeatAction.Action>(){
                    //    new BeatAction.Action(modifiedBeat - 4, delegate { SpawnSlowCarFar(modifiedBeat - 4); } )
                    //});
                    QueuedCars.Add(new SpawnCarArgs(modifiedBeat - 4, false, false));
                }
            }
        }
        public void PreRandomizeWalkers()
        {
            IEnumerable<RiqEntity> walkers = EventCaller.GetAllInGameManagerList("freezeFrame", new string[] { "spawnPerson" }).Where(e => (PersonDirection)e["direction"] == PersonDirection.Random);
            foreach (RiqEntity e in walkers)
            {
                if (!WalkerDirections.ContainsKey(e))
                {
                    float rand = UnityEngine.Random.Range(0.0f, 1.0f);
                    if (rand >= 0.5)
                    {
                        WalkerDirections.Add(e, PersonDirection.Left);
                    }
                    else
                    {
                        WalkerDirections.Add(e, PersonDirection.Right);
                    }
                }
            }
            List<RiqEntity> keysToRemove = new();
            foreach (RiqEntity key in WalkerDirections.Keys)
            {
                if (!walkers.Contains(key))
                    keysToRemove.Add(key);
            }
            foreach (RiqEntity key in keysToRemove)
            {
                WalkerDirections.Remove(key);
            }
            Debug.Log($"Walker Count: {WalkerDirections.Count}");
        }
        public void CarbageCollection()
        {
            foreach (Transform child in FarCarSpawn)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in NearCarSpawn)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in WalkerSpawn)
            {
                Destroy(child.gameObject);
            }
        }
        public RiqEntity GetLastEntityOfType(double beat, string datamodel)
        {
            foreach (RiqEntity e in EventCaller.GetAllInGameManagerList("freezeFrame", new string[] { datamodel }).OrderBy(e => -e.beat))
            {
                if (e.beat < beat)
                    return e;
            }
            return null;
        }
        public List<RiqEntity> GetCurrentlyActiveEntities(double beat, string datamodel)
        {
            return GetCurrentlyActiveEntities(beat, new string[] { datamodel });
        }
        public List<RiqEntity> GetCurrentlyActiveEntities(double beat, string[] datamodel)
        {
            List<RiqEntity> result = new();
            foreach (RiqEntity e in EventCaller.GetAllInGameManagerList("freezeFrame", datamodel))
            {
                if (beat >= e.beat && beat <= e.beat + e.length)
                    result.Add(e);
                if (e.beat > beat)
                    break;
            }
            return result;
        }
        /*protected static System.Random GetSeededRandom(float? mulch = null) // i just made this term up i have no idea if it has any basis in actual programming
        {
            if (SuperSeed is null)
                SuperSeed = new System.Random().Next();
            
            if (mulch is not null)
            {
                int seed = BitConverter.ToInt32(BitConverter.GetBytes(mulch.Value));
                return new System.Random(SuperSeed.Value * seed);
            }

            return new System.Random(SuperSeed.Value);
        }*/

        // ENUMS
        public enum CarType : int
        {
            SlowCar = 0,
            FastCar = 1
        }
        public enum PhotoType : int
        {
            Random = 0,
            Default = 1,
            Ninja = 2,
            Ghost = 3,
            Rats = 4,
            PeaceSign = 5,
            GirlfriendRight = 6,
            GirlfriendLeft = 7,
            Dude1Right = 8,
            Dude1Left = 9,
            Dude2Right = 10,
            Dude2Left = 11,
        }
        public enum PersonType : int
        {
            Dude1 = 0,
            Dude2 = 1,
            Girlfriend = 2
        }
        public enum PersonDirection : int
        {
            Random = 0,
            Right = 1,
            Left = 2
        }
        public enum GradeType : int
        {
            Symbols = 0,
            Thumbs = 1,
            None = 2
        }
        public enum CustomCrowdType : int
        {
            TealDancers = 0,
            YellowDancers = 1,
            PinkDancers = 2,
            CyanDancers = 3,
        }

        // STRUCTS
        public struct PhotoArgs
        {
            public CarType Car;
            public PhotoType PhotoType;
            public float State;

            public PhotoArgs(CarType car, PhotoType photoType, float state)
            {
                Car = car;
                PhotoType = photoType;
                State = state;
            }
        }
        public struct WalkerArgs
        {
            public Animator Walker;
            public double StartTime;
            public double Length;
            public string AnimName;

            public WalkerArgs(Animator walker, double startTime, double length, string animName)
            {
                Walker = walker;
                StartTime = startTime;
                Length = length;
                AnimName = animName;
            }
        }
        public struct SignMoveArgs
        {
            public string AnimName;
            public double StartTime;
            public double Length;
            public Util.EasingFunction.Ease Ease;

            public SignMoveArgs(string animName, double startTime, double length, Util.EasingFunction.Ease ease)
            {
                AnimName = animName;
                StartTime = startTime;
                Length = length;
                Ease = ease;
            }
        }
        public struct CameraManMoveArgs
        {
            public double StartBeat;
            public double Length;
            public Vector3 StartPos;
            public Vector3 EndPos;
            public Util.EasingFunction.Ease Ease;

            public CameraManMoveArgs(double startBeat, double length, Vector3 startPos, Vector3 endPos, Util.EasingFunction.Ease ease)
            {
                StartBeat = startBeat;
                Length = length;
                StartPos = startPos;
                EndPos = endPos;
                Ease = ease;
            }
        }
        public struct CameraManRotateArgs
        {
            public double StartBeat;
            public double Length;
            public float StartRot;
            public float EndRot;
            public Util.EasingFunction.Ease Ease;

            public CameraManRotateArgs(double startBeat, double length, float startRot, float endRot, Util.EasingFunction.Ease ease)
            {
                StartBeat = startBeat;
                Length = length;
                StartRot = startRot;
                EndRot = endRot;
                Ease = ease;
            }
        }
        public struct CameraManScaleArgs
        {
            public double StartBeat;
            public double Length;
            public Vector3 StartScale;
            public Vector3 EndScale;
            public Util.EasingFunction.Ease Ease;

            public CameraManScaleArgs(double startBeat, double length, Vector3 startScale, Vector3 endScale, Util.EasingFunction.Ease ease)
            {
                StartBeat = startBeat;
                Length = length;
                StartScale = startScale;
                EndScale = endScale;
                Ease = ease;
            }
        }
        public struct SpawnCarArgs
        {
            public double Beat;
            public bool Near;
            public bool Fast;

            public SpawnCarArgs(double beat, bool fast, bool near)
            {
                Beat = beat;
                Near = near;
                Fast = fast;
            }
        }
    }
}