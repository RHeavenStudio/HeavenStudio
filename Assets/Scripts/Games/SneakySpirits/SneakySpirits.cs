using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbGhostLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("sneakySpirits", "Sneaky Spirits", "0058CE", false, false, new List<GameAction>()
            {
                new GameAction("spawnGhost", "Ghost")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; SneakySpirits.PreSpawnGhost(e.beat, e.length, e["volume1"], e["volume2"], e["volume3"], e["volume4"], e["volume5"], e["volume6"],
                        e["volume7"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("volume1", new EntityTypes.Integer(0, 100, 100), "Move Volume 1", "What height and what volume should this move be at?"),
                        new Param("volume2", new EntityTypes.Integer(0, 100, 100), "Move Volume 2", "What height and what volume should this move be at?"),
                        new Param("volume3", new EntityTypes.Integer(0, 100, 100), "Move Volume 3", "What height and what volume should this move be at?"),
                        new Param("volume4", new EntityTypes.Integer(0, 100, 100), "Move Volume 4", "What height and what volume should this move be at?"),
                        new Param("volume5", new EntityTypes.Integer(0, 100, 100), "Move Volume 5", "What height and what volume should this move be at?"),
                        new Param("volume6", new EntityTypes.Integer(0, 100, 100), "Move Volume 6", "What height and what volume should this move be at?"),
                        new Param("volume7", new EntityTypes.Integer(0, 100, 100), "Move Volume 7", "What height and what volume should this move be at?"),
                    }
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_SneakySpirits;
    public class SneakySpirits : Minigame
    {
        public struct QueuedGhost
        {
            public float beat;
            public float length;
            public List<int> volumes;
        }
        [Header("Components")]
        [SerializeField] Animator bowAnim;
        [SerializeField] Animator doorAnim;
        [SerializeField] SneakySpiritsGhost movingGhostPrefab;
        [SerializeField] SneakySpiritsGhostDeath deathGhostPrefab;
        [SerializeField] List<Transform> ghostPositions = new List<Transform>();
        [Header("Variables")]
        private static List<QueuedGhost> queuedGhosts = new List<QueuedGhost>();

        public static SneakySpirits instance;

        void OnDestroy()
        {
            if (queuedGhosts.Count > 0) queuedGhosts.Clear();
            Conductor.instance.SetMinigamePitch(1f);
        }

        void Awake()
        {
            instance = this;
            Conductor.instance.SetMinigamePitch(1f);
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedGhosts.Count > 0)
                {
                    foreach(var ghost in queuedGhosts)
                    {
                        SpawnGhost(ghost.beat, ghost.length, ghost.volumes);
                    }
                    queuedGhosts.Clear();
                }
            }
            else if (!cond.isPlaying)
            {
                queuedGhosts.Clear();
                Conductor.instance.SetMinigamePitch(1f);
            }
        }

        public static void PreSpawnGhost(float beat, float length, int volume1, int volume2, int volume3, int volume4, int volume5, int volume6, int volume7)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("sneakySpirits/moving", beat, 1f, volume1 * 0.01f),
                new MultiSound.Sound("sneakySpirits/moving", beat + length, 1f, volume2 * 0.01f),
                new MultiSound.Sound("sneakySpirits/moving", beat + length * 2, 1f, volume3 * 0.01f),
                new MultiSound.Sound("sneakySpirits/moving", beat + length * 3, 1f, volume4 * 0.01f),
                new MultiSound.Sound("sneakySpirits/moving", beat + length * 4, 1f, volume5 * 0.01f),
                new MultiSound.Sound("sneakySpirits/moving", beat + length * 5, 1f, volume6 * 0.01f),
                new MultiSound.Sound("sneakySpirits/moving", beat + length * 6, 1f, volume7 * 0.01f),
            }, forcePlay: true);
            if (GameManager.instance.currentGame == "sneakySpirits")
            {
                SneakySpirits.instance.SpawnGhost(beat, length, new List<int>()
                {
                    volume1, volume2, volume3, volume4, volume5, volume6, volume7
                });
            }
            else
            {
                queuedGhosts.Add(new QueuedGhost
                {
                    beat = beat,
                    length = length,
                    volumes = new List<int>()
                    {
                        volume1, volume2, volume3, volume4, volume5, volume6, volume7
                    }
                });
            }
        }

        public void SpawnGhost(float beat, float length, List<int> volumes)
        {
            ScheduleInput(beat, length * 7, InputType.STANDARD_DOWN, Just, Out, Out);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length * 3, delegate { bowAnim.DoScaledAnimationAsync("BowDraw", 0.25f); })
            });

            List<BeatAction.Action> ghostSpawns = new List<BeatAction.Action>();
            for(int i = 0; i < 7; i++)
            {
                float spawnBeat = beat + length * i;
                if (spawnBeat >= Conductor.instance.songPositionInBeats)
                {
                    SneakySpiritsGhost spawnedGhost = Instantiate(movingGhostPrefab, ghostPositions[i], false);
                    spawnedGhost.transform.position = new Vector3(spawnedGhost.transform.position.x, spawnedGhost.transform.position.y - (1 - volumes[i] * 0.01f) * 2f, spawnedGhost.transform.position.z);
                    spawnedGhost.Init(spawnBeat, length);
                }
            }
        }

        void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            Success(caller);
        }

        void Success(PlayerActionEvent caller)
        {
            SneakySpiritsGhostDeath spawnedDeath = Instantiate(deathGhostPrefab, transform, false);
            spawnedDeath.animToPlay = "GhostDieNose";
            spawnedDeath.startBeat = caller.startBeat + caller.timer;
            spawnedDeath.length = 1f;
            spawnedDeath.gameObject.SetActive(true);
            Jukebox.PlayOneShotGame("sneakySpirits/hit");
            bowAnim.DoScaledAnimationAsync("BowRecoil", 0.25f);
            Conductor.instance.SetMinigamePitch(0.25f);
            doorAnim.DoScaledAnimationAsync("DoorOpen", 0.5f);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { Conductor.instance.SetMinigamePitch(1f); doorAnim.DoScaledAnimationAsync("DoorClose", 0.5f);})
            });
        }

        void Out(PlayerActionEvent caller)
        {

        }
    }
}
