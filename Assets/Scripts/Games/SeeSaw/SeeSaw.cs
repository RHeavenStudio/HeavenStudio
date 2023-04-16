using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class RvlSeeSawLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("seeSaw", "See-Saw", "ffb4f7", false, false, new List<GameAction>()
            {
                new GameAction("longLong", "Long Long")
                {
                    defaultLength = 4f,
                },
                new GameAction("longShort", "Long Short")
                {
                    defaultLength = 3f
                },
                new GameAction("shortLong", "Short Long")
                {
                    defaultLength = 3f
                },
                new GameAction("shortShort", "Short Short")
                {
                    defaultLength = 3f
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class SeeSaw : Minigame
    {
        [Header("Components")]
        [SerializeField] Animator seeSawAnim;

        [Header("Properties")]
        bool canPrepare = true;

        private int currentJumpIndex;

        private List<DynamicBeatmap.DynamicEntity> allJumpEvents = new List<DynamicBeatmap.DynamicEntity>();

        public static SeeSaw instance;

        private void Awake()
        {
            instance = this;
            var jumpEvents = EventCaller.GetAllInGameManagerList("seeSaw", new string[] { "longLong", "longShort", "shortLong", "shortShort" });
            List<DynamicBeatmap.DynamicEntity> tempEvents = new List<DynamicBeatmap.DynamicEntity>();
            for (int i = 0; i < jumpEvents.Count; i++)
            {
                if (jumpEvents[i].beat + jumpEvents[i].beat >= Conductor.instance.songPositionInBeats)
                {
                    tempEvents.Add(jumpEvents[i]);
                }
            }

            allJumpEvents = tempEvents;
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (allJumpEvents.Count > 0)
                {
                    if (currentJumpIndex < allJumpEvents.Count && currentJumpIndex >= 0)
                    {
                        if (cond.songPositionInBeats >= allJumpEvents[currentJumpIndex].beat)
                        {
                            currentJumpIndex++;
                        }
                        else if (cond.songPositionInBeats >= allJumpEvents[currentJumpIndex].beat - ((allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortLong" || allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortShort") ? 1 : 2))
                        {
                            if (currentJumpIndex == 0)
                            {
                                if (canPrepare)
                                {
                                    Jukebox.PlayOneShotGame("seeSaw/prepareHigh", allJumpEvents[currentJumpIndex].beat - ((allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortLong" || allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortShort") ? 1 : 2));
                                    canPrepare = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void LongLong(float beat)
        {

        }
    }

}
