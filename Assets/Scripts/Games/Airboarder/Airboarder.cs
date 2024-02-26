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
            return new Minigame("airboarder", "Airboarder", "ffffff", false, false, new List<GameAction>()
            {

                new GameAction("bop", "Bop")
                {
                    function = delegate {Airboarder.instance.Bop(eventCaller.currentEntity.beat);},
                    defaultLength = 1f,
                    resizable = false
                },

                new GameAction("duck", "Duck")
                {
                    function = delegate {Airboarder.instance.PreCueDuck(eventCaller.currentEntity.beat);},
                    defaultLength = 4f,
                    resizable = false
                },


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


    public class Airboarder : Minigame
    {
        

        public static Airboarder instance;
        

        private void Awake()
        {
            instance = this;
        }

        public void Bop(double beat)
        {}

        public void Update()
        {
            var cond = Conductor.instance;
            var currentBeat = cond.songPositionInBeatsAsDouble;

        }

        public void PreCueDuck(double beat)
        {
            CueDuck(beat);
        }    

        public void CueDuck(double beat)
        {
            ScheduleInput(beat, 3f, InputAction_BasicPress, DuckSuccess, DuckMiss, DuckEmpty);
            BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate {SoundByte.PlayOneShotGame("airboarder/ready");}),
                new BeatAction.Action(beat + 1, delegate {SoundByte.PlayOneShotGame("airboarder/crouch");}),
                new BeatAction.Action(beat + 2, delegate {SoundByte.PlayOneShotGame("airboarder/crouch");})
            });

        }

        public void DuckSuccess(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("airboarder/crouch");
        }

        public void DuckMiss(PlayerActionEvent caller){}

        public void DuckEmpty(PlayerActionEvent caller){}


    }
}