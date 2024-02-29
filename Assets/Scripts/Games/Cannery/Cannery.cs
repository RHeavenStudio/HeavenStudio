using System;
using System.Collections.Generic;

using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class PcoCanneryLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("cannery", "The Cannery", "554899", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out Cannery instance)) {
                            instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["auto"], eventCaller.currentEntity["toggle"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Toggle if the alarm should bop for the duration of this event."),
                        new Param("auto", false, "Bop (Auto)", "Toggle if Dog Ninja should automatically bop until another Bop event is reached."),
                    }
                },
                new GameAction("can", "Can")
                {
                    preFunction = delegate {
                        SoundByte.PlayOneShotGame("cannery/ding", eventCaller.currentEntity.beat, forcePlay: true);
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out Cannery instance)) {
                            instance.SendCan(eventCaller.currentEntity.beat);
                        }
                    },
                    defaultLength = 2,
                },
                new GameAction("blackout", "Blackout")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out Cannery instance)) {
                            instance.Blackout();
                        }
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("alarmColor", "Alarm Color")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out Cannery instance)) {
                            instance.AlarmColor(e.beat, e.length, e["startColor"], e["endColor"], e["ease"]);
                        }
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("startColor", Cannery.alarmColor , "Start Color", "Set the color at the start of the event.", new() { new((_, _) => true, new string[] { "startColor" }) }),
                        new Param("endColor",   Cannery.alarmColor, "End Color",   "Set the color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Instant, "Ease", "Set the easing of the action."),
                    }
                },
            }
            // ,
            // new List<string>() { "mob", "normal" },
            // "mobcannery", "en",
            // new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_Cannery;
    public class Cannery : Minigame
    {
        [Header("Objects")]
        [SerializeField] Can can;
        [SerializeField] GameObject blackout;
        [SerializeField] Material alarmMat;
        [SerializeField] SpriteRenderer bgPlaneSR;

        [Header("Animators")]
        [SerializeField] Animator conveyorBeltAnim;
        [SerializeField] Animator alarmAnim;
        public Animator dingAnim;
        public Animator cannerAnim;

        // private ColorEase bgColorEase = new(Color.white);
        public static readonly Color alarmColor = new Color(0.8627f, 0.3725f, 0.0313f);
        public double aStartBeat;
        public float aLength;
        public Color aStartColor, aEndColor;
        public Util.EasingFunction.Ease aEase = Util.EasingFunction.Ease.Instant;
        public Util.EasingFunction.Function aEaseFunc;

        private bool alarmBop = true;

        private void Awake()
        {
            can.gameObject.SetActive(false);
            aStartColor = aEndColor = alarmColor;
        }

        private void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress)) {
                cannerAnim.DoScaledAnimationAsync("CanEmpty", 0.5f);
                SoundByte.PlayOneShotGame("cannery/can");
                SoundByte.PlayOneShot("nearMiss");
            }
            conveyorBeltAnim.DoNormalizedAnimation("Move", (conductor.songPositionInBeats / 2) % 1);

            float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(conductor.songPositionInBeatsAsDouble, aLength));

            Color newColor = alarmColor;
            if (!double.IsNaN(normalizedBeat)) {
                aEaseFunc ??= Util.EasingFunction.GetEasingFunction(aEase);
                float newR = aEaseFunc(aStartColor.r, aEndColor.r, normalizedBeat);
                float newG = aEaseFunc(aStartColor.g, aEndColor.g, normalizedBeat);
                float newB = aEaseFunc(aStartColor.b, aEndColor.b, normalizedBeat);
                newColor = new Color(newR, newG, newB);
            }

            alarmMat.SetColor("_ColorAlpha", newColor);
        }

        public override void OnPlay(double beat) => OnGameSwitch(beat);
        public override void OnStop(double beat) => OnGameSwitch(beat);
        public override void OnGameSwitch(double beat)
        {
            List<RiqEntity> events = GameManager.instance.Beatmap.Entities.FindAll(e => e.datamodel.Split('/')[0] == "cannery");
            List<RiqEntity> cans = events.FindAll(e => e.datamodel == "cannery/can" && beat > e.beat && beat < e.beat + 1);
            foreach (var can in cans) {
                SendCan(can.beat);
            }

            RiqEntity alarmEvent = events.FindLast(e => e.datamodel == "cannery/alarmColor" && e.beat < beat);
            if (alarmEvent != null) {
                var e = alarmEvent;
                AlarmColor(e.beat, e.length, e["startColor"], e["endColor"], e["ease"]);
            } else {
                AlarmColor(0, 0, alarmColor, alarmColor, (int)Util.EasingFunction.Ease.Instant);
            }
        }

        public override void OnLateBeatPulse(double beat)
        {
            if (alarmBop) {
                alarmAnim.DoScaledAnimationAsync("Bop", 0.5f);
            }
        }

        public void Bop(double beat, float length, bool auto, bool bop)
        {
            alarmBop = auto;
            if (bop) {
                List<BeatAction.Action> actions = new();
                for (int i = 0; i < length; i++) {
                    actions.Add(new(beat + i, delegate { alarmAnim.DoScaledAnimationAsync("Bop", 0.5f); }));
                }
                if (actions.Count > 0) BeatAction.New(this, actions);
            }
        }

        public void AlarmColor(double beat, float length, Color startColor, Color endColor, int ease)
        {
            aStartBeat = beat;
            aLength = length;
            aStartColor = startColor;
            aEndColor = endColor;
            aEase = (Util.EasingFunction.Ease)ease;
            aEaseFunc = Util.EasingFunction.GetEasingFunction(aEase);
        }

        public void SendCan(double beat)
        {
            // do the ding animation on the beat
            BeatAction.New(this, new() { new(beat, delegate { dingAnim.DoScaledAnimationAsync("Ding", 0.5f); }) });

            Can newCan = Instantiate(can, transform);
            newCan.startBeat = beat;
            newCan.gameObject.SetActive(true);
        }

        public void Blackout()
        {
            blackout.SetActive(!blackout.activeSelf);
        }
    }
}
