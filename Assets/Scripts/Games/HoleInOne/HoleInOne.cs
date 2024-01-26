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
    public static class RvlHoleInOneLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("holeInOne", "Hole in One", "6ab99e", false, false, new List<GameAction>()
            {
                new GameAction("testanims", "Test Animation")
                {
                    function = delegate { HoleInOne.instance.DoTestAnim(eventCaller.currentEntity.beat); },
                },
                new GameAction("mandrill", "Mandrill (no visuals)")
                {
                    function = delegate { HoleInOne.instance.DoMandrill(eventCaller.currentEntity.beat); },
                    defaultLength = 4.0f,
                },
                new GameAction("monkey", "Monkey (sound only)")
                {
                    function = delegate { HoleInOne.instance.DoMonkey(eventCaller.currentEntity.beat); },
                    defaultLength = 3.0f,
                }
            }//,
            // new List<string>() { "rvl", "normal" },
            // "rvlgolf", "en",
            // new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class HoleInOne : Minigame
    {
        public Animator MonkeyAnimator;
        public GameObject Monkey;

        public static HoleInOne instance;

        // Start is called before the first frame update
        void Awake()
        {
            HoleInOne.instance = this;
            MonkeyAnimator = Monkey.GetComponent<Animator>();
        }

        public void DoTestAnim(double beat)
        {
            SoundByte.PlayOneShotGame("holeInOne/whale");
            MonkeyAnimator.Play("MonkeySpin");
        }

        public void DoMandrill(double beat)
        {
            //Mandrill Multisound
            ScheduleInput(beat, 3f, InputAction_BasicPress, CatchSuccess, CatchSuccess1, CatchSuccess2);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("holeInOne/mandrill1", beat),
                new MultiSound.Sound("holeInOne/mandrill2", beat + 1f),
                new MultiSound.Sound("holeInOne/mandrill3", beat + 2f),
                new MultiSound.Sound("holeInOne/mandrill4", beat + 3f),
            });

            // BeatAction.New(instance, new List<BeatAction.Action>()
            //     {
            //     new BeatAction.Action(beat,     delegate { FarCrane.DoScaledAnimationAsync("Drop", 0.5f);}),
            //     new BeatAction.Action(beat + 1.0f,     delegate { FarCrane.DoScaledAnimationAsync("Open", 0.5f);}),
            //     new BeatAction.Action(beat + 1.5f,     delegate { FarCrane.DoScaledAnimationAsync("Lift", 0.5f);}),
            //     });

        }

        public void DoMonkey(double beat)
        {
            //Monkey Multisound
            ScheduleInput(beat, 2f, InputAction_BasicPress, CatchSuccess, CatchSuccess1, CatchSuccess2);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("holeInOne/monkey1", beat),
                new MultiSound.Sound("holeInOne/monkey2", beat + 1f),
                new MultiSound.Sound("holeInOne/hole1", beat + 2f),
            });

            // BeatAction.New(instance, new List<BeatAction.Action>()
            //     {
            //     new BeatAction.Action(beat,     delegate { FarCrane.DoScaledAnimationAsync("Drop", 0.5f);}),
            //     new BeatAction.Action(beat + 1.0f,     delegate { FarCrane.DoScaledAnimationAsync("Open", 0.5f);}),
            //     new BeatAction.Action(beat + 1.5f,     delegate { FarCrane.DoScaledAnimationAsync("Lift", 0.5f);}),
            //     });

        }

        public void CatchSuccess(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("holeInOne/whale");
        }

        public void CatchSuccess1(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("holeInOne/whale");
        }

        public void CatchSuccess2(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("holeInOne/whale");
        }
    }
}