using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlWorkingDoughLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("workingDough", "Working Dough \n<color=#eb5454>[WIP]</color>", "090909", false, false, new List<GameAction>()
            {
                new GameAction("beat intervals", "Start Interval")
                {
                    function = delegate { WorkingDough.instance.SetIntervalStart(eventCaller.currentEntity.beat, eventCaller.currentEntity.length);  },
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("small ball", "Small Ball")
                {
                    function = delegate { WorkingDough.instance.SpawnBall(eventCaller.currentEntity.beat, false);  },
                    defaultLength = 2f,
                },
                new GameAction("big ball", "Big Ball")
                {
                    function = delegate { WorkingDough.instance.SpawnBall(eventCaller.currentEntity.beat, true);  },
                    defaultLength = 2f,
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_WorkingDough;
    public class WorkingDough : Minigame
    {
        [Header("Components")]
        [SerializeField] GameObject doughDudesNPC; //Jump animations
        [SerializeField] GameObject doughDudesPlayer; //Jump animations
        [SerializeField] GameObject ballTransporterRightNPC; //Close and open animations
        [SerializeField] GameObject ballTransporterLeftNPC; //Close and open animations
        [SerializeField] GameObject ballTransporterRightPlayer; //Close and open animations
        [SerializeField] GameObject ballTransporterLeftPlayer; //Close and open animations
        [SerializeField] GameObject npcImpact;
        [SerializeField] GameObject playerImpact;
        [SerializeField] GameObject smallBallNPC;
        [SerializeField] GameObject bigBallNPC;
        [SerializeField] Transform ballHolder;
        [SerializeField] SpriteRenderer arrowSRLeftNPC;
        [SerializeField] SpriteRenderer arrowSRRightNPC;
        [SerializeField] SpriteRenderer arrowSRLeftPlayer;
        [SerializeField] SpriteRenderer arrowSRRightPlayer;
        [SerializeField] GameObject NPCBallTransporters;
        [SerializeField] GameObject PlayerBallTransporters;

        [Header("Variables")]
        public bool intervalStarted;
        //float intervalStartBeat;
        public float beatInterval = 4f;
        public bool bigMode;

        [Header("Curves")]
        public BezierCurve3D npcEnterUpCurve;
        public BezierCurve3D npcEnterDownCurve;
        public BezierCurve3D npcExitUpCurve;
        public BezierCurve3D npcExitDownCurve;

        [Header("Resources")]
        public Sprite whiteArrowSprite;
        public Sprite redArrowSprite;

        public static WorkingDough instance;

        void Awake()
        {
            instance = this;
        }

        public void SetIntervalStart(float beat, float interval)
        {
            if (!intervalStarted)
            {
                intervalStarted = true;
                //Open npc transporters
                ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftOpen", 0, 0);
                ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0);
                BeatAction.New(ballTransporterLeftNPC, new List<BeatAction.Action>()
                {
                    //End interval
                    new BeatAction.Action(beat + interval, delegate { intervalStarted = false; }),
                    //Open player transporters
                    new BeatAction.Action(beat + interval, delegate { ballTransporterLeftPlayer.GetComponent<Animator>().Play("BallTransporterLeftOpen", 0, 0); }),
                    new BeatAction.Action(beat + interval, delegate { ballTransporterRightPlayer.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0); }),
                    new BeatAction.Action(beat + interval, delegate {
                        if (bigMode)
                        {
                            NPCBallTransporters.GetComponent<Animator>().Play("NPCExitBigMode", 0, 0);
                        }
                    }),
                    //Close npc transporters
                    new BeatAction.Action(beat + interval + 0.5f, delegate { ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftClose", 0, 0); }),
                    new BeatAction.Action(beat + interval + 0.5f, delegate { ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightClose", 0, 0); }),
                    //Close player transporters
                    new BeatAction.Action(beat + interval * 2 + 0.5f, delegate { ballTransporterLeftPlayer.GetComponent<Animator>().Play("BallTransporterLeftClose", 0, 0); }),
                    new BeatAction.Action(beat + interval * 2 + 0.5f, delegate { ballTransporterRightPlayer.GetComponent<Animator>().Play("BallTransporterRightClose", 0, 0); }),
                });
            }

            //intervalStartBeat = beat;
            beatInterval = interval;
        }

        public void SpawnBall(float beat, bool isBig)
        {
            if (!intervalStarted)
            {
                SetIntervalStart(beat, beatInterval);
            }
            var objectToSpawn = isBig ? bigBallNPC : smallBallNPC;
            var spawnedBall = GameObject.Instantiate(objectToSpawn, ballHolder);

            var ballComponent = spawnedBall.GetComponent<NPCDoughBall>();
            ballComponent.startBeat = beat;
            ballComponent.exitUpCurve = npcExitUpCurve;
            ballComponent.enterUpCurve = npcEnterUpCurve;
            ballComponent.exitDownCurve = npcExitDownCurve;
            ballComponent.enterDownCurve = npcEnterDownCurve;

            spawnedBall.SetActive(true);

            if (isBig && !bigMode)
            {
                NPCBallTransporters.GetComponent<Animator>().Play("NPCGoBigMode", 0, 0);
                bigMode = true;
            }

            arrowSRLeftNPC.sprite = redArrowSprite;
            BeatAction.New(doughDudesNPC, new List<BeatAction.Action>()
            {
                //Jump and play sound
                new BeatAction.Action(beat + 0.1f, delegate { arrowSRLeftNPC.sprite = whiteArrowSprite; }),
                new BeatAction.Action(beat + 1f, delegate { doughDudesNPC.GetComponent<Animator>().Play(isBig ? "BigDoughJump" :"SmallDoughJump", 0, 0); }),
                new BeatAction.Action(beat + 1f, delegate { Jukebox.PlayOneShotGame(isBig ? "workingDough/NPCBigBall" : "workingDough/NPCSmallBall"); }),
                new BeatAction.Action(beat + 1f, delegate { Jukebox.PlayOneShotGame("workingDough/SmallBall"); }),
                new BeatAction.Action(beat + 1f, delegate { npcImpact.SetActive(true); }),
                new BeatAction.Action(beat + 1.1f, delegate { npcImpact.SetActive(false); }),
                new BeatAction.Action(beat + 1.9f, delegate { arrowSRRightNPC.sprite = redArrowSprite; }),
                new BeatAction.Action(beat + 2f, delegate { arrowSRRightNPC.sprite = whiteArrowSprite; }),
            });

        }

        void Update()
        {
            if(PlayerInput.Pressed())
            {
                doughDudesPlayer.GetComponent<Animator>().Play("SmallDoughJump", 0, 0);
                Jukebox.PlayOneShotGame("workingDough/PlayerSmallBall");
            }
            else if (PlayerInput.AltPressed())
            {
                doughDudesPlayer.GetComponent<Animator>().Play("BigDoughJump", 0, 0);
                Jukebox.PlayOneShotGame("workingDough/PlayerBigBall");
            }
        }
    }
}
