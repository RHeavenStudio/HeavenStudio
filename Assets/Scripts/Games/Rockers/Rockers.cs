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
                    resizable = true
                },
                new GameAction("riff", "Riff")
                {
                    function = delegate { var e = eventCaller.currentEntity; Rockers.instance.Riff(e.beat, new int[6]
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
                new GameAction("mute", "Mute")
                {
                    function = delegate { Rockers.instance.Mute(); },
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
        public static Rockers instance;

        public static CallAndResponseHandler crHandlerInstance;

        [Header("Rockers")]
        [SerializeField] private RockersRocker JJ;
        public RockersRocker Soshi;

        [Header("Input")]
        [SerializeField] RockersInput rockerInputRef;

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
            }
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                crHandlerInstance = null;
            }
        }

        public void StartInterval(float beat, float length)
        {
            crHandlerInstance.StartInterval(beat, length);
        }

        public void Riff(float beat, int[] pitches, bool gleeClubJJ, int[] pitchesPlayer, bool gleeClubPlayer)
        {
            JJ.StrumStrings(gleeClubJJ, pitches);
            crHandlerInstance.AddEvent(beat, "riff", new List<CallAndResponseHandler.CallAndResponseEventParam>()
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

        public void Mute()
        {
            JJ.Mute();
        }

        public void PassTurn(float beat, float length)
        {
            if (crHandlerInstance.queuedEvents.Count > 0)
            {
                List<CallAndResponseHandler.CallAndResponseEvent> crEvents = crHandlerInstance.PassTurn();

                foreach (var crEvent in crEvents)
                {
                    if (crEvent.tag == "riff")
                    {
                        RockersInput riffComp = Instantiate(rockerInputRef, transform);
                        riffComp.Init(crEvent["gleeClub"], new int[6] { crEvent["1"], crEvent["2"], crEvent["3"], crEvent["4"], crEvent["5"], crEvent["6"] }, beat, beat + length + crEvent.relativeBeat);
                    }
                }
            }
        }
    }
}

