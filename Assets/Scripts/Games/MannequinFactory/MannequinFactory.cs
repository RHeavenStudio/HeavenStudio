using System.Collections.Generic;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbMannequinFactoryLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("mannequinFactory", "Mannequin Factory", "554899", false, false, new List<GameAction>()
            {
                new GameAction("headOut", "Send Head Out")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out MannequinFactory instance)) {
                            var e = eventCaller.currentEntity;
                            instance.HeadOut(e.beat, 1);
                        }
                    }, 
                    defaultLength = 7,
                },
                new GameAction("misalignedHeadOut", "Send Misaligned Head Out")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out MannequinFactory instance)) {
                            var e = eventCaller.currentEntity;
                            instance.HeadOut(e.beat, 2);
                        }
                    }, 
                    defaultLength = 7,
                },
                new GameAction("randomHeadOut", "Send Random Head Out")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out MannequinFactory instance)) {
                            var e = eventCaller.currentEntity;
                            instance.HeadOut(e.beat, 0);
                        }
                    }, 
                    defaultLength = 7,
                },
                new GameAction("changeText", "Change Text")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out MannequinFactory instance)) {
                            var e = eventCaller.currentEntity;
                            instance.SignText.text = eventCaller.currentEntity["text"];
                        }
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("text", "Mannequin Factory", "Text", "The text to be displayed on the sign"),
                    }
                },
                new GameAction("bgColor", "Change Background Color")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out MannequinFactory instance)) {
                            var e = eventCaller.currentEntity;
                            instance.BackgroundColor(e.beat, e.length, e["colorStart"], e["colorEnd"], e["ease"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("colorStart", new Color(0.97f, 0.94f, 0.51f, 1f), "Start Color", "The color to start fading from."),
                        new Param("colorEnd",   new Color(0.97f, 0.94f, 0.51f, 1f), "End Color", "The color to end the fade."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "The ease to use for color fade", new() {
                            new Param.CollapseParam((x, _) => (int)x != (int)Util.EasingFunction.Ease.Instant, new[] { "colorStart" }),
                        }),
                    },
                    resizable = true
                },
            }
            // ,
            // new List<string>() {"agb", "normal"},
            // "agbmannequin", "en",
            // new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MannequinFactory;
    public class MannequinFactory : Minigame
    {
        [Header("Animators")]
        public Animator HandAnim;
        public Animator StampAnim;
        
        [Header("References")]
        [SerializeField] SpriteRenderer bg;
        public TMP_Text SignText;
        public GameObject MannequinHeadObject;

        protected static bool IA_PadLeft(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt);
        }
        protected static new bool IA_TouchFlick(out double dt)
        {
            return PlayerInput.GetTouchUp(InputController.ActionsTouch.Tap, out dt) && PlayerInput.GetFlick(out _)
                    && !instance.IsExpectingInputNow(InputAction_Second);
        }
        protected static bool IA_TouchPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)// && !PlayerInput.GetFlick(out _)
                    && !instance.IsExpectingInputNow(InputAction_First);
        }

        public static PlayerInput.InputAction InputAction_First =
            new("AgbMannequinFactoryTouchFirst", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadLeft, IA_TouchFlick, IA_Empty);

        public static PlayerInput.InputAction InputAction_Second =
            new("AgbMannequinFactoryTouchSecond", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadBasicPress, IA_TouchPress, IA_Empty);

        // awww man a static instance 😢
        private static MannequinFactory instance;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            bool touch = PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch;
            if (PlayerInput.GetIsAction(InputAction_First) && (!IsExpectingInputNow(InputAction_First) || (touch && !IsExpectingInputNow(InputAction_First) && !IsExpectingInputNow(InputAction_Second))))
            {
                HandAnim.DoScaledAnimationAsync("SlapEmpty", 0.3f);
            }

            if (PlayerInput.GetIsAction(InputAction_Second) && (!IsExpectingInputNow(InputAction_Second) || (touch && !IsExpectingInputNow(InputAction_First) && !IsExpectingInputNow(InputAction_Second))))
            {
                StampAnim.DoScaledAnimationAsync("StampEmpty", 0.3f);
            }

            BackgroundColorUpdate();
        }

        public override void OnGameSwitch(double beat)
        {
            var events = gameManager.Beatmap.Entities.FindAll(e => e.datamodel.Split('/')[0] == "mannequinFactory");

            foreach (var item in events.FindAll(e => e.datamodel is "headOut" or "misalignedHeadOut" or "randomHeadOut"))
            {
                
            }

            var bg = events.FindLast(e => e.datamodel == "mannequinFactory/bgColor" && e.beat < beat);
            if (bg != null) {
                BackgroundColor(bg.beat, bg.length, bg["colorStart"], bg["colorEnd"], bg["ease"]);
            } else {
                Color color = new Color(0.97f, 0.94f, 0.51f, 1f);
                BackgroundColor(0, 0, color, color, (int)EasingFunction.Ease.Instant);
            }
        }

        public static void HeadOutSFX(double beat, int cueType)
        {
            if (cueType == 0) cueType = Random.Range(1, 3);
            var sfx = new List<MultiSound.Sound>() {
                new MultiSound.Sound("mannequinFactory/drum", beat      ),
                new MultiSound.Sound("mannequinFactory/drum", beat + 0.5),
                new MultiSound.Sound("mannequinFactory/drum", beat + 1.5),
                new MultiSound.Sound("mannequinFactory/drum", beat + 2  ),
            };
            if (cueType == 1) {
                for (int i = 0; i < 7; i++) {
                    sfx.Add(new($"mannequinFactory/drumroll{i + 1}", beat + 3 + (i * 0.1667)));
                }
            } else {
                sfx.AddRange(new MultiSound.Sound[] {
                    new("mannequinFactory/drum", beat + 0.75),
                    new("mannequinFactory/drum", beat + 1   ),
                });
            }
            MultiSound.Play(sfx.ToArray(), forcePlay: true);
        }

        public void HeadOut(double beat, int cueType)
        {
            MannequinHead head = Instantiate(MannequinHeadObject, transform).GetComponent<MannequinHead>();
            head.game = this;
            head.startBeat = beat;
            head.needClap = cueType == 2;
        }

        private double colorStartBeat = -1;
        private float colorLength = 0f;
        private Color colorStart, colorEnd = new Color(0.97f, 0.94f, 0.51f, 1f); // obviously put to the default colour of the game
        private Util.EasingFunction.Ease colorEase; // putting Util in case this game is using Jukebox

        //call this in update
        private void BackgroundColorUpdate()
        {
            float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(colorStartBeat, colorLength));

            var func = Util.EasingFunction.GetEasingFunction(colorEase);

            float newR = func(colorStart.r, colorEnd.r, normalizedBeat);
            float newG = func(colorStart.g, colorEnd.g, normalizedBeat);
            float newB = func(colorStart.b, colorEnd.b, normalizedBeat);

            bg.color = new Color(newR, newG, newB);
        }

        public void BackgroundColor(double beat, float length, Color colorStartSet, Color colorEndSet, int ease)
        {
            colorStartBeat = beat;
            colorLength = length;
            colorStart = colorStartSet;
            colorEnd = colorEndSet;
            colorEase = (Util.EasingFunction.Ease)ease;
        }
    }
}
