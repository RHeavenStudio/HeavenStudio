using System;
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
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SlotMonster instance)) {
                            var e = eventCaller.currentEntity;
                            instance.StartInterval(e, e["auto"], e["eyeType"], 0);
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
                new GameAction("gameplayModifiers", "Gameplay Modifiers")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out SlotMonster instance)) {
                            var e = eventCaller.currentEntity;
                            instance.GameplayModifiers(e["lottery"], e["stars"]);
                        }
                    },
                    defaultLength = 0.5f,
                    priority = 1,
                    parameters = new List<Param>()
                    {
                        new Param("lottery", true, "Lottery", "Toggle if the win particles should play after a successful sequence.", new() {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "stars" }),
                        }),
                        new Param("stars", false, "Use Stars", "Use stars instead of coins? (From the Korean version of RH DS)"),
                    },
                },
            }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using System.Linq;
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

        [Header("Objects")]
        [SerializeField] SlotButton[] buttons;
        [SerializeField] ParticleSystem winParticles;
        public Color buttonFlashColor = new(1f, 1f, 0.68f);

        private List<RiqEntity> gameEntities;
        private bool doWin = true;

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
            gameEntities = gameManager.Beatmap.Entities.FindAll(c => c.datamodel.Split('/')[0] == "slotMonster");
            foreach (RiqEntity e in gameEntities.FindAll(e => e.datamodel == "slotMonster/startInterval" && e.beat < beat && e.beat + e.length > beat)) {
                StartInterval(e, e["auto"], e["eyeType"], beat);
            }
        }

        // make sure the current button is always between 0 and 2 (buttons 1-3)
        private int GetCurrentButton() => Array.FindIndex(buttons, button => !button.pressed);

        private bool HitButton(bool isHit = false, int timing = 0) // returns true if it's the last one
        {
            int thisButton = GetCurrentButton();
            if (thisButton == -1) return false;
            bool isLast = thisButton == maxButtons - 1;
            for (int i = thisButton; i < (isLast ? 3 : thisButton + 1); i++)
            {
                buttons[thisButton].Press(isHit);
                if (eyeAnims[thisButton].IsPlayingAnimationNames("Spin")) {
                    int eyeSprite = currentEyeSprite;
                    string anim = "EyeItem";
                    if (!isHit) {
                        eyeSprite = UnityEngine.Random.Range(1, 9);
                        if (eyeSprite >= currentEyeSprite) eyeSprite++;
                        anim += eyeSprite;
                    } else { // only do all this if it's actually a hit
                        if (timing == -1) { // if the timing is early
                            eyeSprite = (eyeSprite + 1) % 9;
                        }
                        anim += eyeSprite + 1;
                        if (timing != 0) { // if it's a barely
                            anim += "Barely";
                        }
                    }
                    Debug.Log(anim);
                    eyeAnims[thisButton].Play(anim, 0, 0);
                }
            }
            bool isMiss = buttons.Any(x => x.missed);
            string hitSfx = "slotMonster/stop_" + (isLast && isHit && !isMiss ? "hit" : (thisButton + 1));
            SoundByte.PlayOneShotGame(hitSfx, forcePlay: true);
            if (isLast) {
                if (rollingSound != null) rollingSound.Stop();
                if (isHit && doWin) {
                    SoundByte.PlayOneShotGame("slotMonster/win");
                    winParticles.Play();
                }
            }
            currentButton++;
            return isLast && isHit;
        }

        public void StartInterval(RiqEntity si, bool autoPass, int eyeSprite, double gameSwitchBeat)
        {
            List<RiqEntity> slotActions = gameEntities.FindAll(e => e.datamodel == "slotMonster/slot" && e.beat >= si.beat && e.beat < si.beat + si.length);
            if (slotActions.Count <= 0) return;

            SoundByte.PlayOneShotGame("slotMonster/start_touch", forcePlay: true);
            smAnim.DoScaledAnimationFromBeatAsync("Prepare", 0.5f, si.beat);
            foreach (var button in buttons) {
                button.anim.Play("PopUp", 0, 1);
            }

            List<MultiSound.Sound> sounds = new();
            List<BeatAction.Action> actions = new();
            maxButtons = Mathf.Min(slotActions.Count, 3);
            for (int i = 0; i < maxButtons; i++) // limit to 3 actions
            {
                buttons[i].anim.Play("PopUp", 0, 0);
                buttons[i].pressed = false;
                eyeAnims[i].Play("Idle", 0, 1);
                int whichSlot = i;
                RiqEntity slot = slotActions[whichSlot];
                // buttons[i].timing = slot.beat;
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
                BeatAction.New(this, new() { new(si.beat + si.length, delegate {
                    currentEyeSprite = eyeSprite == 0 ? UnityEngine.Random.Range(1, 10) : eyeSprite;
                    PassTurn(si.beat + si.length, 1, si.beat, si, slotActions);
                })});
            }
        }

        public void PassTurn(double beat, float length, double startBeat = -1, RiqEntity startInterval = null, List<RiqEntity> slotActions = null)
        {
            smAnim.DoScaledAnimationFromBeatAsync("Release", 0.5f, beat);
            for (int i = 0; i < eyeAnims.Length; i++) {
                eyeAnims[i].DoScaledAnimationAsync("Spin", 0.5f);
            }
            SoundByte.PlayOneShotGame("slotMonster/start_rolling", forcePlay: true);
            rollingSound = SoundByte.PlayOneShotGame("slotMonster/rolling", looping: true, forcePlay: true);

            startInterval ??= gameEntities.FindLast(e => e.datamodel == "slotMonster/startInterval" && e.beat + e.length < beat);
            if (startBeat < 0) {
                startBeat = startInterval.beat;
            }
            slotActions ??= gameEntities.FindAll(e => e.datamodel == "slotMonster/slot" && e.beat >= startInterval.beat && e.beat < startInterval.beat + startInterval.length);

            List<BeatAction.Action> actions = new();
            for (int i = 0; i < Mathf.Min(slotActions.Count, 3); i++) // limit to 3 actions
            {
                int whichSlot = i;
                double slotBeat = slotActions[i].beat;

                actions.Add(new(beat + length + slotBeat - startBeat, delegate {
                    buttons[whichSlot].TryFlash();
                }));

                PlayerActionEvent input = ScheduleInput(beat, slotBeat - startBeat + length, InputAction_BasicPress, ButtonHit, null, null, () => {
                    return GetCurrentButton() == whichSlot && !buttons[whichSlot].pressed;
                });
            }
            actions.Add(new(beat + length + startInterval.beat + startInterval.length, delegate {
                if (rollingSound != null) rollingSound.Stop();
                if (buttons.Any(x => x.missed)) { // if it's a miss
                    smAnim.DoScaledAnimationAsync("Lose", 0.5f);
                }
            }));
            BeatAction.New(this, actions);
        }

        private void ButtonHit(PlayerActionEvent caller, float state)
        {
            int timing = state switch {
                >= 1f => -1,
                <= -1f => 1,
                _ => 0,
            };
            bool isWin = HitButton(true, timing);
            if (isWin) {
                if (rollingSound != null) rollingSound.Stop();
                smAnim.DoScaledAnimationAsync("Win", 0.5f);
            }
            if (state is >= 1f or <= -1f) SoundByte.PlayOneShot("nearMiss");
        }

        public void ButtonColor(Color[] colors, Color flashColor)
        {
            for (int i = 0; i < buttons.Length; i++) {
                buttons[i].color = colors[i];
            }
            buttonFlashColor = flashColor;
        }

        public void GameplayModifiers(bool lottery, bool stars)
        {
            // var sheetAnim = winParticles.textureSheetAnimation;
            doWin = lottery;
        }
    }
}