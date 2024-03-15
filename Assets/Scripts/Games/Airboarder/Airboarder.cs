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
    /// Minigame loaders handle the setup of your minigame.
    /// Here, you designate the game prefab, define entities, and mark what AssetBundle to load

    /// Names of minigame loaders follow a specific naming convention of `PlatformcodeNameLoader`, where:
    /// `Platformcode` is a three-leter platform code with the minigame's origin
    /// `Name` is a short internal name
    /// `Loader` is the string "Loader"

    /// Platform codes are as follows:
    /// Agb: Gameboy Advance    ("Advance Gameboy")
    /// Ntr: Nintendo DS        ("Nitro")
    /// Rvl: Nintendo Wii       ("Revolution")
    /// Ctr: Nintendo 3DS       ("Centrair")
    /// Mob: Mobile
    /// Pco: PC / Other

    /// Fill in the loader class label, "*prefab name*", and "*Display Name*" with the relevant information
    /// For help, feel free to reach out to us on our discord, in the #development channel.
    public static class NtrAirboarderLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("airboarder", "Airboarder", "fbd4f2", false, false, new List<GameAction>()
            {

                new GameAction("bop", "Bop")
                {
                    function = delegate {Airboarder.instance.BopToggle(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["auto"], eventCaller.currentEntity["toggle"]);},
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Toggle if the airboarders should bop for the duration of this event."),
                        new Param("auto", false, "Autobop", "Toggle if the airboarders should bop automatically until another Bop event is reached."),
                    }
                },

                new GameAction("duck", "Duck")
                {
                    function = delegate {Airboarder.instance.PrepareJump(eventCaller.currentEntity.beat, eventCaller.currentEntity["ready"]);},
                    defaultLength = 4f,
                    resizable = false,
                    parameters = new List<Param>()
                    {
                        new Param("ready", true, "Play Ready Sound", "Toggle if the ready sound plays."),
                    }
                },

                new GameAction("crouch", "Charged Duck")
                {
                    function = delegate {Airboarder.instance.PrepareJump(eventCaller.currentEntity.beat, eventCaller.currentEntity["ready"]);},
                    defaultLength = 4f,
                    resizable = false,
                    parameters = new List<Param>()
                    {
                        new Param("ready", true, "Play Ready Sound", "Toggle if the ready sound plays."),
                    }
                },

                new GameAction("jump", "Jump")
                {
                    function = delegate {Airboarder.instance.PrepareJump(eventCaller.currentEntity.beat, eventCaller.currentEntity["ready"]);},
                    defaultLength = 4f,
                    resizable = false,
                    parameters = new List<Param>()
                    {
                        new Param("ready", false, "Play Ready Sound", "Toggle if the ready sound plays."),
                    }
                },

                new GameAction("forceCharge", "Force Charge")
                {
                    function = delegate {Airboarder.instance.ForceCharge(); },
                    defaultLength = 0.5f,
                    resizable = false,
                },


                new GameAction("letsGo", "YEAAAAAH LET'S GO")
                {
                    function = delegate {Airboarder.instance.YeahLetsGo(eventCaller.currentEntity.beat, eventCaller.currentEntity["sound"]);},
                    defaultLength = 8f,
                    resizable = false,
                    parameters = new List<Param>()
                    {
                        new Param("sound", true, "Play Sound", "Toggle if the 'YEAAAAAH LET'S GO' voice clip plays."),
                    }
                },

                new GameAction("fade background", "Background Color")
                {
                    function = delegate {Airboarder.instance.BackgroundColor(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["colorStart"], eventCaller.currentEntity["colorEnd"], eventCaller.currentEntity["ease"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorStart", Color.white, "Start Color", "Set the color at the start of the event."),
                        new Param("colorEnd", Airboarder.defaultBGColor, "End Color", "Set the color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                },


                new GameAction("camera", "Camera Controls")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        // var rotation = new Vector3(0, e["valA"], 0);
                        Airboarder.instance.ChangeCamera(eventCaller.currentEntity.beat, eventCaller.currentEntity["valA"], eventCaller.currentEntity["valB"], eventCaller.currentEntity.length, (Util.EasingFunction.Ease)eventCaller.currentEntity["type"], eventCaller.currentEntity["additive"]);
                    },
                    defaultLength = 4,
                    resizable = true,
                    hidden = true,
                    parameters = new List<Param>() {
                        new Param("valA", new EntityTypes.Integer(-360, 360, 0), "Rotation", "Set the rotation of the camera around the pivot point."),
                        new Param("valB", new EntityTypes.Float(0.1f, 4f, 0.5f), "Zoom", "Set the camera's level of zoom."),
                        new Param("type", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action."),
                        new Param("additive", true, "Additive Rotation", "Toggle if the above rotation should be added to the current angle instead of setting the target angle to travel to.")
                    }
                },
            }
            // ,
            // new List<string>() {"ntr", "normal"},
            // "ntrAirboarder", "en",
            // new List<string>() { }
            );
        }
    }
}



namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.

    using Scripts_Airboarder;

    public class Airboarder : Minigame
    {
        

        public static Airboarder instance;

        public static Color defaultBGColor = new Color(0.9921569f, 0.7686275f, 0.9921569f);

        public bool wantsCrouch;
        [Header("Materials")]
        [SerializeField] private Material bgMaterial;
        [SerializeField] private Material fadeMaterial;
        [SerializeField] private Material[] floorMaterial;
        
        [Header("Camera")]
        [SerializeField] Transform cameraPivot;
        [SerializeField] Transform cameraPos;
        [SerializeField] float cameraFOV;

        [Header("Objects")]
        [SerializeField] Arch archBasic;
        [SerializeField] Wall wallBasic;
        [SerializeField] GameObject floor;

        [Header("Animators")]
        [SerializeField] public Animator CPU1;
        [SerializeField] public Animator CPU2;
        [SerializeField] public Animator Player;
        [SerializeField] public Animator Dog;
        [SerializeField] public Animator Tail;
        [SerializeField] public Animator Floor;

        bool goBop;
        public bool cpu1CantBop = false;
        public bool cpu2CantBop = false;
        public bool playerCantBop = false;
        
        public double startBeat;
        public double switchBeat;


        double cameraRotateBeat = double.MaxValue;
        double cameraRotateLength;
        Util.EasingFunction.Ease cameraRotateEase;
        float cameraRotateLast = 0, cameraScaleLast = 1;
        float cameraRotateNext = 0, cameraScaleNext = 1;


        public float startFloor;

        private void Awake()
        {
            instance = this;
            SetupBopRegion("airboarder", "bop", "auto");   
            wantsCrouch = false;
            GameCamera.AdditionalPosition = cameraPos.position + (Quaternion.Euler(cameraPos.rotation.eulerAngles) * Vector3.forward * 10f);
            GameCamera.AdditionalRotEuler = cameraPos.rotation.eulerAngles;
            GameCamera.AdditionalFoV = cameraFOV;
            
        }

        public override void OnGameSwitch(double beat)
        {
            
            List<BeatAction.Action> actions = new()
            {};
            wantsCrouch = false;

            double switchBeat = beat;
            double startBeat = double.MaxValue;
            double endBeat = double.MaxValue;
            
            var entities = GameManager.instance.Beatmap.Entities;
            //find when the next game switch/remix end happens
            var nextGameSwitches = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).FindAll(x => x.beat > beat && x.datamodel != "gameManager/switchGame/airboarder");
            double nextGameSwitchBeat = double.MaxValue;

            //lists arch and wall events
            List<RiqEntity> blockEvents = gameManager.Beatmap.Entities.FindAll(e => e.datamodel is "airboarder/duck" or "airboarder/crouch" or "airboarder/jump" && e.beat >= beat && e.beat < endBeat);
        
            foreach (var e in blockEvents)
            {
                switch (e.datamodel) {
                    case "airboarder/duck":
                    RequestArch(e.beat - 25, false);
                    break;
                    case "airboarder/crouch":
                    RequestArch(e.beat - 25, true);
                    break;
                    case "airboarder/jump":
                    RequestWall(e.beat - 25);
                    break;
                }
            }
            PersistColor (beat);
        }

        private void Start()
        {
            EntityPreCheck(Conductor.instance.songPositionInBeatsAsDouble);
            bgMaterial.color = defaultBGColor;
            fadeMaterial.color = defaultBGColor;
        }

        void EntityPreCheck(double beat)
        {
            cameraRotateBeat = double.MaxValue;
            cameraRotateLength = 0;
            cameraRotateEase = Util.EasingFunction.Ease.Linear;
            cameraRotateLast = 0; cameraScaleLast = 1;
            cameraRotateNext = 0; cameraScaleNext = 1;
            
            List<RiqEntity> prevEntities = GameManager.instance.Beatmap.Entities.FindAll(c => c.beat < beat && c.datamodel.Split(0) == "airboarder");
            RiqEntity lastGameSwitch = GameManager.instance.Beatmap.Entities.FindLast(c => c.beat <= beat && c.datamodel == "gameManager/switchGame/airboarder");

            if (lastGameSwitch == null) return;
            List<RiqEntity> cameraEntities = prevEntities.FindAll(c => c.beat >= lastGameSwitch.beat && c.datamodel == "airboarder/camera");

            foreach (var entity in cameraEntities)
            {
                ChangeCamera(entity.beat, entity["valA"], entity["valB"], entity.length, (Util.EasingFunction.Ease)entity["type"], entity["additive"]);
            }

            UpdateCamera(beat);
        }

        public override void OnPlay(double beat)
        {
            EntityPreCheck(beat);
            OnGameSwitch(beat);
        }

        void UpdateCamera(double beat)
        {
            if (beat >= cameraRotateBeat)
            {
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(cameraRotateEase);
                float rotProg = Conductor.instance.GetPositionFromBeat(cameraRotateBeat, cameraRotateLength, true);
                rotProg = Mathf.Clamp01(rotProg);
                float rot = func(cameraRotateLast, cameraRotateNext, rotProg);
                cameraPivot.rotation = Quaternion.Euler(0, rot, 0);
                cameraPivot.localScale = Vector3.one * func(cameraScaleLast, cameraScaleNext, rotProg);
            }

            GameCamera.AdditionalPosition = cameraPos.position + (Quaternion.Euler(cameraPos.rotation.eulerAngles) * Vector3.forward * 10f);
            GameCamera.AdditionalRotEuler = cameraPos.rotation.eulerAngles;
            GameCamera.AdditionalFoV = cameraFOV;
        }

        public void Update()
        {
            var cond = Conductor.instance;
            var currentBeat = cond.songPositionInBeatsAsDouble;
            BackgroundColorUpdate();
            
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, 5f);

            Floor.Play("moving", 0, normalizedBeat);
            Floor.speed = 0;
            Dog.Play("run", 0, normalizedBeat*7.5f);
            Dog.Play("wag",1,normalizedBeat*2.5f);
            CPU1.Play("hover",0,normalizedBeat);
            CPU2.Play("hover",0,normalizedBeat);
            Player.Play("hover",0,normalizedBeat);

          
            if (cond.isPlaying && !cond.isPaused){
                if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress)){
                    if (wantsCrouch)
                    {
                        Player.DoScaledAnimationAsync("charge",1f, 0, 1);
                        playerCantBop = true;
                    }
                    else 
                    {
                        Player.DoScaledAnimationAsync("duck",1f, 0, 1);
                        SoundByte.PlayOneShotGame("airboarder/crouch");
                        BeatAction.New(this, new() {
                            new(currentBeat, ()=>playerCantBop = true),
                            new(currentBeat+1.5f, ()=>playerCantBop = false)});
                    }
                }
                if (PlayerInput.GetIsAction(InputAction_BasicRelease) && !IsExpectingInputNow(InputAction_BasicRelease)){
                    if (wantsCrouch)
                    {
                        Player.DoScaledAnimationAsync("hold",1f, 0, 1);
                        playerCantBop = false;
                    }
                }
                
                if (PlayerInput.GetIsAction(InputAction_FlickRelease) && !IsExpectingInputNow(InputAction_FlickRelease))
                {
                    if ( PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
                    {
                        Player.DoScaledAnimationAsync("jump",1f, 0, 1);
                        SoundByte.PlayOneShotGame("airboarder/jump");
                        playerCantBop = false;}
                }
            }

            UpdateCamera(currentBeat);

        }

        private ColorEase bgColorEase = new(defaultBGColor);

        //call this in update
        private void BackgroundColorUpdate()
        {
            bgMaterial.color = bgColorEase.GetColor();
            fadeMaterial.color = bgColorEase.GetColor();
        
        }
        public void BackgroundColor(double beat, float length, Color startColor, Color endColor, int ease)
        {
            bgColorEase = new(beat, length, startColor, endColor, ease);
        }
        

        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("airboarder", new string[] { "fade background" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["colorStart"], lastEvent["colorEnd"], lastEvent["ease"]);
            }
        }

        public void ForceCharge()
        {
            CPU1.DoScaledAnimationAsync("charge", 1f, 0, 1);
            CPU2.DoScaledAnimationAsync("charge", 1f, 0, 1);
            Player.DoScaledAnimationAsync("charge", 1f, 0, 1);
            cpu1CantBop = true;
            cpu2CantBop = true;
            playerCantBop = true;
            wantsCrouch = true;
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat))
            {
                Bop();
            }

        }

        public void PrepareJump(double beat, bool readySound)
        {
            if (readySound)
            {
                SoundByte.PlayOneShotGame("airboarder/ready");
            }

        }

        public void ChangeCamera(double beat, float rotation, float camZoom, double length, Util.EasingFunction.Ease ease, bool additive = true)
        {
            cameraRotateBeat = beat;
            cameraRotateLength = length;
            cameraRotateEase = ease;
            cameraRotateLast = cameraRotateNext % 360f;
            cameraScaleLast = cameraScaleNext;
            cameraScaleNext = camZoom;
            if (additive)
            {
                cameraRotateNext = cameraRotateLast + rotation;
            }
            else
            {
                cameraRotateNext = rotation;
            }
        }

        public void BopToggle(double beat, float length, bool boarders, bool autoBop)
        {
            
            if (boarders)
            {
                List<BeatAction.Action> bops = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    bops.Add(new BeatAction.Action(beat + i, delegate { Bop(); }));
                }
                BeatAction.New(instance, bops);
            }
        }

        public void Bop()
        {
            if (!playerCantBop){
            Player.DoScaledAnimationAsync("bop",0.5f, 0, 1);
            }
            if (!cpu1CantBop){
            CPU1.DoScaledAnimationAsync("bop",0.5f, 0, 1);
            }
            if (!cpu2CantBop){
            CPU2.DoScaledAnimationAsync("bop",0.5f, 0, 1);
            }
        }

        public void YeahLetsGo(double beat, bool voiceOn)
        {
            if(voiceOn)
            {
                BeatAction.New(instance, new List<BeatAction.Action>(){
                    new BeatAction.Action(beat, delegate {SoundByte.PlayOneShotGame("airboarder/start1");}),
                    new BeatAction.Action(beat + 6.5, delegate {SoundByte.PlayOneShotGame("airboarder/start2");}),
                    new BeatAction.Action(beat + 7, delegate {SoundByte.PlayOneShotGame("airboarder/start3");}),
                });
            }
            BeatAction.New(instance, new List<BeatAction.Action>(){
                new BeatAction.Action(beat, delegate {CPU1.DoScaledAnimationAsync("letsgo", 1f, 0, 1);}),
                new BeatAction.Action(beat, delegate {CPU2.DoScaledAnimationAsync("letsgo", 1f, 0, 1);}),
                new BeatAction.Action(beat, delegate {Player.DoScaledAnimationAsync("letsgo", 1f, 0, 1);})
            }

            );
        }

        public void MissSound(double beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("airboarder/miss1", beat),
                new MultiSound.Sound("airboarder/missvox", beat),
                new MultiSound.Sound("airboarder/miss2", beat + 0.25f),
                new MultiSound.Sound("airboarder/miss3", beat + 0.75f),
                new MultiSound.Sound("airboarder/miss4", beat + 0.875f),
                new MultiSound.Sound("airboarder/miss5", beat + 1f),
                new MultiSound.Sound("airboarder/miss6", beat + 1.125f),
                new MultiSound.Sound("airboarder/miss7", beat + 1.25f),
                new MultiSound.Sound("airboarder/miss8", beat + 1.5f),
                new MultiSound.Sound("airboarder/miss9", beat + 1.75f),
                new MultiSound.Sound("airboarder/miss10", beat + 2f),
                new MultiSound.Sound("airboarder/miss11", beat + 2.25f),
                new MultiSound.Sound("airboarder/miss12", beat + 2.5f),
                new MultiSound.Sound("airboarder/miss13", beat + 2.75f),
                new MultiSound.Sound("airboarder/miss14", beat + 3f),
                new MultiSound.Sound("airboarder/miss15", beat + 3.25f)
            });
        }

        public void RequestArch(double beat, bool crouch)
        {
            Arch newArch = Instantiate(archBasic, transform);
            newArch.appearBeat = beat;
            newArch.gameObject.SetActive(true);
            if (crouch) {
                archBasic.CueCrouch(beat+25);
            } else {
                newArch.CueDuck(beat+25);
            }
        }

        public void RequestWall(double beat)
        {
            Wall newWall = Instantiate(wallBasic, transform);
            newWall.appearBeat = beat;
            newWall.gameObject.SetActive(true);
            newWall.CueJump(beat+25);
        }




        

        


    }
}