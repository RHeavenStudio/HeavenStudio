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
                }


            },

            new List<string>() {"ntr", "normal"},
            "ntrAirboarder", "en",
            new List<string>() { }
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

        public bool wantsCrouch;

        [Header("Objects")]
        [SerializeField] Arch archBasic;
        [SerializeField] Wall wallBasic;

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


        public float startFloor;

        private void Awake()
        {
            instance = this;
            SetupBopRegion("airboarder", "bop", "auto");   
            wantsCrouch = false;
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
        
            foreach (var e in entities.FindAll(e => e.beat >= beat && e.beat < endBeat && e.datamodel is "airboarder/duck" or "airboarder/crouch" or "airboarder/jump"))
                {
                switch (e.datamodel) {
                    case "airboarder/duck":
                    // do the stuff
                    RequestArch(e.beat-25);
                    archBasic.CueDuck(e.beat);
                    break;
                    case "airboarder/crouch":
                    // do the stuff
                    RequestArch(e.beat-25);
                    archBasic.CueCrouch(e.beat);
                    break;
                    case "airboarder/jump":
                    RequestWall(e.beat-25);
                    wallBasic.CueJump(e.beat);
                    break;
                }
                }
            
            


                
        }


        

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }


        public void Update()
        {
            var cond = Conductor.instance;
            var currentBeat = cond.songPositionInBeatsAsDouble;
            
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

        public void RequestArch(double beat)
        {
            Arch newArch = Instantiate(archBasic, transform);
            newArch.appearBeat = beat;
            newArch.gameObject.SetActive(true);
        }

        public void RequestWall(double beat)
        {
            Wall newWall = Instantiate(wallBasic, transform);
            newWall.appearBeat = beat;
            newWall.gameObject.SetActive(true);
        }




        

        


    }
}