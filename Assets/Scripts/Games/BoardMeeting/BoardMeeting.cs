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

        [Header("Properties")]
        [SerializeField] int executiveCount = 4;
        [SerializeField] List<BMExecutive> executives = new List<BMExecutive>();

        public static BoardMeeting instance;

        private void Awake()
        {
            instance = this;
            InitExecutives();
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
    }
}

