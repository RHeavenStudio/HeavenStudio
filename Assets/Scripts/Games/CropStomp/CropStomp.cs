using DG.Tweening;
using NaughtyBezierCurves;
using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrCropLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("cropStomp", "Crop Stomp", "BFDEA6", false, false, new List<GameAction>()
            {
                new GameAction("start marching", "Start Marching")
                {
                    function = delegate { CropStomp.instance.StartMarching(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2f,
                    inactiveFunction = delegate { CropStomp.MarchInactive(eventCaller.currentEntity.beat); }
                },
                new GameAction("veggies", "Veggies")
                {
                    defaultLength = 4f, 
                    resizable = true
                },
                new GameAction("mole", "Mole")
                {
                    defaultLength = 2f
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_CropStomp;

    public class CropStomp : Minigame
    {
        const float stepDistance = 2.115f;
        public static float[] moleSoundOffsets = new float[]{ 0.134f, 0.05f, 0.061f };

        float scrollRate => stepDistance / (Conductor.instance.pitchedSecPerBeat * 2f);
        float grassWidth;
        float dotsWidth = 19.2f;

        private float newBeat = -1f; // So that marching can happen on beat 0.
        private float marchStartBeat = -1f;
        private float marchOffset;
        private int currentMarchBeat;
        private int stepCount;
        private bool isStepping;
        
        private static float inactiveStart = -1f;

        public bool isMarching => marchStartBeat != -1f;

        [NonSerialized] public bool isFlicking;

        public GameObject baseVeggie;
        public GameObject baseMole;
        public Animator legsAnim;
        public Animator bodyAnim;
        public Transform farmerTrans;
        public SpriteRenderer grass;
        public Transform grassTrans;
        public Transform dotsTrans;
        public Transform scrollingHolder;
        public Transform veggieHolder;
        public Farmer farmer;
        public BezierCurve3D pickCurve;
        public BezierCurve3D moleCurve;

        private Tween shakeTween;

        public static CropStomp instance;

        private void Awake()
        {
            instance = this;// Finding grass sprite width for grass scrolling.
            farmer.Init();
            var grassSprite = grass.sprite;
            var borderLeft = grassSprite.rect.xMin + grassSprite.border.x;
            var borderRight = grassSprite.rect.xMax - grassSprite.border.z;
            var borderWidthPixels = borderRight - borderLeft;
            grassWidth = borderWidthPixels / grassSprite.pixelsPerUnit;

            legsAnim.Play("LiftFront", 0, 1); // Start with leg up.

            // Initialize vegetables.
            var cond = Conductor.instance;
            var entities = GameManager.instance.Beatmap.entities;

            float startBeat = cond.songPositionInBeats;

            if (inactiveStart == -1f)
            {
                // Find the beat of the closest "start marching" event.
                var marchStarts = entities.FindAll(m => m.datamodel == "cropStomp/start marching");
                for (int i = 0; i < marchStarts.Count; i++)
                {
                    var sampleBeat = marchStarts[i].beat;
                    if (cond.songPositionInBeats <= sampleBeat + 0.25f) // 0.25-beat buffer in case the start marching event is directly next to the game switch event.
                    {
                        startBeat = sampleBeat;
                        break;
                    }
                }
            }
            else
            {
                // Find the beat of the next step, assuming marching started at inactiveStart.
                int stepsPassed = 0;

                while (inactiveStart + (stepsPassed * 2f) < cond.songPositionInBeats)
                {
                    stepsPassed++;

                    if (stepsPassed > 1000)
                    {
                        Debug.Log("Loop broke!");
                        return;
                    }
                }

                startBeat = inactiveStart + (stepsPassed * 2f);

                // Cue the marching proper to begin when applicable.
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(startBeat - 0.25f, delegate { StartMarching(startBeat); })
                });

                inactiveStart = -1f;
            }

            // Veggie and mole events.
            var vegEvents = entities.FindAll(v => v.datamodel == "cropStomp/veggies");
            var moleEvents = entities.FindAll(m => m.datamodel == "cropStomp/mole");

            // Spawn veggies.
            for (int i = 0; i < vegEvents.Count; i++)
            {
                var vegBeat = vegEvents[i].beat;
                var vegLength = vegEvents[i].length;

                // Only consider veggie events that aren't past the start point.
                if (startBeat <= vegBeat + vegLength)
                {
                    int veggiesInEvent = Mathf.CeilToInt(vegLength + 1) / 2;

                    for (int b = 0; b < veggiesInEvent; b++)
                    {
                        var targetVeggieBeat = vegBeat + 2f * b;
                        if (startBeat <= targetVeggieBeat)
                        {
                            SpawnVeggie(targetVeggieBeat, startBeat, false);
                        }
                    }
                }
            }

            // Spawn moles.
            for (int i = 0; i < moleEvents.Count; i++)
            {
                var moleBeat = moleEvents[i].beat;

                if (startBeat <= moleBeat)
                {
                    SpawnVeggie(moleBeat, startBeat, true);
                }
            }
        }

        List<DynamicBeatmap.DynamicEntity> cuedMoleSounds = new List<DynamicBeatmap.DynamicEntity>();
        private void Update()
        {
            var cond = Conductor.instance;

            if (!cond.isPlaying)
                return;

            // Mole sounds.
            var moleEvents = GameManager.instance.Beatmap.entities.FindAll(m => m.datamodel == "cropStomp/mole");
            for (int i = 0; i < moleEvents.Count; i++)
            {
                var moleEvent = moleEvents[i];
                var timeToEvent = moleEvent.beat - cond.songPositionInBeats;
                if (timeToEvent <= 4f && timeToEvent > 2f && !cuedMoleSounds.Contains(moleEvent))
                {
                    cuedMoleSounds.Add(moleEvent);
                    MultiSound.Play(new MultiSound.Sound[] { new MultiSound.Sound("cropStomp/moleNyeh", (moleEvent.beat - 2f) - moleSoundOffsets[0] * Conductor.instance.songBpm / 60f),
                                                            new MultiSound.Sound("cropStomp/moleHeh1", (moleEvent.beat - 1.5f) - moleSoundOffsets[1] * Conductor.instance.songBpm / 60f),
                                                            new MultiSound.Sound("cropStomp/moleHeh2", (moleEvent.beat - 1f) - moleSoundOffsets[2] * Conductor.instance.songBpm / 60f) });
                }
            }

            if (!isMarching)
                return;
            // Debug.Log(newBeat);

            if (cond.ReportBeat(ref newBeat, marchOffset, true))
            {
                currentMarchBeat += 1;

                PlayAnims();
                if (currentMarchBeat % 2 != 0) //step sound
                {
                    MultiSound.Play(new MultiSound.Sound[] {new MultiSound.Sound("cropStomp/hmm", newBeat + marchOffset)});
                }
            }

            // Object scroll.
            var scrollPos = scrollingHolder.localPosition;
            var newScrollX = scrollPos.x + (scrollRate * Time.deltaTime);
            scrollingHolder.localPosition = new Vector3(newScrollX, scrollPos.y, scrollPos.z);

            // Grass scroll.
            var grassPos = grassTrans.localPosition;

            var newGrassX = grassPos.x + (scrollRate * Time.deltaTime);
            newGrassX = (newGrassX % (grassWidth * 4.5f));

            grassTrans.localPosition = new Vector3(newGrassX, grassPos.y, grassPos.z);

            // Dots scroll
            var dotsPos = dotsTrans.localPosition;

            var newDotsX = dotsPos.x + (scrollRate * Time.deltaTime);
            newDotsX = (newDotsX % dotsWidth);

            dotsTrans.localPosition = new Vector3(newDotsX, dotsPos.y, dotsPos.z);
        }

        private void LateUpdate()
        {
            if (!isMarching)
                return;

            if (PlayerInput.PressedUp())
            {
                // Don't play raise animation if successfully flicked.
                if (!isFlicking)
                    bodyAnim.Play("Raise");
            }

            isFlicking = false;
        }

        private void PlayAnims()
        {
            // Step.
            if (currentMarchBeat % 2 != 0)
            {
                // Don't step if already stomped.
                if (!isStepping)
                {
                    stepCount += 1;
                    var stepAnim = (stepCount % 2 != 0 ? "StepFront" : "StepBack");

                    legsAnim.Play(stepAnim, 0, 0);

                    isStepping = true;
                }

            }
            // Lift.
            else
            {
                var liftAnim = (stepCount % 2 != 0 ? "LiftBack" : "LiftFront");
                legsAnim.Play(liftAnim, 0, 0);

                var farmerPos = farmerTrans.localPosition;
                farmerTrans.localPosition = new Vector3(farmerPos.x - stepDistance, farmerPos.y, farmerPos.z);

                isStepping = false;
            }
        }

        public void StartMarching(float beat)
        {
            marchStartBeat = beat;
            marchOffset = marchStartBeat % 1;
            currentMarchBeat = 0;
            stepCount = 0;

            farmer.nextStompBeat = beat;
        }

        public void Stomp()
        {
            // Don't increment step counter if autostep stepped already.
            if (!isStepping)
                stepCount += 1;

            var stompAnim = (stepCount % 2 != 0 ? "StompFront" : "StompBack");
            
            legsAnim.Play(stompAnim, 0, 0);

            Jukebox.PlayOneShotGame("cropStomp/stomp");

            if (shakeTween != null)
                shakeTween.Kill(true);

            DOTween.Punch(() => GameCamera.additionalPosition, x => GameCamera.additionalPosition = x, new Vector3(0, 0.75f, 0), 
                Conductor.instance.pitchedSecPerBeat*0.5f, 18, 1f);

            isStepping = true;
        }

        private void SpawnVeggie(float beat, float startBeat, bool isMole)
        {
            var newVeggie = GameObject.Instantiate(isMole ? baseMole : baseVeggie, veggieHolder).GetComponent<Veggie>();

            newVeggie.targetBeat = beat;

            var veggieX = (beat - startBeat) * -stepDistance / 2f;
            newVeggie.transform.localPosition = new Vector3(veggieX, 0f, 0f);
            newVeggie.Init();
            newVeggie.gameObject.SetActive(true);
        }
        
        public static void MarchInactive(float beat)
        {
            if (GameManager.instance.currentGame == "cropStomp") //this function is only meant for making march sounds while the game is inactive
            {
                return;
            }
            inactiveStart = beat;
            DynamicBeatmap.DynamicEntity gameSwitch = GameManager.instance.Beatmap.entities.Find(c => c.beat >= beat && c.datamodel == "gameManager/switchGame/cropStomp");
            if (gameSwitch == null)
                return;
            int length = Mathf.CeilToInt((gameSwitch.beat - beat)/2);
            MultiSound.Sound[] sounds = new MultiSound.Sound[length];
            for(int i = 0; i < length; i++)
            {
                sounds[i] = new MultiSound.Sound("cropStomp/hmm", beat + i*2);
            }
            MultiSound.Play(sounds, forcePlay:true);
        }
    }
}
