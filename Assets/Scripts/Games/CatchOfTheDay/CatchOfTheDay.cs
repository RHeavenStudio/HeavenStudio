using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;
using System.Runtime.CompilerServices;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    /// Minigame loaders handle the setup of your minigame.
    /// Here, you designate the game prefab, define entities, and mark what AssetBundle to load

    /// Names of minigame loaders follow a specific naming convention of `PlatformcodeNameLoader`, where:
    /// `Platformcode` is a three-leter platform code with the minigame's origin
    /// `Name` is a short internal name
    /// `Loader` is the string "Loader"

    /// Platform codes are as follows:
    /// Agb: Gameboy Advance    ("Advance Gameboy")
    /// Ntr: Nintendo DS        ("Nitro")
    /// Rvl: Nintendo Wii       ("Revolution")
    /// Ctr: Nintendo 3DS       ("Centrair")
    /// Mob: Mobile
    /// Pco: PC / Other

    /// Fill in the loader class label, "*prefab name*", and "*Display Name*" with the relevant information
    /// For help, feel free to reach out to us on our discord, in the #development channel.
    public static class RvlCatchOfTheDayLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("catchOfTheDay", "Catch of the Day", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("fish1", "Quicknibble")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish01(e.beat); CatchOfTheDay.Instance.NewLake(e); },
                    inactiveFunction = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish01(e.beat); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("layout", CatchOfTheDay.FishLayout.Random, "Layout", "Set the layout for the scene."),
                        new Param("useCustomColor", false, "Custom Color", "Set whether or not to use a custom color.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "colorTop", "colorBottom" })
                        }),
                        new Param("colorTop",    new Color(0.7098039f, 0.8705882f, 0.8705882f), "Top Color",    "The color for the top part of the background."),
                        new Param("colorBottom", new Color(0.4666667f, 0.7372549f, 0.8196079f), "Bottom Color", "The color for the bottom part of the background."),
                        new Param("sceneDelay", new EntityTypes.Float(0f, 32f, 1f), "Scene Change Delay", "Amount of beats to wait before changing to the next scene."),
                    },
                },
                new GameAction("fish2", "Pausegill")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish02(e.beat, e["countIn"]); CatchOfTheDay.Instance.NewLake(e); },
                    inactiveFunction = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish02(e.beat, e["countIn"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("countIn", false, "Count-In", "Play the \"And Go!\" sound effect as a count in to the cue."),
                        new Param("layout", CatchOfTheDay.FishLayout.Random, "Layout", "Set the layout for the scene."),
                        new Param("useCustomColor", false, "Custom Color", "Set whether or not to use a custom color.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "colorTop", "colorBottom" })
                        }),
                        new Param("colorTop",    new Color(0.7098039f, 0.8705882f, 0.8705882f), "Top Color",    "The color for the top part of the background."),
                        new Param("colorBottom", new Color(0.4666667f, 0.7372549f, 0.8196079f), "Bottom Color", "The color for the bottom part of the background."),
                        new Param("sceneDelay", new EntityTypes.Float(0f, 32f, 1f), "Scene Change Delay", "Amount of beats to wait before changing to the next scene."),
                    },
                },
                new GameAction("fish3", "Threefish")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish03(e.beat, e["countIn"]); CatchOfTheDay.Instance.NewLake(e); },
                    inactiveFunction = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish03(e.beat, e["countIn"]); },
                    defaultLength = 5.5f,
                    parameters = new List<Param>()
                    {
                        new Param("countIn", false, "Count-In", "Play the \"One Two Three Go!\" sound effect as a count in to the cue."),
                        new Param("layout", CatchOfTheDay.FishLayout.Random, "Layout", "Set the layout for the scene."),
                        new Param("useCustomColor", false, "Custom Color", "Set whether or not to use a custom color.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "colorTop", "colorBottom" })
                        }),
                        new Param("colorTop",    new Color(0.7098039f, 0.8705882f, 0.8705882f), "Top Color",    "The color for the top part of the background."),
                        new Param("colorBottom", new Color(0.4666667f, 0.7372549f, 0.8196079f), "Bottom Color", "The color for the bottom part of the background."),
                        new Param("sceneDelay", new EntityTypes.Float(0f, 32f, 1f), "Scene Change Delay", "Amount of beats to wait before changing to the next scene."),
                    },
                },
                /*new GameAction("setBGColor", "Set Background Colors")
                {
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("startColorTop",    new Color(0.7098039f, 0.8705882f, 0.8705882f), "Start Top Color",    "The color for the top part of the background at which to start."),
                        new Param("startColorBottom", new Color(0.4666667f, 0.7372549f, 0.8196079f), "Start Bottom Color", "The color for the bottom part of the background at which to start."),
                        new Param("endColorTop",      new Color(0.7098039f, 0.8705882f, 0.8705882f), "End Top Color",      "The color for the top part of the background at which to end."),
                        new Param("endColorBottom",   new Color(0.4666667f, 0.7372549f, 0.8196079f), "End Bottom Color",   "The color for the bottom part of the background at which to end."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the function.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (Util.EasingFunction.Ease)x != Util.EasingFunction.Ease.Instant, new string[] { "startColorTop", "startColorBottom" })
                        })
                    },
                },
                new GameAction("sceneTransition", "Scene Transition")
                {
                    defaultLength = 0.5f
                },*/
                
            },
            new List<string>() {"rvl", "normal"},
            "rvlfishing", "en"
            //, chronologicalSortKey: 1107210021
            );
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class CatchOfTheDay : Minigame
    {
        /*
        BIG LIST OF TODOS
        - implement inputs
        - bubble particles
        - ping @hexiedecimal
        - custom layouts/colors
        - scene transitions
        - transition block
        - make stuff update in editor
        - color stuff white
        - removec omments
        */
        public static CatchOfTheDay Instance
        {
            get
            {
                if (GameManager.instance.minigame is CatchOfTheDay instance)
                    return instance;
                return null;
            }
        }

        [SerializeField] GameObject LakeScenePrefab;
        [SerializeField] Transform LakeSceneHolder;

        public int? LastLayout;
        public Dictionary<RiqEntity, LakeScene> ActiveLakes = new();

        public static void Dummy()
        {
            // TODO REMOVE ME BEFORE PUBLISH I AM JUST A PLACEHOLDER
        }

        private void Update()
        {
            //SetDesiredColorAtBeat(Conductor.instance.songPositionInBeats);
        }
        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }
        public override void OnGameSwitch(double beat)
        {
            DestroyOrphanedLakes();
            // get active fishes
            foreach (RiqEntity e in GetActiveFishes(beat))
            {
                NewLake(e);
            }
            if (ActiveLakes.Count <= 0)
            {
                RiqEntity nextFish = GetNextFish(beat);
                if (nextFish is not null)
                    NewLake(nextFish);
            }
        }

        public static void Cue_Fish01(double beat)
        {
            MultiSound.Play(new MultiSound.Sound[]{
                new MultiSound.Sound("catchOfTheDay/quick1", beat),
                new MultiSound.Sound("catchOfTheDay/quick2", beat + 1),
            }, forcePlay: true);
        }
        public static void Cue_Fish02(double beat, bool countIn)
        {
            MultiSound.Play(new MultiSound.Sound[]{
                new MultiSound.Sound("catchOfTheDay/pausegill1", beat),
                new MultiSound.Sound("catchOfTheDay/pausegill2", beat + 0.5),
                new MultiSound.Sound("catchOfTheDay/pausegill3", beat + 1),
            }, forcePlay: true);

            if (countIn)
            {
                MultiSound.Play(new MultiSound.Sound[]{
                    new MultiSound.Sound("count-ins/and", beat + 2),
                    new MultiSound.Sound(UnityEngine.Random.Range(0.0f, 1.0f) > 0.5 ? "count-ins/go1" : "count-ins/go2", beat + 2),
                }, forcePlay: true, game: false);
            }
        }
        public static void Cue_Fish03(double beat, bool countIn)
        {
            MultiSound.Play(new MultiSound.Sound[]{
                new MultiSound.Sound("catchOfTheDay/threefish1", beat),
                new MultiSound.Sound("catchOfTheDay/threefish2", beat + 0.25),
                new MultiSound.Sound("catchOfTheDay/threefish3", beat + 0.5),
                new MultiSound.Sound("catchOfTheDay/threefish4", beat + 1)
            }, forcePlay: true);
            if (countIn)
            {
                MultiSound.Play(new MultiSound.Sound[]{
                    new MultiSound.Sound("count-ins/one1", beat + 2),
                    new MultiSound.Sound("count-ins/two1", beat + 3),
                    new MultiSound.Sound("count-ins/three1", beat + 4),
                    new MultiSound.Sound(UnityEngine.Random.Range(0.0f, 1.0f) > 0.5 ? "count-ins/go1" : "count-ins/go2", beat + 4.5),
                }, forcePlay: true, game: false);
            }
        }

        public void DestroyOrphanedLakes()
        {
            List<GameObject> toDestroy = new();
            for (int i = 0; i < LakeSceneHolder.childCount; i++)
            {
                LakeScene lake = LakeSceneHolder.GetChild(i).gameObject.GetComponent<LakeScene>();
                if (lake == null || (!ActiveLakes.ContainsValue(lake) && !lake.IsDummy))
                    toDestroy.Add(LakeSceneHolder.GetChild(i).gameObject);
            }
            foreach (GameObject obj in toDestroy)
            {
                Destroy(obj);
            }
            Debug.Log("Lakes cleared.");
        }
        public List<RiqEntity> GetActiveFishes(double beat)
        {
            Debug.Log("Getting active fishes.");
            return EventCaller.GetAllInGameManagerList("catchOfTheDay", new string[] { "fish1", "fish2", "fish3" }).FindAll(e => e.beat <= beat && e.beat + e.length - 1 + e["sceneDelay"] >= beat);
        }
        public RiqEntity GetNextFish(double beat)
        {
            Debug.Log("Seeking next fish...");
            return EventCaller.GetAllInGameManagerList("catchOfTheDay", new string[] { "fish1", "fish2", "fish3" }).OrderBy(e => e.beat).FirstOrDefault(e => e.beat >= beat);
        }
        public LakeScene NewLake(RiqEntity e)
        {
            if (ActiveLakes.ContainsKey(e))
                return null;
            
            Debug.Log("Adding new lake...");
            LakeScene lake = Instantiate(LakeScenePrefab, LakeSceneHolder).GetComponent<LakeScene>();
            LastLayout = lake.Setup(e, this, LastLayout);
            ActiveLakes.Add(e, lake);
            Debug.Log("New lake added.");
            return lake;
        }

        public enum FishLayout : int
        {
            Random = -1,
            LayoutA = 0,
            LayoutB = 1,
            LayoutC = 2
        }
    }
}