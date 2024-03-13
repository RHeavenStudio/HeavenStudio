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
    public static class RvlCatchOfTheDayLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("catchOfTheDay", "Catch of the Day", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("fish1", "Quicknibble")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish01(e.beat); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                    },
                },
                new GameAction("fish2", "Pausegill")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish02(e.beat, e["countIn"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("countIn", false, "Count-In", "Play the \"And Go!\" sound effect as a count in to the cue."),
                    },
                },
                new GameAction("fish3", "Threefish")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish03(e.beat, e["countIn"]); },
                    defaultLength = 5.5f,
                    parameters = new List<Param>()
                    {
                        new Param("countIn", false, "Count-In", "Play the \"One Two Three Go!\" sound effect as a count in to the cue."),
                    },
                },
            },
            new List<string>() {"rvl", "normal"},
            "rvlfishing", "en"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class CatchOfTheDay : Minigame
    {
        public static void Dummy()
        {
            // TODO REMOVE ME BEFORE PUBLISH I AM JUST A PLACEHOLDER
        }

        public static void Cue_Fish01(double beat)
        {
            MultiSound.Play(new MultiSound.Sound[]{
                new MultiSound.Sound("catchOfTheDay/quick1", beat),
            });

            SoundByte.PlayOneShotGame("catchOfTheDay/quick1", beat, forcePlay: true);
            SoundByte.PlayOneShotGame("catchOfTheDay/quick2", beat + 1, forcePlay: true);
        }
        public static void Cue_Fish02(double beat, bool countIn)
        {
            SoundByte.PlayOneShotGame("catchOfTheDay/pausegill1", beat, forcePlay: true);
            SoundByte.PlayOneShotGame("catchOfTheDay/pausegill2", beat + 0.5, forcePlay: true);
            SoundByte.PlayOneShotGame("catchOfTheDay/pausegill3", beat + 1, forcePlay: true);
            if (countIn)
            {
                SoundByte.PlayOneShot("count-ins/and", beat + 2);
                SoundByte.PlayOneShot("count-ins/go1", beat + 3);
            }
        }
        public static void Cue_Fish03(double beat, bool countIn)
        {
            MultiSound.Play(new MultiSound.Sound[]{
                new MultiSound.Sound("catchOfTheDay/threefish1", beat),
                new MultiSound.Sound("catchOfTheDay/threefish2", beat + 0.25),
                new MultiSound.Sound("catchOfTheDay/threefish3", beat + 0.5),
                new MultiSound.Sound("catchOfTheDay/threefish4", beat + 1)
            }, forcePlay: true);
            if (countIn)
            {
                MultiSound.Play(new MultiSound.Sound[]{
                    new MultiSound.Sound("count-ins/one1", beat + 2),
                    new MultiSound.Sound("count-ins/two1", beat + 3),
                    new MultiSound.Sound("count-ins/three1", beat + 4),
                    new MultiSound.Sound("count-ins/go1", beat + 4.5),
                }, forcePlay: true, game: false);
            }
        }
    }
}