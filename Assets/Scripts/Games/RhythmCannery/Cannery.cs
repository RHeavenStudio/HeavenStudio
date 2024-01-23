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

        [Header("Animators")]
        [SerializeField] Animator conveyorBeltAnim;
        [SerializeField] Animator alarmAnim;
        public Animator dingAnim;
        public Animator cannerAnim;

        private bool alarmBop = true;

        public static Cannery instance;

        void Awake()
        {
            instance = this;
            canGO.SetActive(false);
        }

        void Update()
        {
            conveyorBeltAnim.DoNormalizedAnimation("Move", (Conductor.instance.songPositionInBeats / 2) % 1);
        }

        public override void OnGameSwitch(double beat)
        {
            List<RiqEntity> cans = GameManager.instance.Beatmap.Entities.FindAll(e => e.datamodel == "cannery/can" && beat > e.beat && beat < e.beat + 1);
            foreach (var can in cans) {
                SendCan(can.beat);
            }
        }

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        public override void OnBeatPulse(double beat)
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

        public static void CanSFX(double beat)
        {
            SoundByte.PlayOneShotGame("cannery/ding", beat);
        }

        public void SendCan(double beat)
        {
            // dingAnim.DoScaledAnimationFromBeatAsync("Ding", 0.5f, beat);
            // do the ding animation on the beat
            BeatAction.New(this, new() { new(beat, delegate { dingAnim.DoScaledAnimationFromBeatAsync("Ding", 0.5f, beat); }) });

            Can newCan = Instantiate(canGO, transform).GetComponent<Can>();
            newCan.startBeat = beat;
            newCan.gameObject.SetActive(true);
        }

        public void Blackout()
        {
            blackout.SetActive(!blackout.activeSelf);
        }
    }
}
