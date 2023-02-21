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
                    preFunction = delegate { var e = eventCaller.currentEntity; SneakySpirits.PreSpawnGhost(e.beat, e.length, e["slowDown"], e["volume1"], e["volume2"], e["volume3"], e["volume4"], e["volume5"], e["volume6"],
                        e["volume7"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("slowDown", true, "Slowdown Effect", "Should there be a slowdown effect when the ghost is hit?"),
                        new Param("volume1", new EntityTypes.Integer(0, 100, 100), "Move Volume 1", "What height and what volume should this move be at?"),
                        new Param("volume2", new EntityTypes.Integer(0, 100, 100), "Move Volume 2", "What height and what volume should this move be at?"),
                        new Param("volume3", new EntityTypes.Integer(0, 100, 100), "Move Volume 3", "What height and what volume should this move be at?"),
                        new Param("volume4", new EntityTypes.Integer(0, 100, 100), "Move Volume 4", "What height and what volume should this move be at?"),
                        new Param("volume5", new EntityTypes.Integer(0, 100, 100), "Move Volume 5", "What height and what volume should this move be at?"),
                        new Param("volume6", new EntityTypes.Integer(0, 100, 100), "Move Volume 6", "What height and what volume should this move be at?"),
                        new Param("volume7", new EntityTypes.Integer(0, 100, 100), "Move Volume 7", "What height and what volume should this move be at?"),
                    }
                },
                new GameAction("movebow", "Bow Enter or Exit")
                {
                    function = delegate {var e = eventCaller.currentEntity; SneakySpirits.instance.MoveBow(e.beat, e.length, e["exit"], e["ease"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("exit", true, "Enter?", "Should the bow exit or enter?"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "Which ease should the movement have?")
                    }
                },
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
            public bool slowDown;
            public List<int> volumes;
        }
        [Header("Components")]
        [SerializeField] Animator bowAnim;
        [SerializeField] Animator bowHolderAnim;
        [SerializeField] Animator doorAnim;
        [SerializeField] SneakySpiritsGhost movingGhostPrefab;
        [SerializeField] SneakySpiritsGhostDeath deathGhostPrefab;
        [SerializeField] GameObject arrowMissPrefab;
        [SerializeField] GameObject ghostMissPrefab;
        [SerializeField] List<Transform> ghostPositions = new List<Transform>();
        [Header("Variables")]
        private static List<QueuedGhost> queuedGhosts = new List<QueuedGhost>();
        private bool hasArrowLoaded;
        float movingLength;
        float movingStartBeat;
        bool isMoving;
        string moveAnim;
        EasingFunction.Ease lastEase;

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
                        SpawnGhost(ghost.beat, ghost.length, ghost.slowDown, ghost.volumes);
                    }
                    queuedGhosts.Clear();
                }
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN) && hasArrowLoaded)
                {
                    WhiffArrow(cond.songPositionInBeats);
                }
                if (isMoving)
                {
                    float normalizedBeat = cond.GetPositionFromBeat(movingStartBeat, movingLength);
                    EasingFunction.Function func = EasingFunction.GetEasingFunction(lastEase);
                    float newPos = func(0f, 1f, normalizedBeat);
                    bowHolderAnim.DoNormalizedAnimation(moveAnim, newPos);
                    if (normalizedBeat >= 1f)
                    {
                        isMoving = false;
                    }
                }
            }
            else if (!cond.isPlaying)
            {
                queuedGhosts.Clear();
                Conductor.instance.SetMinigamePitch(1f);
            }
        }

        public void MoveBow(float beat, float length, bool enter, int ease)
        {
            movingStartBeat = beat;
            movingLength = length;
            moveAnim = enter ? "Enter" : "Exit";
            isMoving = true;
            lastEase = (EasingFunction.Ease)ease;
        }

        public static void PreSpawnGhost(float beat, float length, bool slowDown, int volume1, int volume2, int volume3, int volume4, int volume5, int volume6, int volume7)
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
                SneakySpirits.instance.SpawnGhost(beat, length, slowDown, new List<int>()
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
                    },
                    slowDown = slowDown,
                });
            }
        }

        public void SpawnGhost(float beat, float length, bool slowDown, List<int> volumes)
        {
            if (slowDown)
            {
                ScheduleInput(beat, length * 7, InputType.STANDARD_DOWN, Just, Miss, Out);
            }
            else
            {
                ScheduleInput(beat, length * 7, InputType.STANDARD_DOWN, JustNoSlowDown, Miss, Out);
            }
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length * 3, delegate { bowAnim.DoScaledAnimationAsync("BowDraw", 0.25f); hasArrowLoaded = true; })
            });

            List<BeatAction.Action> ghostSpawns = new List<BeatAction.Action>();
            for(int i = 0; i < 7; i++)
            {
                float spawnBeat = beat + length * i;
                if (spawnBeat >= Conductor.instance.songPositionInBeats)
                {
                    SneakySpiritsGhost spawnedGhost = Instantiate(movingGhostPrefab, ghostPositions[i], false);
                    spawnedGhost.transform.position = new Vector3(spawnedGhost.transform.position.x, spawnedGhost.transform.position.y - (1 - volumes[i] * 0.01f) * 2.5f, spawnedGhost.transform.position.z);
                    spawnedGhost.Init(spawnBeat, length);
                }
            }
        }

        void WhiffArrow(float beat)
        {
            GameObject spawnedArrow = Instantiate(arrowMissPrefab, transform);
            spawnedArrow.SetActive(true);
            spawnedArrow.GetComponent<Animator>().DoScaledAnimationAsync("ArrowRecoil", 0.5f);
            bowAnim.DoScaledAnimationAsync("BowRecoil", 0.25f);
            hasArrowLoaded = false;
            Jukebox.PlayOneShotGame("sneakySpirits/arrowMiss", -1, 2);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 3f, delegate { 
                    if (GameManager.instance.currentGame == "sneakySpirits") 
                    {
                        Destroy(spawnedArrow);
                    } 
                })
            });
        }

        void Just(PlayerActionEvent caller, float state)
        {
            if (!hasArrowLoaded) return;
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("sneakySpirits/ghostScared");
                WhiffArrow(caller.startBeat + caller.timer);
                GameObject spawnedGhost = Instantiate(ghostMissPrefab, transform);
                spawnedGhost.SetActive(true);
                spawnedGhost.GetComponent<Animator>().DoScaledAnimationAsync("GhostBarely", 0.5f);
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 2f, delegate {
                        if (GameManager.instance.currentGame == "sneakySpirits")
                        {
                            Destroy(spawnedGhost);
                        }
                    })
                });
                return;
            }
            Success(caller, true);
        }
        
        void JustNoSlowDown(PlayerActionEvent caller, float state)
        {
            if (!hasArrowLoaded) return;
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("sneakySpirits/ghostScared");
                WhiffArrow(caller.startBeat + caller.timer);
                GameObject spawnedGhost = Instantiate(ghostMissPrefab, transform);
                spawnedGhost.SetActive(true);
                spawnedGhost.GetComponent<Animator>().DoScaledAnimationAsync("GhostBarely", 0.5f);
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 2f, delegate {
                        if (GameManager.instance.currentGame == "sneakySpirits")
                        {
                            Destroy(spawnedGhost);
                        }
                    })
                });
                return;
            }
            Success(caller, false);
        }

        void Success(PlayerActionEvent caller, bool slowDown)
        {
            SneakySpiritsGhostDeath spawnedDeath = Instantiate(deathGhostPrefab, transform, false);
            int randomNumber = UnityEngine.Random.Range(0, 4);

            switch (randomNumber)
            {
                case 0:
                    spawnedDeath.animToPlay = "GhostDieNose";
                    break;
                case 1:
                    spawnedDeath.animToPlay = "GhostDieMouth";
                    break;
                case 2:
                    spawnedDeath.animToPlay = "GhostDieBody";
                    break;
                case 3:
                    spawnedDeath.animToPlay = "GhostDieCheek";
                    break;
            }
            hasArrowLoaded = false;
            spawnedDeath.startBeat = caller.startBeat + caller.timer;
            spawnedDeath.length = 1f;
            spawnedDeath.gameObject.SetActive(true);
            Jukebox.PlayOneShotGame("sneakySpirits/hit");
            bowAnim.DoScaledAnimationAsync("BowRecoil", 0.25f);
            if (slowDown) Conductor.instance.SetMinigamePitch(0.25f);
            doorAnim.DoScaledAnimationAsync("DoorOpen", 0.5f);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate 
                { 
                    if (slowDown) Conductor.instance.SetMinigamePitch(1f); 
                    doorAnim.DoScaledAnimationAsync("DoorClose", 0.5f);
                })
            });
        }

        void Miss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("sneakySpirits/ghostEscape");
            GameObject spawnedGhost = Instantiate(ghostMissPrefab, transform);
            spawnedGhost.SetActive(true);
            spawnedGhost.GetComponent<Animator>().DoScaledAnimationAsync("GhostMiss", 0.5f);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate {
                    if (GameManager.instance.currentGame == "sneakySpirits")
                    {
                        Jukebox.PlayOneShotGame("sneakySpirits/laugh", -1, 1.2f);
                        spawnedGhost.GetComponent<Animator>().DoScaledAnimationAsync("GhostLaugh", 0.25f);
                    }
                }),
                new BeatAction.Action(caller.startBeat + caller.timer + 2.5f, delegate {
                    if (GameManager.instance.currentGame == "sneakySpirits")
                    {
                        Destroy(spawnedGhost);
                    }
                })
            });
        }

        void Out(PlayerActionEvent caller)
        {

        }
    }
}
