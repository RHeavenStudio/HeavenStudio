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
                    function = delegate {Airboarder.instance.RequestArch(eventCaller.currentEntity.beat);},
                    defaultLength = 4f,
                    resizable = false
                },

                new GameAction("crouch", "Charged Duck")
                {
                    function = delegate {Airboarder.instance.CueCrouch(eventCaller.currentEntity.beat);},
                    defaultLength = 4f,
                    resizable = false
                },

                new GameAction("jump", "Jump")
                {
                    function = delegate {Airboarder.instance.CueJump(eventCaller.currentEntity.beat, eventCaller.currentEntity["ready"]);},
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

        public struct Requesters
        {
            public bool wantsArch;
            public bool wantsArchCrouch;
            public bool wantsWall;
        }

        [Header("Objects")]
        [SerializeField] Arch archBasic;

        [Header("Animators")]
        [SerializeField] Animator CPU1;
        [SerializeField] Animator CPU2;
        [SerializeField] public Animator Player;
        [SerializeField] Animator Dog;
        [SerializeField] Animator Floor;

        bool goBop;
        public bool cpu1CantBop = false;
        public bool cpu2CantBop = false;
        public bool playerCantBop = false;
        
        public double startBeat;
        public double switchBeat;
        public double endBeat;
        public double maxValue;

        public float startFloor;

        private void Awake()
        {
            instance = this;
            SetupBopRegion("airboarder", "bop", "auto");
            
            
            
            
        }

        public override void OnGameSwitch(double beat)
        {
            endBeat = maxValue;
            var entities = GameManager.instance.Beatmap.Entities;
            //find when the next game switch/remix end happens
            RiqEntity firstEnd = entities.Find(c => c.datamodel is "gameManager/switchGame/airboarder" or "gameManager/end" && c.beat > beat);
            endBeat = firstEnd?.beat ?? maxValue;
            
            //lists arch and wall events
            List<RiqEntity> archEvents = EventCaller.GetAllInGameManagerList("airboarder", new string[] { "duck"});
            List<RiqEntity> crouchEvents = EventCaller.GetAllInGameManagerList("airboarder", new string[] { "crouch"});


            //spawns an arch for each arch event
            for (int i = 0; i <archEvents.Count; i++){
                var archBeat = archEvents[i].beat;

                if (startBeat <= archBeat - 28){
                        
                            var targetArchBeat = archBeat - 28f;
                            if (switchBeat <= targetArchBeat){
                                RequestArch(targetArchBeat);
                                
                            }
                        
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
            
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, 2f);
            Floor.Play("moving", -1, normalizedBeat);
            Floor.speed = 0;
            
          
            if (cond.isPlaying && !cond.isPaused){
                if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress)){
                Player.GetComponent<Animator>().DoScaledAnimationAsync("duck",1f);}
                if (PlayerInput.GetIsAction(InputAction_BasicRelease) && !IsExpectingInputNow(InputAction_BasicRelease)){
                Player.GetComponent<Animator>().DoScaledAnimationAsync("hold",1f);
                playerCantBop = false;}
                if (PlayerInput.GetIsAction(InputAction_FlickRelease) && !IsExpectingInputNow(InputAction_FlickRelease)){
                Player.GetComponent<Animator>().DoScaledAnimationAsync("hold",1f);
                playerCantBop = false;}
                              

            }

        }

        public void FloorMovement (double beat)
        {
            
        }




        public override void OnBeatPulse(double beat)
        {
            
            if (goBop)
            {
                Bop();
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
            Player.GetComponent<Animator>().DoScaledAnimationAsync("bop",0.5f);
            }
            if (!cpu1CantBop){
            CPU1.GetComponent<Animator>().DoScaledAnimationAsync("bop",0.5f);
            }
            if (!cpu2CantBop){
            CPU2.GetComponent<Animator>().DoScaledAnimationAsync("bop",0.5f);
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
                new BeatAction.Action(beat, delegate {CPU1.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);}),
                new BeatAction.Action(beat, delegate {CPU2.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);}),
                new BeatAction.Action(beat, delegate {Player.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);})
            }

            );
        }

        public void RequestArch(double beat)
        {
            Arch newArch = Instantiate(archBasic);
            newArch.appearBeat = beat;
            newArch.gameObject.SetActive(true);
            
        }



        public void CueDuck(double beat)
        {
            
            BeatAction.New(instance, new List<BeatAction.Action>() {
                
                new BeatAction.Action(beat, delegate {cpu1CantBop = true;} ),  
                new BeatAction.Action(beat, delegate {CPU1.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);}),
                new BeatAction.Action(beat, delegate {SoundByte.PlayOneShotGame("airboarder/ready");}),
                new BeatAction.Action(beat + 1, delegate {cpu2CantBop = true;} ),
                new BeatAction.Action(beat+1, delegate {CPU1.GetComponent<Animator>().DoScaledAnimationAsync("duck", 1f);}),
                new BeatAction.Action(beat+1, delegate {CPU2.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);}),                
                new BeatAction.Action(beat + 1, delegate {SoundByte.PlayOneShotGame("airboarder/crouch");}),
                new BeatAction.Action(beat+2, delegate {playerCantBop = true;} ),
                new BeatAction.Action(beat+2, delegate {CPU2.GetComponent<Animator>().DoScaledAnimationAsync("duck", 1f);}),                
                new BeatAction.Action(beat+2, delegate {Player.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);}),                
                new BeatAction.Action(beat + 2, delegate {SoundByte.PlayOneShotGame("airboarder/crouch");}),
                new BeatAction.Action(beat+2.5, delegate {cpu1CantBop = false;} ),
                new BeatAction.Action(beat+3.5, delegate {cpu2CantBop = false;} ),
                new BeatAction.Action(beat+4.5, delegate {playerCantBop = false;} ),
            });

        }

        public void CueCrouch(double beat)
        {

            BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate {cpu1CantBop = true;}),
                new BeatAction.Action(beat, delegate {CPU1.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);}),
                new BeatAction.Action(beat, delegate {SoundByte.PlayOneShotGame("airboarder/ready");}),

                new BeatAction.Action(beat+1, delegate {cpu2CantBop = true;} ),  
                new BeatAction.Action(beat + 1, delegate {SoundByte.PlayOneShotGame("airboarder/crouchCharge");}),
                new BeatAction.Action(beat+1, delegate {CPU1.GetComponent<Animator>().DoScaledAnimationAsync("charge", 1f);}),
                new BeatAction.Action(beat+1, delegate {CPU2.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);}), 

                            
                new BeatAction.Action(beat+2, delegate {CPU2.GetComponent<Animator>().DoScaledAnimationAsync("charge", 1f);}),                
                new BeatAction.Action(beat+2, delegate {Player.GetComponent<Animator>().DoScaledAnimationAsync("letsgo", 1f);}),                
                new BeatAction.Action(beat + 2, delegate {SoundByte.PlayOneShotGame("airboarder/crouchCharge");})
            });

        }

        public void CueJump(double beat, bool readySound)
        {
            ScheduleInput(beat, 3f, InputAction_FlickRelease, JumpSuccess, JumphMiss, JumpEmpty);
            if (readySound)
            {
                SoundByte.PlayOneShotGame("airboarder/ready");
            }

            BeatAction.New(instance, new List<BeatAction.Action>() {

                new BeatAction.Action(beat+1, delegate {CPU1.GetComponent<Animator>().DoScaledAnimationAsync("jump", 1f);}),
                new BeatAction.Action(beat+1, delegate {cpu1CantBop = false;} ),              
                new BeatAction.Action(beat + 1, delegate {SoundByte.PlayOneShotGame("airboarder/jump");}),
                new BeatAction.Action(beat+2, delegate {CPU2.GetComponent<Animator>().DoScaledAnimationAsync("jump", 1f);}),                
                new BeatAction.Action(beat+2, delegate {cpu2CantBop = false;} ),  
                new BeatAction.Action(beat + 2, delegate {SoundByte.PlayOneShotGame("airboarder/jump");}),
            });

        }


        public void DuckSuccess(double beat)
        {
            Player.GetComponent<Animator>().DoScaledAnimationAsync("duck", 1f);
            SoundByte.PlayOneShotGame("airboarder/crouch");
        }

        public void DuckMiss(double beat){
            Player.GetComponent<Animator>().DoScaledAnimationAsync("hit1",1f);
        }

        public void DuckEmpty(double beat){
            Player.GetComponent<Animator>().DoScaledAnimationAsync("hit2", 1f);
        }

        public void CrouchSuccess(double beat)
        {
            Player.GetComponent<Animator>().DoScaledAnimationAsync("charge", 1f);
            SoundByte.PlayOneShotGame("airboarder/crouchCharge");
            playerCantBop = true;
        }

        public void CrouchMiss(double beat){
            Player.GetComponent<Animator>().DoScaledAnimationAsync("hit1",1f);
        }

        public void CrouchEmpty(double beat){
            Player.GetComponent<Animator>().DoScaledAnimationAsync("hit2", 1f);
        }

        public void JumpSuccess(PlayerActionEvent caller, float state)
        {
            Player.GetComponent<Animator>().DoScaledAnimationAsync("jump", 1f);
            SoundByte.PlayOneShotGame("airboarder/jump");
            playerCantBop = false;
        }

        public void JumphMiss(PlayerActionEvent caller){
            Player.GetComponent<Animator>().DoScaledAnimationAsync("hit1",1f);
            playerCantBop = false;
        }

        public void JumpEmpty(PlayerActionEvent caller){
            Player.GetComponent<Animator>().DoScaledAnimationAsync("hit2", 1f);
            playerCantBop = false;
        }
            


    }
}