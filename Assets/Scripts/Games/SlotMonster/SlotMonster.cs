using System;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;
using System.Linq;
using System.ComponentModel;

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
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SlotMonster instance)) {
                            var e = eventCaller.currentEntity;
                            instance.StartInterval(e.beat, e.length, e["auto"], e["eyeType"], 0);
                        }
                    },
                    defaultLength = 3f,
                    resizable = true,
                    preFunctionLength = 1,
                    priority = 2,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval."),
                        new Param("eyeType", SlotMonster.EyeTypes.Random, "Eye Sprite", "Set the eye sprite to be used."),
                    },
                },
                new GameAction("slot", "Slot")
                {
                    inactiveFunction = delegate {
                        // SoundByte.PlayOneShotGame("slotMonster/start_touch", eventCaller.currentEntity.beat, forcePlay: true);
                        SoundByte.PlayOneShotGame("slotMonster/start_touch", forcePlay: true);
                    },
                    defaultLength = 0.5f,
                    priority = 1,
                    parameters = new List<Param>()
                    {
                        new Param("drum", SlotMonster.DrumTypes.Default, "Drum SFX", "Set the drum SFX to be used. Default is Bass on the beat, and Snare off the beat.")
                    },
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SlotMonster instance)) {
                            var e = eventCaller.currentEntity;
                            instance.PassTurn(e.beat, e.length);
                        }
                    },
                    defaultLength = 1f,
                    priority = 1,
                },
                new GameAction("buttonColor", "Button Color")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SlotMonster instance)) {
                            var e = eventCaller.currentEntity;
                            instance.ButtonColor(new Color[] { e["button1"], e["button2"], e["button3"] }, e["flash"]);
                        }
                    },
                    defaultLength = 1f,
                    priority = 1,
                    parameters = new List<Param>()
                    {
                        new Param("button1", new Color(0.38f, 0.98f, 0.25f), "Button 1 Color", "Set the color of the first button."),
                        new Param("button2", new Color(0.8f, 0.28f, 0.95f), "Button 2 Color", "Set the color of the second button."),
                        new Param("button3", new Color(0.87f, 0f, 0f), "Button 3 Color", "Set the color of the third button."),
                        new Param("flash", new Color(1f, 1f, 0.68f), "Button Flash Color", "Set the color of the flash of the buttons."),
                    },
                },
            }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_SlotMonster;
    public class SlotMonster : Minigame
    {
        public enum DrumTypes
        {
            Default,
            Bass,
            Snare,
        }

        public enum EyeTypes
        {
            Random,
            Note,
            Ring,
            Cake,
            Flower,
            Beverage,
            Mushroom,
            Key,
            Ribbon,
            Hat,
            Barista,
        }

        [Header("Animators")]
        [SerializeField] Animator smAnim;
        [SerializeField] Animator[] eyeAnims;

        [SerializeField] SlotButton[] buttons;
        public Color buttonFlashColor;

        private List<RiqEntity> gameEntities;

        private Sound rollingSound;
        private int currentEyeSprite = 1;
        private int maxButtons;
        private int currentButton;

        private void Awake()
        {
            foreach (var button in buttons) {
                button.Init(this);
            }
        }

        private void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress)) {
                _ = HitButton();
                ScoreMiss();
            }
        }

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            gameEntities = GameManager.instance.Beatmap.Entities.FindAll(c => c.datamodel.Split('/')[0] == "slotMonster");
            foreach (RiqEntity e in gameEntities.FindAll(e => e.datamodel == "slotMonster/startInterval" && e.beat < beat && e.beat + e.length > beat)) {
                StartInterval(e.beat, e.length, e["auto"], e["eyeType"], beat);
            }
        }

        // make sure the current button is always between 0 and 2 (buttons 1-3)
        private int GetCurrentButton() => Array.FindIndex(buttons, button => !button.pressed);

        private bool HitButton(bool isHit = false) // returns true if it's the last one
        {
            int thisButton = GetCurrentButton();
            if (thisButton == -1) return false;
            bool lastButton = thisButton == maxButtons - 1;
            string hitSfx = "slotMonster/stop_" + (lastButton && isHit ? "hit" : (thisButton + 1));
            SoundByte.PlayOneShotGame(hitSfx, forcePlay: true);
            for (int i = thisButton; i < (lastButton ? 3 : thisButton + 1); i++)
            {
                buttons[thisButton].Press();
                if (eyeAnims[thisButton].IsPlayingAnimationNames("Spin")) {
                    int eyeSprite = currentEyeSprite;
                    if (!isHit) {
                        do {
                            eyeSprite = UnityEngine.Random.Range(1, 10);
                        } while (eyeSprite == currentEyeSprite);
                    }
                    Debug.Log("EyeItem" + eyeSprite);
                    eyeAnims[thisButton].Play("EyeItem" + eyeSprite, 0, 0);
                }
            }
            if (lastButton) {
                if (rollingSound != null) rollingSound.Stop();
            }
            currentButton++;
            return lastButton && isHit;
        }

        public void StartInterval(double beat, float length, bool autoPass, int eyeSprite, double gameSwitchBeat)
        {
            List<RiqEntity> slotActions = gameEntities.FindAll(e => e.datamodel == "slotMonster/slot" && e.beat >= beat && e.beat < beat + length);

            SoundByte.PlayOneShotGame("slotMonster/start_touch", forcePlay: true);
            smAnim.DoScaledAnimationFromBeatAsync("Prepare", 0.5f, beat);
            foreach (var button in buttons) {
                button.anim.Play("PopUp", 0, 1);
            }

            List<MultiSound.Sound> sounds = new();
            List<BeatAction.Action> actions = new();
            maxButtons = Mathf.Min(slotActions.Count, 3);
            for (int i = 0; i < maxButtons; i++) // limit to 3 actions
            {
                buttons[i].anim.Play("PopUp", 0, 0);
                int whichSlot = i;
                RiqEntity slot = slotActions[whichSlot];
                if (slot.beat < gameSwitchBeat) continue;
                string sfx = "";
                if (slot["drum"] == (int)DrumTypes.Default) {
                    sfx = slot.beat % 1 == 0 ? "bass" : "snare";
                } else {
                    sfx = Enum.GetName(typeof(DrumTypes), (int)slot["drum"]).ToLower();
                }
                // Debug.Log(sfx);
                sounds.Add(new(sfx + "DrumNTR", slot.beat));
                actions.Add(new(slot.beat, delegate {
                    buttons[whichSlot].TryFlash();
                }));
            }
            MultiSound.Play(sounds.ToArray(), false);
            BeatAction.New(this, actions);

            if (autoPass) {
                BeatAction.New(this, new() { new(beat + length, delegate {
                    currentEyeSprite = eyeSprite == 0 ? UnityEngine.Random.Range(1, 10) : eyeSprite;
                    PassTurn(beat + length, 1, beat, slotActions);
                })});
            }
        }

        public void PassTurn(double beat, float length, double startBeat = -1, List<RiqEntity> slotActions = null)
        {
            smAnim.DoScaledAnimationFromBeatAsync("Release", 0.5f, beat);
            for (int i = 0; i < eyeAnims.Length; i++) {
                eyeAnims[i].DoScaledAnimationAsync("Spin", 0.5f);
            }
            SoundByte.PlayOneShotGame("slotMonster/start_rolling", forcePlay: true);
            rollingSound = SoundByte.PlayOneShotGame("slotMonster/rolling", looping: true, forcePlay: true);
            if (startBeat < 0 || slotActions == null) {
                var startInterval = gameEntities.FindLast(e => e.datamodel == "slotMonster/startInterval" && e.beat + e.length < beat);
                startBeat = startInterval.beat;
                slotActions ??= gameEntities.FindAll(e => e.datamodel == "slotMonster/slot" && e.beat >= startInterval.beat && e.beat < startInterval.beat + startInterval.length);
            }

            List<BeatAction.Action> actions = new();
            for (int i = 0; i < Mathf.Min(slotActions.Count, 3); i++) // limit to 3 actions
            {
                int whichSlot = i;
                double slotBeat = slotActions[i].beat;

                actions.Add(new(beat + length + slotBeat - startBeat, delegate {
                    buttons[whichSlot].TryFlash();
                }));

                // Debug.Log("input scheduled at : " + (beat + length + slotBeat - startBeat));
                PlayerActionEvent input = ScheduleInput(beat, slotBeat - startBeat + length, InputAction_BasicPress, ButtonHit, null, null);
                // input.IsHittable = () => {
                //     int currentButton = GetCurrentButton();
                //     return currentButton == whichSlot && !buttonsPressed[whichSlot];
                // };
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

        // private void ButtonMiss(PlayerActionEvent caller)
        // {
        //
        // }

        public void ButtonColor(Color[] colors, Color flashColor)
        {
            for (int i = 0; i < buttons.Length; i++) {
                buttons[i].color = colors[i];
            }
            buttonFlashColor = flashColor;
        }
    }
}