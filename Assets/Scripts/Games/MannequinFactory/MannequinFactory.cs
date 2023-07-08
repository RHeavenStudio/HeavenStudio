using System.Collections.Generic;
using HeavenStudio.Util;
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
                        var e = eventCaller.currentEntity;
                        MannequinFactory.instance.HeadOut(e.beat, e["type"]);
                    }, 
                    defaultLength = 7,
                    parameters = new List<Param>()
                    {
                        new Param("type", MannequinFactory.HeadOutTypes.NoClap, "Cue Type", "Should the mannequin go out wrong or not?"),
                    }
                },
                new GameAction("changeText", "Change Text")
                {
                    function = delegate { MannequinFactory.instance.ChangeSignText(eventCaller.currentEntity["text"]); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("text", "Mannequin Factory", "Text", "The text to be displayed on the sign"),
                    }
                },
                new GameAction("bgColor", "Change Background Color")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MannequinFactory.instance.BackgroundColor(e["start"], e["end"], e.length, e["instant"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("start", new Color(0.97f, 0.94f, 0.51f, 1f), "Start Color", "The color to start fading from."),
                        new Param("end",   new Color(0.97f, 0.94f, 0.51f, 1f), "End Color", "The color to end the fade."),
                        new Param("instant", false, "Instant", "If checked, the background color will instantly change to the start color.")
                    },
                    resizable = true
                },
            },
            new List<string>() {"agb", "normal"},
            "agbmannequin", "en",
            new List<string>() {}
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
        [SerializeField] TMP_Text SignText;
        public GameObject MannequinHeadObject;
        Tween bgColorTween;

        public enum HeadOutTypes
        {
            Random,
            NoClap,
            Clap,
        }

        public static MannequinFactory instance;
        
        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                StampAnim.DoScaledAnimationAsync("StampEmpty");
            }

            if (PlayerInput.Pressed(true) && PlayerInput.GetSpecificDirection(3) && !IsExpectingInputNow(InputType.DIRECTION_LEFT_DOWN)) 
            {
                HandAnim.DoScaledAnimationAsync("Slap");
            }
        }

        public void HeadOut(double beat, int cueType)
        {
            if (cueType == 0) cueType = UnityEngine.Random.Range(1, 3);
            var sfx = new List<MultiSound.Sound>() {
                new MultiSound.Sound("mannequinFactory/drum", beat      ),
                new MultiSound.Sound("mannequinFactory/drum", beat + 0.5),
                new MultiSound.Sound("mannequinFactory/drum", beat + 1.5),
                new MultiSound.Sound("mannequinFactory/drum", beat + 2  ),
            };
            if (cueType == 1) {
                for (int i = 0; i < 7; i++) {
                    sfx.Add(new MultiSound.Sound($"mannequinFactory/drumroll{i + 1}", beat + 3 + (i * 0.1667)));
                }
            } else {
                sfx.AddRange(new MultiSound.Sound[] {
                    new MultiSound.Sound("mannequinFactory/drum", beat + 0.75),
                    new MultiSound.Sound("mannequinFactory/drum", beat + 1   ),
                });
            }
            MultiSound.Play(sfx.ToArray());

            MannequinHead head = Instantiate(MannequinHeadObject, gameObject.transform).GetComponent<MannequinHead>();
            head.startBeat = beat;
            head.withClap = (cueType == 2);
        }

        public void ChangeSignText(string text)
        {
            SignText.text = text;
        }

        public void BackgroundColor(Color start, Color end, float beats, bool instant)
        {
            if (bgColorTween != null) bgColorTween.Kill(true);

            bg.color = instant ? end : start;
            if (!instant) bgColorTween = bg.DOColor(end, Conductor.instance.secPerBeat * beats);
        }
    }
}
