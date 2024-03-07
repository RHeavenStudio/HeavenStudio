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
            return new Minigame("slotMonster", "Slot Monster", "d10914", false, false, new List<GameAction>()
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
                            instance.GameplayModifiers(e["lottery"], e["lotteryAmount"], e["lotterySpeed"], e["stars"]);
                        }
                    },
                    defaultLength = 0.5f,
                    priority = 1,
                    parameters = new List<Param>()
                    {
                        new Param("lottery", true, "Lottery", "Toggle if the win particles should play after a successful sequence.", new() {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "lotteryAmount", "lotterySpeed", "stars" }),
                        }),
                        new Param("lotteryAmount", new EntityTypes.Float(0, 10, 1), "Lottery Amount", "Set the amount of win particles."),
                        new Param("lotterySpeed", new EntityTypes.Float(0.5f, 3, 1), "Lottery Speed", "Set the speed of the win particles."),
                        new Param("stars", false, "Use Stars", "Use stars instead of coins? (From the Korean version of RH DS)"),
                    },
                },
            },
            new List<string>() { "ntr", "normal" },
            "ntrslotmonster", "en",
            new List<string>() {}
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
        private int[] eyeSprites = new int[3];

        [Header("Objects")]
        [SerializeField] SlotButton[] buttons;
        [SerializeField] ParticleSystem winParticles;
        public Color buttonFlashColor = new(1f, 1f, 0.68f);

        private List<RiqEntity> gameEntities;
        private bool doWin = true;
        private bool inputsActive = false;

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

        private void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress) && inputsActive && !buttons[currentButton].pressed) {
                HitButton();
                ScoreMiss();
            }
        }

        private void LateUpdate()
        {
            currentButton = Array.FindIndex(buttons, button => !button.pressed);
        }

        private void HitButton(bool isHit = false, int timing = 0)
        {
            bool isLast = currentButton == maxButtons - 1;
            buttons[currentButton].Press(!isHit || timing != 0);
            for (int i = currentButton; i < (isLast ? 3 : currentButton + 1); i++)
            {
                if (eyeAnims[i].IsPlayingAnimationNames("Spin")) {
                    int eyeSprite = currentEyeSprite;
                    string anim = "EyeItem";
                    if (!isHit) {
                        do {
                            eyeSprite = UnityEngine.Random.Range(1, 10);
                        } while (eyeSprite == currentEyeSprite || eyeSprites.Contains(eyeSprite));

                        eyeSprites[i] = eyeSprite;
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
                    eyeAnims[i].Play(anim, 0, 0);
                }
            }
            bool isMiss = buttons.Any(x => x.missed);
            string hitSfx = "slotMonster/stop_" + (isLast && isHit && !isMiss ? "hit" : (currentButton + 1));
            SoundByte.PlayOneShotGame(hitSfx, forcePlay: true);
            if (isLast) {
                if (rollingSound != null) rollingSound.Stop();
                inputsActive = false;
                if (isHit && !isMiss) {
                    smAnim.DoScaledAnimationAsync("Win", 0.5f);
                    if (doWin) {
                        SoundByte.PlayOneShotGame("slotMonster/win");
                        winParticles.Play();
                    }
                } else {
                    smAnim.DoScaledAnimationAsync("Lose", 0.5f);
                }
            }
        }

        public void StartInterval(RiqEntity si, bool autoPass, int eyeSprite, double gameSwitchBeat)
        {
            if (rollingSound != null) rollingSound.Stop();
            List<RiqEntity> slotActions = gameEntities.FindAll(e => e.datamodel == "slotMonster/slot" && e.beat >= si.beat && e.beat < si.beat + si.length);
            if (slotActions.Count <= 0) return;

            SoundByte.PlayOneShotGame("slotMonster/start_touch", forcePlay: true);
            smAnim.DoScaledAnimationFromBeatAsync("Prepare", 0.5f, si.beat);

            List<MultiSound.Sound> sounds = new();
            List<BeatAction.Action> actions = new();
            maxButtons = Mathf.Min(slotActions.Count, 3);
            for (int i = 0; i < maxButtons; i++) // limit to 3 actions
            {
                buttons[i].Ready();
                eyeAnims[i].Play("Idle", 0, 1);
                int whichSlot = i;
                RiqEntity slot = slotActions[whichSlot];
                if (slot.beat < gameSwitchBeat) continue;
                string sfx = "";
                if (slot["drum"] == (int)DrumTypes.Default) {
                    sfx = slot.beat % 1 == 0 ? "bass" : "snare";
                } else {
                    sfx = Enum.GetName(typeof(DrumTypes), (int)slot["drum"]).ToLower();
                }
                sounds.Add(new(sfx + "DrumNTR", slot.beat));
                actions.Add(new(slot.beat, delegate {
                    buttons[whichSlot].TryFlash();
                }));
            }
            MultiSound.Play(sounds.ToArray(), false);
            BeatAction.New(this, actions);

            if (autoPass) {
                BeatAction.New(this, new() { new(si.beat + si.length, delegate {
                    currentEyeSprite = eyeSprite == 0 ? UnityEngine.Random.Range(1, 10) : eyeSprite - 1;
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

            List<BeatAction.Action> actions = new() {
                new(beat, delegate { inputsActive = true; })
            };
            for (int i = 0; i < Mathf.Min(slotActions.Count, 3); i++) // limit to 3 actions
            {
                int whichSlot = i;
                double slotBeat = slotActions[i].beat;

                actions.Add(new(beat + length + slotBeat - startBeat, delegate {
                    buttons[whichSlot].TryFlash();
                }));

                buttons[whichSlot].input = ScheduleInput(beat, slotBeat - startBeat + length, InputAction_BasicPress, ButtonHit, i == slotActions.Count - 1 ? ButtonEndMiss : null, null, () => {
                    return currentButton == whichSlot && !buttons[whichSlot].pressed;
                });
            }
            BeatAction.New(this, actions);
        }

        private void ButtonHit(PlayerActionEvent caller, float state)
        {
            int timing = state switch {
                >= 1f => -1,
                <= -1f => 1,
                _ => 0,
            };
            HitButton(true, timing);
            if (state is >= 1f or <= -1f) SoundByte.PlayOneShot("nearMiss");
        }

        private void ButtonEndMiss(PlayerActionEvent caller)
        {
            Debug.Log("miss i guess");
            if (rollingSound != null) rollingSound.Stop();
            inputsActive = false;
            smAnim.DoScaledAnimationAsync("Lose", 0.5f);
            foreach (var anim in eyeAnims) {
                if (anim.IsPlayingAnimationNames("Spin")) {
                    anim.Play("Idle", 0, 1);
                }
            }
        }

        public void ButtonColor(Color[] colors, Color flashColor)
        {
            for (int i = 0; i < buttons.Length; i++) {
                buttons[i].color = colors[i];
            }
            buttonFlashColor = flashColor;
        }

        public void GameplayModifiers(bool lottery, float amount, float speed, bool stars)
        {
            var sheetAnim = winParticles.textureSheetAnimation;
            sheetAnim.frameOverTime = stars ? 1 : 0;

            var rotOverTime = winParticles.rotationOverLifetime;
            rotOverTime.z = new ParticleSystem.MinMaxCurve(stars ? -5 : 0, stars ? 5 : 0);

            var emission = winParticles.emission;
            emission.rateOverTime = amount * 13;

            var main = winParticles.main;
            main.simulationSpeed = speed;

            doWin = lottery;
        }
    }
}