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
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.cameraMove = e["camMove"]; SeeSaw.instance.LongLong(e.beat, e["high"], e["height"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("high", false, "High", "Will they perform high jumps?"),
                        new Param("height", new EntityTypes.Float(0, 1, 0), "Height", "Controls how high the high jump will go, 0 is the minimum height, 1 is the maximum height."),
                        new Param("camMove", true, "Camera Movement", "Will the camera follow saw when it jumps up high?")
                    }
                },
                new GameAction("longShort", "Long Short")
                {
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.cameraMove = e["camMove"]; SeeSaw.instance.LongShort(e.beat, e["high"], e["height"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("high", false, "High", "Will they perform high jumps?"),
                        new Param("height", new EntityTypes.Float(0, 1, 0), "Height", "Controls how high the high jump will go, 0 is the minimum height, 1 is the maximum height."),
                        new Param("camMove", true, "Camera Movement", "Will the camera follow saw when it jumps up high?")
                    }
                },
                new GameAction("shortLong", "Short Long")
                {
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.cameraMove = e["camMove"]; SeeSaw.instance.ShortLong(e.beat, e["high"], e["height"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("high", false, "High", "Will they perform high jumps?"),
                        new Param("height", new EntityTypes.Float(0, 1, 0), "Height", "Controls how high the high jump will go, 0 is the minimum height, 1 is the maximum height."),
                        new Param("camMove", true, "Camera Movement", "Will the camera follow saw when it jumps up high?")
                    }
                },
                new GameAction("shortShort", "Short Short")
                {
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.cameraMove = e["camMove"]; SeeSaw.instance.ShortShort(e.beat, e["high"], e["height"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("high", false, "High", "Will they perform high jumps?"),
                        new Param("height", new EntityTypes.Float(0, 1, 0), "Height", "Controls how high the high jump will go, 0 is the minimum height, 1 is the maximum height."),
                        new Param("camMove", true, "Camera Movement", "Will the camera follow saw when it jumps up high?")
                    }
                },
                new GameAction("changeBgColor", "Change Background Color")
                {
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.ChangeColor(e.beat, e.length, e["colorFrom"], e["colorTo"], e["colorFrom2"], e["colorTo2"], e["ease"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorFrom", SeeSaw.defaultBGColor, "Start Color (Top)", "The color the background will start at."),
                        new Param("colorTo", SeeSaw.defaultBGColor, "End Color (Top)", "The color the background will end at."),
                        new Param("colorFrom2", SeeSaw.defaultBGColorBottom, "Start Color (Bottom)", "The color the background will start at."),
                        new Param("colorTo2", SeeSaw.defaultBGColorBottom, "End Color (Bottom)", "The color the background will end at."),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "The ease of the fade.")
                    }
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.Bop(e.beat, e.length, e["bopSee"], e["bopSaw"], e["autoSee"], e["autoSaw"], e["strumSee"], e["strumSaw"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bopSee", true, "See Bop", "Should see bop?"),
                        new Param("bopSaw", true, "Saw Bop", "Should saw bop?"),
                        new Param("autoSee", false, "See Bop (Auto)", "Should see auto bop?"),
                        new Param("autoSaw", false, "Saw Bop (Auto)", "Should saw auto bop?"),
                        new Param("strumSee", false, "See Strum", "Should see do the strum bop?"),
                        new Param("strumSaw", false, "Saw Strum", "Should saw do the strum bop?"),
                    }
                },
                new GameAction("choke", "Choke")
                {
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.instance.Choke(e.beat, e.length, e["see"], e["saw"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("see", true, "See", "Will See Choke?"),
                        new Param("saw", true, "Saw", "Will Saw Choke?")
                    }
                },
                new GameAction("recolor", "Color Pallete")
                {
                    function = delegate { var e = eventCaller.currentEntity; SeeSaw.ChangeMappingColor(e["fill"], e["outline"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("outline", SeeSaw.defaultOtherColor, "Outline Color", "The color of the outlines on see and saw."),
                        new Param("fill", Color.white, "Fill Color", "The color of the fills on see and saw")
                    }
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_SeeSaw;
    using System;
    using System.Linq;

    public class SeeSaw : Minigame
    {
        private static Color _defaultBGColor;
        public static Color defaultBGColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FF00E4", out _defaultBGColor);
                return _defaultBGColor;
            }
        }
        private static Color _defaultBGColorBottom;
        public static Color defaultBGColorBottom
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FFB4F7", out _defaultBGColorBottom);
                return _defaultBGColorBottom;
            }
        }

        private static Color _defaultOtherColor;
        public static Color defaultOtherColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#0A103C", out _defaultOtherColor);
                return _defaultOtherColor;
            }
        }

        [Header("Components")]
        [SerializeField] Animator seeSawAnim;
        [SerializeField] SeeSawGuy see;
        [SerializeField] SeeSawGuy saw;
        [SerializeField] ParticleSystem leftWhiteOrbs;
        [SerializeField] ParticleSystem rightBlackOrbs;

        //bg stuffs
        [SerializeField] SpriteRenderer gradient;
        [SerializeField] SpriteRenderer bgLow;
        [SerializeField] SpriteRenderer bgHigh;

        [SerializeField] SpriteRenderer[] recolors;

        [Header("Properties")]
        [NonSerialized] public bool sawShouldBop;
        [NonSerialized] public bool seeShouldBop;
        GameEvent bop = new GameEvent();
        float bgColorStartBeat;
        float bgColorLength;
        EasingFunction.Ease lastEase;
        Color colorFrom;
        Color colorTo;
        Color colorFrom2;
        Color colorTo2;
        bool canPrepare = true;
        bool canStartJump;
        public bool cameraMove = true;
        [SerializeField] SuperCurveObject.Path[] jumpPaths;
        [Header("Color Mapping")]
        public Material MappingMaterial;
        public static Color FillColor = Color.white;
        public static Color OutlineColor = new Color(0.03921569f, 0.0627451f, 0.2352941f, 1);

        private int currentJumpIndex;

        private List<DynamicBeatmap.DynamicEntity> allJumpEvents = new List<DynamicBeatmap.DynamicEntity>();

        public static SeeSaw instance;

        private void Awake()
        {
            instance = this;
            GrabJumpEvents();
        }

        private void Start()
        {
            if (allJumpEvents.Count > 0 && allJumpEvents[0].datamodel is "seeSaw/shortLong" or "seeSaw/shortShort")
            {
                saw.anim.Play("GetUp_In", 0, 1);
                saw.transform.position = saw.landInTrans.position;
            }
            MappingMaterial.SetColor("_ColorBravo", FillColor);
            MappingMaterial.SetColor("_ColorDelta", OutlineColor);
            foreach (var recolor in recolors)
            {
                recolor.color = OutlineColor;
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

        private void GrabJumpEvents()
        {
            var jumpEvents = EventCaller.GetAllInGameManagerList("seeSaw", new string[] { "longLong", "longShort", "shortLong", "shortShort" });
            List<DynamicBeatmap.DynamicEntity> tempEvents = new List<DynamicBeatmap.DynamicEntity>();
            for (int i = 0; i < jumpEvents.Count; i++)
            {
                if (jumpEvents[i].beat >= Conductor.instance.songPositionInBeats)
                {
                    tempEvents.Add(jumpEvents[i]);
                }
            }
            tempEvents.Sort((s1, s2) => s1.beat.CompareTo(s2.beat));
            List<DynamicBeatmap.DynamicEntity> tempEvents2 = new List<DynamicBeatmap.DynamicEntity>();
            if (tempEvents.Count > 1)
            {
                float goodBeat = tempEvents[0].beat + tempEvents[0].length;
                for (int i = 1; i < tempEvents.Count; i++)
                {
                    if (tempEvents[i].beat != goodBeat)
                    {
                        tempEvents2.Add(tempEvents[i]);
                    }
                    else
                    {
                        goodBeat = tempEvents[i].beat + tempEvents[i].length;
                    }
                }
            }
            tempEvents = tempEvents.Except(tempEvents2).ToList();
            allJumpEvents = tempEvents;
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                float normalizedBeat = cond.GetPositionFromBeat(bgColorStartBeat, bgColorLength);
                if (normalizedBeat > 0 && normalizedBeat <= 1)
                {
                    EasingFunction.Function func = EasingFunction.GetEasingFunction(lastEase);
                    float newColorR = func(colorFrom.r, colorTo.r, normalizedBeat);
                    float newColorG = func(colorFrom.g, colorTo.g, normalizedBeat);
                    float newColorB = func(colorFrom.b, colorTo.b, normalizedBeat);
                    bgHigh.color = new Color(newColorR, newColorG, newColorB);
                    gradient.color = new Color(newColorR, newColorG, newColorB);
                    newColorR = func(colorFrom2.r, colorTo2.r, normalizedBeat);
                    newColorG = func(colorFrom2.g, colorTo2.g, normalizedBeat);
                    newColorB = func(colorFrom2.b, colorTo2.b, normalizedBeat);
                    bgLow.color = new Color(newColorR, newColorG, newColorB);
                }
                if (allJumpEvents.Count > 0 && !(see.dead || saw.dead))
                {
                    if (currentJumpIndex < allJumpEvents.Count && currentJumpIndex >= 0)
                    {
                        if (currentJumpIndex == 0 
                            || allJumpEvents[currentJumpIndex].beat > allJumpEvents[currentJumpIndex - 1].length + ((allJumpEvents[currentJumpIndex].datamodel is "seeSaw/longShort" or "seeSaw/shortShort") ? 1 : 2))
                        {
                            if (cond.songPositionInBeats >= allJumpEvents[currentJumpIndex].beat - ((allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortLong" 
                                || allJumpEvents[currentJumpIndex].datamodel == "seeSaw/shortShort") ? 1 : 2))
                            {
                                if (canPrepare && cond.songPositionInBeats < allJumpEvents[currentJumpIndex].beat)
                                {
                                    bool inJump = allJumpEvents[currentJumpIndex].datamodel is "seeSaw/shortLong" or "seeSaw/shortShort";
                                    float beatToJump = allJumpEvents[currentJumpIndex].beat - (inJump ? 1 : 2);
                                    Jukebox.PlayOneShotGame("seeSaw/prepareHigh", beatToJump);
                                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                                    {
                                        new BeatAction.Action(beatToJump, delegate { see.SetState(inJump ? SeeSawGuy.JumpState.StartJumpIn : SeeSawGuy.JumpState.StartJump, beatToJump); see.canBop = false; })
                                    });
                                    canPrepare = false;
                                }
                            }
                        }

                    }
                }
                if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
                {
                    if (seeShouldBop)
                    {
                        see.Bop();
                    }
                    if (sawShouldBop)
                    {
                        saw.Bop();
                    }
                }
            }
        }

        public static void ChangeMappingColor(Color colorFill, Color colorOutline)
        {
            OutlineColor = colorOutline;
            FillColor = colorFill;
            if (GameManager.instance.currentGame == "seeSaw")
            {
                instance.UpdateColors();
            }
        }

        private void UpdateColors()
        {
            MappingMaterial.SetColor("_ColorBravo", FillColor);
            MappingMaterial.SetColor("_ColorDelta", OutlineColor);
            foreach (var recolor in recolors)
            {
                recolor.color = OutlineColor;
            }
        }

        public void Choke(float beat, float length, bool seeChoke, bool sawChoke)
        {
            if (seeChoke) see.Choke(beat, length);
            if (sawChoke) saw.Choke(beat, length);
        }

        public void Bop(float beat, float length, bool bopSee, bool bopSaw, bool autoSee, bool autoSaw, bool strumSee, bool strumSaw)
        {
            seeShouldBop = autoSee;
            sawShouldBop = autoSaw;
            see.strum = strumSee;
            saw.strum = strumSaw;
            if (bopSee || bopSaw)
            {
                List<BeatAction.Action> bops = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    bops.Add(new BeatAction.Action(beat + i, delegate
                    {
                        if (bopSaw)
                        {
                            saw.Bop();
                        }
                        if (bopSee)
                        {
                            see.Bop();
                        }
                    }));
                }
                BeatAction.New(instance.gameObject, bops);
            }
        }

        public void ChangeColor(float beat, float length, Color color1, Color color2, Color color3, Color color4, int ease)
        {
            bgColorStartBeat = beat;
            bgColorLength = length;
            colorFrom = color1;
            colorTo = color2;
            colorFrom2 = color3;
            colorTo2 = color4;
            lastEase = (EasingFunction.Ease)ease;
        }

        public void LongLong(float beat, bool high, float height)
        {
            if (see.dead || saw.dead) return;
            if (currentJumpIndex != 0)
            {
                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat)
                {
                    return;
                }
            }
            saw.canBop = false;
            canStartJump = false;
            if (canPrepare) see.SetState(SeeSawGuy.JumpState.StartJump, beat - 2);
            canPrepare = false;
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
                see.Land(SeeSawGuy.LandType.Normal, false);
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
                    saw.canBop = true;
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + 4);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 3.75f, delegate { see.canBop = true; }),
                        new BeatAction.Action(beat + 4, delegate { see.Land(SeeSawGuy.LandType.Normal, true); canPrepare = true; canStartJump = true;})
                    });
                }
            } 
        }

        public void LongShort(float beat, bool high, float height)
        {
            if (see.dead || saw.dead) return;
            if (currentJumpIndex != 0)
            {
                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat)
                {
                    return;
                }
            }
            saw.canBop = false;
            canStartJump = false;
            if (canPrepare) see.SetState(SeeSawGuy.JumpState.StartJump, beat - 2);
            canPrepare = false;
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
                see.Land(SeeSawGuy.LandType.Normal, false);
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
                    saw.canBop = true;
                    float beatLength = see.ShouldEndJumpOut() ? 4 : 3;
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + beatLength);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + beatLength - 0.25f, delegate { see.canBop = true; }),
                        new BeatAction.Action(beat + beatLength, delegate { see.Land(SeeSawGuy.LandType.Normal, true); canPrepare = true; canStartJump = true;})
                    });
                }
            }
        }

        public void ShortLong(float beat, bool high, float height)
        {
            if (see.dead || saw.dead) return;
            if (currentJumpIndex != 0)
            {
                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat)
                {
                    return;
                }
            }
            saw.canBop = false;
            canStartJump = false;
            if (canPrepare) see.SetState(SeeSawGuy.JumpState.StartJumpIn, beat - 1);
            canPrepare = false;
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
                    saw.canBop = true;
                    float beatLength = see.ShouldEndJumpOut() ? 3 : 2;
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + beatLength);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + beatLength - 0.25f, delegate { see.canBop = true; }),
                        new BeatAction.Action(beat + beatLength, delegate { see.Land(SeeSawGuy.LandType.Normal, false); canPrepare = true; canStartJump = true; })
                    });
                }
            }
        }

        public void ShortShort(float beat, bool high, float height)
        {
            if (see.dead || saw.dead) return;
            if (currentJumpIndex != 0)
            {
                if (currentJumpIndex >= allJumpEvents.Count || allJumpEvents[currentJumpIndex].beat != beat) 
                {
                    return;
                } 
            }
            saw.canBop = false;
            canStartJump = false;
            if (canPrepare) see.SetState(SeeSawGuy.JumpState.StartJumpIn, beat - 1);
            canPrepare = false;
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
                    saw.canBop = true;
                    Jukebox.PlayOneShotGame("seeSaw/otherLand", beat + 2);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 1.75f, delegate { see.canBop = true; }),
                        new BeatAction.Action(beat + 2, delegate { see.Land(SeeSawGuy.LandType.Normal, false); canPrepare = true; canStartJump = true;})
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

        void DetermineSeeJump(float beat, bool miss = false, bool high = false, float height = 0)
        {
            if (currentJumpIndex >= 0
                && (allJumpEvents[currentJumpIndex - 1].datamodel == "seeSaw/longLong" || allJumpEvents[currentJumpIndex - 1].datamodel == "seeSaw/shortLong"))
            {
                if (NextJumpEventIsOnBeat())
                {
                    if (allJumpEvents[currentJumpIndex].datamodel is "seeSaw/longLong" or "seeSaw/shortLong")
                    {
                        see.SetState(high ? SeeSawGuy.JumpState.HighOutOut : SeeSawGuy.JumpState.OutOut, beat, miss, height);
                    }
                    else if (allJumpEvents[currentJumpIndex].datamodel is "seeSaw/longShort" or "seeSaw/shortShort")
                    {
                        see.SetState(high ? SeeSawGuy.JumpState.HighOutIn : SeeSawGuy.JumpState.OutIn, beat, miss, height);
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
                        see.SetState(high ? SeeSawGuy.JumpState.HighInOut : SeeSawGuy.JumpState.InOut, beat, miss, height);
                    }
                    else if (allJumpEvents[currentJumpIndex].datamodel is "seeSaw/longShort" or "seeSaw/shortShort")
                    {
                        see.SetState(high ? SeeSawGuy.JumpState.HighInIn : SeeSawGuy.JumpState.InIn, beat, miss, height);
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

        public void SpawnOrbs(bool white, float beat)
        {
            ParticleSystem ps = null;
            if (white)
            {
                ps = Instantiate(leftWhiteOrbs, leftWhiteOrbs.transform.parent);
            }
            else
            {
                ps = Instantiate(rightBlackOrbs, rightBlackOrbs.transform.parent);
            }
            ParticleSystem psChild = ps.transform.GetChild(1).GetComponent<ParticleSystem>();
            psChild.SetAsyncScaling(0.65f);
            ps.transform.GetChild(2).GetComponent<ParticleSystem>().SetAsyncScaling(0.5f);
            psChild.transform.GetChild(1).GetComponent<ParticleSystem>().SetAsyncScaling(0.5f);
            ps.PlayScaledAsync(0.65f);
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
            saw.Land(SeeSawGuy.LandType.Normal, false);
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
            Jukebox.PlayOneShotGame("seeSaw/playerVoiceLong2", caller.timer + caller.startBeat + 1f, 1, 1, false, false, 0.0104166666f);
        }

        public void JustLongHigh(PlayerActionEvent caller, float state)
        {
            seeSawAnim.transform.localScale = new Vector3(1, 1, 1);
            if (state <= -1f || state >= 1f)
            {
                seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
                Jukebox.PlayOneShotGame("seeSaw/ow");
                saw.Land(SeeSawGuy.LandType.Barely, true);
                DetermineSeeJump(caller.timer + caller.startBeat, true, true, allJumpEvents[currentJumpIndex - 1]["height"]);
                return;
            }
            DetermineSeeJump(caller.timer + caller.startBeat, false, true, allJumpEvents[currentJumpIndex - 1]["height"]);
            seeSawAnim.DoScaledAnimationAsync("Lightning", 0.5f);
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
            Jukebox.PlayOneShotGame("seeSaw/playerVoiceLong2", caller.timer + caller.startBeat + 1f, 1, 1, false, false, 0.0104166666f);
        }

        public void MissLong(PlayerActionEvent caller)
        {
            seeSawAnim.transform.localScale = new Vector3(1, 1, 1);
            seeSawAnim.DoScaledAnimationAsync("Bad", 0.5f);
            Jukebox.PlayOneShotGame("seeSaw/miss");
            saw.Land(SeeSawGuy.LandType.Miss, true);
            DetermineSeeJump(caller.timer + caller.startBeat, true, allJumpEvents[currentJumpIndex - 1]["high"], allJumpEvents[currentJumpIndex - 1]["height"]);
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
                DetermineSeeJump(caller.timer + caller.startBeat, true, true, allJumpEvents[currentJumpIndex - 1]["height"]);
                return;
            }
            DetermineSeeJump(caller.timer + caller.startBeat, false, true, allJumpEvents[currentJumpIndex - 1]["height"]);
            seeSawAnim.DoScaledAnimationAsync("Lightning", 0.5f);
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
            DetermineSeeJump(caller.timer + caller.startBeat, true, allJumpEvents[currentJumpIndex - 1]["high"], allJumpEvents[currentJumpIndex - 1]["height"]);
        }

        public void Empty(PlayerActionEvent caller) { }
    }

}
