using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class RvlBoardMeetingLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("boardMeeting", "Board Meeting", "FFFFFF", false, false, new List<GameAction>()
            {
                new GameAction("prepare", "Prepare")
                {
                    function = delegate { BoardMeeting.instance.Prepare(); }
                },
                new GameAction("spin", "Spin")
                {
                    function = delegate {var e = eventCaller.currentEntity; BoardMeeting.instance.Spin(e["start"], e["end"]); },
                    parameters = new List<Param>()
                    {
                        new Param("start", new EntityTypes.Integer(1, 40, 1), "Starting Pig", "Which pig from the far left (1) or far right (4) should be the first to spin?"),
                        new Param("end", new EntityTypes.Integer(1, 40, 4), "Ending Pig", "Which pig from the far left (1) or far right (4) should be the last to spin?")
                    }
                },
                new GameAction("stop", "Stop")
                {
                    function = delegate { var e = eventCaller.currentEntity; BoardMeeting.instance.Stop(e.beat, e.length); },
                    resizable = true
                },
                new GameAction("assStop", "Assistant Stop")
                {
                    function = delegate { var e = eventCaller.currentEntity; BoardMeeting.instance.AssistantStop(e.beat); },
                    defaultLength = 3f
                },
                new GameAction("changeCount", "Change Executives")
                {
                    function = delegate { BoardMeeting.instance.ChangeExecutiveCount(eventCaller.currentEntity["amount"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("amount", new EntityTypes.Integer(3, 40, 4), "Amount", "How many executives will there be?")
                    }
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_BoardMeeting;
    using UnityEngine.Rendering;

    public class BoardMeeting : Minigame
    {
        [Header("Components")]
        [SerializeField] Transform farLeft;
        [SerializeField] Transform farRight;
        [SerializeField] Animator assistantAnim;

        [Header("Properties")]
        [SerializeField] int executiveCount = 4;
        [SerializeField] List<BMExecutive> executives = new List<BMExecutive>();

        public static BoardMeeting instance;

        private void Awake()
        {
            instance = this;
            InitExecutives();
        }

        public void AssistantStop(float beat)
        {
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { assistantAnim.DoScaledAnimationAsync("One", 0.5f); }),
                new BeatAction.Action(beat + 1, delegate { assistantAnim.DoScaledAnimationAsync("Three", 0.5f); }),
                new BeatAction.Action(beat + 2, delegate 
                { 
                    foreach (var executive in executives)
                    {
                        if (executive.player) continue;
                        executive.Stop();
                    }
                }),
            });
            ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, JustAssistant, Miss, Empty);
        }

        public void Stop(float beat, float length)
        {
            List<BeatAction.Action> stops = new List<BeatAction.Action>();
            for (int i = 0; i < executiveCount; i++)
            {
                if (executives[i].player) break;
                int index = i;
                stops.Add(new BeatAction.Action(beat + length * i, delegate { executives[index].Stop(); }));
            }
            BeatAction.New(instance.gameObject, stops);
            ScheduleInput(beat, length * (executiveCount - 1), InputType.STANDARD_DOWN, Just, Miss, Empty);
        }

        public void Prepare()
        {
            foreach (var executive in executives)
            {
                executive.Prepare();
            }
        }

        public void Spin(int start, int end)
        {
            if (start > executiveCount || end > executiveCount) return;
            for (int i = start - 1; i < end; i++)
            {
                executives[i].Spin();
            }
        }

        public void InitExecutives()
        {
            float startPos = farLeft.position.x;
            float maxWidth = Mathf.Abs(farLeft.position.x - farRight.position.x);

            for (int i = 0; i < executiveCount; i++)
            {
                BMExecutive executive;
                if (i == 0) executive = executives[0];
                else executive = Instantiate(executives[0], transform);

                executive.transform.localPosition = new Vector3(startPos + ((maxWidth / (executiveCount - 1)) * i), 0);
                executive.GetComponent<SortingGroup>().sortingOrder = i;

                if (i > 0)
                    executives.Add(executive);

                if (i == executiveCount - 1)
                    executive.player = true;
            }
        }

        public void ChangeExecutiveCount(int count)
        {
            for (int i = 1; i < executiveCount; i++)
            {
                Destroy(executives[i].gameObject);
            }
            executives.RemoveRange(1, executiveCount - 1);
            executiveCount = count;
            InitExecutives();
        }

        void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            executives[executiveCount - 1].Stop();
        }

        void JustAssistant(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            executives[executiveCount - 1].Stop();
            assistantAnim.DoScaledAnimationAsync("Stop", 0.5f);
        }

        void Miss(PlayerActionEvent caller)
        {

        }

        void Empty(PlayerActionEvent caller) { }
    }
}

