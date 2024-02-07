using System;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;
using System.Linq;

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
                            instance.StartInterval(e.beat, e.length, e["auto"], e["eyeType"], e, 0);
                        }
                    },
                    defaultLength = 3f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval."),
                        new Param("eyeType", SlotMonster.EyeTypes.Random, "Eye Sprite", "Set the eye sprite to be used."),
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
                    parameters = new List<Param>()
                    {
                        new Param("button1", new Color(), "Button 1 Color", "Set the color of the first button."),
                        new Param("button2", new Color(), "Button 2 Color", "Set the color of the second button."),
                        new Param("button3", new Color(), "Button 3 Color", "Set the color of the third button."),
                        new Param("flash", new Color(), "Button Flash Color", "Set the color of the flash of the buttons."),
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
        [SerializeField] Sprite[] eyeSprites;
        [SerializeField] Animator[] eyeAnims;
        [SerializeField] SpriteRenderer[] eyeSRs;

        [SerializeField] Animator[] buttonAnims;
        // used to ease between button colors and button flash colors! wow
        private Color[] buttonColors;
        private Color buttonFlashColor;

        private List<RiqEntity> gameEntities;

        private Sound rollingSound;
        private int currentEyeSprite = 1;
        private int maxButtons;
        private int currentButton;

        // public static SlotMonster instance;

        private void Awake()
        {
            // eyeSRs = eyeAnims.Where(x => x.GetComponent<SpriteRenderer>()).ToArray();
        }

        private void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress)) {
                _ = HitButton();
            }
        }

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            gameEntities = GameManager.instance.Beatmap.Entities.FindAll(c => c.datamodel.Split('/')[0] == "slotMonster");
            foreach (RiqEntity interval in gameEntities.FindAll(e => e.datamodel == "slotMonster/startInterval" && e.beat < beat && e.beat + e.length > beat))
            {
                StartInterval(interval.beat, interval.length, interval["auto"], interval["eyeType"], interval, beat);
            }
        }

        // make sure the current button is always between 0 and 2 (buttons 1-3)
        private int GetCurrentButton() => currentButton %= 3;

        private bool HitButton(bool isHit = false) // returns true if it's the last one
        {
            int thisButton = GetCurrentButton();
            // Debug.Log("BUTTON HIT : " + thisButton);
            bool lastButton = thisButton == maxButtons - 1;
            string hitSfx = "slotMonster/stop_" + (lastButton && isHit ? "hit" : (thisButton + 1));
            SoundByte.PlayOneShotGame(hitSfx, forcePlay: true);
            for (int i = thisButton; i < (lastButton ? 3 : thisButton + 1); i++)
            {
                buttonAnims[thisButton].DoScaledAnimationAsync("Press", 0.5f);
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
            currentButton++;
            return lastButton && isHit;
        }

        public void StartInterval(double beat, float length, bool autoPass, int eyeSprite, RiqEntity startInterval, double gameSwitchBeat)
        {
            List<RiqEntity> slotActions = gameEntities.FindAll(e => e.datamodel == "slotMonster/slot" && e.beat >= startInterval.beat && e.beat < startInterval.beat + startInterval.length);

            smAnim.DoScaledAnimationFromBeatAsync("Prepare", 0.5f, beat);
            SoundByte.PlayOneShotGame("slotMonster/start_touch", forcePlay: true);

            List<MultiSound.Sound> sounds = new();
            List<BeatAction.Action> actions = new();
            maxButtons = Mathf.Min(slotActions.Count, 3);
            for (int i = 0; i < maxButtons; i++) // limit to 3 actions
            {
                buttonAnims[i].Play("Idle", 0, 0);
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
                    // if (buttonAnims[whichSlot].IsAnimationNotPlaying()) {
                    //     buttonAnims[whichSlot].DoScaledAnimationAsync("Flash", 0.5f);
                    // }
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
            for (int i = 0; i < eyeAnims.Length; i++)
            {
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

                // actions.Add(new(beat + length + slotBeat - startBeat, delegate { buttonAnims[whichSlot].DoScaledAnimationAsync("Flash"); }));

                // Debug.Log("input scheduled at : " + (beat + length + slotBeat - startBeat));
                PlayerActionEvent input = ScheduleInput(beat, slotBeat - startBeat + length, InputAction_BasicPress, ButtonHit, null, null);
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

        // private void ButtonMiss(PlayerActionEvent caller)
        // {
            
        // }

        public void ButtonColor(Color[] baseColors, Color flashColor)
        {
            buttonColors = baseColors;
            buttonFlashColor = flashColor;
        }
    }
}