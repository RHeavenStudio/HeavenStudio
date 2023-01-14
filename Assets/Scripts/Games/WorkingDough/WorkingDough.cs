using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

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
                    defaultLength = 1f,
                },
                new GameAction("big ball", "Big Ball")
                {
                    function = delegate { WorkingDough.instance.SpawnBall(eventCaller.currentEntity.beat, true);  },
                    defaultLength = 1f,
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_WorkingDough;
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

        [Header("Variables")]
        public bool intervalStarted;
        //float intervalStartBeat;
        public float beatInterval = 4f;

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
                    //Close npc transporters
                    new BeatAction.Action(beat + interval, delegate { ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftClose", 0, 0); }),
                    new BeatAction.Action(beat + interval, delegate { ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightClose", 0, 0); }),
                    //Open player transporters
                    new BeatAction.Action(beat + interval, delegate { ballTransporterLeftPlayer.GetComponent<Animator>().Play("BallTransporterLeftOpen", 0, 0); }),
                    new BeatAction.Action(beat + interval, delegate { ballTransporterRightPlayer.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0); }),
                    //Close player transporters
                    new BeatAction.Action(beat + interval * 2, delegate { ballTransporterLeftPlayer.GetComponent<Animator>().Play("BallTransporterLeftClose", 0, 0); }),
                    new BeatAction.Action(beat + interval * 2, delegate { ballTransporterRightPlayer.GetComponent<Animator>().Play("BallTransporterRightClose", 0, 0); }),
                });
            }

            //intervalStartBeat = beat;
            beatInterval = interval;
        }

        public void SpawnBall(float beat, bool isBig)
        {
            if (isBig)
            {
                BeatAction.New(doughDudesNPC, new List<BeatAction.Action>()
                {
                    //Jump and play sound
                    new BeatAction.Action(beat + 0.5f, delegate { doughDudesNPC.GetComponent<Animator>().Play("BigDoughJump", 0, 0); }),
                    new BeatAction.Action(beat + 0.5f, delegate { Jukebox.PlayOneShotGame("workingDough/NPCBigBall"); }),
                    new BeatAction.Action(beat + 0.5f, delegate { Jukebox.PlayOneShotGame("workingDough/SmallBall"); }),
                    new BeatAction.Action(beat + 0.5f, delegate { npcImpact.SetActive(true); }),
                    new BeatAction.Action(beat + 0.6f, delegate { npcImpact.SetActive(false); }),
                });
            }
            else
            {
                BeatAction.New(doughDudesNPC, new List<BeatAction.Action>()
                {
                    //Jump and play sound
                    new BeatAction.Action(beat + 0.5f, delegate { doughDudesNPC.GetComponent<Animator>().Play("SmallDoughJump", 0, 0); }),
                    new BeatAction.Action(beat + 0.5f, delegate { Jukebox.PlayOneShotGame("workingDough/NPCSmallBall"); }),
                    new BeatAction.Action(beat + 0.5f, delegate { Jukebox.PlayOneShotGame("workingDough/SmallBall"); }),
                    new BeatAction.Action(beat + 0.5f, delegate { npcImpact.SetActive(true); }),
                    new BeatAction.Action(beat + 0.6f, delegate { npcImpact.SetActive(false); }),
                });
            }
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
