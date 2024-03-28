using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;
using System.Runtime.CompilerServices;

using HeavenStudio.Games.Scripts_CatchOfTheDay;
using UnityEngine.AI;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class RvlCatchOfTheDayLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("catchOfTheDay", "Catch of the Day", "b5dede", false, false, new List<GameAction>()
            {
                new GameAction("fish1", "Quicknibble")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish01(e); CatchOfTheDay.Instance.NewLake(e); },
                    inactiveFunction = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish01(e); },
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
                        new Param("fgManta", false, "Foreground Stingray", "Spawn a stingray in the foreground of the scene."),
                        new Param("bgManta", false, "Background Stingray", "Spawn a stingray in the background of the scene."),
                        new Param("schoolFish", false, "School of Fish", "Spawn a school of fish to as a distraction.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "fishDensity" })
                        }),
                        new Param("fishDensity", new EntityTypes.Float(0f, 1f, 1f), "Fish Density", "Set the density for the fish in the school."),
                    },
                },
                new GameAction("fish2", "Pausegill")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish02(e); CatchOfTheDay.Instance.NewLake(e); },
                    inactiveFunction = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish02(e); },
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
                        new Param("fgManta", false, "Foreground Stingray", "Spawn a stingray in the foreground of the scene."),
                        new Param("bgManta", false, "Background Stingray", "Spawn a stingray in the background of the scene."),
                        new Param("schoolFish", false, "School of Fish", "Spawn a school of fish to as a distraction.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "fishDensity" })
                        }),
                        new Param("fishDensity", new EntityTypes.Float(0f, 1f, 1f), "Fish Density", "Set the density for the fish in the school."),
                    },
                },
                new GameAction("fish3", "Threefish")
                {
                    function = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish03(e); CatchOfTheDay.Instance.NewLake(e); },
                    inactiveFunction = delegate {var e = eventCaller.currentEntity; CatchOfTheDay.Cue_Fish03(e); },
                    defaultLength = 5.5f,
                    parameters = new List<Param>()
                    {
                        new Param("countIn", false, "Count-In", "Play the \"One Two Three Go!\" sound effect as a count in to the cue."),
                        new Param("fakeOut", false, "Fake-Out", "If enabled, a quicknibble will be shown initially, before being chased away by the threefish."),
                        new Param("layout", CatchOfTheDay.FishLayout.Random, "Layout", "Set the layout for the scene."),
                        new Param("useCustomColor", false, "Custom Color", "Set whether or not to use a custom color.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "colorTop", "colorBottom" })
                        }),
                        new Param("colorTop",    new Color(0.7098039f, 0.8705882f, 0.8705882f), "Top Color",    "The color for the top part of the background."),
                        new Param("colorBottom", new Color(0.4666667f, 0.7372549f, 0.8196079f), "Bottom Color", "The color for the bottom part of the background."),
                        new Param("sceneDelay", new EntityTypes.Float(0f, 32f, 1f), "Scene Change Delay", "Amount of beats to wait before changing to the next scene."),
                        new Param("fgManta", false, "Foreground Stingray", "Spawn a stingray in the foreground of the scene."),
                        new Param("bgManta", false, "Background Stingray", "Spawn a stingray in the background of the scene."),
                        new Param("schoolFish", false, "School of Fish", "Spawn a school of fish to as a distraction.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "fishDensity" })
                        }),
                        new Param("fishDensity", new EntityTypes.Float(0f, 1f, 1f), "Fish Density", "Set the density for the fish in the school."),
                    },
                },
            },
            new List<string>() {"rvl", "normal"},
            "rvlfishing", "en"
            //, chronologicalSortIndex: 21
            );
        }
    }
}

namespace HeavenStudio.Games
{
    public class CatchOfTheDay : Minigame
    {
        /*
        BIG LIST OF TODOS
        - ping @hexiedecimal
        - scene transitions
        - wait for upscale
        - make ann movable
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

        [SerializeField] Animator Angler;
        [SerializeField] GameObject LakeScenePrefab;
        [SerializeField] Transform LakeSceneHolder;

        public int? LastLayout;
        public Dictionary<RiqEntity, LakeScene> ActiveLakes = new();

        public static Dictionary<RiqEntity, MultiSound> FishSounds = new();

        private void Update()
        {
            if (!conductor.isPlaying && !conductor.isPaused && ActiveLakes.Count <= 0)
            {
                List<RiqEntity> activeFishes = GetActiveFishes(conductor.songPositionInBeatsAsDouble);
                if (activeFishes.Count > 0)
                    NewLake(activeFishes[0]);
                else
                    SpawnNextFish(conductor.songPositionInBeatsAsDouble);
            }
        }
        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }
        public override void OnGameSwitch(double beat)
        {
            DestroyOrphanedLakes();
            CleanupFishSounds();
            // get active fishes
            foreach (RiqEntity e in GetActiveFishes(beat))
            {
                NewLake(e);
            }
            if (ActiveLakes.Count <= 0)
            {
                SpawnNextFish(beat);
            }
        }

        public static void Cue_Fish01(RiqEntity e)
        {
            CleanupFishSounds();

            double beat = e.beat;

            FishSounds.Add(e, MultiSound.Play(new MultiSound.Sound[]{
                new MultiSound.Sound("catchOfTheDay/quick1", beat),
                new MultiSound.Sound("catchOfTheDay/quick2", beat + 1),
            }, forcePlay: true));

            if (Instance != null && Instance.ActiveLakes.ContainsKey(e))
                Instance.ActiveLakes[e]._MultiSound = FishSounds[e];
        }
        public static void Cue_Fish02(RiqEntity e)
        {
            CleanupFishSounds();

            double beat = e.beat;
            bool countIn = e["countIn"];

            FishSounds.Add(e, MultiSound.Play(new MultiSound.Sound[]{
                new MultiSound.Sound("catchOfTheDay/pausegill1", beat),
                new MultiSound.Sound("catchOfTheDay/pausegill2", beat + 0.5),
                new MultiSound.Sound("catchOfTheDay/pausegill3", beat + 1),
            }, forcePlay: true));

            if (countIn)
            {
                MultiSound.Play(new MultiSound.Sound[]{
                    new MultiSound.Sound("count-ins/and", beat + 2),
                    new MultiSound.Sound(UnityEngine.Random.Range(0.0f, 1.0f) > 0.5 ? "count-ins/go1" : "count-ins/go2", beat + 2),
                }, forcePlay: true, game: false);
            }

            if (Instance != null && Instance.ActiveLakes.ContainsKey(e))
                Instance.ActiveLakes[e]._MultiSound = FishSounds[e];
        }
        public static void Cue_Fish03(RiqEntity e)
        {
            CleanupFishSounds();

            double beat = e.beat;
            bool countIn = e["countIn"];

            FishSounds.Add(e, MultiSound.Play(new MultiSound.Sound[]{
                new MultiSound.Sound("catchOfTheDay/threefish1", beat),
                new MultiSound.Sound("catchOfTheDay/threefish2", beat + 0.25),
                new MultiSound.Sound("catchOfTheDay/threefish3", beat + 0.5),
                new MultiSound.Sound("catchOfTheDay/threefish4", beat + 1)
            }, forcePlay: true));
            if (countIn)
            {
                MultiSound.Play(new MultiSound.Sound[]{
                    new MultiSound.Sound("count-ins/one1", beat + 2),
                    new MultiSound.Sound("count-ins/two1", beat + 3),
                    new MultiSound.Sound("count-ins/three1", beat + 4),
                    new MultiSound.Sound(UnityEngine.Random.Range(0.0f, 1.0f) > 0.5 ? "count-ins/go1" : "count-ins/go2", beat + 4.5),
                }, forcePlay: true, game: false);
            }
            
            if (Instance != null && Instance.ActiveLakes.ContainsKey(e))
                Instance.ActiveLakes[e]._MultiSound = FishSounds[e];
        }

        public void DoPickAnim()
        {
            Angler.DoScaledAnimationAsync("Pick", 0.5f);
        }
        public void DoJustAnim()
        {
            Angler.DoScaledAnimationAsync("Just", 0.5f);
        }
        public void DoMissAnim()
        {
            Angler.DoScaledAnimationAsync("Miss", 0.5f);
        }
        public void DoThroughAnim()
        {
            Angler.DoScaledAnimationAsync("Through", 0.5f);
        }
        public void DoOutAnim()
        {
            Angler.DoScaledAnimationAsync("Through", 0.5f);
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
        }
        public static void CleanupFishSounds()
        {
            List<RiqEntity> expiredKeys = new();
            foreach (KeyValuePair<RiqEntity, MultiSound> kv in FishSounds)
            {
                if (kv.Value == null)
                    expiredKeys.Add(kv.Key);
            }
            foreach (RiqEntity key in expiredKeys)
                FishSounds.Remove(key);
        }
        public List<RiqEntity> GetActiveFishes(double beat)
        {
            return EventCaller.GetAllInGameManagerList("catchOfTheDay", new string[] { "fish1", "fish2", "fish3" }).FindAll(e => e.beat <= beat && e.beat + e.length - 1 + e["sceneDelay"] >= beat);
        }
        public RiqEntity GetNextFish(double beat)
        {
            RiqEntity gameSwitch = GetNextGameSwitch(beat);
            return EventCaller.GetAllInGameManagerList("catchOfTheDay", new string[] { "fish1", "fish2", "fish3" }).OrderBy(e => e.beat).FirstOrDefault(e => e.beat >= beat && (gameSwitch is null || e.beat < gameSwitch.beat));
        }
        public RiqEntity GetNextGameSwitch(double beat)
        {
            return EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).OrderBy(e => e.beat).FirstOrDefault(e => e.beat > beat && e.datamodel != "gameManager/switchGame/catchOfTheDay");
        }
        public LakeScene NewLake(RiqEntity e)
        {
            if (ActiveLakes.ContainsKey(e))
                return null;
            
            int sort = EventCaller.GetAllInGameManagerList("catchOfTheDay", new string[] { "fish1", "fish2", "fish3" }).FindIndex(x => e == x);
            if (sort < 0)
                return null;
            
            CleanupFishSounds();

            LakeScene lake = Instantiate(LakeScenePrefab, LakeSceneHolder).GetComponent<LakeScene>();
            LastLayout = lake.Setup(e, this, LastLayout, int.MaxValue - sort);
            ActiveLakes.Add(e, lake);
            if (FishSounds.ContainsKey(e))
                lake._MultiSound = FishSounds[e];
            return lake;
        }
        public bool SpawnNextFish(double beat)
        {
            RiqEntity nextFish = GetNextFish(beat);
            if (nextFish is not null)
            {
                NewLake(nextFish);
                return true;
            }
            return false;
        }
        public void DisposeLake(LakeScene lake)
        {
            ActiveLakes.Remove(lake.Entity);

            if (ActiveLakes.Count <= 0)
            {
                if (SpawnNextFish(conductor.songPositionInBeatsAsDouble))
                    Destroy(lake.gameObject);
            }
            else
                Destroy(lake.gameObject);
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

// This minigame ported by playinful. ☆