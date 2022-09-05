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
            return new Minigame("builtToScaleDS", "Built To Scale (DS)", "00BB00", true, false, new List<GameAction>()
            {
                new GameAction("spawn blocks", "Spawn Blocks")
                {
                    resizable = true
                },
                new GameAction("play piano", "Play Note")
                {
                    function = delegate { BuiltToScaleDS.instance.PlayPiano(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["type"]); }, 
                    resizable = true, 
                    parameters = new List<Param>() 
                    {
                        new Param("type", new EntityTypes.Integer(-24, 24, 0), "Semitones", "The number of semitones up or down this note should be pitched")
                    } 
                },
            });
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
        public Transform renderQuadTrans;
        public Transform camPivot;
        public Camera camComp;

        [Header("References")]
        public SkinnedMeshRenderer environmentRenderer;
        public GameObject flyingRodBase;
        public GameObject movingBlocksBase;
        public GameObject hitPartsBase;
        public GameObject missPartsBase;
        public Transform partsHolder;
        public Transform blocksHolder;
        public Animator shooterAnim;
        public Animator elevatorAnim;

        [Header("Properties")]
        public float beltSpeed = 1f;

        private Material beltMaterial;
        private Material[] environmentMaterials;
        private float currentBeltOffset;

        [NonSerialized] public bool shootingThisFrame;
        
        public static BuiltToScaleDS instance;

        private void Awake()
        {
            instance = this;

            environmentMaterials = environmentRenderer.materials;
            beltMaterial = Instantiate(environmentMaterials[8]);
            environmentMaterials[8] = beltMaterial;
            renderQuadTrans.gameObject.SetActive(true);

            var cam = GameCamera.instance.camera;
            var camHeight = 2f * cam.orthographicSize;
            var camWidth = camHeight * cam.aspect;
            renderQuadTrans.localScale = new Vector3(camWidth, camHeight, 1f);

            camComp.depthTextureMode = camComp.depthTextureMode | DepthTextureMode.Depth;

            elevatorAnim.Play("MakeRod", 0, 1f);
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

            currentBeltOffset = (currentBeltOffset + Time.deltaTime * -beltSpeed) % 1f;
            beltMaterial.mainTextureOffset = new Vector2(0f, currentBeltOffset);
            environmentRenderer.materials = environmentMaterials;
        }

        void LateUpdate()
        {
            var shooterState = shooterAnim.GetCurrentAnimatorStateInfo(0);
            bool canShoot = !shooterState.IsName("Shoot") || shooterAnim.IsAnimationNotPlaying();

            if (canShoot && PlayerInput.Pressed() && !shootingThisFrame)
            {
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
            var pianoPitch = Mathf.Pow(2f, (1f / 12f) * semiTones) *Conductor.instance.musicSource.pitch;
            var pianoSource = Jukebox.PlayOneShotGame("builtToScaleDS/Piano", -1, pianoPitch, 0.8f, true);

            pianoSource.SetLoopParams(beat + length, 0.1f);
        }
    }
}