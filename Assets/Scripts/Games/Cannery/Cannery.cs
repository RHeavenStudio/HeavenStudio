using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class MobCanneryLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("cannery", "The Cannery", "554899", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { Cannery.instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["auto"], eventCaller.currentEntity["toggle"]); },
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Toggle if the alarm should bop for the duration of this event."),
                        new Param("auto", false, "Bop (Auto)", "Toggle if Dog Ninja should automatically bop until another Bop event is reached."),
                    }
                },
                new GameAction("can", "Can")
                {
                    preFunction = delegate {
                        Cannery.CanSFX(eventCaller.currentEntity.beat);
                        if (GameManager.instance.currentGame == "cannery") {
                            Cannery.instance.SendCan(eventCaller.currentEntity.beat);
                        }
                    },
                    defaultLength = 2,
                },
                new GameAction("blackout", "Blackout")
                {
                    function = delegate { Cannery.instance.Blackout(); },
                    defaultLength = 0.5f,
                },
                new GameAction("backgroundColor", "Background Color")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        Cannery.instance.BackgroundColorChange(e.beat, e.length, e["startColor"], e["endColor"], e["ease"]);
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("startColor", new Color(1, 1, 1), "Start Color", "Set the color at the start of the event."),
                        new Param("endColor",   new Color(1, 1, 1), "End Color",   "Set the color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Instant, "Ease", "Set the easing of the action."),
                    },
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
                        new Param("startColor", new Color(0.8627f, 0.3725f, 0.0313f), "Start Color", "Set the color at the start of the event.", new() { new((_, _) => true, new string[] { "startColor" }) }),
                        new Param("endColor",   new Color(0.8627f, 0.3725f, 0.0313f), "End Color",   "Set the color at the end of the event."),
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
        [SerializeField] GameObject canGO;
        [SerializeField] GameObject blackout;
        [SerializeField] Material alarmMat;
        [SerializeField] SpriteRenderer bgPlaneSR;

        [Header("Animators")]
        [SerializeField] Animator conveyorBeltAnim;
        [SerializeField] Animator alarmAnim;
        public Animator dingAnim;
        public Animator cannerAnim;

        private ColorEase bgColorEase;
        private ColorEase alarmColorEase;

        private bool alarmBop = true;

        // public static Cannery instance;

        private void Awake()
        {
            // instance = this;
            canGO.SetActive(false);
        }

        private void Update()
        {
            conveyorBeltAnim.DoNormalizedAnimation("Move", (Conductor.instance.songPositionInBeats / 2) % 1);

            bgPlaneSR.color = GetNewColor(bgColorEase);
            alarmMat.SetColor("_ColorAlpha", GetNewColor(alarmColorEase));
        }

        public override void OnGameSwitch(double beat)
        {
            List<RiqEntity> events = GameManager.instance.Beatmap.Entities.FindAll(e => e.datamodel.Split('/')[0] == "cannery");
            List<RiqEntity> cans = events.FindAll(e => e.datamodel == "cannery/can" && beat > e.beat && beat < e.beat + 1);
            foreach (var can in cans) {
                SendCan(can.beat);
            }
            RiqEntity bgEvent = events.FindLast(e => e.datamodel == "cannery/backgroundColor" && e.beat < beat);
            if (bgEvent != null) {
                var e = bgEvent;
                BackgroundColorChange(e.beat, e.length, e["startColor"], e["endColor"], e["ease"]);
            } else {
                BackgroundColorChange(0, 0, new Color(0.8627f, 0.3725f, 0.0313f), new Color(0.8627f, 0.3725f, 0.0313f), 0);
            }
            RiqEntity alarmEvent = events.FindLast(e => e.datamodel == "cannery/alarmColor" && e.beat < beat);
            if (alarmEvent != null) {
                var e = alarmEvent;
                BackgroundColorChange(e.beat, e.length, e["startColor"], e["endColor"], e["ease"]);
            } else {
                BackgroundColorChange(0, 0, Color.white, Color.white, 0);
            }
        }

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
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
            alarmColorEase = new(beat, length, startColor, endColor, ease);
        }

        public void BackgroundColorChange(double beat, float length, Color startColor, Color endColor, int ease)
        {
            bgColorEase = new(beat, length, startColor, endColor, ease);
        }

        public static void CanSFX(double beat)
        {
            SoundByte.PlayOneShotGame("cannery/ding", beat);
        }

        public void SendCan(double beat)
        {
            // do the ding animation on the beat
            BeatAction.New(this, new() { new(beat, delegate { dingAnim.DoScaledAnimationFromBeatAsync("Ding", 0.5f, beat); }) });

            Can newCan = Instantiate(canGO, transform).GetComponent<Can>();
            newCan.game = this;
            newCan.startBeat = beat;
            newCan.gameObject.SetActive(true);
        }

        public void Blackout()
        {
            blackout.SetActive(!blackout.activeSelf);
        }
    }
}
