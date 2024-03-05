using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using Jukebox;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class PcoNailLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("nailCarpenter", "Nail Carpenter", "fab96e", false, false, new List<GameAction>()
            {
                new GameAction("puddingNail", "Pudding Nail")
                {
                    function = delegate {NailCarpenter.instance.PlaySound();},
                    defaultLength = 8f,
                    resizable = true
                },
                new GameAction("cherryNail", "Cherry Nail")
                {
                    function = delegate {NailCarpenter.instance.PlaySound();},
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("cakeNail", "Cake Nail")
                {
                    function = delegate {NailCarpenter.instance.PlaySound();},
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("cakeLongNail", "Cake Long Nail")
                {
                    function = delegate {NailCarpenter.instance.PlaySound();},
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("slideShoji", "Slide Shoji")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        NailCarpenter.instance.SlideShoji(e.beat, e.length, e["fillRatio"], e["ease"], e["mute"]); 
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("fillRatio", new EntityTypes.Float(0f, 1f, 0.3f), "Ratio", "Set how much of the screen the shoji covers."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action."),
                        new Param("mute", false, "Mute", "Toggle if the cue should be muted.")
                    }
                },
            
            },
            new List<string>() { "pco", "normal" },
            "pconail", "en",
            new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_NailCarpenter;

    public class NailCarpenter : Minigame
    {
        public GameObject baseNail;
        public GameObject baseLongNail;
        public GameObject baseSweet;
        public Animator Carpenter;
        public Animator EyeAnim;
        public Animator EffectExclamRed;
        public Animator EffectExclamBlue;

        public Transform scrollingHolder;
        public Transform nailHolder;
        public Transform boardTrans;
        public Transform shojiTrans;
        const float nailDistance = -4f;
        const float boardWidth = 19.2f;
        float scrollRate => nailDistance / (Conductor.instance.pitchedSecPerBeat * 2f);

        private bool missed;
        private bool hasSlurped;

        const int IAAltDownCat = IAMAXCAT;
        const int IAAltUpCat = IAMAXCAT + 1;

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }

        protected static bool IA_TouchRegularPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt);
        }
        protected static bool IA_TouchAltPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_AltStart)
                && !instance.IsExpectingInputNow(InputAction_RegPress);
        }

        protected static bool IA_PadAltRelease(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltRelease(out double dt)
        {
            return PlayerInput.GetSqueezeUp(out dt);
        }


        public static PlayerInput.InputAction InputAction_RegPress =
            new("PcoNailRegStart", new int[] {  IAPressCat, IAPressCat, IAPressCat },
            IA_PadBasicPress, IA_TouchRegularPress, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_AltStart =
            new("PcoNailAltStart", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);
        public static PlayerInput.InputAction InputAction_AltFinish =
            new("PcoNailAltFinish", new int[] { IAAltUpCat, IAFlickCat, IAAltUpCat },
            IA_PadAltRelease, IA_TouchFlick, IA_BatonAltRelease);
        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("PcoNailTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);

        public static NailCarpenter instance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }

        double slideBeat = double.MaxValue;
        double slideLength;
        Util.EasingFunction.Ease slideEase;
        float slideRatioLast = 0, slideRatioNext = 0;


        void Update()
        {
            var cond = Conductor.instance;
            var currentBeat = cond.songPositionInBeatsAsDouble;

            if (!cond.isPlaying) return;

            // Debug.Log(newBeat);

            if (PlayerInput.GetIsAction(InputAction_RegPress) && !IsExpectingInputNow(InputAction_RegPress))
            {
                SoundByte.PlayOneShot("miss");
                Carpenter.DoScaledAnimationAsync("carpenterHit", 0.25f);
                hasSlurped = false;
                // ScoreMiss();
            }
            if (PlayerInput.GetIsAction(InputAction_AltStart) && !IsExpectingInputNow(InputAction_AltStart))
            {
                SoundByte.PlayOneShot("miss");
                Carpenter.DoScaledAnimationAsync("carpenterHit", 0.25f);
                hasSlurped = false;
                // ScoreMiss();
            }

            // Object scroll.
            var scrollPos = scrollingHolder.localPosition;
            var newScrollX = scrollPos.x + (scrollRate * Time.deltaTime);
            scrollingHolder.localPosition = new Vector3(newScrollX, scrollPos.y, scrollPos.z);

            // Board scroll.
            var boardPos = boardTrans.localPosition;
            var newBoardX = boardPos.x + (scrollRate * Time.deltaTime);
            newBoardX %= boardWidth;
            boardTrans.localPosition = new Vector3(newBoardX, boardPos.y, boardPos.z);

            UpdateShoji(currentBeat);
        }

        public override void OnGameSwitch(double beat)
        {
            double startBeat;
            double endBeat = double.MaxValue;
            var entities = GameManager.instance.Beatmap.Entities;

            startBeat = beat;
            // find out when the next game switch (or remix end) happens
            RiqEntity firstEnd = entities.Find(c => (c.datamodel.StartsWith("gameManager/switchGame") || c.datamodel.Equals("gameManager/end")) && c.beat > startBeat);
            endBeat = firstEnd?.beat ?? double.MaxValue;

            // Nail events.
            List<RiqEntity> pudNailEvents = entities.FindAll(v => v.datamodel == "nailCarpenter/puddingNail");
            List<RiqEntity> chrNailEvents = entities.FindAll(v => v.datamodel == "nailCarpenter/cherryNail");
            List<RiqEntity> cakeNailEvents = entities.FindAll(v => v.datamodel == "nailCarpenter/cakeNail");
            List<RiqEntity> cklNailEvents = entities.FindAll(v => v.datamodel == "nailCarpenter/cakeLongNail");

            var cherryTargetBeats = new List<double>(){};

            // Spawn cake and nail.
            for (int i = 0; i < cakeNailEvents.Count; i++) {
                var nailBeat = cakeNailEvents[i].beat;
                var nailLength = cakeNailEvents[i].length;

                // Only consider nailgie events that aren't past the start point.
                if (startBeat <= nailBeat + nailLength) {
                    int nailInEvent = Mathf.CeilToInt(nailLength + 2) / 4;

                    for (int b = 0; b < nailInEvent; b++)
                    {
                        var targetNailBeat = nailBeat + (4f * b);
                        if (startBeat <= targetNailBeat && targetNailBeat < endBeat)
                        {
                            sounds.Add(new MultiSound.Sound("nailCarpenter/alarm", targetNailBeat));
                            BeatAction.New(instance, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(targetNailBeat, delegate
                                {
                                    EffectExclamRed.DoScaledAnimationAsync("exclamAppear", 0.25f);
                                })
                            });
                            SpawnSweet(targetNailBeat, startBeat,
                                (b==0 ? Sweet.sweetsType.ShortCake : Sweet.sweetsType.Cherry));
                            SpawnNail(targetNailBeat+1f, startBeat);
                            SpawnSweet(targetNailBeat+2f, startBeat, Sweet.sweetsType.Cherry);
                            SpawnNail(targetNailBeat+2.5f, startBeat);
                            SpawnNail(targetNailBeat+3.5f, startBeat);
                        }
                    }
                    cherryTargetBeats.Add(nailBeat + 4f * nailInEvent);
                }
            }
            // Spawn pudding and nail.
            for (int i = 0; i < pudNailEvents.Count; i++) {
                var nailBeat = pudNailEvents[i].beat;
                var nailLength = pudNailEvents[i].length;

                // Only consider nailgie events that aren't past the start point.
                if (startBeat <= nailBeat + nailLength) {
                    int nailInEvent = Mathf.CeilToInt(nailLength)/2;
                    for (int b = 0; b < nailInEvent; b++)
                    {
                        var targetNailBeat = nailBeat + (2f * b);

                        if (startBeat <= targetNailBeat && targetNailBeat < endBeat)
                        {
                            sounds.Add(new MultiSound.Sound("nailCarpenter/one", targetNailBeat));
                            SpawnSweet(targetNailBeat, startBeat,
                                (IsInRange(cherryTargetBeats, targetNailBeat) ? Sweet.sweetsType.Cherry :
                                Sweet.sweetsType.Pudding));
                            SpawnNail(targetNailBeat+1f, startBeat);
                        }
                    }
                }
            }
            // Spawn cherrypudding and nail.
            for (int i = 0; i < chrNailEvents.Count; i++) {
                var nailBeat = chrNailEvents[i].beat;
                var nailLength = chrNailEvents[i].length;

                // Only consider nailgie events that aren't past the start point.
                if (startBeat <= nailBeat + nailLength) {
                    int nailInEvent = Mathf.CeilToInt(nailLength + 1f) / 4;

                    for (int b = 0; b < nailInEvent; b++)
                    {
                        var targetNailBeat = nailBeat + (4f * b);
                        if (startBeat <= targetNailBeat && targetNailBeat < endBeat)
                        {
                            sounds.Add(new MultiSound.Sound("nailCarpenter/three", targetNailBeat));
                            SpawnSweet(targetNailBeat, startBeat,
                                (IsInRange(cherryTargetBeats, targetNailBeat) ? Sweet.sweetsType.Cherry :
                                Sweet.sweetsType.CherryPudding));
                            SpawnNail(targetNailBeat+1f, startBeat);
                            SpawnNail(targetNailBeat+2f, startBeat);
                            SpawnNail(targetNailBeat+3f, startBeat);
                        }
                    }
                }
            }
            // Spawn long nail.
            for (int i = 0; i < cklNailEvents.Count; i++) {
                var nailBeat = cklNailEvents[i].beat;
                var nailLength = cklNailEvents[i].length;

                // Only consider nailgie events that aren't past the start point.
                if (startBeat <= nailBeat + nailLength) {
                    int nailInEvent = Mathf.CeilToInt(nailLength + 2) / 4;

                    for (int b = 0; b < nailInEvent; b++)
                    {
                        var targetNailBeat = nailBeat + (4f * b);
                        if (startBeat <= targetNailBeat && targetNailBeat < endBeat)
                        {
                            sounds.Add(new MultiSound.Sound("nailCarpenter/signal1", targetNailBeat));
                            sounds.Add(new MultiSound.Sound("nailCarpenter/signal2", targetNailBeat+1f));
                            BeatAction.New(instance, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(targetNailBeat, delegate
                                {
                                    EffectExclamBlue.DoScaledAnimationAsync("exclamAppear", 0.25f);
                                }),
                                new BeatAction.Action(targetNailBeat+1.5f, delegate
                                {
                                    Carpenter.DoScaledAnimationAsync("carpenterArmUp", 0.25f);
                                }),
                            });
                            SpawnSweet(targetNailBeat, startBeat,
                                (IsInRange(cherryTargetBeats, targetNailBeat) ? Sweet.sweetsType.Cherry :
                                Sweet.sweetsType.LayerCake));
                            SpawnNail(targetNailBeat+1f, startBeat);
                            SpawnLongNail(targetNailBeat+2f, startBeat);
                        }
                    }
                }
            }
        }

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        public void SlideShoji(double beat, double length, float fillRatio, int ease, bool mute)
        {
            if (!mute) MultiSound.Play(new MultiSound.Sound[]{ new MultiSound.Sound("nailCarpenter/open", beat)});
            slideBeat = beat;
            slideLength = length;
            slideEase = (Util.EasingFunction.Ease)ease;
            slideRatioLast = slideRatioNext;
            slideRatioNext = fillRatio;
        }
        void UpdateShoji(double beat)
        {
            if (beat >= slideBeat)
            {
                float slideLast = 17.8f *(1-slideRatioLast);
                float slideNext = 17.8f *(1-slideRatioNext);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(slideEase);
                float slideProg = Conductor.instance.GetPositionFromBeat(slideBeat, slideLength, true);
                slideProg = Mathf.Clamp01(slideProg);
                float slide = func(slideLast, slideNext, slideProg);
                shojiTrans.localPosition = new Vector3(slide, 0, 0);
            }
        }

        private void SpawnNail(double beat, double startBeat)
        {
            var newNail = Instantiate(baseNail, nailHolder).GetComponent<Nail>();

            newNail.targetBeat = beat;

            var nailX = (beat - startBeat) * -nailDistance / 2f;
            newNail.transform.localPosition = new Vector3((float)nailX, 0f, 0f);
            newNail.Init();
            newNail.gameObject.SetActive(true);
        }
        private void SpawnLongNail(double beat, double startBeat)
        {
            var newNail = Instantiate(baseLongNail, nailHolder).GetComponent<LongNail>();

            newNail.targetBeat = beat;

            var nailX = (beat - startBeat + 0.5f) * -nailDistance / 2f;
            newNail.transform.localPosition = new Vector3((float)nailX, 0f, 0f);
            newNail.Init();
            newNail.gameObject.SetActive(true);
        }
        private void SpawnSweet(double beat, double startBeat, Sweet.sweetsType sweetType)
        {
            var newSweet = Instantiate(baseSweet, nailHolder).GetComponent<Sweet>();

            newSweet.targetBeat = beat;
            newSweet.sweetType = sweetType;

            var sweetX = (beat - startBeat) * -nailDistance / 2f;
            newSweet.transform.localPosition = new Vector3((float)sweetX, 0f, 0f);
            newSweet.gameObject.SetActive(true);
            newSweet.Init();
        }

        bool IsInRange(List<double> list, double num)
        {
        foreach (double item in list)
        {
            if (num >= item && num <= item + .5f)
            {
                return true;
            }
        }
        return false;
        }


        // MultiSound.Play may not work in OnPlay (OnGameSwitch?), so I play the audio using an alternative method.
        List<MultiSound.Sound> sounds = new List<MultiSound.Sound>(){};
        bool isPlayed = false;
        public void PlaySound()
        {
            if (isPlayed) return;
            if (sounds.Count > 0) {
                MultiSound.Play(sounds.ToArray());
                isPlayed = true;
                sounds = null;
            }
        }

    }
}