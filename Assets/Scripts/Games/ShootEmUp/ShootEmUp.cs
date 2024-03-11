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
    public static class NtrShootEmUpLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("shootEmUp", "Shoot-'Em-Up", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("start interval", "Start Interval")
                {
                    preFunction = delegate { RhythmTweezers.PreInterval(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }, 
                    defaultLength = 4f, 
                    resizable = true,
                },
                new GameAction("spawn enemy", "Spawn Enemy")
                {
                    defaultLength = 0.5f,
                    function = delegate {var e = eventCaller.currentEntity; ShootEmUp.instance.SpawnEnemy(e.beat, e["x"], e["y"]); },
                    parameters = new List<Param>()
                    {
                        new Param("x", new EntityTypes.Integer(-3, 3, 0), "X"),
                        new Param("y", new EntityTypes.Integer(-3, 3, 0), "Y"),
                    },
                },
            },
            new List<string>() { "ntr", "normal" }, "ntrShootEmUp", "en", new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_ShootEmUp;
    public class ShootEmUp : Minigame
    {
        public GameObject baseEnemy;
        public Transform enemyHolder;

        private struct QueuedInterval
        {
            public double beat;
            public float interval;
        }
        private static List<QueuedInterval> queuedIntervals = new List<QueuedInterval>();

        protected static bool IA_PadAnyDown(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }
        public static PlayerInput.InputAction InputAction_Press =
            new("AgbShootPress", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadAnyDown, IA_TouchBasicPress, IA_BatonBasicPress);

        public static ShootEmUp instance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }

        public void SpawnEnemy(double beat, int x, int y)
        {
            SoundByte.PlayOneShotGame("shootEmUp/spawn", beat);
            var newEnemy = Instantiate(baseEnemy, enemyHolder).GetComponent<Enemy>();
            newEnemy.targetBeat = beat;
            newEnemy.gameObject.transform.localPosition = new Vector3(5.05f*x/3, 2.5f*y/3 + 1.25f, 0);
            
            Vector3 angle = new Vector3(0, 0, 0);
            if (x > 0 && y > 0) {
                angle = new Vector3(0, 0, -70);
            } else if (x < 0 && y > 0) {
                angle = new Vector3(0, 0, 70);
            } else if (x > 0 && y < 0) {
                angle = new Vector3(0, 0, -110);
            } else if (x < 0 && y < 0) {
                angle = new Vector3(0, 0, 110);
            }
            newEnemy.SpawnEffect.eulerAngles = angle;

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    newEnemy.gameObject.SetActive(true);
                    newEnemy.GetComponent<Animator>().Play("enemySpawn", 0, 0);
                })
            });
        }

        public static void PreInterval(double beat, float interval = 4f, bool autoPassTurn = true)
        {
            if (GameManager.instance.currentGame == "shootEmUp")
            {
                // instance.SetIntervalStart(beat, beat, interval, autoPassTurn);
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval()
                {
                    beat = beat,
                    interval = interval,
                });
            }
        }
    }
}