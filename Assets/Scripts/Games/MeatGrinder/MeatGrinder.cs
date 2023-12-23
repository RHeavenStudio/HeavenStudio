using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class PcoMeatLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            List<Param> reactionParams = new() {
                new Param("tackReaction", MeatGrinder.TackExpressions.None, "Tack Reaction", "If this is hit, what expression should tack do?", new List<Param.CollapseParam>() {
                    new((x, y) => (int)x != (int)MeatGrinder.TackExpressions.None, new string[] { "tackReactionBeats" }),
                }),
                new Param("tackReactionBeats", new EntityTypes.Float(0.5f, 10, 1), "Tack React After", "The amount of beats to wait until tack reacts"),
                new Param("bossReaction", MeatGrinder.BossExpressions.None, "Boss Reaction", "If this is hit, what expression should boss do?", new List<Param.CollapseParam>() {
                    new((x, y) => (int)x != (int)MeatGrinder.BossExpressions.None, new string[] { "bossReactionBeats" }),
                }),
                new Param("bossReactionBeats", new EntityTypes.Float(0, 10, 0), "Boss React After", "The amount of beats to wait until boss reacts"),
            };

            return new Minigame("meatGrinder", "Meat Grinder", "501d18", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MeatGrinder.instance.Bop(e.beat, e.length, e["bop"], e["bossBop"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Boss Bops?", "Does Boss bop?"),
                        new Param("bossBop", false, "Boss Bops? (Auto)", "Does Boss Auto bop?"),
                    },
                    resizable = true,
                    priority = 1,
                },
                new GameAction("MeatToss", "Meat Toss")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        SoundByte.PlayOneShotGame("meatGrinder/toss", forcePlay: true);
                        MeatGrinder.instance.MeatToss(e.beat, e["bacon"], e["tackReaction"], e["tackReactionBeats"], e["bossReaction"], e["bossReactionBeats"]);
                    },
                    inactiveFunction = delegate {
                        SoundByte.PlayOneShotGame("meatGrinder/toss", forcePlay: true);
                        MeatGrinder.QueueMeatToss(eventCaller.currentEntity);
                    },
                    defaultLength = 2f,
                    priority = 2,
                    parameters = new List<Param>()
                    {
                        new Param("bacon", false, "Bacon Ball", "Throw a bacon ball instead of the typical meat"),
                    }.Concat(reactionParams).ToList(), // doing this because i want these params to always be the same
                },
                new GameAction("StartInterval", "Start Interval")
                {
                    defaultLength = 4f,
                    resizable = true,
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        MeatGrinder.PreInterval(e.beat, e.length, e["auto"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn")
                    },
                    preFunctionLength = 1
                },
                new GameAction("MeatCall", "Meat Call")
                {
                    inactiveFunction = delegate { SoundByte.PlayOneShotGame("meatGrinder/signal", forcePlay: true); },
                    defaultLength = 0.5f,
                    priority = 2,
                    preFunctionLength = 1f,
                    parameters = reactionParams,
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    preFunction = delegate { MeatGrinder.PrePassTurn(eventCaller.currentEntity.beat); },
                    preFunctionLength = 1
                },
                new GameAction("expressions", "Expressions")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MeatGrinder.instance.DoExpressions(e["tackExpression"], e["bossExpression"]);
                    },
                    parameters = new List<Param>() {
                        new Param("tackExpression", MeatGrinder.TackExpressions.Content, "Tack Expression", "The expression Tack will display"),
                        new Param("bossExpression", MeatGrinder.BossExpressions.None, "Boss Expression", "The expression Boss will display"),
                    }
                },
                new GameAction("cartGuy", "Cart Guy")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MeatGrinder.instance.CartGuy(e.beat, e.length, e["spider"], e["direction"], e["ease"]);
                    },
                    resizable = true,
                    defaultLength = 16,
                    parameters = new List<Param>() {
                        new Param("spider", false, "On Phone", "Put a spider in the box?"),
                        new Param("direction", MeatGrinder.CartGuyDirection.Right, "Direction", "The direction the cart will be carted to."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "What ease will the cart use?"),
                    }
                },
                new GameAction("gears", "Gears")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MeatGrinder.instance.Gears(e.beat, e.length, e["ease"], e["type"], e["speed"]);
                    },
                    resizable = true,
                    defaultLength = 1,
                    parameters = new List<Param>() {
                        new Param("type", MeatGrinder.GearMode.Constant, "Ease", "What type of movement will the gears use?", new List<Param.CollapseParam>() {
                            new((x, y) => (int)x != (int)MeatGrinder.GearMode.None, new string[] { "ease" }),
                        }),
                        new Param("speed", new EntityTypes.Float(0, 10, 1), "Speed", "How fast will the gears go?"),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "What ease will the gears speed up with?"),
                    }
                },
            }
            // ,
            // new List<string>() { "pco", "normal", "repeat" },
            // "pcomeat", "en",
            // new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_MeatGrinder;

    public class MeatGrinder : Minigame
    {
        private static List<QueuedInterval> queuedIntervals = new();

        private struct QueuedInterval
        {
            public double beat;
            public float length;
            public bool autoPassTurn;
        }

        public struct UpdateEasing
        {
            public double beat;
            public float length;
            public Util.EasingFunction.Ease ease;
        }

        public struct Reaction
        {
            public int expression;
            public double beat;
            public Reaction(int expression, double beat) {
                this.expression = expression;
                this.beat = beat;
            }
        }

        private static List<RiqEntity> queuedMeats = new();

        [Header("Objects")]
        public GameObject MeatBase;

        [Header("Animators")]
        public Animator BossAnim;
        public Animator TackAnim;
        [SerializeField] Animator CartGuyParentAnim;
        [SerializeField] Animator CartGuyAnim;
        [SerializeField] Animator[] GearAnims;

        [Header("Variables")]
        private bool bossBop = true;
        public bool bossAnnoyed = false;
        private UpdateEasing cartEase;
        private bool cartPhone = false;
        private string cartDir = "Left";
        private UpdateEasing gearEase;
        private GearMode gearType = GearMode.Constant;
        private float oldGearSpeed = 1;
        private float currentGearSpeed = 1;
        private float newGearSpeed = 1;
        private const string sfxName = "meatGrinder/";

        public static MeatGrinder instance;

        public enum TackExpressions
        {
            None,
            Content,
            Smug,
            Wonder,
        }

        public enum BossExpressions
        {
            None,
            Eyebrow,
            Scared,
        }

        public enum CartGuyDirection
        {
            Right,
            Left,
        }

        public enum GearMode
        {
            None,
            Constant,
            Beat,
        }

        protected static bool IA_PadAny(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }

        public static PlayerInput.InputAction InputAction_Press =
            new("PcoMeatPress", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadAny, IA_TouchBasicPress, IA_BatonBasicPress);

        private void Awake()
        {
            instance = this;
            SetupBopRegion("meatGrinder", "bop", "bossBop");
            MeatBase.SetActive(false);
        }

        private void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_Press) && !IsExpectingInputNow(InputAction_Press))
            {
                TackAnim.DoScaledAnimationAsync("TackEmptyHit", 0.5f);
                TackAnim.SetBool("tackMeated", false);
                SoundByte.PlayOneShotGame("meatGrinder/whiff");
                bossAnnoyed = false;
            }

            if (bossAnnoyed) BossAnim.SetBool("bossAnnoyed", true);

            if (passedTurns.Count > 0)
            {
                foreach (double pass in passedTurns)
                {
                    PassTurnStandalone(pass);
                }
                passedTurns.Clear();
            }

            if (gearEase.length != 0)
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(gearEase.beat, gearEase.length);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(gearEase.ease);
                currentGearSpeed = func(oldGearSpeed, newGearSpeed, normalizedBeat);
                Debug.Log(currentGearSpeed);
                if (normalizedBeat >= 1) cartEase.length = 0;
            }

            if (cartEase.length != 0)
            {
                // CartGuyParentAnim.gameObject.SetActive(true);
                if (cartPhone) CartGuyAnim.Play("Phone", 0, 0);
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(cartEase.beat, cartEase.length);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(cartEase.ease);
                float newPos = func(0f, 1f, normalizedBeat);
                CartGuyParentAnim.DoNormalizedAnimation($"Move{cartDir}", newPos);
                if (normalizedBeat >= 1) cartEase.length = 0;
            }

            CartGuyParentAnim.gameObject.SetActive(cartEase.length != 0);

            if (gearType != GearMode.Beat) {
                UpdateGears();
            }
        }

        public override void OnLateBeatPulse(double beat)
        {
            if (!BossAnim.IsPlayingAnimationNames("BossCall", "BossSignal", "BossScared") && BeatIsInBopRegion(beat))
            {
                BossAnim.DoScaledAnimationAsync(bossAnnoyed ? "BossMiss" : "Bop", 0.5f);
            }

            if (CartGuyParentAnim.gameObject.activeSelf) {
                Debug.Log(cartPhone ? "PhoneBop" : "Bop");
                CartGuyAnim.DoScaledAnimationAsync(cartPhone ? "PhoneBop" : "Bop", 0.5f);
            }

            if (gearType == GearMode.Beat) {
                UpdateGears();
            }
        }

        private void UpdateGears()
        {
            Action<Animator> action = gearType switch {
                GearMode.Constant => gear => gear.DoNormalizedAnimation(gear.gameObject.name == "Small" ? "SpinForeverLeft" : "SpinForeverRight", (Conductor.instance.songPositionInBeats / (10 * newGearSpeed)) % 1),
                GearMode.Beat =>     gear => gear.DoScaledAnimationAsync(gear.gameObject.name == "Small" ? "SpinOnceLeft" : "SpinOnceRight", 0.5f),
                _ =>                 gear => gear.Play("Nothing", 0, 0), // GearMode.None
            };
            foreach (Animator gear in GearAnims) {
                action.Invoke(gear);
            }
        }

        public override void OnGameSwitch(double beat)
        {
            if (queuedIntervals.Count > 0)
            {
                foreach (var interval in queuedIntervals) StartInterval(interval.beat, interval.length, beat, interval.autoPassTurn);
                queuedIntervals.Clear();
            }
            if (queuedMeats.Count > 0)
            {
                foreach (var meat in queuedMeats) MeatToss(meat.beat, meat["bacon"], meat["tackReaction"], meat["tackReactionBeats"], meat["bossReaction"], meat["bossReactionBeats"]);
                queuedMeats.Clear();
            }
            OnPlay(beat);
        }

        public override void OnPlay(double beat)
        {
            RiqEntity cg = GameManager.instance.Beatmap.Entities.Find(c => c.datamodel == "meatGrinder/cartGuy");
            if (cg != null)
            {
                CartGuy(cg.beat, cg.length, cg["spider"], cg["direction"], cg["ease"]);
            }
        }

        private List<RiqEntity> GetRelevantMeatCallsBetweenBeat(double beat, double endBeat)
        {
            return EventCaller.GetAllInGameManagerList("meatGrinder", new string[] { "MeatCall" }).FindAll(x => x.beat >= beat && x.beat < endBeat);
        }

        public void Bop(double beat, double length, bool doesBop, bool autoBop)
        {
            bossBop = autoBop;
            if (doesBop)
            {
                var actions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    actions.Add(new BeatAction.Action(beat + i, delegate {
                        if (!BossAnim.IsPlayingAnimationNames("BossCall", "BossSignal")) {
                            BossAnim.DoScaledAnimationAsync(bossAnnoyed ? "BossMiss" : "Bop", 0.5f);
                        }
                    }));
                }
                BeatAction.New(instance, actions);
            }
        }

        public void DoExpressions(int tackExpression, int bossExpression = 0)
        {
            if (tackExpression != (int)TackExpressions.None) {
                string tackAnim = ((TackExpressions)tackExpression).ToString();
                TackAnim.DoScaledAnimationAsync("Tack" + tackAnim, 0.5f);
            }
            if (bossExpression != (int)BossExpressions.None) {
                string bossAnim = ((BossExpressions)bossExpression).ToString();
                BossAnim.DoScaledAnimationAsync("Boss" + bossAnim, 0.5f);
            }
        }

        public void CartGuy(double beat, float length, bool spider, int direction, int ease)
        {
            cartEase = new() {
                beat = beat,
                length = length,
                ease = (Util.EasingFunction.Ease)ease,
            };
            cartPhone = spider;
            cartDir = direction == 0 ? "Right" : "Left";
        }

        public void Gears(double beat, float length, int ease, int type, float speed)
        {
            gearEase = new() {
                beat = beat,
                length = length,
                ease = (Util.EasingFunction.Ease)ease,
            };
            gearType = (GearMode)type;

            oldGearSpeed = newGearSpeed;
            newGearSpeed = speed;
        }

        public static void PreInterval(double beat, float length, bool autoPassTurn)
        {
            SoundByte.PlayOneShotGame("meatGrinder/startSignal", beat - 1, forcePlay: true);

            if (GameManager.instance.currentGame == "meatGrinder")
            {
                instance.StartInterval(beat, length, beat, autoPassTurn);
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval()
                {
                    beat = beat,
                    length = length,
                    autoPassTurn = autoPassTurn
                });
            }
        }

        public void StartInterval(double beat, float length, double gameSwitchBeat, bool autoPassTurn)
        {
            List<BeatAction.Action> actions = new() {
                new(beat - 1, delegate { BossAnim.DoScaledAnimationFromBeatAsync("BossSignal", 0.5f, beat - 1); }),
            };

            var allCallEvents = GetRelevantMeatCallsBetweenBeat(beat, beat + length);
            allCallEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
            for (int i = 0; i < allCallEvents.Count; i++)
            {
                double eventBeat = allCallEvents[i].beat;

                if (eventBeat >= gameSwitchBeat)
                {
                    actions.Add(new BeatAction.Action(eventBeat, delegate
                    {
                        BossAnim.DoScaledAnimationAsync("BossCall", 0.5f);
                        SoundByte.PlayOneShotGame("meatGrinder/signal");
                    }));
                }
            }

            BeatAction.New(this, actions);

            if (autoPassTurn)
            {
                PassTurn(beat + length, beat, length, allCallEvents);
            }
        }

        public static void QueueMeatToss(RiqEntity entity)
        {
            queuedMeats.Add(entity);
        }

        public void MeatToss(double beat, bool bacon, int tackReaction, float tackReactionBeats, int bossReaction, float bossReactionBeats)
        {
            Meat meat = Instantiate(MeatBase, transform).GetComponent<Meat>();
            meat.gameObject.SetActive(true);
            meat.startBeat = beat;
            meat.meatType = bacon ? Meat.MeatType.BaconBall : Meat.MeatType.DarkMeat;
            meat.tackReaction = new Reaction(tackReaction, tackReactionBeats);
            meat.bossReaction = new Reaction(bossReaction, bossReactionBeats);
            // meat.reaction = reaction;
        }

        public static void PrePassTurn(double beat)
        {
            if (GameManager.instance.currentGame == "meatGrinder")
            {
                instance.PassTurnStandalone(beat);
            }
            else
            {
                passedTurns.Add(beat);
            }
        }

        private static List<double> passedTurns = new();

        private void PassTurnStandalone(double beat)
        {
            var lastInterval = EventCaller.GetAllInGameManagerList("meatGrinder", new string[] { "StartInterval" }).FindLast(x => x.beat <= beat);
            if (lastInterval != null) PassTurn(beat, lastInterval.beat, lastInterval.length);
        }

        private void PassTurn(double beat, double intervalBeat, float intervalLength, List<RiqEntity> allCallEvents = null)
        {
            if (allCallEvents == null) {
                allCallEvents = GetRelevantMeatCallsBetweenBeat(intervalBeat, intervalBeat + intervalLength);
                allCallEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
            }
            List<BeatAction.Action> meatCalls = new();
            for (int i = 0; i < allCallEvents.Count; i++)
            {
                double relativeBeat = allCallEvents[i].beat - intervalBeat;
                var tackReaction = new Reaction(allCallEvents[i]["tackReaction"], allCallEvents[i]["tackReactionBeats"]);
                var bossReaction = new Reaction(allCallEvents[i]["bossReaction"], allCallEvents[i]["bossReactionBeats"]);
                meatCalls.Add(new BeatAction.Action(beat + relativeBeat - 1, delegate
                {
                    Meat meat = Instantiate(MeatBase, transform).GetComponent<Meat>();
                    meat.gameObject.SetActive(true);
                    meat.startBeat = beat + relativeBeat - 1;
                    meat.meatType = Meat.MeatType.LightMeat;
                    meat.tackReaction = tackReaction;
                    meat.bossReaction = bossReaction;
                }));
            }
            BeatAction.New(this, meatCalls);
        }
    }
}