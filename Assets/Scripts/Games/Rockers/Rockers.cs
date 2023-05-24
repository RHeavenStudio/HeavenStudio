using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class NtrRockersLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("rockers", "Rockers", "EB4C94", false, false, new List<GameAction>()
            {
                new GameAction("intervalStart", "Start Interval")
                {
                    function = delegate { var e = eventCaller.currentEntity; Rockers.instance.StartInterval(e.beat, e.length); },
                    defaultLength = 8f,
                    resizable = true,
                    preFunction = delegate { Rockers.PreMoveCamera(eventCaller.currentEntity.beat); }
                },
                new GameAction("riff", "Riff")
                {
                    function = delegate { var e = eventCaller.currentEntity; Rockers.instance.Riff(e.beat, e.length, new int[6]
                    {
                        e["1JJ"],
                        e["2JJ"],
                        e["3JJ"],
                        e["4JJ"],
                        e["5JJ"],
                        e["6JJ"],
                    }, e["gcJJ"], new int[6]
                    {
                        e["1S"],
                        e["2S"],
                        e["3S"],
                        e["4S"],
                        e["5S"],
                        e["6S"],
                    }, e["gcS"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("1JJ", new EntityTypes.Integer(-1, 24, 0), "E2 String (JJ)", "How many semitones up is this string pitched? If left at -1, this string will not play."),
                        new Param("2JJ", new EntityTypes.Integer(-1, 24, 0), "A2 String (JJ)", "How many semitones up is this string pitched? If left at -1, this string will not play."),
                        new Param("3JJ", new EntityTypes.Integer(-1, 24, 0), "D3 String (JJ)", "How many semitones up is this string pitched? If left at -1, this string will not play."),
                        new Param("4JJ", new EntityTypes.Integer(-1, 24, 0), "G3 String (JJ)", "How many semitones up is this string pitched? If left at -1, this string will not play."),
                        new Param("5JJ", new EntityTypes.Integer(-1, 24, 0), "B3 String (JJ)", "How many semitones up is this string pitched? If left at -1, this string will not play."),
                        new Param("6JJ", new EntityTypes.Integer(-1, 24, 0), "E4 String (JJ)", "How many semitones up is this string pitched? If left at -1, this string will not play."),
                        new Param("gcJJ", false, "Glee Club Guitar (JJ)", "Will JJ use the same guitar as in the glee club lessons?"),
                        new Param("1S", new EntityTypes.Integer(-1, 24, 0), "E2 String (Soshi)", "How many semitones up is this string pitched? If left at -1, this string will not play."),
                        new Param("2S", new EntityTypes.Integer(-1, 24, 0), "A2 String (Soshi)", "How many semitones up is this string pitched? If left at -1, this string will not play."),
                        new Param("3S", new EntityTypes.Integer(-1, 24, 0), "D3 String (Soshi)", "How many semitones up is this string pitched? If left at -1, this string will not play."),
                        new Param("4S", new EntityTypes.Integer(-1, 24, 0), "G3 String (Soshi)", "How many semitones up is this string pitched? If left at -1, this string will not play."),
                        new Param("5S", new EntityTypes.Integer(-1, 24, 0), "B3 String (Soshi)", "How many semitones up is this string pitched? If left at -1, this string will not play."),
                        new Param("6S", new EntityTypes.Integer(-1, 24, 0), "E4 String (Soshi)", "How many semitones up is this string pitched? If left at -1, this string will not play."),
                        new Param("gcS", false, "Glee Club Guitar (Soshi)", "Will Soshi use the same guitar as in the glee club lessons?")
                    }
                },
                new GameAction("prepare", "Prepare")
                {
                    function = delegate { Rockers.instance.Mute(eventCaller.currentEntity["who"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("who", Rockers.WhoMutes.JJ, "Who?", "Who will prepare? (Soshi is only affected by this event in auto-play.)")
                    }
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    function = delegate { var e = eventCaller.currentEntity; Rockers.instance.PassTurn(e.beat, e.length); },
                    resizable = true
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_Rockers;
    public class Rockers : Minigame
    {
        public enum WhoMutes
        {
            JJ,
            Soshi,
            Both
        }
        public static Rockers instance;

        public static CallAndResponseHandler crHandlerInstance;

        [Header("Rockers")]
        [SerializeField] private RockersRocker JJ;
        public RockersRocker Soshi;

        [Header("Input")]
        [SerializeField] RockersInput rockerInputRef;

        private float lastTargetCameraX = 0;
        private float targetCameraX = 0;
        private float cameraMoveBeat = -1;
        private static List<float> queuedCameraEvents = new List<float>();

        private void Awake()
        {
            instance = this;
            if (crHandlerInstance == null)
            {
                crHandlerInstance = new CallAndResponseHandler(8);
            }
        }

        private void Start()
        {
            if (PlayerInput.Pressing())
            {
                Soshi.Mute();
            }
        }

        private void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                crHandlerInstance = null;
            }
            if (queuedCameraEvents.Count > 0) queuedCameraEvents.Clear();
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused) 
            { 
                if (PlayerInput.Pressed())
                {
                    Soshi.Mute();
                }
                if (PlayerInput.PressedUp() && !IsExpectingInputNow(InputType.STANDARD_UP))
                {
                    Soshi.UnHold();
                }

                if (queuedCameraEvents.Count > 0)
                {
                    foreach (var cameraEvent in queuedCameraEvents)
                    {
                        MoveCamera(cameraEvent);
                    }
                    queuedCameraEvents.Clear();
                }

                float normalizedBeat = cond.GetPositionFromBeat(cameraMoveBeat, 1f);

                if (normalizedBeat >= 0f && normalizedBeat <= 1f)
                {
                    EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInOutQuint);

                    float newX = func(lastTargetCameraX, targetCameraX, normalizedBeat);
                    GameCamera.additionalPosition = new Vector3(newX, 0, 0);
                }
            }
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                crHandlerInstance = null;
            }
        }

        public static void PreMoveCamera(float beat)
        {
            if (GameManager.instance.currentGame == "rockers")
            {
                instance.MoveCamera(beat - 1);
            }
            queuedCameraEvents.Add(beat - 1);
        }

        private void MoveCamera(float beat)
        {
            lastTargetCameraX = GameCamera.additionalPosition.x;
            targetCameraX = JJ.transform.localPosition.x;
            cameraMoveBeat = beat;
        }

        public void StartInterval(float beat, float length)
        {
            crHandlerInstance.StartInterval(beat, length);
            if (GameManager.instance.autoplay) Soshi.UnHold();
        }

        public void Riff(float beat, float length, int[] pitches, bool gleeClubJJ, int[] pitchesPlayer, bool gleeClubPlayer)
        {
            JJ.StrumStrings(gleeClubJJ, pitches);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate { JJ.Mute(); })
            });
            crHandlerInstance.AddEvent(beat, length, "riff", new List<CallAndResponseHandler.CallAndResponseEventParam>()
            {
                new CallAndResponseHandler.CallAndResponseEventParam("gleeClub", gleeClubPlayer),
                new CallAndResponseHandler.CallAndResponseEventParam("1", pitchesPlayer[0]),
                new CallAndResponseHandler.CallAndResponseEventParam("2", pitchesPlayer[1]),
                new CallAndResponseHandler.CallAndResponseEventParam("3", pitchesPlayer[2]),
                new CallAndResponseHandler.CallAndResponseEventParam("4", pitchesPlayer[3]),
                new CallAndResponseHandler.CallAndResponseEventParam("5", pitchesPlayer[4]),
                new CallAndResponseHandler.CallAndResponseEventParam("6", pitchesPlayer[5]),
            });
        }

        public void Mute(int whoMutes)
        {
            if (whoMutes is (int)WhoMutes.JJ or (int)WhoMutes.Both)
            {
                JJ.Mute();
            }
            if (whoMutes is (int)WhoMutes.Soshi or (int)WhoMutes.Both)
            {
                if (GameManager.instance.autoplay) Soshi.Mute();
            }
        }

        public void PassTurn(float beat, float length)
        {
            if (crHandlerInstance.queuedEvents.Count > 0)
            {
                List<CallAndResponseHandler.CallAndResponseEvent> crEvents = crHandlerInstance.queuedEvents;

                foreach (var crEvent in crEvents)
                {
                    if (crEvent.tag == "riff")
                    {
                        RockersInput riffComp = Instantiate(rockerInputRef, transform);
                        riffComp.Init(crEvent["gleeClub"], new int[6] { crEvent["1"], crEvent["2"], crEvent["3"], crEvent["4"], crEvent["5"], crEvent["6"] }, beat, length + crEvent.relativeBeat);
                        ScheduleInput(beat, length + crEvent.relativeBeat + crEvent.length, InputType.STANDARD_DOWN, JustMute, MuteMiss, Empty);
                    }
                }
                crHandlerInstance.queuedEvents.Clear();
                JJ.UnHold();

                lastTargetCameraX = GameCamera.additionalPosition.x;
                targetCameraX = Soshi.transform.localPosition.x;
                cameraMoveBeat = beat;
            }
        }

        private void JustMute(PlayerActionEvent caller, float state)
        {
            Soshi.Mute();
        } 

        private void MuteMiss(PlayerActionEvent caller)
        {

        }

        private void Empty(PlayerActionEvent caller)
        {

        }
    }
}

