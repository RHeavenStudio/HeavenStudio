using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbTramLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tramAndPauline", "Tram & Pauline", "adb5e7", false, false, new List<GameAction>()
            {
                new GameAction("prepare", "Prepare")
                {
                    function = delegate { TramAndPauline.instance.Prepare(eventCaller.currentEntity.beat, (TramAndPauline.TramOrPauline)eventCaller.currentEntity["who"]); },
                    parameters = new List<Param>()
                    {
                        new Param("who", TramAndPauline.TramOrPauline.Pauline, "Who Prepares?")
                    }
                },
                new GameAction("pauline", "Pauline")
                {
                    function = delegate { TramAndPauline.instance.Jump(eventCaller.currentEntity.beat, TramAndPauline.TramOrPauline.Pauline, eventCaller.currentEntity["toggle"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Audience Reaction")
                    }
                },
                new GameAction("tram", "Tram")
                {
                    function = delegate { TramAndPauline.instance.Jump(eventCaller.currentEntity.beat, TramAndPauline.TramOrPauline.Tram, eventCaller.currentEntity["toggle"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Audience Reaction")
                    }
                },
                new GameAction("shape", "Change Transformation")
                {
                    function = delegate 
                    {
                        var e = eventCaller.currentEntity;
                        TramAndPauline.instance.SetTransformation(e["tram"], e["pauline"]);
                    }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("pauline", true, "Pauline is a Fox?"),
                        new Param("tram", true, "Tram is a Fox?")
                    }
                },
                new GameAction("curtains", "Curtains")
                {
                    function = delegate 
                    { 
                        var e = eventCaller.currentEntity;
                        TramAndPauline.instance.SetCurtain(e.beat, e.length, e["ease"], e["toggle"]);
                    },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Going Up?"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease")
                    }
                }
            }
            );
        }
    }
}
namespace HeavenStudio.Games
{
    using Scripts_TramAndPauline;
    public class TramAndPauline : Minigame
    {
        public enum TramOrPauline
        {
            Pauline = 0,
            Tram = 1,
            Both = 2
        }

        public static TramAndPauline instance;

        [Header("Components")]
        [SerializeField] private AgbAnimalKid tram;
        [SerializeField] private AgbAnimalKid pauline;
        [SerializeField] private Animator curtainAnim;
        [SerializeField] private Animator audienceAnim;

        private double curtainBeat = -1;
        private float curtainLength = 0;
        private bool goingUp = true;
        private Util.EasingFunction.Ease curtainEase = Util.EasingFunction.Ease.Linear;

        private void Awake()
        {
            instance = this;
            Update();
        }

        private void Update()
        {
            float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(curtainBeat, curtainLength));

            var func = Util.EasingFunction.GetEasingFunction(curtainEase);

            float newPos = func(goingUp ? 1 : 0, goingUp ? 0 : 1, normalizedBeat);

            curtainAnim.DoNormalizedAnimation("Curtain", newPos);
        }

        public void SetCurtain(double beat, float length, int ease, bool goingUp2)
        {
            goingUp = goingUp2;
            curtainLength = length;
            curtainBeat = beat;
            curtainEase = (Util.EasingFunction.Ease)ease;
        }

        public void SetTransformation(bool tramB, bool paulineB)
        {
            tram.SetTransform(tramB);
            pauline.SetTransform(paulineB);
        }

        public override void OnGameSwitch(double beat)
        {
            PersistCurtain(beat);
            PersistTransformation(beat);
            PersistPrepare(beat);
        }

        public override void OnPlay(double beat)
        {
            PersistCurtain(beat);
            PersistTransformation(beat);
            PersistPrepare(beat);
        }

        private void PersistCurtain(double beat)
        {
            var lastEvent = EventCaller.GetAllInGameManagerList("tramAndPauline", new string[] { "curtains" }).FindLast(x => x.beat < beat);
            if (lastEvent != null)
            {
                SetCurtain(lastEvent.beat, lastEvent.length, lastEvent["ease"], lastEvent["toggle"]);
            }
        }

        private void PersistTransformation(double beat)
        {
            bool isFoxTram = true;
            bool isFoxPauline = true;

            double baseBeat = 0f;

            var lastEvent = EventCaller.GetAllInGameManagerList("tramAndPauline", new string[] { "shape" }).FindLast(x => x.beat < beat);
            if (lastEvent != null)
            {
                baseBeat = lastEvent.beat;
                isFoxTram = lastEvent["tram"];
                isFoxPauline = lastEvent["pauline"];
            }

            var allTramEvents = EventCaller.GetAllInGameManagerList("tramAndPauline", new string[] { "tram" }).FindAll(x => x.beat >= baseBeat && x.beat + 1 < beat);
            var allPaulineEvents = EventCaller.GetAllInGameManagerList("tramAndPauline", new string[] { "pauline" }).FindAll(x => x.beat >= baseBeat && x.beat + 1 < beat);

            if (allTramEvents.Count % 2 != 0) isFoxTram = !isFoxTram;
            if (allPaulineEvents.Count % 2 != 0) isFoxPauline = !isFoxPauline;

            SetTransformation(isFoxTram, isFoxPauline);
        }

        private void PersistPrepare(double beat)
        {
            var allEvents = EventCaller.GetAllInGameManagerList("tramAndPauline", new string[] { "prepare", "tram", "pauline" }).FindAll(x => x.beat < beat);
            if (allEvents.Count == 0) return;
            allEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
            var lastEvent = allEvents[allEvents.Count - 1];

            if (lastEvent != null && lastEvent.datamodel == "tramAndPauline/prepare")
            {
                Prepare(lastEvent.beat, (TramOrPauline)lastEvent["who"], true);
            }
        }

        public void Prepare(double beat, TramOrPauline who, bool instant = false)
        {
            switch (who)
            {
                case TramOrPauline.Pauline:
                    pauline.Prepare(beat, instant);
                    break;
                case TramOrPauline.Tram:
                    tram.Prepare(beat, instant);
                    break;
                case TramOrPauline.Both:
                    pauline.Prepare(beat, instant);
                    tram.Prepare(beat, instant);
                    break;
            }
        }

        public void Jump(double beat, TramOrPauline who, bool react)
        {
            switch (who)
            {
                case TramOrPauline.Pauline:
                    PaulineJump(beat, react);
                    break;
                case TramOrPauline.Tram:
                    TramJump(beat, react);
                    break;
                case TramOrPauline.Both:
                    PaulineJump(beat, react);
                    TramJump(beat, react);
                    break;
            }
        }

        private void TramJump(double beat, bool audienceReact)
        {
            SoundByte.PlayOneShotGame("tramAndPauline/jump" + UnityEngine.Random.Range(1, 3));
            tram.Jump(beat);
            ScheduleInput(beat, 1, InputType.DIRECTION_DOWN, audienceReact ? TramJustAudience : TramJust, Empty, Empty);
        }

        private void PaulineJump(double beat, bool audienceReact)
        {
            SoundByte.PlayOneShotGame("tramAndPauline/jump" + UnityEngine.Random.Range(1, 3));
            pauline.Jump(beat);
            ScheduleInput(beat, 1, InputType.STANDARD_DOWN, audienceReact ? PaulineJustAudience : PaulineJust, Empty, Empty);
        }

        private void TramJust(PlayerActionEvent caller, float state)
        {
            tram.Transform(state >= 1f || state <= -1f);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
                return;
            }
            SoundByte.PlayOneShotGame("tramAndPauline/transformTram");
        }

        private void PaulineJust(PlayerActionEvent caller, float state)
        {
            pauline.Transform(state >= 1f || state <= -1f);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
                return;
            }
            SoundByte.PlayOneShotGame("tramAndPauline/transformPauline");
        }

        private void TramJustAudience(PlayerActionEvent caller, float state)
        {
            tram.Transform(state >= 1f || state <= -1f);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
                return;
            }
            SoundByte.PlayOneShotGame("tramAndPauline/transformTram");
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 1, delegate
                {
                    audienceAnim.DoScaledAnimationAsync("Happy", 0.5f);
                })
            });

        }

        private void PaulineJustAudience(PlayerActionEvent caller, float state)
        {
            pauline.Transform(state >= 1f || state <= -1f);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
                return;
            }
            SoundByte.PlayOneShotGame("tramAndPauline/transformPauline");
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 1, delegate
                {
                    audienceAnim.DoScaledAnimationAsync("Happy", 0.5f);
                })
            });
        }

        private void Empty(PlayerActionEvent caller) { }
    }
}
