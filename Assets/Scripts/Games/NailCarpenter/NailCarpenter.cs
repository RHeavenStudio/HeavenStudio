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
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; NailCarpenter.instance.ToggleBop(e.beat, e.length, e["toggle2"], e["toggle"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle2", true, "Bop", "Toggle if Boss should bop for the duration of this event."),
                        new Param("toggle", false, "Bop (Auto)", "Toggle if the man should automatically bop until another Bop event is reached.")
                    }
                },
                new GameAction("puddingNail", "Pudding Nail")
                {
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("cherryNail", "Cherry Nail")
                {
                    defaultLength = 2f,
                    resizable = true
                },
                new GameAction("cakeNail", "Cake Nail")
                {
                    defaultLength = 2f,
                    resizable = true
                },
                new GameAction("cakeLongNail", "Cake Long Nail")
                {
                    defaultLength = 2f,
                    resizable = true
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
        public GameObject basePudding;
        [SerializeField] ParticleSystem splashEffect;
        public Animator HammerArm;
        public Animator EffectSweat;
        public Animator EffectShock;

        public Transform scrollingHolder;
        public Transform nailHolder;
        public Transform boardTrans;
        const float nailDistance = -9f;
        const float boardWidth = 19.2f;
        float scrollRate => nailDistance / (Conductor.instance.pitchedSecPerBeat * 2f);

        private bool missed;
        private bool hasSlurped;

        enum sweetsType
        {
            Pudding=0,
            LayerCake=1,
        };


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
        protected static bool IA_TouchAltPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_AltStart);
        }

        protected static bool IA_PadAltRelease(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltRelease(out double dt)
        {
            return PlayerInput.GetSqueezeUp(out dt);
        }

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
            SetupBopRegion("nailCarpenter", "bop", "toggle");
        }

        public override void OnBeatPulse(double beat)
        {
            // if (BeatIsInBopRegion(beat)) SomenPlayer.DoScaledAnimationAsync("HeadBob", 0.5f);
        }

        void Update()
        {
            var cond = Conductor.instance;

            if (!cond.isPlaying) return;

            // Debug.Log(newBeat);

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                SoundByte.PlayOneShot("miss");
                HammerArm.DoScaledAnimationAsync("hammerHit", 0.5f);
                hasSlurped = false;
                EffectSweat.DoScaledAnimationAsync("BlobSweating", 0.5f);
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

            var sounds = new List<MultiSound.Sound>(){};
            // Spawn pudding and nail.
            for (int i = 0; i < pudNailEvents.Count; i++) {
                var nailBeat = pudNailEvents[i].beat;
                var nailLength = pudNailEvents[i].length;

                // Only consider nailgie events that aren't past the start point.
                if (startBeat <= nailBeat + nailLength) {
                    int nailInEvent = Mathf.CeilToInt(nailLength);
                    for (int b = 0; b < nailInEvent; b++)
                    {
                        var targetNailBeat = nailBeat + (1f * b);

                        if (startBeat <= targetNailBeat && targetNailBeat < endBeat)
                        {
                            sounds.Add(new MultiSound.Sound("nailCarpenter/one", targetNailBeat));
                            SpawnPudding(targetNailBeat, startBeat, (int)sweetsType.Pudding);
                            SpawnNail(targetNailBeat+0.5f, startBeat);
                        }
                    }
                }
            }
            // Spawn cherry and nail.
            for (int i = 0; i < chrNailEvents.Count; i++) {
                var nailBeat = chrNailEvents[i].beat;
                var nailLength = chrNailEvents[i].length;

                // Only consider nailgie events that aren't past the start point.
                if (startBeat <= nailBeat + nailLength) {
                    int nailInEvent = Mathf.CeilToInt(nailLength + 1) / 2;

                    for (int b = 0; b < nailInEvent; b++)
                    {
                        var targetNailBeat = nailBeat + (2f * b);
                        if (startBeat <= targetNailBeat && targetNailBeat < endBeat)
                        {
                            sounds.Add(new MultiSound.Sound("nailCarpenter/three", targetNailBeat));
                            SpawnPudding(targetNailBeat, startBeat, (int)sweetsType.Pudding);
                            SpawnNail(targetNailBeat+0.5f, startBeat);
                            SpawnNail(targetNailBeat+1.0f, startBeat);
                            SpawnNail(targetNailBeat+1.5f, startBeat);
                        }
                    }
                }
            }
            // Spawn cake and nail.
            for (int i = 0; i < cakeNailEvents.Count; i++) {
                var nailBeat = cakeNailEvents[i].beat;
                var nailLength = cakeNailEvents[i].length;

                // Only consider nailgie events that aren't past the start point.
                if (startBeat <= nailBeat + nailLength) {
                    int nailInEvent = Mathf.CeilToInt(nailLength + 1) / 2;

                    for (int b = 0; b < nailInEvent; b++)
                    {
                        var targetNailBeat = nailBeat + (2f * b);
                        if (startBeat <= targetNailBeat && targetNailBeat < endBeat)
                        {
                            sounds.Add(new MultiSound.Sound("nailCarpenter/alarm", targetNailBeat));
                            SpawnPudding(targetNailBeat, startBeat, (int)sweetsType.Pudding);
                            SpawnNail(targetNailBeat+0.5f, startBeat);
                            SpawnPudding(targetNailBeat+1.0f, startBeat, (int)sweetsType.Pudding);
                            SpawnNail(targetNailBeat+1.25f, startBeat);
                            SpawnNail(targetNailBeat+1.75f, startBeat);
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
                    int nailInEvent = Mathf.CeilToInt(nailLength + 1) / 2;

                    for (int b = 0; b < nailInEvent; b++)
                    {
                        var targetNailBeat = nailBeat + (2f * b);
                        if (startBeat <= targetNailBeat && targetNailBeat < endBeat)
                        {
                            sounds.Add(new MultiSound.Sound("nailCarpenter/signal1", targetNailBeat));
                            sounds.Add(new MultiSound.Sound("nailCarpenter/signal2", targetNailBeat+1f));
                            SpawnPudding(targetNailBeat, startBeat, (int)sweetsType.LayerCake);
                            SpawnNail(targetNailBeat+0.5f, startBeat);
                            SpawnLongNail(targetNailBeat+1f, startBeat);
                        }
                    }
                }
            }

            if (sounds.Count > 0) {
                MultiSound.Play(sounds.ToArray(), forcePlay: true);
            }
        }

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        public void ToggleBop(double beat, float length, bool bopOrNah, bool autoBop)
        {
            if (bopOrNah)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            // SomenPlayer.DoScaledAnimationAsync("HeadBob", 0.5f);
                        })
                    });
                }
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
        private void SpawnPudding(double beat, double startBeat, int sweetType)
        {
            var newPudding = Instantiate(basePudding, nailHolder).GetComponent<Pudding>();

            newPudding.targetBeat = beat;
            newPudding.puddingType = 2*sweetType;

            var puddingX = (beat - startBeat) * -nailDistance / 2f;
            newPudding.transform.localPosition = new Vector3((float)puddingX, 0f, 0f);
            newPudding.Init();
            newPudding.gameObject.SetActive(true);
        }
    }
}