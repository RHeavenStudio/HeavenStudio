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
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.LongLong(e.beat, e["high"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("high", false, "High", "Will they perform high jumps?")
                    }
                },
                new GameAction("longShort", "Long Short")
                {
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.LongShort(e.beat, e["high"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("high", false, "High", "Will they perform high jumps?")
                    }
                },
                new GameAction("shortLong", "Short Long")
                {
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.ShortLong(e.beat, e["high"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("high", false, "High", "Will they perform high jumps?")
                    }
                },
                new GameAction("shortShort", "Short Short")
                {
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.ShortShort(e.beat, e["high"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("high", false, "High", "Will they perform high jumps?")
                    }
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using HeavenStudio.Games.Scripts_SpaceSoccer;
    using Scripts_SeeSaw;
    public class SeeSaw : Minigame
    {
        [Header("Components")]
        [SerializeField] Animator seeSawAnim;
        [SerializeField] SeeSawGuy see;
        [SerializeField] SeeSawGuy saw;

        [Header("Properties")]
        bool canPrepare = true;
        bool canStartJump;
        [SerializeField] SuperCurveObject.Path[] jumpPaths;

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

        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (SuperCurveObject.Path path in jumpPaths)
            {
                if (path.preview)
                {
                    see.DrawEditorGizmo(path);
                }
            }
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
                        if (currentJumpIndex == 0 || allJumpEvents[currentJumpIndex].beat > allJumpEvents[currentJumpIndex - 1].length + ((allJumpEvents[currentJumpIndex].datamodel == "seeSaw/longShort" 
                            || allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortShort") ? 1 : 2))
                        {
                            if (cond.songPositionInBeats >= allJumpEvents[currentJumpIndex].beat - ((allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortLong" 
                                || allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortShort") ? 1 : 2))
                            {
                                if (canPrepare && cond.songPositionInBeats < allJumpEvents[currentJumpIndex].beat)
                                {
                                    float beatToJump = allJumpEvents[currentJumpIndex].beat - ((allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortLong" || allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortShort") ? 1 : 2);
                                    Jukebox.PlayOneShotGame("seeSaw/prepareHigh", beatToJump);
                                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                                    {
                                        new BeatAction.Action(beatToJump, delegate { see.SetState(SeeSawGuy.JumpState.StartJump, beatToJump); })
                                    });
                                    canPrepare = false;
                                }
                            }
                        }

                    }
                }
            }
        }

        public void LongLong(float beat, bool high)
        {
            if (currentJumpIndex != 0)
            {
                if (currentJumpIndex < allJumpEvents.Count && currentJumpIndex > 0)
                {
                    if (allJumpEvents[currentJumpIndex - 1].beat + allJumpEvents[currentJumpIndex - 1].length != beat && !canPrepare && !canStartJump)
                    {
                        currentJumpIndex++;
                        return;
                    }
                }
            }
            canStartJump = false;
            seeSawAnim.transform.localScale = new Vector3(-1, 1, 1);
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            if (high)
            {
                see.Land(true, SeeSawGuy.LandType.Big);
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("seeSaw/otherHighJump", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceLong1", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceLong2", beat + 1),
                    new MultiSound.Sound("seeSaw/midAirShine", beat + 1),
                });
            }
            else
            {
                see.Land(true, SeeSawGuy.LandType.Normal);
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("seeSaw/otherLongJump", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceLong1", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceLong2", beat + 1),
                });
            }

            ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, high ? JustLongHigh : JustLong, MissLong, Empty);
            if (currentJumpIndex < allJumpEvents.Count) 
            { 
                if (currentJumpIndex >= 0)
                {
                    currentJumpIndex++;
                }
                DetermineSawJump(beat);

                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat + 4)
                {
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + 6);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 6, delegate { canPrepare = true; canStartJump = true; })
                    });
                }
            } 
        }

        public void LongShort(float beat, bool high)
        {
            if (currentJumpIndex != 0)
            {
                if (currentJumpIndex < allJumpEvents.Count && currentJumpIndex > 0)
                {
                    if (allJumpEvents[currentJumpIndex - 1].beat + allJumpEvents[currentJumpIndex - 1].length != beat && !canPrepare && !canStartJump)
                    {
                        currentJumpIndex++;
                        return;
                    }
                }
            }
            canStartJump = false;
            seeSawAnim.transform.localScale = new Vector3(-1, 1, 1);
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            if (high)
            {
                see.Land(true, SeeSawGuy.LandType.Big);
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("seeSaw/otherHighJump", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceLong1", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceLong2", beat + 1),
                    new MultiSound.Sound("seeSaw/midAirShine", beat + 1),
                });
            }
            else
            {
                see.Land(true, SeeSawGuy.LandType.Normal);
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("seeSaw/otherLongJump", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceLong1", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceLong2", beat + 1),
                });
            }
            ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, high ? JustShortHigh : JustShort, MissShort, Empty);
            if (currentJumpIndex < allJumpEvents.Count)
            {
                if (currentJumpIndex >= 0)
                {
                    currentJumpIndex++;
                }
                DetermineSawJump(beat);

                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat + 3)
                {
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + 4);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 4, delegate { canPrepare = true; canStartJump = true; })
                    });
                }
            }
        }

        public void ShortLong(float beat, bool high)
        {
            if (currentJumpIndex != 0)
            {
                if (currentJumpIndex < allJumpEvents.Count && currentJumpIndex > 0)
                {
                    if (allJumpEvents[currentJumpIndex - 1].beat + allJumpEvents[currentJumpIndex - 1].length != beat && !canPrepare && !canStartJump)
                    {
                        currentJumpIndex++;
                        return;
                    }
                }
            }
            canStartJump = false;
            seeSawAnim.transform.localScale = new Vector3(-1, 1, 1);
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            if (high)
            {
                see.Land(false, SeeSawGuy.LandType.Big);
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("seeSaw/otherHighJump", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceShort1", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceShort2", beat + 0.5f),
                    new MultiSound.Sound("seeSaw/midAirShine", beat + 0.5f),
                });
            }
            else
            {
                see.Land(false, SeeSawGuy.LandType.Normal);
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("seeSaw/otherShortJump", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceShort1", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceShort2", beat + 0.5f),
                });
            }
            ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, high ? JustLongHigh : JustLong, MissLong, Empty);
            if (currentJumpIndex < allJumpEvents.Count)
            {
                if (currentJumpIndex >= 0)
                {
                    currentJumpIndex++;
                }
                DetermineSawJump(beat);

                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat + 3)
                {
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + 5);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 5, delegate { canPrepare = true; canStartJump = true; })
                    });
                }
            }
        }

        public void ShortShort(float beat, bool high)
        {
            if (currentJumpIndex != 0)
            {
                if (currentJumpIndex < allJumpEvents.Count && currentJumpIndex > 0)
                {
                    if (allJumpEvents[currentJumpIndex - 1].beat + allJumpEvents[currentJumpIndex - 1].length != beat && !canPrepare && !canStartJump) 
                    {
                        currentJumpIndex++;
                        return;
                    } 
                }
            }
            canStartJump = false;
            seeSawAnim.transform.localScale = new Vector3(-1, 1, 1);
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            if (high)
            {
                see.Land(false, SeeSawGuy.LandType.Big);
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("seeSaw/otherHighJump", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceShort1", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceShort2", beat + 0.5f),
                    new MultiSound.Sound("seeSaw/midAirShine", beat + 0.5f),
                });
            }
            else
            {
                see.Land(false, SeeSawGuy.LandType.Normal);
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("seeSaw/otherShortJump", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceShort1", beat),
                    new MultiSound.Sound("seeSaw/otherVoiceShort2", beat + 0.5f),
                });
            }
            ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, high ? JustShortHigh : JustShort, MissShort, Empty);
            if (currentJumpIndex < allJumpEvents.Count)
            {
                if (currentJumpIndex >= 0)
                {
                    currentJumpIndex++;
                }
                DetermineSawJump(beat);

                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat + 2)
                {
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + 3);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 3, delegate { canPrepare = true; canStartJump = true; })
                    });
                }
            }
        }

        public SuperCurveObject.Path GetPath(string name)
        {
            foreach (SuperCurveObject.Path path in jumpPaths)
            {
                if (path.name == name)
                {
                    return path;
                }
            }
            return default(SuperCurveObject.Path);
        }

        void DetermineSeeJump(float beat)
        {
            if (currentJumpIndex >= 0
                && (allJumpEvents[currentJumpIndex - 1].datamodel == "seeSaw/longLong" || allJumpEvents[currentJumpIndex - 1].datamodel == "seeSaw/shortLong"))
            {
                see.SetState(SeeSawGuy.JumpState.OutOut, beat);
            }
            else if (currentJumpIndex >= 0
                && (allJumpEvents[currentJumpIndex - 1].datamodel == "seeSaw/longShort" || allJumpEvents[currentJumpIndex - 1].datamodel == "seeSaw/shortShort"))
            {
                
            }
        }

        void DetermineSawJump(float beat)
        {
            if (currentJumpIndex >= 0
                && (allJumpEvents[currentJumpIndex - 1].datamodel == "seeSaw/longLong" || allJumpEvents[currentJumpIndex - 1].datamodel == "seeSaw/longShort"))
            {
                saw.SetState(SeeSawGuy.JumpState.OutOut, beat);
            }
            else if (currentJumpIndex >= 0
                && (allJumpEvents[currentJumpIndex - 1].datamodel == "seeSaw/shortLong" || allJumpEvents[currentJumpIndex - 1].datamodel == "seeSaw/shortShort"))
            {

            }
        }

        public void JustLong(PlayerActionEvent caller, float state)
        {
            seeSawAnim.transform.localScale = new Vector3(1, 1, 1);
            DetermineSeeJump(caller.timer + caller.startBeat);
            if (state <= -1f || state >= 1f)
            {
                seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
                Jukebox.PlayOneShotGame("seeSaw/ow");
                saw.Land(true, SeeSawGuy.LandType.Barely);
                return;
            }
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            saw.Land(true, SeeSawGuy.LandType.Normal);
            if (currentJumpIndex >= 0 && currentJumpIndex != allJumpEvents.Count 
                && allJumpEvents[currentJumpIndex].beat == allJumpEvents[currentJumpIndex - 1].beat + allJumpEvents[currentJumpIndex - 1].length 
                && allJumpEvents[currentJumpIndex]["high"])
            {
                Jukebox.PlayOneShotGame("seeSaw/playerHighJump");
            }
            else
            {
                Jukebox.PlayOneShotGame("seeSaw/playerLongJump");
            }
            Jukebox.PlayOneShotGame("seeSaw/playerVoiceLong1");
            Jukebox.PlayOneShotGame("seeSaw/just");
            Jukebox.PlayOneShotGame("seeSaw/playerVoiceLong2", caller.timer + caller.startBeat + 1f);
        }

        public void JustLongHigh(PlayerActionEvent caller, float state)
        {
            seeSawAnim.transform.localScale = new Vector3(1, 1, 1);
            DetermineSeeJump(caller.timer + caller.startBeat);
            if (state <= -1f || state >= 1f)
            {
                seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
                Jukebox.PlayOneShotGame("seeSaw/ow");
                saw.Land(true, SeeSawGuy.LandType.Barely);
                return;
            }
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            Jukebox.PlayOneShotGame("seeSaw/explosionBlack");
            saw.Land(true, SeeSawGuy.LandType.Big);
            if (currentJumpIndex >= 0 && currentJumpIndex != allJumpEvents.Count
                && allJumpEvents[currentJumpIndex].beat == allJumpEvents[currentJumpIndex - 1].beat + allJumpEvents[currentJumpIndex - 1].length
                && allJumpEvents[currentJumpIndex]["high"])
            {
                Jukebox.PlayOneShotGame("seeSaw/playerHighJump");
            }
            else
            {
                Jukebox.PlayOneShotGame("seeSaw/playerLongJump");
            }
            Jukebox.PlayOneShotGame("seeSaw/playerVoiceLong1");
            Jukebox.PlayOneShotGame("seeSaw/just");
            Jukebox.PlayOneShotGame("seeSaw/playerVoiceLong2", caller.timer + caller.startBeat + 1f);
        }

        public void MissLong(PlayerActionEvent caller)
        {
            seeSawAnim.transform.localScale = new Vector3(1, 1, 1);
            seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
            Jukebox.PlayOneShotGame("seeSaw/miss");
            saw.Land(true, SeeSawGuy.LandType.Miss);
            DetermineSeeJump(caller.timer + caller.startBeat);
        }

        public void JustShort(PlayerActionEvent caller, float state)
        {
            seeSawAnim.transform.localScale = new Vector3(1, 1, 1);
            DetermineSeeJump(caller.timer + caller.startBeat);
            if (state <= -1f || state >= 1f)
            {
                seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
                Jukebox.PlayOneShotGame("seeSaw/ow");
                saw.Land(false, SeeSawGuy.LandType.Barely);
                return;
            }
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            saw.Land(false, SeeSawGuy.LandType.Normal);
            if (currentJumpIndex >= 0 && currentJumpIndex != allJumpEvents.Count
                && allJumpEvents[currentJumpIndex].beat == allJumpEvents[currentJumpIndex - 1].beat + allJumpEvents[currentJumpIndex - 1].length
                && allJumpEvents[currentJumpIndex]["high"])
            {
                Jukebox.PlayOneShotGame("seeSaw/playerHighJump");
            }
            else
            {
                Jukebox.PlayOneShotGame("seeSaw/playerShortJump");
            }
            Jukebox.PlayOneShotGame("seeSaw/playerVoiceShort1");
            Jukebox.PlayOneShotGame("seeSaw/just");
            Jukebox.PlayOneShotGame("seeSaw/playerVoiceShort2", caller.timer + caller.startBeat + 0.5f);
        }

        public void JustShortHigh(PlayerActionEvent caller, float state)
        {
            seeSawAnim.transform.localScale = new Vector3(1, 1, 1);
            DetermineSeeJump(caller.timer + caller.startBeat);
            if (state <= -1f || state >= 1f)
            {
                seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
                Jukebox.PlayOneShotGame("seeSaw/ow");
                saw.Land(false, SeeSawGuy.LandType.Barely);
                return;
            }
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            Jukebox.PlayOneShotGame("seeSaw/explosionWhite");
            saw.Land(false, SeeSawGuy.LandType.Big);
            if (currentJumpIndex >= 0 && currentJumpIndex != allJumpEvents.Count
                && allJumpEvents[currentJumpIndex].beat == allJumpEvents[currentJumpIndex - 1].beat + allJumpEvents[currentJumpIndex - 1].length
                && allJumpEvents[currentJumpIndex]["high"])
            {
                Jukebox.PlayOneShotGame("seeSaw/playerHighJump");
            }
            else
            {
                Jukebox.PlayOneShotGame("seeSaw/playerShortJump");
            }
            Jukebox.PlayOneShotGame("seeSaw/playerVoiceShort1");
            Jukebox.PlayOneShotGame("seeSaw/just");
            Jukebox.PlayOneShotGame("seeSaw/playerVoiceShort2", caller.timer + caller.startBeat + 0.5f);
        }

        public void MissShort(PlayerActionEvent caller)
        {
            seeSawAnim.transform.localScale = new Vector3(1, 1, 1);
            seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
            Jukebox.PlayOneShotGame("seeSaw/miss");
            saw.Land(false, SeeSawGuy.LandType.Miss);
            DetermineSeeJump(caller.timer + caller.startBeat);
        }

        public void Empty(PlayerActionEvent caller) { }
    }

}
