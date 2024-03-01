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
                    defaultLength = 4f,
                    resizable = false,
                    priority = 1
                },

                new GameAction("crouch", "Charged Duck")
                {
                    defaultLength = 4f,
                    resizable = false,
                    priority = 1
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
        }

        public override void OnGameSwitch(double beat)
        {
            List<BeatAction.Action> actions = new()
            {};

            double switchBeat = beat;
            double startBeat = double.MaxValue;
            double endBeat = double.MaxValue;
            
            var entities = GameManager.instance.Beatmap.Entities;
            //find when the next game switch/remix end happens
            var nextGameSwitches = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).FindAll(x => x.beat > beat && x.datamodel != "gameManager/switchGame/airboarder");
            double nextGameSwitchBeat = double.MaxValue;
            
            
            List<RiqEntity> relevantArches = GetAllArches(beat, nextGameSwitchBeat);
            
            foreach (var archReg in relevantArches)
            {
                RequestArch(archReg.beat-25);
                archBasic.CueDuck(archReg.beat);
          
              }
            SetupCrouches(beat, nextGameSwitchBeat);
            SetupWalls(beat, nextGameSwitchBeat);


                
        }

        public void SetupArches (double beat, double nextSwitch)
        {
            List<RiqEntity> relevantArches = GetAllArches(beat, nextSwitch);
            
            foreach (var archReg in relevantArches)
            {
                RequestArch(archReg.beat-25);
                archBasic.CueDuck(archReg.beat);
          
              }
        }

        public void SetupCrouches (double beat, double nextSwitch)
        {
            List<RiqEntity> relevantCrouches = GetAllCrouches(beat, nextSwitch);
            foreach (var archCro in relevantCrouches)
            {
                RequestArch(archCro.beat-25);
                archBasic.CueCrouch(archCro.beat);
          
              }
        }

        public void SetupWalls (double beat, double nextSwitch)
        {
            List<RiqEntity> relevantWalls = GeteAllWalls(beat, nextSwitch);
            foreach (var wallReg in relevantWalls)
            {
                RequestWall(wallReg.beat-25);
                wallBasic.CueJump(wallReg.beat);
          
              }
        }
        

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        private List<RiqEntity> GetAllArches(double beat, double endBeat)
        {
            //lists arch and wall events
            return EventCaller.GetAllInGameManagerList("airboarder", new string[] { "duck"}).FindAll(x => x.beat >= beat && x.beat < endBeat);
        }

        private List<RiqEntity> GetAllCrouches(double beat, double endBeat)
        {
            //lists arch and wall events
            return EventCaller.GetAllInGameManagerList("airboarder", new string[] { "crouch"}).FindAll(x => x.beat >= beat && x.beat < endBeat);
        }

        private List<RiqEntity> GeteAllWalls(double beat, double endBeat)
        {
            //lists arch and wall events
            return EventCaller.GetAllInGameManagerList("airboarder", new string[] { "jump"}).FindAll(y => y.beat >= beat && y.beat < endBeat);
        }

        public void Update()
        {
            var cond = Conductor.instance;
            var currentBeat = cond.songPositionInBeatsAsDouble;
            
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, 5f);

            Floor.Play("moving", 0, normalizedBeat);
            Floor.speed = 0;
            Dog.Play("run", 0, normalizedBeat*5);
            Tail.Play("wag",0,normalizedBeat*5);
            CPU1.Play("hover",0,normalizedBeat);
            CPU2.Play("hover",0,normalizedBeat);
            Player.Play("hover",0,normalizedBeat);

          
            if (cond.isPlaying && !cond.isPaused){
                if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress)){
                    if (wantsCrouch)
                    {
                        Player.GetComponent<Animator>().DoScaledAnimationAsync("charge",1f, 0, 1);
                        playerCantBop = true;
                    }
                    else 
                    {
                        Player.GetComponent<Animator>().DoScaledAnimationAsync("duck",1f, 0, 1);
                    }
                }
                if (PlayerInput.GetIsAction(InputAction_BasicRelease) && !IsExpectingInputNow(InputAction_BasicRelease)){
                Player.GetComponent<Animator>().DoScaledAnimationAsync("hold",1f, 0, 1);
                playerCantBop = false;}
                if (PlayerInput.GetIsAction(InputAction_FlickRelease) && !IsExpectingInputNow(InputAction_FlickRelease)){
                Player.GetComponent<Animator>().DoScaledAnimationAsync("hold",1f, 0, 1);
                playerCantBop = false;}
            }

        }

        public override void OnBeatPulse(double beat)
        {
            if (goBop)
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

        public void BopToggle(double beat, float length, bool shouldBop, bool autoBop)
        {
            goBop = autoBop;
            if (shouldBop)
            {
                Bop();
            }
            if (autoBop) { return;}
            if (shouldBop)
            {
                List<BeatAction.Action> bops = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    bops.Add(new BeatAction.Action(beat + i, delegate { Bop(); }));
                }
            }
        }

        public void Bop()
        {
            if (!playerCantBop){
            Player.GetComponent<Animator>().DoScaledAnimationAsync("bop",0.5f, 0, 1);
            }
            if (!cpu1CantBop){
            CPU1.GetComponent<Animator>().DoScaledAnimationAsync("bop",0.5f, 0, 1);
            }
            if (!cpu2CantBop){
            CPU2.GetComponent<Animator>().DoScaledAnimationAsync("bop",0.5f, 0, 1);
            }
        }

        public void YeahLetsGo(double beat, bool voiceOn)
        {
            if(voiceOn)
            {
                BeatAction.New(instance, new List<BeatAction.Action>(){
                    new BeatAction.Action(beat, delegate {SoundByte.PlayOneShotGame("airboarder/start1");}),
                    new BeatAction.Action(beat + 6.5, delegate {SoundByte.PlayOneShotGame("airboarder/start2");}),
                });
            }
            BeatAction.New(instance, new List<BeatAction.Action>(){
                new BeatAction.Action(beat, delegate {CPU1.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f, 0, 1);}),
                new BeatAction.Action(beat, delegate {CPU2.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f, 0, 1);}),
                new BeatAction.Action(beat, delegate {Player.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f, 0, 1);})
            }

            );
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