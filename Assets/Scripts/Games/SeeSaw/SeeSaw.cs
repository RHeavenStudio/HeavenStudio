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
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.LongLong(e.beat, e["high"], e["height"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("high", false, "High", "Will they perform high jumps?"),
                        new Param("height", new EntityTypes.Float(0, 1, 0), "Height", "Controls how high the high jump will go, 0 is the minimum height, 1 is the maximum height.")
                    }
                },
                new GameAction("longShort", "Long Short")
                {
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.LongShort(e.beat, e["high"], e["height"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("high", false, "High", "Will they perform high jumps?"),
                        new Param("height", new EntityTypes.Float(0, 1, 0), "Height", "Controls how high the high jump will go, 0 is the minimum height, 1 is the maximum height.")
                    }
                },
                new GameAction("shortLong", "Short Long")
                {
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.ShortLong(e.beat, e["high"], e["height"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("high", false, "High", "Will they perform high jumps?"),
                        new Param("height", new EntityTypes.Float(0, 1, 0), "Height", "Controls how high the high jump will go, 0 is the minimum height, 1 is the maximum height.")
                    }
                },
                new GameAction("shortShort", "Short Short")
                {
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.ShortShort(e.beat, e["high"], e["height"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    { 
                        new Param("high", false, "High", "Will they perform high jumps?"), 
                        new Param("height", new EntityTypes.Float(0, 1, 0), "Height", "Controls how high the high jump will go, 0 is the minimum height, 1 is the maximum height.") 
                    }
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
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

        private void Start()
        {
            if (allJumpEvents.Count > 0 && allJumpEvents[0].datamodel is "seeSaw/shortLong" or "seeSaw/shortShort")
            {
                saw.anim.Play("GetUp_In", 0, 1);
                saw.transform.position = saw.landInTrans.position;
            }
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
                                    bool inJump = allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortLong" || allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortShort";
                                    float beatToJump = allJumpEvents[currentJumpIndex].beat - (inJump ? 1 : 2);
                                    Jukebox.PlayOneShotGame("seeSaw/prepareHigh", beatToJump);
                                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                                    {
                                        new BeatAction.Action(beatToJump, delegate { see.SetState(inJump ? SeeSawGuy.JumpState.StartJumpIn : SeeSawGuy.JumpState.StartJump, beatToJump); })
                                    });
                                    canPrepare = false;
                                }
                            }
                        }

                    }
                }
            }
        }

        public void LongLong(float beat, bool high, float height)
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
                see.Land(SeeSawGuy.LandType.Big, true);
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
                see.Land(SeeSawGuy.LandType.Normal, true);
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
                DetermineSawJump(beat, high, height);

                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat + 4)
                {
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + 4);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 4, delegate { see.Land(SeeSawGuy.LandType.Normal, true); canPrepare = true; canStartJump = true; })
                    });
                }
            } 
        }

        public void LongShort(float beat, bool high, float height)
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
                see.Land(SeeSawGuy.LandType.Big, true);
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
                see.Land(SeeSawGuy.LandType.Normal, true);
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
                DetermineSawJump(beat, high, height);

                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat + 3)
                {
                    float beatLength = see.ShouldEndJumpOut() ? 4 : 3;
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + beatLength);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + beatLength, delegate { see.Land(SeeSawGuy.LandType.Normal, true); canPrepare = true; canStartJump = true; })
                    });
                }
            }
        }

        public void ShortLong(float beat, bool high, float height)
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
                see.Land(SeeSawGuy.LandType.Big, false);
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
                see.Land(SeeSawGuy.LandType.Normal, false);
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
                DetermineSawJump(beat, high, height);

                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat + 3)
                {
                    float beatLength = see.ShouldEndJumpOut() ? 3 : 2;
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + beatLength);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + beatLength, delegate { see.Land(SeeSawGuy.LandType.Normal, false); canPrepare = true; canStartJump = true; })
                    });
                }
            }
        }

        public void ShortShort(float beat, bool high, float height)
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
                see.Land(SeeSawGuy.LandType.Big, false);
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
                see.Land(SeeSawGuy.LandType.Normal, false);
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
                DetermineSawJump(beat, high, height);

                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat + 2)
                {
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + 2);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 2, delegate { see.Land(SeeSawGuy.LandType.Normal, false); canPrepare = true; canStartJump = true; })
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

        void DetermineSeeJump(float beat, bool miss = false)
        {
            if (currentJumpIndex >= 0
                && (allJumpEvents[currentJumpIndex - 1].datamodel == "seeSaw/longLong" || allJumpEvents[currentJumpIndex - 1].datamodel == "seeSaw/shortLong"))
            {
                if (NextJumpEventIsOnBeat())
                {
                    if (allJumpEvents[currentJumpIndex].datamodel is "seeSaw/longLong" or "seeSaw/shortLong")
                    {
                        see.SetState(SeeSawGuy.JumpState.OutOut, beat, miss);
                    }
                    else if (allJumpEvents[currentJumpIndex].datamodel is "seeSaw/longShort" or "seeSaw/shortShort")
                    {
                        see.SetState(SeeSawGuy.JumpState.OutIn, beat, miss);
                    }
                }
                else
                {
                    if (see.ShouldEndJumpOut())
                    {
                        see.SetState(SeeSawGuy.JumpState.EndJumpOut, beat, miss);
                    }
                    else
                    {
                        see.SetState(SeeSawGuy.JumpState.EndJumpIn, beat, miss);
                    }
                }

            }
            else if (currentJumpIndex >= 0
                && (allJumpEvents[currentJumpIndex - 1].datamodel == "seeSaw/longShort" || allJumpEvents[currentJumpIndex - 1].datamodel == "seeSaw/shortShort"))
            {
                if (NextJumpEventIsOnBeat())
                {
                    if (allJumpEvents[currentJumpIndex].datamodel is "seeSaw/longLong" or "seeSaw/shortLong")
                    {
                        see.SetState(SeeSawGuy.JumpState.InOut, beat, miss);
                    }
                    else if (allJumpEvents[currentJumpIndex].datamodel is "seeSaw/longShort" or "seeSaw/shortShort")
                    {
                        see.SetState(SeeSawGuy.JumpState.InIn, beat, miss);
                    }
                }
                else
                {
                    if (see.ShouldEndJumpOut())
                    {
                        see.SetState(SeeSawGuy.JumpState.EndJumpOut, beat, miss);
                    }
                    else
                    {
                        see.SetState(SeeSawGuy.JumpState.EndJumpIn, beat, miss);
                    }
                }
            }
        }

        void DetermineSawJump(float beat, bool high, float height)
        {
            if (currentJumpIndex >= 0)
            {
                if (allJumpEvents[currentJumpIndex - 1].datamodel is "seeSaw/longShort" or "seeSaw/longLong")
                {
                    if (currentJumpIndex < allJumpEvents.Count)
                    {
                        if (allJumpEvents[currentJumpIndex].datamodel is "seeSaw/longShort" or "seeSaw/longLong")
                        {
                            saw.SetState(high ? SeeSawGuy.JumpState.HighOutOut : SeeSawGuy.JumpState.OutOut, beat, false, height);
                        }
                        else
                        {
                            saw.SetState(high ? SeeSawGuy.JumpState.HighOutIn : SeeSawGuy.JumpState.OutIn, beat, false, height);
                        }
                    }
                    else
                    {
                        saw.SetState(high ? SeeSawGuy.JumpState.HighOutOut : SeeSawGuy.JumpState.OutOut, beat, false, height);
                    }
                }
                else
                {
                    if (currentJumpIndex < allJumpEvents.Count)
                    {
                        if (allJumpEvents[currentJumpIndex].datamodel is "seeSaw/longShort" or "seeSaw/longLong")
                        {
                            saw.SetState(high ? SeeSawGuy.JumpState.HighInOut : SeeSawGuy.JumpState.InOut, beat, false, height);
                        }
                        else
                        {
                            saw.SetState(high ? SeeSawGuy.JumpState.HighInIn : SeeSawGuy.JumpState.InIn, beat, false, height);
                        }
                    }
                    else
                    {
                        saw.SetState(high ? SeeSawGuy.JumpState.HighInIn : SeeSawGuy.JumpState.InIn, beat, false, height);
                    }
                }
            }
        }

        bool NextJumpEventIsOnBeat()
        {
            return currentJumpIndex < allJumpEvents.Count && allJumpEvents[currentJumpIndex].beat == allJumpEvents[currentJumpIndex - 1].beat + allJumpEvents[currentJumpIndex - 1].length;
        }

        public void JustLong(PlayerActionEvent caller, float state)
        {
            seeSawAnim.transform.localScale = new Vector3(1, 1, 1);
            if (state <= -1f || state >= 1f)
            {
                seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
                Jukebox.PlayOneShotGame("seeSaw/ow");
                saw.Land(SeeSawGuy.LandType.Barely, true);
                DetermineSeeJump(caller.timer + caller.startBeat, true);
                return;
            }
            DetermineSeeJump(caller.timer + caller.startBeat);
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            saw.Land(SeeSawGuy.LandType.Normal, true);
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
            if (state <= -1f || state >= 1f)
            {
                seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
                Jukebox.PlayOneShotGame("seeSaw/ow");
                saw.Land(SeeSawGuy.LandType.Barely, true);
                DetermineSeeJump(caller.timer + caller.startBeat, true);
                return;
            }
            DetermineSeeJump(caller.timer + caller.startBeat);
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            Jukebox.PlayOneShotGame("seeSaw/explosionBlack");
            saw.Land(SeeSawGuy.LandType.Big, true);
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
            saw.Land(SeeSawGuy.LandType.Miss, true);
            DetermineSeeJump(caller.timer + caller.startBeat, true);
        }

        public void JustShort(PlayerActionEvent caller, float state)
        {
            seeSawAnim.transform.localScale = new Vector3(1, 1, 1);
            if (state <= -1f || state >= 1f)
            {
                seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
                Jukebox.PlayOneShotGame("seeSaw/ow");
                saw.Land(SeeSawGuy.LandType.Barely, false);
                DetermineSeeJump(caller.timer + caller.startBeat, true);
                return;
            }
            DetermineSeeJump(caller.timer + caller.startBeat);
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            saw.Land(SeeSawGuy.LandType.Normal, false);
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
            if (state <= -1f || state >= 1f)
            {
                seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
                Jukebox.PlayOneShotGame("seeSaw/ow");
                saw.Land(SeeSawGuy.LandType.Barely, false);
                DetermineSeeJump(caller.timer + caller.startBeat, true);
                return;
            }
            DetermineSeeJump(caller.timer + caller.startBeat);
            seeSawAnim.DoScaledAnimationAsync("Good", 0.5f);
            Jukebox.PlayOneShotGame("seeSaw/explosionWhite");
            saw.Land(SeeSawGuy.LandType.Big, false);
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
            saw.Land(SeeSawGuy.LandType.Miss, false);
            DetermineSeeJump(caller.timer + caller.startBeat, true);
        }

        public void Empty(PlayerActionEvent caller) { }
    }

}
