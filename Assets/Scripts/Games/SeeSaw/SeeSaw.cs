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
                    function = delegate { SeeSaw.instance.LongLong(eventCaller.currentEntity.beat); },
                    defaultLength = 4f,
                },
                new GameAction("longShort", "Long Short")
                {
                    function = delegate { SeeSaw.instance.LongShort(eventCaller.currentEntity.beat); },
                    defaultLength = 3f
                },
                new GameAction("shortLong", "Short Long")
                {
                    function = delegate { SeeSaw.instance.ShortLong(eventCaller.currentEntity.beat); },
                    defaultLength = 3f
                },
                new GameAction("shortShort", "Short Short")
                {
                    function = delegate { SeeSaw.instance.ShortShort(eventCaller.currentEntity.beat); },
                    defaultLength = 2f
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
                        if (currentJumpIndex == 0 || allJumpEvents[currentJumpIndex].beat > allJumpEvents[currentJumpIndex - 1].length + ((allJumpEvents[currentJumpIndex].datamodel == "seeSaw/longShort" || allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortShort") ? 1 : 2))
                        {
                            if (cond.songPositionInBeats >= allJumpEvents[currentJumpIndex].beat - ((allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortLong" || allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortShort") ? 1 : 2))
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
            seeSawAnim.transform.localScale = new Vector3(-1, 1, 1);
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("seeSaw/otherLongJump", beat),
                new MultiSound.Sound("seeSaw/otherVoiceLong1", beat),
                new MultiSound.Sound("seeSaw/otherVoiceLong2", beat + 1),
            });
            ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, JustLong, MissLong, Empty);
            if (currentJumpIndex < allJumpEvents.Count) 
            { 
                if (currentJumpIndex >= 0)
                {
                    currentJumpIndex++;
                }

                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat + 4)
                {
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + 6);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 6, delegate { canPrepare = true; })
                    });
                }
            } 
        }

        public void LongShort(float beat)
        {
            seeSawAnim.transform.localScale = new Vector3(-1, 1, 1);
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("seeSaw/otherLongJump", beat),
                new MultiSound.Sound("seeSaw/otherVoiceLong1", beat),
                new MultiSound.Sound("seeSaw/otherVoiceLong2", beat + 1),
            });
            ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, JustShort, MissShort, Empty);
            if (currentJumpIndex < allJumpEvents.Count)
            {
                if (currentJumpIndex >= 0)
                {
                    currentJumpIndex++;
                }

                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat + 3)
                {
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + 4);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 4, delegate { canPrepare = true; })
                    });
                }
            }
        }

        public void ShortLong(float beat)
        {
            seeSawAnim.transform.localScale = new Vector3(-1, 1, 1);
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("seeSaw/otherShortJump", beat),
                new MultiSound.Sound("seeSaw/otherVoiceShort1", beat),
                new MultiSound.Sound("seeSaw/otherVoiceShort2", beat + 0.5f),
            });
            ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, JustLong, MissLong, Empty);
            if (currentJumpIndex < allJumpEvents.Count)
            {
                if (currentJumpIndex >= 0)
                {
                    currentJumpIndex++;
                }

                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat + 3)
                {
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + 5);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 5, delegate { canPrepare = true; })
                    });
                }
            }
        }

        public void ShortShort(float beat)
        {
            seeSawAnim.transform.localScale = new Vector3(-1, 1, 1);
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("seeSaw/otherShortJump", beat),
                new MultiSound.Sound("seeSaw/otherVoiceShort1", beat),
                new MultiSound.Sound("seeSaw/otherVoiceShort2", beat + 0.5f),
            });
            ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, JustShort, MissShort, Empty);
            if (currentJumpIndex < allJumpEvents.Count)
            {
                if (currentJumpIndex >= 0)
                {
                    currentJumpIndex++;
                }

                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat + 2)
                {
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + 3);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 3, delegate { canPrepare = true; })
                    });
                }
            }
        }

        public void JustLong(PlayerActionEvent caller, float state)
        {
            seeSawAnim.transform.localScale = new Vector3(1, 1, 1);
            if (state <= -1f || state >= 1f)
            {
                seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
                Jukebox.PlayOneShotGame("seeSaw/ow");
                return;
            }
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("seeSaw/playerLongJump", caller.timer + caller.startBeat),
                new MultiSound.Sound("seeSaw/playerVoiceLong1", caller.timer + caller.startBeat),
                new MultiSound.Sound("seeSaw/just", caller.timer + caller.startBeat),
                new MultiSound.Sound("seeSaw/playerVoiceLong2", caller.timer + caller.startBeat + 1),
            });
        }

        public void MissLong(PlayerActionEvent caller)
        {
            seeSawAnim.transform.localScale = new Vector3(1, 1, 1);
            seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
            Jukebox.PlayOneShotGame("seeSaw/miss");
        }

        public void JustShort(PlayerActionEvent caller, float state)
        {
            seeSawAnim.transform.localScale = new Vector3(1, 1, 1);
            if (state <= -1f || state >= 1f)
            {
                seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
                Jukebox.PlayOneShotGame("seeSaw/ow");
                return;
            }
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("seeSaw/playerLongJump", caller.timer + caller.startBeat),
                new MultiSound.Sound("seeSaw/playerVoiceShort1", caller.timer + caller.startBeat),
                new MultiSound.Sound("seeSaw/just", caller.timer + caller.startBeat),
                new MultiSound.Sound("seeSaw/playerVoiceShort2", caller.timer + caller.startBeat + 0.5f),
            });
        }

        public void MissShort(PlayerActionEvent caller)
        {
            seeSawAnim.transform.localScale = new Vector3(1, 1, 1);
            seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
            Jukebox.PlayOneShotGame("seeSaw/miss");
        }

        public void Empty(PlayerActionEvent caller) { }
    }

}
