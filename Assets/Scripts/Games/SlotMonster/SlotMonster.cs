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
                        SlotMonster.instance.StartInterval(e.beat, e.length, e["auto"], e, e.beat);
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
                    priority = 1,
                },
                new GameAction("passTurn", "Pass Turn")
                {
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
            List<RiqEntity> slotActions = gameEntities.FindAll(e => e.datamodel == "slotMonster/slot" && e.beat > startInterval.beat && e.beat < startInterval.beat + startInterval.length);

            smAnim.DoScaledAnimationFromBeatAsync("Prepare", 0.5f, beat);
            SoundByte.PlayOneShotGame("slotMonster/start_touch", forcePlay: true);
            foreach (var eye in eyeAnims)
            {
                eye.DoScaledAnimationAsync("Spin", 0.5f);
            }

            List<BeatAction.Action> actions = new();
            for (int i = 0; i < Mathf.Min(slotActions.Count, 3); i++) // limit to 3 actions
            {
                double slotBeat = slotActions[i].beat;
                if (slotBeat > gameSwitchBeat) continue;
                actions.Add(new(slotBeat, delegate {
                    buttonAnims[i].DoScaledAnimationAsync("Flash", 0.5f);
                }));
            }
            BeatAction.New(this, actions);

            if (autoPass) {
                PassTurn(beat + length, slotActions);
            }
        }

        public void PassTurn(double beat, List<RiqEntity> slotActions = null)
        {
            SoundByte.PlayOneShotGame("slotMonster/start_rolling", forcePlay: true);
            rollingSound = SoundByte.PlayOneShotGame("slotMonster/rolling", looping: true, forcePlay: true);
            if (slotActions == null) {
                var startInterval = gameEntities.FindLast(e => e.datamodel == "slotMonster/startInterval" && e.beat + e.length < beat);
                slotActions = gameEntities.FindAll(e => e.datamodel == "slotMonster/slot" && e.beat > startInterval.beat && e.beat < startInterval.beat + startInterval.length);
            }

            List<BeatAction.Action> actions = new();
            for (int i = 0; i < Mathf.Min(slotActions.Count, 3); i++) // limit to 3 actions
            {
                int whichSlot = i;

                actions.Add(new(slotActions[whichSlot].beat, delegate { buttonAnims[whichSlot].DoScaledAnimationAsync("Flash"); }));

                PlayerActionEvent input = ScheduleInput(beat, slotActions[i].beat, InputAction_BasicPress, ButtonHit, null, null);
                input.IsHittable = () => GetCurrentButton() == whichSlot;
            }
            BeatAction.New(this, actions);
        }

        private void ButtonHit(PlayerActionEvent caller, float state)
        {
            HitButton(true);
            if (state is >= 1f or <= -1f) SoundByte.PlayOneShot("nearMiss");
        }

        private void ButtonMiss(PlayerActionEvent caller)
        {
            
        }
    }
}