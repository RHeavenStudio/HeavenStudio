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
    public static class AgbBonOdoriLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("bonOdori", "The☆Bon Odori \n<color=#adadad>(Za☆Bon Odori)</color>", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("pan", "Pan")
                {
                    function = delegate { BonOdori.instance.Clap(eventCaller.currentEntity.beat); },
                    defaultLength = 2f,
                }

            
            
            });

        }  

    }
        
};


namespace HeavenStudio.Games
{



    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class BonOdori : Minigame
    {
        
        [Header("Animators")]
        public Animator AnimatorGeneral;
        public static BonOdori instance { get; set; }
        public void Awake()
        {

        }
        public void Clap(double beat)
        {   
            ScheduleInput(beat, 1f, InputAction_BasicPress, CatchSuccess, CatchMiss, CatchEmpty);
            AnimatorGeneral.Play("ClapGeneral", 0, 0);
            new MultiSound.Sound("coinToss/cowbell2", 1f, offset: 0.01f);

        }

        public void CatchSuccess(PlayerActionEvent caller, float state)
        {
        AnimatorGeneral.Play("ClapGeneral", 0, 0);
        new MultiSound.Sound("coinToss/cowbell2", 1f, offset: 0.01f);
        }
        
        public void CatchMiss(PlayerActionEvent caller)
        {
        AnimatorGeneral.Play("ClapGeneral", 0, 0);
        new MultiSound.Sound("coinToss/cowbell2", 1f, offset: 0.01f);
        }
        
        
        public void CatchEmpty(PlayerActionEvent caller)
        {
        AnimatorGeneral.Play("ClapGeneral", 0, 0);
        new MultiSound.Sound("coinToss/cowbell2", 1f, offset: 0.01f);
        }
        



    }
}

