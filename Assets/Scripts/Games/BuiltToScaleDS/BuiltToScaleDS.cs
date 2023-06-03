using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using DG.Tweening;
using System;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrFlickLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("builtToScaleDS", "Built To Scale (DS)", "1ad21a", false, false, new List<GameAction>()
            {
                new GameAction("spawn blocks", "Widget")
                {
                    function = delegate {var e = eventCaller.currentEntity; BuiltToScaleDS.instance.MultiplePiano(e.beat, e.length, e["silent"], e["note1"], e["note2"], e["note3"], e["note4"], e["note5"], e["note6"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("silent", false, "Mute Audio", "Whether the piano notes should be muted or not."),
                        new Param("note1", new EntityTypes.Integer(-24, 24, 0), "1st note", "The number of semitones up or down this note should be pitched"),
                        new Param("note2", new EntityTypes.Integer(-24, 24, 2), "2nd note", "The number of semitones up or down this note should be pitched"),
                        new Param("note3", new EntityTypes.Integer(-24, 24, 4), "3rd note", "The number of semitones up or down this note should be pitched"),
                        new Param("note4", new EntityTypes.Integer(-24, 24, 5), "4th note", "The number of semitones up or down this note should be pitched"),
                        new Param("note5", new EntityTypes.Integer(-24, 24, 7), "5th note", "The number of semitones up or down this note should be pitched"),
                        new Param("note6", new EntityTypes.Integer(-24, 24, 12), "6th note", "The number of semitones up or down this note should be pitched (This plays together with the 5th note)"),
                    }
                },
                new GameAction("play piano", "Play Note")
                {
                    function = delegate { BuiltToScaleDS.instance.PlayPiano(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["type"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", new EntityTypes.Integer(-24, 24, 0), "Semitones", "The number of semitones up or down this note should be pitched")
                    },
                },
                new GameAction("color", "Color Palette")
                {
                    function = delegate { var e = eventCaller.currentEntity; BuiltToScaleDS.instance.UpdateMappingColors(e["object"], e["shooter"], e["bg"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("object", Color.white, "Object Color"),
                        new Param("shooter", Color.white, "Shooter Color"),
                        new Param("bg", new Color(0, 1, 0, 1), "Environment Color")
                    }
                },
                new GameAction("lights", "Lights")
                {
                    function = delegate { var e = eventCaller.currentEntity; BuiltToScaleDS.instance.Lights(e.beat, e.length, e["auto"], e["light"] && !e["auto"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Lights (Auto)", "Should the lights auto light?"),
                        new Param("light", false, "Lights", "Should the lights light?")
                    }
                }
            }, new List<string>() { "ntr", "normal" }, "ntrassembly", "en", new List<string>() { });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_BuiltToScaleDS;
    
    public class BuiltToScaleDS : Minigame
    {
        public enum BTSObject { HitPieces, MissPieces, FlyingRod }

        [Header("Camera")]
        [SerializeField] Transform camPos;
        [SerializeField] float cameraFoV;

        [Header("References")]
        [SerializeField] SkinnedMeshRenderer environmentRenderer;
        [SerializeField] SkinnedMeshRenderer elevatorRenderer;
        public GameObject flyingRodBase;
        public GameObject movingBlocksBase;
        public GameObject hitPartsBase;
        public GameObject missPartsBase;
        public Transform partsHolder;
        public Transform blocksHolder;
        public Animator shooterAnim;
        public Animator elevatorAnim;

        [SerializeField] private Material shooterMaterial;
        [SerializeField] private Material objectMaterial;
        [SerializeField] private Material gridPlaneMaterial;
        private Material elevatorMaterial;
        private Material[] gridMaterials;
        private Material[] firstPatternLights;
        private Material[] secondPatternLights;
        private Material[] elevatorObjectMats;

        [Header("Properties")]
        [SerializeField] float beltSpeed = 1f;

        private Material beltMaterial;
        private Material[] environmentMaterials;
        private Material[] elevatorMaterials;
        private float currentBeltOffset;
        private bool lighting = false;
        private bool autoLight = false;
        private bool firstLight = true;

        [NonSerialized] public bool shootingThisFrame;
        [NonSerialized] public bool lastShotOut = false;
        private static Color currentObjectColor = Color.white;
        private static Color currentShooterColor = Color.white;
        private static Color currentEnvironmentColor = new Color(0, 1, 0, 1);
        
        public static BuiltToScaleDS instance;

        private GameEvent lightBeat = new GameEvent();

        private void Awake()
        {
            instance = this;

            environmentMaterials = environmentRenderer.materials;
            elevatorMaterials = elevatorRenderer.materials;
            beltMaterial = Instantiate(environmentMaterials[8]);
            environmentMaterials[8] = beltMaterial;
            elevatorObjectMats = new Material[]
            {
                Instantiate(elevatorMaterials[0]),
                Instantiate(elevatorMaterials[1]),
                Instantiate(elevatorMaterials[2]),
            };
            elevatorMaterials[0] = elevatorObjectMats[0];
            elevatorMaterials[1] = elevatorObjectMats[1];
            elevatorMaterials[2] = elevatorObjectMats[2];
            elevatorMaterial = Instantiate(elevatorMaterials[3]);
            elevatorMaterials[3] = elevatorMaterial;
            gridMaterials = new Material[]
            {
                Instantiate(environmentMaterials[9]),
                Instantiate(environmentMaterials[11]),
                Instantiate(environmentMaterials[12]),
                Instantiate(environmentMaterials[13]),
                Instantiate(environmentMaterials[14]),
            };
            environmentMaterials[9] = gridMaterials[0];
            environmentMaterials[11] = gridMaterials[1];
            environmentMaterials[12] = gridMaterials[2];
            environmentMaterials[13] = gridMaterials[3];
            environmentMaterials[14] = gridMaterials[4];

            firstPatternLights = new Material[]
            {
                Instantiate(environmentMaterials[1]),
                Instantiate(environmentMaterials[2]),
                Instantiate(environmentMaterials[4]),
            };
            environmentMaterials[1] = firstPatternLights[0];
            environmentMaterials[2] = firstPatternLights[1];
            environmentMaterials[4] = firstPatternLights[2];

            secondPatternLights = new Material[]
            {
                Instantiate(environmentMaterials[0]),
                Instantiate(environmentMaterials[3])
            };
            environmentMaterials[0] = secondPatternLights[0];
            environmentMaterials[3] = secondPatternLights[1];

            elevatorAnim.Play("MakeRod", 0, 1f);
            UpdateColors();
        }

        private void OnDestroy()
        {
            currentObjectColor = Color.white;
            currentShooterColor = Color.white;
            currentEnvironmentColor = new Color(0, 1, 0, 1);
            UpdateColors();
        }

        private void Start()
        {
            GameCamera.additionalPosition = camPos.position + (Quaternion.Euler(camPos.eulerAngles) * Vector3.forward * 10f);
            GameCamera.additionalRotEluer = camPos.eulerAngles;
            GameCamera.additionalFoV = cameraFoV;
        }

        public void UpdateMappingColors(Color objectColor, Color shooterColor, Color environmentColor)
        {
            currentObjectColor = objectColor;
            currentShooterColor = shooterColor;
            currentEnvironmentColor = environmentColor;
            UpdateColors();
        }

        private void UpdateColors()
        {
            objectMaterial.SetColor("_Color", currentObjectColor);
            shooterMaterial.SetColor("_Color", currentShooterColor);
            beltMaterial.SetColor("_Color", currentEnvironmentColor);
            gridPlaneMaterial.SetColor("_Color", currentEnvironmentColor);
            elevatorMaterial.SetColor("_Color", currentEnvironmentColor);
            foreach (var mat in gridMaterials)
            {
                mat.SetColor("_Color", currentEnvironmentColor);
            }
            foreach (var mat in elevatorObjectMats)
            {
                mat.SetColor("_Color", currentObjectColor);
            }
            if (!lighting)
            {
                foreach (var mat in firstPatternLights)
                {
                    mat.SetColor("_Color", currentEnvironmentColor);
                }
                foreach (var mat in secondPatternLights)
                {
                    mat.SetColor("_Color", currentEnvironmentColor);
                }
            }
        }

        List<DynamicBeatmap.DynamicEntity> spawnedBlockEvents = new List<DynamicBeatmap.DynamicEntity>();
        void Update()
        {
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused)
                return;

            var currentBeat = Conductor.instance.songPositionInBeats;

            var blockEvents = GameManager.instance.Beatmap.entities.FindAll(c => c.datamodel == "builtToScaleDS/spawn blocks");
            for (int i = 0; i < blockEvents.Count; i++)
            {
                var ev = blockEvents[i];
                if (spawnedBlockEvents.Contains(ev)) continue; // Don't spawn the same blocks multiple times.

                var spawnBeat = ev.beat - ev.length;
                if (currentBeat > spawnBeat && currentBeat < ev.beat + ev.length)
                {
                    SpawnBlocks(spawnBeat, ev.length);
                    spawnedBlockEvents.Add(ev);
                    break;
                }
            }

            if (Conductor.instance.ReportBeat(ref lightBeat.lastReportedBeat, lightBeat.startBeat % 1) && autoLight)
            {
                HandleLights();
            }

            currentBeltOffset = (currentBeltOffset + Time.deltaTime * -beltSpeed) % 1f;
            beltMaterial.mainTextureOffset = new Vector2(0f, currentBeltOffset);
            environmentRenderer.materials = environmentMaterials;
            elevatorRenderer.materials = elevatorMaterials;
        }

        void LateUpdate()
        {
            var shooterState = shooterAnim.GetCurrentAnimatorStateInfo(0);
            bool canShoot = (!shooterState.IsName("Shoot") || shooterAnim.IsAnimationNotPlaying()) && !shootingThisFrame;

            if (canShoot && lastShotOut)
                lastShotOut = false;

            if (canShoot && !lastShotOut && PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                lastShotOut = true;
                shootingThisFrame = true;
                Shoot();
                SpawnObject(BTSObject.FlyingRod);
                Jukebox.PlayOneShotGame("builtToScaleDS/Boing");
            }

            if (!shootingThisFrame)
            {
                if (blocksHolder.childCount == 0 && shooterState.IsName("Windup") && shooterAnim.IsAnimationNotPlaying())
                {
                    shooterAnim.Play("WindDown", 0, 0);
                }
            }

            shootingThisFrame = false;
        }

        public void Lights(float beat, float length, bool autoLights, bool shouldLights)
        {
            autoLight = autoLights;
            lighting = autoLights || shouldLights;
            if (shouldLights)
            {
                List<BeatAction.Action> actions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    actions.Add(new BeatAction.Action(beat + i, delegate { HandleLights(); }));
                }
                if (!autoLights)
                {
                    lighting = false;
                    actions.Add(new BeatAction.Action(beat + length, delegate
                    {
                        foreach (var lightMat in firstPatternLights)
                        {
                            lightMat.DOColor(currentEnvironmentColor, 0.2f);
                        }
                        foreach (var lightMat in secondPatternLights)
                        {
                            lightMat.DOColor(currentEnvironmentColor, 0.2f);
                        }
                    }));
                }
                BeatAction.New(instance.gameObject, actions);
            }
            if (!autoLights && !shouldLights)
            {
                lighting = false;
                foreach (var lightMat in firstPatternLights)
                {
                    lightMat.DOColor(currentEnvironmentColor, 0.2f);
                }
                foreach (var lightMat in secondPatternLights)
                {
                    lightMat.DOColor(currentEnvironmentColor, 0.2f);
                }
            }
        }

        private void HandleLights()
        {
            if (firstLight)
            {
                foreach (var lightMat in firstPatternLights)
                {
                    lightMat.DOColor(Color.white, 0.2f);
                }
                foreach (var lightMat in secondPatternLights)
                {
                    lightMat.DOColor(currentEnvironmentColor, 0.2f);
                }
            }
            else
            {
                foreach (var lightMat in firstPatternLights)
                {
                    lightMat.DOColor(currentEnvironmentColor, 0.2f);
                }
                foreach (var lightMat in secondPatternLights)
                {
                    lightMat.DOColor(Color.white, 0.2f);
                }
            }
            firstLight = !firstLight;
        }

        public void SpawnBlocks(float beat, float length)
        {
            var newBlocks = GameObject.Instantiate(movingBlocksBase, blocksHolder).GetComponent<Blocks>();
            newBlocks.createBeat = beat;
            newBlocks.createLength = length;
            newBlocks.gameObject.SetActive(true);

            SetBlockTime(newBlocks, beat, length);
        }

        const int blockFramesPerSecond = 24;
        const int blockHitFrame = 39;
        const int blockTotalFrames = 80;
        const int spawnFrameOffset = -3;
        List<int> criticalFrames = new List<int> { 7, 15, 23, 31, 39, 47 };
        public void SetBlockTime(Blocks blocks, float spawnBeat, float length)
        {
            float spawnTimeOffset = (float)spawnFrameOffset / (float)blockFramesPerSecond;

            float secondsPerFrame = 1f / blockFramesPerSecond;
            float secondsToHitFrame = secondsPerFrame * blockHitFrame;

            float secondsPerBeat = Conductor.instance.secPerBeat;
            float secondsToHitBeat = secondsPerBeat * 5f * length + spawnTimeOffset;

            float speedMult = secondsToHitFrame / secondsToHitBeat;

            float secondsPastSpawnTime = secondsPerBeat * (Conductor.instance.songPositionInBeats - spawnBeat) + spawnTimeOffset;
            float framesPastSpawnTime = blockFramesPerSecond * speedMult * secondsPastSpawnTime;
            
            // The only way I could deal with Unity's interpolation shenanigans without having a stroke.
            if (criticalFrames.Contains(Mathf.FloorToInt(framesPastSpawnTime)))
                framesPastSpawnTime = Mathf.CeilToInt(framesPastSpawnTime);

            blocks.anim.Play("Move", 0, framesPastSpawnTime / blockTotalFrames);
            blocks.anim.speed = speedMult;
        }

        public void SpawnObject(BTSObject btsObject)
        {
            GameObject prefabToUse = null;
            string animNameToUse = "";

            switch (btsObject)
            {
                case BTSObject.HitPieces:
                    prefabToUse = hitPartsBase;
                    animNameToUse = "PartsHit";
                    break;
                case BTSObject.MissPieces:
                    prefabToUse = missPartsBase;
                    animNameToUse = "PartsMiss";
                    break;
                case BTSObject.FlyingRod:
                    prefabToUse = flyingRodBase;
                    animNameToUse = "Fly";
                    break;
            }

            if (prefabToUse != null)
            {
                var newPiece = GameObject.Instantiate(prefabToUse, partsHolder).GetComponent<BTSPiece>();
                newPiece.gameObject.SetActive(true);
                newPiece.anim.Play(animNameToUse, 0, 0);
            }
        }

        public void Shoot()
        {
            shooterAnim.Play("Shoot", 0, 0);
            elevatorAnim.Play("MakeRod", 0, 0);
        }

        public void PlayPiano(float beat, float length, int semiTones)
        {
            var pianoPitch = Jukebox.GetPitchFromSemiTones(semiTones, true);
            var pianoSource = Jukebox.PlayOneShotGame("builtToScaleDS/Piano", -1, pianoPitch, 0.8f, true);

            pianoSource.SetLoopParams(beat + length, 0.1f);
        }

        public void MultiplePiano(float beat, float length, bool silent, int note1, int note2, int note3, int note4, int note5, int note6)
        {
            if (silent) return;
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { PlayPiano(beat, length, note1); }),
                new BeatAction.Action(beat + length, delegate { PlayPiano(beat + length, length, note2); }),
                new BeatAction.Action(beat + length * 2, delegate { PlayPiano(beat + length * 2, length, note3); }),
                new BeatAction.Action(beat + length * 3, delegate { PlayPiano(beat + length * 3, length, note4); }),
                new BeatAction.Action(beat + length * 4, delegate { PlayPiano(beat + length * 4, 1f, note5); }),
                new BeatAction.Action(beat + length * 4, delegate { PlayPiano(beat + length * 4, 1f, note6); }),
            });
        }
    }
}