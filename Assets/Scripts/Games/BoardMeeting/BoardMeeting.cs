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

            });
        }
    }
}

namespace HeavenStudio.Games
{
    using HeavenStudio.Games.Scripts_ClappyTrio;
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

                executive.transform.localPosition = new Vector3(startPos + ((maxWidth / (executiveCount + 1)) * (i + 1)), 0);
                executive.GetComponent<SortingGroup>().sortingOrder = i;

                if (i > 0)
                    executives.Add(executive);

                if (i == executiveCount - 1)
                    executive.player = true;
            }
        }
    }
}

