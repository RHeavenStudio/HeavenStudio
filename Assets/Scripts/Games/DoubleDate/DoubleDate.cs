using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlDoubleDateLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("doubleDate", "Double Date", "ef854a", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; DoubleDate.instance.Bop(e.beat, e.length, e["bop"], e["autoBop"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Toggle if the two couples should bop for the duration of this event."),
                        new Param("autoBop", false, "Bop (Auto)", "Toggle if the two couples should automatically bop until another Bop event is reached.")
                    }
                },
                new GameAction("soccer", "Soccer Ball")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; DoubleDate.QueueSoccerBall(e.beat, e["b"]); },
                    preFunctionLength = 1f,
                    defaultLength = 2f,
                    parameters = new()
                    {
                        new("b", false, "Weasels Jump", "Toggle if the weasels should jump upon successfully hitting the cue.")
                    }
                },
                new GameAction("basket", "Basket Ball")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; DoubleDate.QueueBasketBall(e.beat, e["b"]); },
                    preFunctionLength = 1f,
                    defaultLength = 2f,
                    parameters = new()
                    {
                        new("b", false, "Weasels Jump", "Toggle if the weasels should jump upon successfully hitting the cue.")
                    }
                },
                new GameAction("football", "Football")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; DoubleDate.QueueFootBall(e.beat, e["b"]); },
                    preFunctionLength = 1f,
                    defaultLength = 2.5f,
                    parameters = new()
                    {
                        new("b", true, "Weasels Jump", "Toggle if the weasels should jump upon successfully hitting the cue.")
                    }
                },
                new GameAction("blush", "Blush")
                {
                    function = delegate { DoubleDate.instance.GirlBlush(); }
                },
                new GameAction("toggleGirls", "Set Girls' Presence")
                {
                    function = delegate { DoubleDate.instance.ToggleGirls(eventCaller.currentEntity["b"]); },
                    defaultLength = 0.5f,
                    parameters = new()
                    {
                        new("b", false, "Present", "Toggle if the girl and the female weasel should appear.")
                    }
                },
                new GameAction("stare", "Boy Looks")
                {
                    function = delegate { DoubleDate.instance.ToggleStare(eventCaller.currentEntity["b"]); },
                    defaultLength = 0.5f,
                    parameters = new()
                    {
                        new("b", true, "Look", "Toggle if the boy should look at the girl.")
                    }
                },
                new GameAction("time", "Time of Day")
                {
                    function = delegate { DoubleDate.instance.SetTime(eventCaller.currentEntity["d"]); },
                    defaultLength = 0.5f,
                    parameters = new()
                    {
                        new("d", DoubleDate.DayTime.Noon, "Time", "Set the time of day.")
                    }
                }
            },
            new List<string>() {"rvl", "normal"},
            "rvldate", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_DoubleDate;

    public class DoubleDate : Minigame
    {
        [Header("Prefabs")]
        [SerializeField] GameObject soccer;
        [SerializeField] GameObject basket;
        [SerializeField] GameObject football;
        [SerializeField] GameObject dropShadow;
        [SerializeField] ParticleSystem leaves;

        [Header("Components")]
        [SerializeField] Animator boyAnim;
        [SerializeField] Animator girlAnim;
        [SerializeField] DoubleDateWeasels weasels;
        [SerializeField] Animator treeAnim;
        [SerializeField] GameObject clouds;
        [SerializeField] private GameObject girlObj;
        [SerializeField] private GameObject girlWeaselObj;
        [SerializeField] private GameObject girlWeaselShockObj;
        [SerializeField] private Material doubleDateCellAnim;
        [SerializeField] private SpriteRenderer bgSquare;
        [SerializeField] private SpriteRenderer bgGradient;
        [SerializeField] private Sprite bgIntro;
        [SerializeField] private Sprite bgLong;

        private Color squareColor;

        [Header("Variables")]
        [SerializeField] private Color _skyColor;
        [SerializeField] private Color noonColor;
        [SerializeField] private float _animSpeed = 1.25f;
        [SerializeField] public float cloudSpeed;
        [SerializeField] public float cloudDistance;
        [SerializeField] public float floorHeight;
        [SerializeField] public float shadowDepthScaleMin;
        [SerializeField] public float shadowDepthScaleMax;
        [SerializeField] SuperCurveObject.Path[] ballBouncePaths;
        double lastGirlGacha = double.MinValue;
        bool canBop = true;
        public static DoubleDate instance;
        public static List<QueuedBall> queuedBalls = new List<QueuedBall>();
        [NonSerialized] public double lastHitWeasel = double.MinValue;

        public enum BallType
        {
            Soccer,
            Basket,
            Football
        }

        public struct QueuedBall
        {
            public double beat;
            public BallType type;
            public bool jump;
        }

        public static PlayerInput.InputAction InputAction_TouchPress =
            new("RvlDateTouchPress", new int[] { IAEmptyCat, IAPressCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicPress, IA_Empty);
        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("RvlDateTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);

        // Editor gizmo to draw trajectories
        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (SuperCurveObject.Path path in ballBouncePaths)
            {
                if (path.preview)
                {
                    soccer.GetComponent<SoccerBall>().DrawEditorGizmo(path);
                }
            }
        }

        public void CheckGirlsPresence(double beat)
        {
            var allEvents = EventCaller.GetAllInGameManagerList("doubleDate", new string[] { "toggleGirls" }).FindAll(x => x.beat < beat);
            if (allEvents.Count == 0) return;
            allEvents.Sort((x, y) => x.beat.CompareTo(y.beat));

            ToggleGirls(allEvents[^1]["b"]);
        }

        public void CheckBoyStare(double beat)
        {
            var allEvents = EventCaller.GetAllInGameManagerList("doubleDate", new string[] { "stare" }).FindAll(x => x.beat < beat);
            if (allEvents.Count == 0) return;
            allEvents.Sort((x, y) => x.beat.CompareTo(y.beat));

            ToggleStare(allEvents[^1]["b"]);
        }

        public enum DayTime
        {
            Day,
            Noon
        }

        private void DayTimeCheck(double beat)
        {
            var allEvents = EventCaller.GetAllInGameManagerList("doubleDate", new string[] { "time" }).FindAll(x => x.beat < beat);
            if (allEvents.Count == 0) return;
            allEvents.Sort((x, y) => x.beat.CompareTo(y.beat));

            SetTime(allEvents[^1]["d"]);
        }

        public void SetTime(int time)
        {
            if (time == (int)DayTime.Noon)
            {
                doubleDateCellAnim.SetColor("_Color", noonColor);
                bgSquare.color = squareColor;
                bgGradient.sprite = bgLong;
                return;
            }

            doubleDateCellAnim.SetColor("_Color", Color.white);
            bgSquare.color = _skyColor;
            bgGradient.sprite = bgIntro;
        }

        public override void OnPlay(double beat)
        {
            queuedBalls.Clear();
            CheckGirlsPresence(beat);
            CheckBoyStare(beat);
            DayTimeCheck(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            CheckGirlsPresence(beat);
            CheckBoyStare(beat);
            DayTimeCheck(beat);
        }

        private void OnDestroy() {
            queuedBalls.Clear();
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        private void Awake()
        {
            instance = this;
            SetupBopRegion("doubleDate", "bop", "autoBop");
            doubleDateCellAnim.SetColor("_Color", noonColor);
            squareColor = bgSquare.color;
        }

        private void Start() {
            clouds.transform.position = Vector3.left * ((Time.realtimeSinceStartup * cloudSpeed) % cloudDistance);
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat)) SingleBop();
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedBalls.Count != 0)
                {
                    foreach (QueuedBall ball in queuedBalls)
                    {
                        switch (ball.type)
                        {
                            case BallType.Soccer:
                                SpawnSoccerBall(ball.beat, ball.jump);
                                break;
                            case BallType.Basket:
                                SpawnBasketBall(ball.beat, ball.jump);
                                break;
                            case BallType.Football:
                                SpawnFootBall(ball.beat, ball.jump);
                                break;
                        }
                    }
                    queuedBalls.Clear();
                }
            }
            else
            {
                if ((!cond.isPaused) && queuedBalls.Count != 0)
                {
                    queuedBalls.Clear();
                }
            }
            if (PlayerInput.GetIsAction(InputAction_TouchPress))
            {
                boyAnim.DoScaledAnimationAsync("Ready", _animSpeed);
            }
            if (PlayerInput.GetIsAction(InputAction_TouchRelease) && !IsExpectingInputNow(InputAction_FlickPress))
            {
                boyAnim.DoScaledAnimationAsync("UnReady", _animSpeed);
            }
            if (PlayerInput.GetIsAction(InputAction_FlickPress) && !IsExpectingInputNow(InputAction_FlickPress))
            {
                SoundByte.PlayOneShotGame("doubleDate/kick_whiff");
                Kick(true, true, false);
            }
            clouds.transform.position = Vector3.left * ((Time.realtimeSinceStartup * cloudSpeed) % cloudDistance);
        }

        public void GirlBlush()
        {
            girlAnim.DoScaledAnimationAsync("GirlBlush", _animSpeed);
        }

        public void ToggleGirls(bool active)
        {
            girlObj.SetActive(active);
            girlWeaselObj.SetActive(active);
            girlWeaselShockObj.SetActive(active);
        }

        private bool _isStaring = false;

        public void ToggleStare(bool active)
        {
            boyAnim.SetBool("Stare", active);
            _isStaring = active;
        }

        public void ToggleBop(bool go)
        {
            canBop = go;
        }

        public void Bop(double beat, float length, bool goBop, bool autoBop)
        {
            if (goBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate { SingleBop(); })
                    });
                }
            }
        }

        void SingleBop()
        {
            if (canBop)
            {
                boyAnim.DoScaledAnimationAsync(_isStaring ? "IdleBop2" : "IdleBop", _animSpeed);
            }
            if (Conductor.instance.songPositionInBeatsAsDouble > lastGirlGacha)
                girlAnim.DoScaledAnimationAsync("GirlBop", _animSpeed);
            weasels.Bop();
        }

        public void Kick(bool hit = true, bool forceNoLeaves = false, bool weaselsHappy = true, bool jump = false)
        {
            if (hit)
            {
                boyAnim.DoScaledAnimationAsync("Kick", _animSpeed);
                if (jump)
                {
                    weasels.Jump();
                    lastGirlGacha = Conductor.instance.songPositionInBeatsAsDouble + 0.5f;
                    girlAnim.DoScaledAnimationAsync("GirlLookUp", _animSpeed);
                }
                else if (weaselsHappy) weasels.Happy();
                if (!forceNoLeaves)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(Conductor.instance.songPositionInBeatsAsDouble + 1f, delegate
                        {
                            leaves.Play();
                            treeAnim.DoScaledAnimationAsync("TreeRustle", _animSpeed);
                        })
                    });
                }
            }
            else
            {
                boyAnim.DoScaledAnimationAsync("Barely", _animSpeed);
                weasels.Surprise();
            }
        }

        public static void QueueSoccerBall(double beat, bool shouldJump)
        {
            if (GameManager.instance.currentGame != "doubleDate")
            {
                queuedBalls.Add(new QueuedBall()
                {
                    beat = beat,
                    type = BallType.Soccer,
                    jump = shouldJump
                });
            }
            else
            {
                instance.SpawnSoccerBall(beat, shouldJump);
            }
            SoundByte.PlayOneShotGame("doubleDate/soccerBounce", beat, forcePlay: true);
        }

        public static void QueueBasketBall(double beat, bool shouldJump)
        {
            if (GameManager.instance.currentGame != "doubleDate")
            {
                queuedBalls.Add(new QueuedBall()
                {
                    beat = beat,
                    type = BallType.Basket,
                    jump = shouldJump
                });
            }
            else
            {
                instance.SpawnBasketBall(beat, shouldJump);
            }
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("doubleDate/basketballBounce", beat),
                new MultiSound.Sound("doubleDate/basketballBounce", beat + 0.75f),
            }, forcePlay: true);
        }

        public static void QueueFootBall(double beat, bool shouldJump)
        {
            if (GameManager.instance.currentGame != "doubleDate")
            {
                queuedBalls.Add(new QueuedBall()
                {
                    beat = beat,
                    type = BallType.Football,
                    jump = shouldJump
                });
            }
            else
            {
                instance.SpawnFootBall(beat, shouldJump);
            }
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("doubleDate/footballBounce", beat),
                new MultiSound.Sound("doubleDate/footballBounce", beat + 0.75f),
            }, forcePlay: true);
        }

        public void SpawnSoccerBall(double beat, bool shouldJump)
        {
            SoccerBall spawnedBall = Instantiate(soccer, instance.transform).GetComponent<SoccerBall>();
            spawnedBall.Init(beat, shouldJump);
        }

        public void SpawnBasketBall(double beat, bool shouldJump)
        {
            Basketball spawnedBall = Instantiate(basket, instance.transform).GetComponent<Basketball>();
            spawnedBall.Init(beat, shouldJump);
        }

        public void SpawnFootBall(double beat, bool shouldJump)
        {
            Football spawnedBall = Instantiate(football, instance.transform).GetComponent<Football>();
            spawnedBall.Init(beat, shouldJump);
        }

        public void MissKick(double beat, bool hit = false)
        {
            lastGirlGacha = Conductor.instance.songPositionInBeatsAsDouble + 1.5f;
            girlAnim.DoScaledAnimationAsync("GirlSad", _animSpeed);
            if (hit)
            {
                lastHitWeasel = Conductor.instance.songPositionInBeatsAsDouble;
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - (0.25f/3f), delegate { weasels.Hit(beat); }),
                });
            }
            else
            {
                lastHitWeasel = Conductor.instance.songPositionInBeatsAsDouble;
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.25, delegate { weasels.Hide(beat + 0.25f); }),
                });
            }
        }

        public SuperCurveObject.Path GetPath(string name)
        {
            foreach (SuperCurveObject.Path path in ballBouncePaths)
            {
                if (path.name == name)
                {
                    return path;
                }
            }
            return default(SuperCurveObject.Path);
        }

        public GameObject MakeDropShadow()
        {
            GameObject shadow = Instantiate(dropShadow, transform);
            return shadow;
        }
    }
}