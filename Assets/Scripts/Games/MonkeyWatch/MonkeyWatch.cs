using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using Jukebox;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlMonkeyWatchLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("monkeyWatch", "Monkey Watch", "f0338d", false, false, new List<GameAction>()
            {
                new GameAction("monkeysAppear", "Monkeys Appear")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MonkeyWatch.instance.MonkeysAppear(e.beat, e.length, e["amount"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("amount", new EntityTypes.Integer(1, 8, 4), "Monkeys to Spawn", "How many monkeys will spawn over the course of this block"),
                    },
                    defaultLength = 8f,
                    resizable = true
                },
                new GameAction("startClapping", "Start Clapping")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MonkeyWatch.StartClapping(e.beat, e["start"]);
                    },
                    preFunction = delegate {
                        MonkeyWatch.PreStartClapping(eventCaller.currentEntity.beat);
                    },
                    preFunctionLength = 1,
                    parameters = new List<Param>()
                    {
                        new Param("start", new EntityTypes.Integer(0, 60, 0), "Start Where", "Where on the clock will the monkey start? (Set to zero to be automatic)"),
                    },
                    defaultLength = 2f,
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        MonkeyWatch.StartClapping(e.beat, e["start"]);
                    },
                },
                new GameAction("offbeatMonkeys", "Offbeat Monkeys")
                {
                    defaultLength = 4,
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute Cue", "Mute the offbeat monkeys's cue"),
                    },
                    resizable = true,
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        MonkeyWatch.WarnPinkMonkeys(e.beat, e.length, e["mute"]);
                    },
                },
                new GameAction("customMonkey", "Custom Monkey")
                {
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("sfx", MonkeyWatch.SfxTypes.First, "Which SFX", "Choose between the first and second \"ki\" sfx")
                    },
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        MonkeyWatch.WarnPinkMonkeys(e.beat, e.length, e["mute"]);
                    },
                },
                new GameAction("monkeyModifiers", "Monkey Modifiers")
                {
                    //preFunction = delegate { var e = eventCaller.currentEntity; MonkeyWatch.OnbeatSwitch(e.beat); },
                    defaultLength = 2f
                },
                new GameAction("balloonMonkey", "Balloon Monkey")
                {
                    //preFunction = delegate { var e = eventCaller.currentEntity; MonkeyWatch.OnbeatSwitch(e.beat); },
                    defaultLength = 2f
                },
            }/*,
            new List<string>() {"rvl", "keep"},
            "rvlwatch", "en",
            new List<string>() {} */
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MonkeyWatch;
    public class MonkeyWatch : Minigame
    {
        [Header("Animators")]
        [SerializeField] MonkeyClicker monkeyPlayer;

        [Header("Objects")]
        [SerializeField] GameObject yellowMonkey;
        [SerializeField] GameObject pinkMonkey;
        [SerializeField] Transform watchHoleParent;
        [SerializeField] Transform monkeyParent;
        public Transform cameraTransform;

        // unserialized variables below
        public enum SfxTypes 
        {
            None,
            First,
            Second,
            Onbeat,
        }

        static List<double> queuedInputs = new();
        Dictionary<double, OffbeatMonkey> offbeatMonkeys = new Dictionary<double, OffbeatMonkey>();
        List<RiqEntity> customMonkeys = new List<RiqEntity>();
        List<GameObject> watchHoles = new List<GameObject>();
        Dictionary<int, Animator> activeMonkeys = new Dictionary<int, Animator>();

        public struct OffbeatMonkey
        {
            public double beat;
            public bool mute;
            public float length;
        }

        static OffbeatMonkey wantOffbeat;
        static double wantClapping = double.MinValue;
        public static bool startedClapping;
        static bool wantPrepare;
        bool cameraMove;
        int currentMonkey;
        static int lastMonkeySpawned = 0;

        public static MonkeyWatch instance;

        void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            Update();
        }

        public override void OnGameSwitch(double beat)
        {
            var events = GameManager.instance.Beatmap.Entities.FindAll(c => c.datamodel.Split('/')[0] == "monkeyWatch");
            customMonkeys = events.FindAll(x => x.datamodel.Split('/')[1].Equals("customMonkey"));

            for (int i = 0; i < watchHoleParent.childCount; i++) 
                watchHoles.Add(watchHoleParent.GetChild(i).gameObject);

            // check if monkeys should spawn by default
            var firstStartClapping = events.Find(x => x.beat >= beat && x.datamodel.Split('/')[1].Equals("startClapping"));
            var firstAppearEvent = events.Find(x => x.beat >= beat && x.datamodel.Split('/')[1].Equals("monkeysAppear"));

            if (firstStartClapping != null) {
                if (firstStartClapping["start"] != 0) lastMonkeySpawned = firstStartClapping["start"];
                if (firstAppearEvent == null || firstStartClapping.beat < firstAppearEvent.beat) {
                    for (int i = 0; i < (firstStartClapping == null ? 4 : firstStartClapping["amount"]); i++) SpawnMonkey(true);
                }
            }

            var offbeatEvents = events.FindAll(x => x.datamodel.Split('/')[1].Equals("offbeatMonkeys"));

            for (int i = 0; i < offbeatEvents.Count; i++)
            {
                var eventBeat = Mathf.Round((float)offbeatEvents[i].beat);
                if (eventBeat >= beat)
                {
                    offbeatMonkeys.Add(
                        // adjusts beat to always align with Clapping()
                        eventBeat + ((eventBeat + firstStartClapping.beat) % 2),
                        new OffbeatMonkey {
                            mute = offbeatEvents[i]["mute"],
                            length = offbeatEvents[i].length,
                        }
                    );
                }
            }

            monkeyPlayer.UpdateRotation(lastMonkeySpawned * -6);
            cameraTransform.transform.eulerAngles = new Vector3(0, 0, lastMonkeySpawned * -6);
            
            double offBeat = double.MinValue;
            if (wantOffbeat.length != 0)
            {
                offBeat = wantOffbeat.beat;
                PinkMonkeys(offBeat, wantOffbeat.length, wantOffbeat.mute, customMonkeys.FindAll(x => x.beat >= offBeat + 2 && x.beat < offBeat + wantOffbeat.length));
                wantOffbeat = new OffbeatMonkey{
                    beat = 0,
                    length = 0,
                    mute = false,
                };
            }
            if (wantClapping != double.MinValue) {
                Clapping(wantClapping, offBeat + wantOffbeat.length);
                wantClapping = double.MinValue;
            }
            if (wantPrepare) {
                MonkeyPrepare(true);
                wantPrepare = false;
            }

            Update();
        }

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        void OnDestroy()
        {
            if (!Conductor.instance.NotStopped()) {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
                if (offbeatMonkeys.Count > 0) offbeatMonkeys.Clear();
                startedClapping = false;
                lastMonkeySpawned = 0;
            }
            
            foreach (var evt in scheduledInputs) evt.Disable();
        }

        public void Update()
        {
            if (cameraMove) {
                cameraTransform.transform.eulerAngles -= new Vector3(0, 0, (5 * (Conductor.instance.songBpm / 100) * Time.deltaTime));
            }
            GameCamera.additionalPosition = cameraTransform.GetChild(0).position;
        }

        void SpawnMonkey(bool instant)
        {
            var watchHole = watchHoles[lastMonkeySpawned].transform.GetSiblingIndex();

            if (instant) watchHoles[watchHole].GetComponent<Animator>().Play("HoleOpen", 0, 1);
            else watchHoles[watchHole].GetComponent<Animator>().DoScaledAnimationAsync("HoleOpen", 0.25f);

            var monkey = Instantiate(yellowMonkey, watchHoles[watchHole].transform.position, new Quaternion(0, 0, 0, 0), watchHoleParent).GetComponent<Animator>();
            activeMonkeys.Add(watchHole, monkey);
            if (instant) monkey.Play("Appear", 0, 1);
            else monkey.DoScaledAnimationAsync("Appear", 0.5f);
            
            lastMonkeySpawned++;
            if (lastMonkeySpawned >= 60) lastMonkeySpawned = 0;
        }

        int MonkeyAngle(int monkey)
        {
            return monkey switch {
                >= 4  and <= 12 => 2,
                >= 12 and <= 19 => 3,
                >= 19 and <= 27 => 4,
                >= 27 and <= 34 => 5,
                >= 34 and <= 42 => 6,
                >= 42 and <= 49 => 7,
                >= 49 and <= 56 => 8,
                _               => 1,
            };
        }

        public void MonkeyPrepare(bool instant = false)
        {
            Debug.Log("prepare :3");
            if (instant) activeMonkeys[currentMonkey].Play($"Prepare{MonkeyAngle(currentMonkey)}", 0, 1);
            else activeMonkeys[currentMonkey].DoScaledAnimationAsync($"Prepare{MonkeyAngle(currentMonkey)}", 0.5f);
            currentMonkey++;
        }

        public void MonkeysAppear(double beat, float length, int monkeyAmount)
        {
            var appear = new List<BeatAction.Action>();
            for (int i = 0; i < monkeyAmount; i++)
            {
                var x = i;
                appear.Add(new BeatAction.Action(beat + (i / (monkeyAmount / length)), delegate { SpawnMonkey(false); }));
            }
            BeatAction.New(gameObject, appear);
        }

        public static void PreStartClapping(double beat)
        {
            if (GameManager.instance.currentGame == "monkeyWatch") {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                    new BeatAction.Action(beat - 1, delegate { instance.MonkeyPrepare(); }),
                });
            } else {
                wantPrepare = true;
            }
        }

        public static void StartClapping(double beat, int start)
        {
            if (GameManager.instance.currentGame is "monkeyWatch") {
                instance.Clapping(beat);
            } else {
                ClappingInactive(beat);
            }
        }

        static void ClappingInactive(double beat)
        {
            wantClapping = beat;
            startedClapping = true;
        }

        void Clapping(double beat, double inputCheck = double.MinValue)
        {
            cameraMove = true;
            //cameraTransform.transform.eulerAngles += Vector3.back*6;
            if (offbeatMonkeys.ContainsKey(beat)) {
                var monkeys = customMonkeys.FindAll(x => x.beat >= beat + 2 && x.beat < beat + offbeatMonkeys[beat].length);
                PinkMonkeys(beat, offbeatMonkeys[beat].length, offbeatMonkeys[beat].mute, monkeys);
                PinkMonkeySFX(beat, offbeatMonkeys[beat].length, offbeatMonkeys[beat].mute, monkeys);
            } else if (offbeatMonkeys.ContainsKey(beat - 2)) {
                inputCheck = (beat - 2) + offbeatMonkeys[beat - 2].length;
            }

            if (inputCheck <= beat) {
                ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, JustYellow, Miss, Nothing);
            }
            
            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + 2, delegate { Clapping(beat + 2, inputCheck); }),
                new BeatAction.Action(beat + 2, delegate { SpawnMonkey(false); }),
            });
        }

        void WatchMonkeys()
        {

        }

        public static void WarnPinkMonkeys(double beat, float length, bool mute)
        {
            wantOffbeat = new OffbeatMonkey {
                beat = beat,
                length = length,
                mute = true,
            };
            PinkMonkeySFX(beat, length, mute, EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "customMonkeys" }));
        }

        public static void PinkMonkeySFX(double beat, float length, bool mute, List<RiqEntity> monkeys)
        {
            var sfx = new List<MultiSound.Sound>();

            if (!mute) {
                sfx.AddRange(new MultiSound.Sound[] {
                    new MultiSound.Sound("monkeyWatch/voiceUki1",      beat       ),
                    new MultiSound.Sound("monkeyWatch/voiceUki1Echo1", beat + 0.25),
                    new MultiSound.Sound("monkeyWatch/voiceUki2",      beat + 1   ),
                    new MultiSound.Sound("monkeyWatch/voiceUki2Echo1", beat + 1.25),
                    new MultiSound.Sound("monkeyWatch/voiceUki3",      beat + 2   ),
                    new MultiSound.Sound("monkeyWatch/voiceUki3Echo1", beat + 2.25),
                });
            }

            if (monkeys.Count == 0) {
                for (int i = 2; i < length; i++) {
                    sfx.AddRange(new MultiSound.Sound[] {
                        new MultiSound.Sound($"monkeyWatch/voiceKi{(i % 2 == 0 ? 1 : 2)}",      beat + i + 0.5 ),
                        new MultiSound.Sound($"monkeyWatch/voiceKi{(i % 2 == 0 ? 1 : 2)}Echo1", beat + i + 0.75),
                    });
                }
            } else {
                for (int i = 0; i < monkeys.Count; i++) {
                    sfx.AddRange(new MultiSound.Sound[] {
                        new MultiSound.Sound($"monkeyWatch/voiceKi{(i % 2 == 0 ? 1 : 2)}",      monkeys[i].beat       ),
                        new MultiSound.Sound($"monkeyWatch/voiceKi{(i % 2 == 0 ? 1 : 2)}Echo1", monkeys[i].beat + 0.25),
                    });
                }
            }

            MultiSound.Play(sfx.ToArray(), forcePlay: true);
        }

        public void PinkMonkeys(double beat, float length, bool mute, List<RiqEntity> monkeys)
        {
            var actions = new List<BeatAction.Action>();
            if (monkeys.Count == 0) {
                for (int i = 2; i < length; i++) {
                    ScheduleInput(beat, i + 0.5, InputType.STANDARD_DOWN, JustPink, Miss, Nothing);
                    actions.Add(new BeatAction.Action(beat + i - 0.5, delegate { MonkeyPrepare(); }));
                }
            } else {
                for (int i = 0; i < monkeys.Count; i++)
                {
                    ScheduleInput(beat, monkeys[i].beat - beat, InputType.STANDARD_DOWN, (monkeys[i]["sfx"] == 3 ? JustYellow : JustPink), Miss, Nothing);
                    actions.Add(new BeatAction.Action(monkeys[i].beat - 1, delegate { MonkeyPrepare(); }));
                }
            }
            BeatAction.New(gameObject, actions);
        }

        void JustYellow(PlayerActionEvent caller, float state)
        {
            Just(state, true);
        }

        void JustPink(PlayerActionEvent caller, float state)
        {
            Just(state, false);
        }

        void Just(float state, bool isYellow)
        {
            string whichAnim = "PlayerClap";
            if (state >= 1f || state <= -1f) {
                SoundByte.PlayOneShot("miss");
                whichAnim += "Barely";
            } else {
                SoundByte.PlayOneShotGame(isYellow ? $"monkeyWatch/clapOnbeat{UnityEngine.Random.Range(1, 5)}" : "monkeyWatch/clapOffbeat");
                if (isYellow)
                    monkeyPlayer.YellowStars.Play();
                else {
                    monkeyPlayer.PinkStars.Play();
                    whichAnim += "Big";
                }
            }
            monkeyPlayer.MonkeyAnim.DoScaledAnimationAsync(whichAnim, 0.5f);
            monkeyPlayer.ClickerAnim.DoScaledAnimationAsync("Click", 0.5f);
            MonkeyPrepare();

            var anim = activeMonkeys[currentMonkey - 1];
            anim.DoScaledAnimationAsync($"Clap{MonkeyAngle(currentMonkey - 1)}", 0.5f);
            anim.SetInteger("hitState", (whichAnim == "PlayerClapBarely" ? 1 : 2));
        }

        void Miss(PlayerActionEvent caller)
        {
            monkeyPlayer.ClickerAnim.DoScaledAnimationAsync("Click", 0.5f);
            MonkeyPrepare();

            var anim = activeMonkeys[currentMonkey - 1];
            anim.SetBool("hasHit", true);
            anim.SetInteger("hitState", 0);
        }

        void Nothing(PlayerActionEvent caller) {}

        /* starting from the first monkey, these are the amount of monkeys clapped before the prepare animation switches.
        4 +
        8 +
        7 +
        8 +
        7 +
        8 +
        7 +
        7 +
        4

        0
        4
        12
        19
        27
        34
        42
        49
        */
    }
}
