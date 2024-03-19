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
    public static class RvlBuiltLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("builtToScaleRvl", "Built To Scale (Wii)", "1ad21a", false, false, new List<GameAction>()
            {
                new GameAction("throw rod", "Throw Rod")
                {
                    function = delegate { var e = eventCaller.currentEntity; BuiltToScaleRvl.instance.ThrowRod(e.beat, e.length, e["direction"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("direction", BuiltToScaleRvl.Direction.Left, "Direction", "Set the direction in which the stick will come out.")
                    },
                },
                new GameAction("shoot rod", "Shoot Rod")
                {
                    function = delegate { var e = eventCaller.currentEntity; BuiltToScaleRvl.instance.ShootRod(e.beat); },
                    defaultLength = 1f,
                },
            }, new List<string>() { "rvl", "normal" }, "rvlbuilt", "en", new List<string>() { });
        }
    }
}

namespace HeavenStudio.Games
{
    public class BuiltToScaleRvl : Minigame
    {
        public enum Direction {
            Left,
            Right,
        }

        public static BuiltToScaleRvl instance;

        const int IAAltDownCat = IAMAXCAT;

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }

        public static PlayerInput.InputAction InputAction_FlickAltPress =
            new("NtrBuiltAltFlickAltPress", new int[] { IAAltDownCat, IAFlickCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchFlick, IA_BatonAltPress);
        

        private void Awake()
        {
            instance = this;
        }

        public void ThrowRod(double beat, double length, int direction)
        {
            if (GameManager.instance.currentGame == "builtToScaleRvl")
            {
                SoundByte.PlayOneShotGame("builtToScaleRvl/left", beat);
            }
            ScheduleInput(beat, length, InputAction_BasicPress, BounceOnHit, BounceOnMiss, Empty);
        }

        public void ShootRod(double beat)
        {
            SoundByte.PlayOneShotGame("builtToScaleRvl/prepare", beat);
            ScheduleInput(beat, 2f, InputAction_FlickAltPress, ShootOnHit, ShootOnMiss, Empty);
        }

        public void BounceOnHit(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("builtToScaleRvl/middleRight");
        }
        public void BounceOnMiss(PlayerActionEvent caller)
        {
            
        }

        public void ShootOnHit(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("builtToScaleRvl/shoot");
        }
        public void ShootOnMiss(PlayerActionEvent caller)
        {
            
        }

        public void Empty(PlayerActionEvent caller) {}
    }
}