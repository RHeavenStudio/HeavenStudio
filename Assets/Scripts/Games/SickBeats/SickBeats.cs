using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class AgbSickBeats
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("sickBeats", "Sick Beats", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; SickBeats.instance.ToggleBop(e.beat, e.length, e["toggle2"], e["toggle"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle2", true, "Bop", "Toggle if Boss should bop for the duration of this event."),
                        new Param("toggle", false, "Bop (Auto)", "Toggle if the man should automatically bop until another Bop event is reached.")
                    }
                },
                new GameAction("virusLeft", "Virus (Right)")
                {
                    function = delegate { var e = eventCaller.currentEntity; SickBeats.instance.PresenceVirus(e.beat, (int)SickBeats.Direction.Right, e["type"]); },
                    parameters = new List<Param>()
                    {
                        new Param("type", SickBeats.VirusType.Blue, "Type", "Determine virus type"),
                    },
                    defaultLength = 4f,
                },
                new GameAction("virusUp", "Virus (Up)")
                {
                    function = delegate { var e = eventCaller.currentEntity; SickBeats.instance.PresenceVirus(e.beat, (int)SickBeats.Direction.Up, e["type"]); },
                    parameters = new List<Param>()
                    {
                        new Param("type", SickBeats.VirusType.Blue, "Type", "Determine virus type"),
                    },
                    defaultLength = 6f,
                },
                new GameAction("virusRight", "Virus (Left)")
                {
                    function = delegate { var e = eventCaller.currentEntity; SickBeats.instance.PresenceVirus(e.beat, (int)SickBeats.Direction.Left, e["type"]); },
                    parameters = new List<Param>()
                    {
                        new Param("type", SickBeats.VirusType.Blue, "Type", "Determine virus type"),
                    },
                    defaultLength = 6f,
                },
                new GameAction("virusDown", "Virus (Down)")
                {
                    function = delegate { var e = eventCaller.currentEntity; SickBeats.instance.PresenceVirus(e.beat, (int)SickBeats.Direction.Down, e["type"]); },
                    parameters = new List<Param>()
                    {
                        new Param("type", SickBeats.VirusType.Blue, "Type", "Determine virus type"),
                    },
                    defaultLength = 6f,
                },
                new GameAction("appear", "Appear")
                {
                    function = delegate {var e = eventCaller.currentEntity; SickBeats.instance.VirusAppearMnl(e.beat, e["direction"], e["type"]); },
                    parameters = new List<Param>()
                    {
                        new Param("direction", SickBeats.Direction.Right, "Direction", "Determine which direction the virus will spawn from"),
                        new Param("type", SickBeats.VirusType.Blue, "Type", "Determine virus type"),
                    },
                    defaultLength = 2f,
                },
                new GameAction("dash", "Dash")
                {
                    function = delegate {var e = eventCaller.currentEntity; SickBeats.instance.VirusDashMnl(e.beat,
                        e["direction"], e["type"], new double[]{e["param1"], e["param2"], e["param3"]}); },
                    parameters = new List<Param>()
                    {
                        new Param("direction", SickBeats.Direction.Up, "Direction", "Determine which direction the virus will spawn from"),
                        new Param("type", SickBeats.VirusType.Blue, "Type", "Determine virus type"),
                        new Param("param1", new EntityTypes.Float(0, 1, 0), "1"),
                        new Param("param2", new EntityTypes.Float(0, 1, 0.125f), "2"),
                        new Param("param3", new EntityTypes.Float(0, 1, 0.25f), "3"),
                    },
                    defaultLength = 1f,
                },
                new GameAction("come", "Come")
                {
                    function = delegate {var e = eventCaller.currentEntity; SickBeats.instance.VirusComeMnl(e.beat, e["type"]); },
                    parameters = new List<Param>()
                    {
                        new Param("type", SickBeats.VirusType.Blue, "Type", "Determine virus type"),
                    },
                    defaultLength = 2f,
                },
                new GameAction("virusColor", "Change Virus Color")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        SickBeats.instance.UpdateMaterialColor(e["colorVirus1"], e["colorVirus2"], e["colorVirus3"], e["colorVirus4"]);
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("colorVirus1", new Color(0f, 1f, 1f), "1st Color", "Set the color of the first virus."),
                        new Param("colorVirus2", new Color(1f, 0.25f, 0.75f), "2nd Color", "Set the color of the second virus."),
                        new Param("colorVirus3", new Color(0f, 0f, 0f), "3rd Color", "Set the color of the third virus."),
                        new Param("colorVirus4", new Color(1f, 1f, 1f), "4th Color", "Set the color of the fourth virus."),
                    }
                },
            },
            new List<string>() { "agb", "normal" }, "agbSickBeats", "en", new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_SickBeats;

    public class SickBeats : Minigame
    {
        [Header("References")]
        public Animator keyAnim;
        public Animator[] forkAnims;
        public Animator doctorAnim;
        public Animator radioAnim;
        public Animator orgAnim;
        public GameObject baseVirus;
        public Transform virusHolder;

        [Header("Colorable")]
        public Material[] RecolorMats;
        public Color[] color = {new Color(0f, 1f, 1f), new Color(1f, 0.25f, 0.75f),
                                new Color(0f, 0f, 0f), new Color(1f, 1f, 1f)};

        [Header("Variables")]
        [SerializeField] double _refillBeat;
        public double RefillBeat 
        {
            get => _refillBeat;
            private set => _refillBeat = value;
        }

        [Serializable]
        public struct DashPatternItem
        {
            public double[] beat;
        }
        [SerializeField] DashPatternItem[] DashPatterns;

        [System.NonSerialized] public bool[] isForkPop = {true, true, true, true};
        [System.NonSerialized] public bool orgAlive = true;
        [System.NonSerialized] public bool docShock = false;

        public enum Direction
        {
            Right,
            Up,
            Left,
            Down,
        }
        public enum VirusType
        {
            Blue,
            Pink,
            Black,
            Custom,
        }

        public static SickBeats instance;

        const int IA_RightPress = IAMAXCAT;
        const int IA_UpPress = IAMAXCAT + 1;
        const int IA_LeftPress = IAMAXCAT + 2;
        const int IA_DownPress = IAMAXCAT + 3;

        protected static bool IA_PadRight(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }
        protected static bool IA_BatonRight(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.East, out dt);
        }
        protected static bool IA_TouchRight(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Right, out dt)
                && (instance.IsExpectingInputNow(InputAction_Right) || instance.IsExpectingInputNow(InputAction_Left));
        }
        protected static bool IA_PadUp(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt);
        }
        protected static bool IA_BatonUp(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.North, out dt);
        }
        protected static bool IA_TouchUp(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_Up);
        }
        protected static bool IA_PadLeft(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt);
        }
        protected static bool IA_BatonLeft(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.West, out dt);
        }
        protected static bool IA_TouchLeft(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Left, out dt)
                && (instance.IsExpectingInputNow(InputAction_Right) || instance.IsExpectingInputNow(InputAction_Left));
        }
        protected static bool IA_PadDown(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt);
        }
        protected static bool IA_BatonDown(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.South, out dt);
        }
        protected static bool IA_TouchDown(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_Down);
        }

        public static PlayerInput.InputAction InputAction_Right =
            new("AgbSickBeatsRight", new int[] { IA_RightPress, IA_RightPress, IA_RightPress },
            IA_PadRight, IA_TouchRight, IA_BatonRight);
        public static PlayerInput.InputAction InputAction_Up =
            new("AgbSickBeatsUp", new int[] { IA_UpPress, IA_UpPress, IA_UpPress },
            IA_PadUp, IA_TouchUp, IA_BatonUp);
        public static PlayerInput.InputAction InputAction_Left =
            new("AgbSickBeatsLeft", new int[] { IA_LeftPress, IA_LeftPress, IA_LeftPress },
            IA_PadLeft, IA_TouchLeft, IA_BatonLeft);
        public static PlayerInput.InputAction InputAction_Down =
            new("AgbSickBeatsDown", new int[] { IA_DownPress, IA_DownPress, IA_DownPress },
            IA_PadDown, IA_TouchDown, IA_BatonDown);

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            SetupBopRegion("sickBeats", "bop", "toggle");
            UpdateMaterialColor(color[0], color[1], color[2], color[3]);
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat)) Bop();
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying || cond.isPaused) return;

            if (PlayerInput.GetIsAction(InputAction_Right) && !IsExpectingInputNow(InputAction_Right))
            {
                if (isForkPop[0]) OutFork(0);
            }
            if (PlayerInput.GetIsAction(InputAction_Up) && !IsExpectingInputNow(InputAction_Up))
            {
                if (isForkPop[1]) OutFork(1);
            }
            if (PlayerInput.GetIsAction(InputAction_Left) && !IsExpectingInputNow(InputAction_Left))
            {
                if (isForkPop[2]) OutFork(2);
            }
            if (PlayerInput.GetIsAction(InputAction_Down) && !IsExpectingInputNow(InputAction_Down))
            {
                if (isForkPop[3]) OutFork(3);
            }
        }

        private void OutFork(int dir)
        {
            var currentBeat = Conductor.instance.songPositionInBeatsAsDouble;

            var actions = new List<BeatAction.Action>();
            keyAnim.Play("push");
            forkAnims[dir].Play("out");
            SoundByte.PlayOneShotGame("sickBeats/1", pitch: UnityEngine.Random.Range(2.75f, 3.25f));
            BeatAction.New(instance, new() {new BeatAction.Action(currentBeat + RefillBeat, delegate {RepopFork(dir);})});

            isForkPop[dir] = false;
        }
        public void RepopFork(int dir)
        {
            forkAnims[dir].Play("repop");

            isForkPop[dir] = true;
        }

        public void ToggleBop(double beat, float length, bool bopOrNah, bool autoBop)
        {
            if (bopOrNah)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new() {new BeatAction.Action(beat + i, delegate {Bop();}) });
                }
            }
        }

        public void Bop()
        {
            radioAnim.DoScaledAnimationAsync("bop", 0.5f);
            if (!docShock) doctorAnim.DoScaledAnimationAsync("bop", 0.5f);
            if (orgAlive) orgAnim.DoScaledAnimationAsync("bop", 0.5f);
        }

        public Virus SpawnVirus(double beat, int dir, int type)
        {
            var newVirus = Instantiate(baseVirus, virusHolder).GetComponent<Virus>();
            newVirus.startBeat = beat;
            newVirus.position = dir;
            newVirus.life = type;
            newVirus.gameObject.SetActive(true);
            newVirus.Init();

            return newVirus;
        }

        public void PresenceVirus(double beat, int dir, int type)
        {
            var newVirus = SpawnVirus(beat, -1, type);

            var actions = new List<BeatAction.Action>();
            
            actions.Add(new BeatAction.Action(beat, delegate {
                newVirus.Come();
                newVirus.position++;
            }));

            switch (dir)
            {
                case (int)Direction.Right:
                    newVirus.startBeat = beat + 2;
                    actions.Add(new BeatAction.Action(beat + 2, delegate {newVirus.Appear();}));
                    break;
                case (int)Direction.Up:
                case (int)Direction.Left:
                case (int)Direction.Down:
                    for(int i = 0; i < dir; i++)
                    {
                        actions.Add(new BeatAction.Action(beat + DashPatterns[dir-1].beat[i], delegate {
                            newVirus.Dash();
                            newVirus.position++;
                        }));
                    }
                    newVirus.startBeat = beat + 4;
                    actions.Add(new BeatAction.Action(beat + 4, delegate {newVirus.Appear();}));
                    break;
            }

            BeatAction.New(instance, actions);
            
        }

        public void VirusAppearMnl(double beat, int dir, int type)
        {
            var newVirus = SpawnVirus(beat, dir, type);
            newVirus.Appear();
        }

        public void VirusDashMnl(double beat, int dir, int type, double[] dashbeats)
        {
            if (dir<1) dir = 1;
            var newVirus = SpawnVirus(beat, 0, type);
            
            var actions = new List<BeatAction.Action>();

            for(int i = 0; i < dir; i++)
            {
                actions.Add(new BeatAction.Action(beat + dashbeats[i], delegate {
                    newVirus.Dash();
                    newVirus.position++;
                }));
            }
            actions.Add(new BeatAction.Action(beat + 2, delegate {Destroy(newVirus.gameObject);}));
            BeatAction.New(instance, actions);
        }

        public void VirusComeMnl(double beat, int type)
        {
            var newVirus = SpawnVirus(beat, -1, type);

            var actions = new List<BeatAction.Action>();
            
            actions.Add(new BeatAction.Action(beat, delegate {
                newVirus.Come();
                newVirus.position++;
            }));
            actions.Add(new BeatAction.Action(beat + 2, delegate {Destroy(newVirus.gameObject);}));
            BeatAction.New(instance, actions);
        }

        

        public void UpdateMaterialColor(Color virus1, Color virus2, Color virus3, Color virus4)
        {
            color[0] = virus1; color[1] = virus2; color[2] = virus3; color[3] = virus4; 

            Recolor(0, new Color(0.75f, 0f, 0f), virus2, virus1);
            Recolor(1, new Color(0.75f, 0f, 0f), virus3, virus2);
            Recolor(2, new Color(0.75f, 0f, 0f), virus4, virus3);
            Recolor(3, new Color(0.75f, 0f, 0f), virus4, virus4);

            void Recolor(int i, Color color1, Color color2, Color color3)
            {
                RecolorMats[i].SetColor("_ColorAlpha", color1);
                RecolorMats[i].SetColor("_ColorBravo", color2);
                RecolorMats[i].SetColor("_ColorDelta", color3);
            }
        }
    }
}