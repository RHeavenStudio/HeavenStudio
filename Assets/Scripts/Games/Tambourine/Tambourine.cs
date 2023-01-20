using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlTambourineLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tambourine", "Tambourine \n<color=#eb5454>[WIP]</color>", "812021", false, false, new List<GameAction>()
            {
                new GameAction("beat intervals", "Start Interval")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.StartInterval(e.beat, e.length); },
                    defaultLength = 4f,
                    resizable = true,
                    priority = 1
                },
                new GameAction("shake", "Shake")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.Shake(e.beat); },
                    defaultLength = 0.5f,
                    priority = 2
                },
                new GameAction("hit", "Hit")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.Hit(e.beat); },
                    defaultLength = 0.5f,
                    priority = 2
                },
                new GameAction("pass turn", "Pass Turn")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.PassTurn(e.beat, e.length); },
                    defaultLength = 0.5f,
                    resizable = true,
                    priority = 1
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.Bop(e.beat, e["whoBops"]); },
                    parameters = new List<Param>()
                    {
                        new Param("whoBops", Tambourine.WhoBops.Both, "Who Bops", "Who will bop."),
                    },
                    defaultLength = 1f,
                    priority = 3
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_Tambourine;
    public class Tambourine : Minigame
    {
        [Header("Components")]
        [SerializeField] Animator handsAnimator;
        [SerializeField] Animator monkeyAnimator;

        public enum WhoBops
        {
            Monkey,
            Player,
            Both
        }

        public static Tambourine instance;

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                handsAnimator.Play("Shake", 0, 0);
                Jukebox.PlayOneShotGame($"tambourine/player/shake/{UnityEngine.Random.Range(1, 6)}");
            }
            else if (PlayerInput.AltPressed() && !IsExpectingInputNow(InputType.STANDARD_ALT_DOWN))
            {
                handsAnimator.Play("Smack", 0, 0);
                Jukebox.PlayOneShotGame($"tambourine/player/hit/{UnityEngine.Random.Range(1, 6)}");
            }
        }

        public void StartInterval(float beat, float interval)
        {

        }

        public void Shake(float beat)
        {
            monkeyAnimator.Play("MonkeyShake", 0, 0);
            Jukebox.PlayOneShotGame($"tambourine/monkey/shake/{UnityEngine.Random.Range(1, 6)}");
        }

        public void Hit(float beat)
        {
            monkeyAnimator.Play("MonkeySmack", 0, 0);
            Jukebox.PlayOneShotGame($"tambourine/monkey/hit/{UnityEngine.Random.Range(1, 6)}");
        }

        public void PassTurn(float beat, float length)
        {
            monkeyAnimator.Play("MonkeyPassTurn", 0, 0);
            Jukebox.PlayOneShotGame($"tambourine/monkey/turnPass/{UnityEngine.Random.Range(1, 6)}");
        }

        public void Bop(float beat, int whoBops)
        {
            switch (whoBops)
            {
                case (int) WhoBops.Monkey:
                    monkeyAnimator.Play("MonkeyBop", 0, 0);
                    break;
                case (int) WhoBops.Player:
                    handsAnimator.Play("Bop", 0, 0);
                    break;
                case (int) WhoBops.Both:
                    monkeyAnimator.Play("MonkeyBop", 0, 0);
                    handsAnimator.Play("Bop", 0, 0);
                    break;
            }
        }
    }
}