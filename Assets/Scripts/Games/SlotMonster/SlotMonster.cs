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
    public static class NtrSlotMonsterLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("slotMonster", "Slot Monster", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("startInterval", "Start Interval")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        SlotMonster.instance.StartInterval(e.beat, e.length, e["auto"], e, 0);
                    },
                    defaultLength = 3f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval.")
                    },
                    priority = 2,
                },
                new GameAction("slot", "Slot")
                {
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("drum", SlotMonster.DrumTypes.Default, "Drum SFX", "Set the drum SFX to be used. Default is Bass on the beat, and Snare off the beat.")
                    },
                    priority = 1,
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        SlotMonster.instance.PassTurn(e.beat, e.length);
                    },
                    defaultLength = 1f,
                    priority = 1,
                },
            }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    public class SlotMonster : Minigame
    {
        public enum DrumTypes
        {
            Default,
            Bass,
            Snare,
        }

        [Header("Animators")]
        [SerializeField] Animator smAnim;
        [SerializeField] Animator[] eyeAnims;
        [SerializeField] Animator[] buttonAnims;

        private List<RiqEntity> gameEntities;

        private Sound rollingSound;
        private int currentButton;

        public static SlotMonster instance;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress)) {
                HitButton();
            }
        }

        public override void OnGameSwitch(double beat)
        {
            gameEntities = GameManager.instance.Beatmap.Entities.FindAll(c => c.datamodel.Split('/')[0] == "slotMonster");
            List<RiqEntity> startIntervals = gameEntities.FindAll(e => e.datamodel == "slotMonster/startInterval" && e.beat < beat && e.beat + e.length > beat);
            foreach (var interval in startIntervals)
            {
                StartInterval(interval.beat, interval.length, interval["auto"], interval, beat);
            }
        }

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        // make sure the current button is always between 0 and 2 (buttons 1-3)
        private int GetCurrentButton() => currentButton %= 3;

        private bool HitButton(bool isHit = false) // returns true if it's the last one
        {
            int thisButton = GetCurrentButton();
            Debug.Log("BUTTON HIT : " + thisButton);
            string hitSfx = "slotMonster/stop_" + (thisButton + 1);
            if (thisButton == 2) {
                if (isHit) hitSfx += "_hit";
            }
            SoundByte.PlayOneShotGame(hitSfx, forcePlay: true);
            buttonAnims[thisButton].DoScaledAnimationAsync("Press", 0.5f);
            currentButton++;
            return thisButton == 2 && isHit;
        }

        public void StartInterval(double beat, float length, bool autoPass, RiqEntity startInterval, double gameSwitchBeat)
        {
            List<RiqEntity> slotActions = gameEntities.FindAll(e => e.datamodel == "slotMonster/slot" && e.beat >= startInterval.beat && e.beat < startInterval.beat + startInterval.length);

            smAnim.DoScaledAnimationFromBeatAsync("Prepare", 0.5f, beat);
            SoundByte.PlayOneShotGame("slotMonster/start_touch", forcePlay: true);

                Debug.Log(Mathf.Min(slotActions.Count, 3));
            List<MultiSound.Sound> sounds = new();
            List<BeatAction.Action> actions = new();
            for (int i = 0; i < Mathf.Min(slotActions.Count, 3); i++) // limit to 3 actions
            {
                int whichSlot = i;
                RiqEntity slot = slotActions[whichSlot];
                if (slot.beat < gameSwitchBeat) continue;
                string sfx = "";
                if (slot["drum"] == (int)DrumTypes.Default) {
                    sfx = slot.beat % 1 == 0 ? "bass" : "snare";
                } else {
                    sfx = Enum.GetName(typeof(DrumTypes), (int)slot["drum"]).ToLower();
                }
                Debug.Log(sfx);
                sounds.Add(new(sfx + "DrumNTR", slot.beat));
                actions.Add(new(slot.beat, delegate {
                    buttonAnims[whichSlot].DoScaledAnimationAsync("Flash", 0.5f);
                }));
            }
            MultiSound.Play(sounds.ToArray(), false);
            BeatAction.New(this, actions);

            if (autoPass) {
                BeatAction.New(this, new() { new(beat + length, delegate { PassTurn(beat + length, 1, beat, slotActions); }) });
            }
        }

        public void PassTurn(double beat, float length, double startBeat = -1, List<RiqEntity> slotActions = null)
        {
            smAnim.DoScaledAnimationFromBeatAsync("Release", 0.5f, beat);
            foreach (var eye in eyeAnims)
            {
                eye.DoScaledAnimationAsync("Spin", 0.5f);
            }
            SoundByte.PlayOneShotGame("slotMonster/start_rolling", forcePlay: true);
            rollingSound = SoundByte.PlayOneShotGame("slotMonster/rolling", looping: true, forcePlay: true);
            if (slotActions == null) {
                var startInterval = gameEntities.FindLast(e => e.datamodel == "slotMonster/startInterval" && e.beat + e.length < beat);
                if (startBeat < 0) startBeat = startInterval.beat;
                slotActions = gameEntities.FindAll(e => e.datamodel == "slotMonster/slot" && e.beat >= startInterval.beat && e.beat < startInterval.beat + startInterval.length);
            }

            List<BeatAction.Action> actions = new();
            for (int i = 0; i < Mathf.Min(slotActions.Count, 3); i++) // limit to 3 actions
            {
                int whichSlot = i;
                double slotBeat = slotActions[i].beat;

                actions.Add(new(slotBeat + beat, delegate { buttonAnims[whichSlot].DoScaledAnimationAsync("Flash"); }));

                Debug.Log("input scheduled at : " + (beat + slotBeat - startBeat + 1));
                PlayerActionEvent input = ScheduleInput(beat, slotBeat - startBeat + 1, InputAction_BasicPress, ButtonHit, null, null);
                input.IsHittable = () => GetCurrentButton() == whichSlot;
            }
            BeatAction.New(this, actions);
        }

        private void ButtonHit(PlayerActionEvent caller, float state)
        {
            bool isWin = HitButton(true);
            if (isWin) {
                if (rollingSound != null) rollingSound.Stop();
                smAnim.DoScaledAnimationAsync("Win", 0.5f);
            }
            if (state is >= 1f or <= -1f) SoundByte.PlayOneShot("nearMiss");
        }

        private void ButtonMiss(PlayerActionEvent caller)
        {
            
        }
    }
}