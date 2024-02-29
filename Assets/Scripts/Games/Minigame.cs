using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.Common;
using HeavenStudio.InputSystem;
using System;
using System.Linq;
using Jukebox;

namespace HeavenStudio.Games
{
    public class Minigame : MonoBehaviour
    {
        public static double ngEarlyTimeBase = 0.1, justEarlyTimeBase = 0.05, aceEarlyTimeBase = 0.01, aceLateTimeBase = 0.01, justLateTimeBase = 0.05, ngLateTimeBase = 0.1;
        public static double rankHiThreshold = 0.8, rankOkThreshold = 0.6;

        public static double ngEarlyTime => ngEarlyTimeBase * Conductor.instance?.SongPitch ?? 1;
        public static double justEarlyTime => justEarlyTimeBase * Conductor.instance?.SongPitch ?? 1;
        public static double aceEarlyTime => aceEarlyTimeBase * Conductor.instance?.SongPitch ?? 1;
        public static double aceLateTime => aceLateTimeBase * Conductor.instance?.SongPitch ?? 1;
        public static double justLateTime => justLateTimeBase * Conductor.instance?.SongPitch ?? 1;
        public static double ngLateTime => ngLateTimeBase * Conductor.instance?.SongPitch ?? 1;

        [SerializeField] public SoundSequence.SequenceKeyValue[] SoundSequences;

        [NonSerialized] public string minigameName;

        [NonSerialized] public GameManager gameManager;
        [NonSerialized] public Conductor conductor;

        #region Premade Input Actions
        protected const int IAEmptyCat = -1;
        protected const int IAPressCat = 0;
        protected const int IAReleaseCat = 1;
        protected const int IAPressingCat = 2;
        protected const int IAFlickCat = 3;
        protected const int IAMAXCAT = 4;

        protected static bool IA_Empty(out double dt)
        {
            dt = 0;
            return false;
        }

        protected static bool IA_PadBasicPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt);
        }
        protected static bool IA_TouchBasicPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt);
        }
        protected static bool IA_BatonBasicPress(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.Face, out dt);
        }

        protected static bool IA_PadBasicRelease(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.East, out dt);
        }
        protected static bool IA_TouchBasicRelease(out double dt)
        {
            return PlayerInput.GetTouchUp(InputController.ActionsTouch.Tap, out dt) && !PlayerInput.GetFlick(out _);
        }
        protected static bool IA_BatonBasicRelease(out double dt)
        {
            return PlayerInput.GetBatonUp(InputController.ActionsBaton.Face, out dt);
        }

        protected static bool IA_PadBasicPressing(out double dt)
        {
            dt = 0;
            return PlayerInput.GetPad(InputController.ActionsPad.East);
        }
        protected static bool IA_TouchBasicPressing(out double dt)
        {
            dt = 0;
            return PlayerInput.GetTouch(InputController.ActionsTouch.Tap);
        }
        protected static bool IA_BatonBasicPressing(out double dt)
        {
            dt = 0;
            return PlayerInput.GetBaton(InputController.ActionsBaton.Face);
        }

        protected static bool IA_TouchFlick(out double dt)
        {
            return PlayerInput.GetFlick(out dt);
        }

        public static PlayerInput.InputAction InputAction_BasicPress =
            new("BasicPress", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadBasicPress, IA_TouchBasicPress, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_BasicRelease =
            new("BasicRelease", new int[] { IAReleaseCat, IAReleaseCat, IAReleaseCat },
            IA_PadBasicRelease, IA_TouchBasicRelease, IA_BatonBasicRelease);
        public static PlayerInput.InputAction InputAction_BasicPressing =
            new("BasicRelease", new int[] { IAReleaseCat, IAReleaseCat, IAReleaseCat },
            IA_PadBasicPressing, IA_TouchBasicPressing, IA_BatonBasicPressing);

        public static PlayerInput.InputAction InputAction_FlickPress =
            new("FlickPress", new int[] { IAPressCat, IAFlickCat, IAPressCat },
            IA_PadBasicPress, IA_TouchFlick, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_FlickRelease =
            new("FlickRelease", new int[] { IAReleaseCat, IAFlickCat, IAReleaseCat },
            IA_PadBasicRelease, IA_TouchFlick, IA_BatonBasicRelease);
        #endregion

        public List<PlayerActionEvent> scheduledInputs = new List<PlayerActionEvent>();

        /// <summary>
        /// Schedule an Input for a later time in the minigame. Executes the methods put in parameters
        /// </summary>
        /// <param name="startBeat">When the scheduling started (in beats)</param>
        /// <param name="timer">How many beats later should the input be expected</param>
        /// <param name="inputAction">The input action that's expected</param>
        /// <param name="OnHit">Method to run if the Input has been Hit</param>
        /// <param name="OnMiss">Method to run if the Input has been Missed</param>
        /// <param name="OnBlank">Method to run whenever there's an Input while this is Scheduled (Shouldn't be used too much)</param>
        /// <returns></returns>
        public PlayerActionEvent ScheduleInput(
            double startBeat,
            double timer,
            PlayerInput.InputAction inputAction,
            PlayerActionEvent.ActionEventCallbackState OnHit,
            PlayerActionEvent.ActionEventCallback OnMiss,
            PlayerActionEvent.ActionEventCallback OnBlank,
            PlayerActionEvent.ActionEventHittableQuery HittableQuery = null
            )
        {
            // List<RiqEntity> gameSwitches = GameManager.instance.Beatmap.Entities.FindAll(c => c.beat <= startBeat + timer && c.datamodel.Split("/")[0] == "switchGame");
            // if (gameSwitches != null && gameSwitches[^1].datamodel.Split("/")[1] != gameObject.name)
            // {
            //     return null;
            // }

            GameObject evtObj = new("ActionEvent" + (startBeat + timer));

            PlayerActionEvent evt = evtObj.AddComponent<PlayerActionEvent>();

            evt.startBeat = startBeat;
            evt.timer = timer;
            evt.InputAction = inputAction;
            evt.OnHit = OnHit;
            evt.OnMiss = OnMiss;
            evt.OnBlank = OnBlank;
            evt.IsHittable = HittableQuery;

            evt.OnDestroy = RemoveScheduledInput;

            evt.canHit = true;
            evt.enabled = true;

            evt.transform.parent = this.transform.parent;

            evt.minigame = minigameName;

            evtObj.SetActive(true);

            scheduledInputs.Add(evt);

            return evt;
        }

        public PlayerActionEvent ScheduleAutoplayInput(double startBeat,
            double timer,
            PlayerInput.InputAction inputAction,
            PlayerActionEvent.ActionEventCallbackState OnHit,
            PlayerActionEvent.ActionEventCallback OnMiss,
            PlayerActionEvent.ActionEventCallback OnBlank)
        {
            PlayerActionEvent evt = ScheduleInput(startBeat, timer, inputAction, OnHit, OnMiss, OnBlank);
            evt.autoplayOnly = true;
            evt.minigame = minigameName;
            return evt;
        }

        public PlayerActionEvent ScheduleUserInput(double startBeat,
            double timer,
            PlayerInput.InputAction inputAction,
            PlayerActionEvent.ActionEventCallbackState OnHit,
            PlayerActionEvent.ActionEventCallback OnMiss,
            PlayerActionEvent.ActionEventCallback OnBlank,
            PlayerActionEvent.ActionEventHittableQuery HittableQuery = null)
        {
            PlayerActionEvent evt = ScheduleInput(startBeat, timer, inputAction, OnHit, OnMiss, OnBlank, HittableQuery);
            evt.noAutoplay = true;
            evt.minigame = minigameName;
            return evt;
        }

        //Clean up method used whenever a PlayerActionEvent has finished
        public void RemoveScheduledInput(PlayerActionEvent evt)
        {
            scheduledInputs.Remove(evt);
        }

        public PlayerActionEvent GetClosestScheduledInput(int[] actionCats)
        {
            int catIdx = (int)PlayerInput.CurrentControlStyle;
            int cat = actionCats[catIdx];
            PlayerActionEvent closest = null;

            foreach (PlayerActionEvent toCompare in scheduledInputs)
            {
                // ignore inputs that are for sequencing in autoplay
                if (toCompare.autoplayOnly) continue;
                if (toCompare.InputAction == null) continue;

                if (closest == null)
                {
                    if (toCompare.InputAction.inputLockCategory[catIdx] == cat)
                        closest = toCompare;
                }
                else
                {
                    double t1 = closest.startBeat + closest.timer;
                    double t2 = toCompare.startBeat + toCompare.timer;

                    if (t2 < t1)
                    {
                        if (toCompare.InputAction.inputLockCategory[catIdx] == cat)
                            closest = toCompare;
                    }
                }
            }

            return closest;
        }

        public bool IsExpectingInputNow(int[] wantActionCategory)
        {
            PlayerActionEvent input = GetClosestScheduledInput(wantActionCategory);
            if (input == null) return false;
            return input.IsExpectingInputNow();
        }

        public bool IsExpectingInputNow(PlayerInput.InputAction wantAction)
        {
            return IsExpectingInputNow(wantAction.inputLockCategory);
        }

        // now should fix the fast bpm problem
        public static double NgEarlyTime(float pitch = -1)
        {
            if (pitch < 0)
                return 1 - ngEarlyTime;
            return 1 - (ngEarlyTimeBase * pitch);
        }

        public static double JustEarlyTime(float pitch = -1)
        {
            if (pitch < 0)
                return 1 - justEarlyTime;
            return 1 - (justEarlyTimeBase * pitch);
        }

        public static double JustLateTime(float pitch = -1)
        {
            if (pitch < 0)
                return 1 + justLateTime;
            return 1 + (justLateTimeBase * pitch);
        }

        public static double NgLateTime(float pitch = -1)
        {
            if (pitch < 0)
                return 1 + ngLateTime;
            return 1 + (ngLateTimeBase * pitch);
        }

        public static double AceEarlyTime(float pitch = -1)
        {
            if (pitch < 0)
                return 1 - aceEarlyTime;
            return 1 - (aceEarlyTimeBase * pitch);
        }

        public static double AceLateTime(float pitch = -1)
        {
            if (pitch < 0)
                return 1 + aceLateTime;
            return 1 + (aceLateTimeBase * pitch);
        }

        public virtual void OnGameSwitch(double beat)
        {
        }

        public virtual void OnTimeChange()
        {

        }

        public virtual void OnPlay(double beat)
        {

        }

        public virtual void OnStop(double beat)
        {
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        // mainly for bopping logic
        public virtual void OnBeatPulse(double beat)
        {

        }

        // added because OnBeatPulse had some animation issues going on
        // if your bopping overlaps with other animations, use this instead
        public virtual void OnLateBeatPulse(double beat)
        {

        }

        public static MultiSound PlaySoundSequence(string game, string name, double startBeat, params SoundSequence.SequenceParams[] args)
        {
            Minigames.Minigame gameInfo = GameManager.instance.GetGameInfo(game);
            foreach (SoundSequence.SequenceKeyValue pair in gameInfo.LoadedSoundSequences)
            {
                if (pair.name == name)
                {
                    // Debug.Log($"Playing sound sequence {pair.name} at beat {startBeat}");
                    return pair.sequence.Play(startBeat);
                }
            }
            Debug.LogWarning($"Sound sequence {name} not found in game {game} (did you build AssetBundles?)");
            return null;
        }

        public void ScoreMiss(float weight = 1f)
        {
            double beat = Conductor.instance?.songPositionInBeatsAsDouble ?? -1;
            GameManager.instance.ScoreInputAccuracy(beat, 0, true, NgLateTime(), weight, false);
        }

        public void ToggleSplitColoursDisplay(bool on)
        {
        }

        #region Bop

        protected enum DefaultBopEnum
        {
            Off = 0,
            On = 1,
        }

        protected Dictionary<double, int> bopRegion = new();

        public bool BeatIsInBopRegion(double beat)
        {
            if (bopRegion.Count == 0) return true;

            int bop = 0;
            foreach (var item in bopRegion)
            {
                if (beat < item.Key) break;
                if (beat >= item.Key) bop = item.Value;
            }
            return (DefaultBopEnum)bop == DefaultBopEnum.On;
        }

        public int BeatIsInBopRegionInt(double beat)
        {
            if (bopRegion.Count == 0) return 0;

            int bop = 0;
            foreach (var item in bopRegion)
            {
                if (beat < item.Key) break;
                if (beat >= item.Key) bop = item.Value;
            }
            return bop;
        }

        protected void SetupBopRegion(string gameName, string eventName, string toggleName, bool isBool = true)
        {
            var allEvents = EventCaller.GetAllInGameManagerList(gameName, new string[] { eventName });
            if (allEvents.Count == 0) return;
            allEvents.Sort((x, y) => x.beat.CompareTo(y.beat));

            foreach (var e in allEvents)
            {
                if (bopRegion.ContainsKey(e.beat))
                {
                    // Debug.Log("Two bops on the same beat, ignoring this one");
                    continue;
                }
                if (isBool)
                {
                    bopRegion.Add(e.beat, e[toggleName] ? 1 : 0);
                }
                else
                {
                    bopRegion.Add(e.beat, e[toggleName]);
                }
            }
        }

        protected void AddBopRegionEvents(string gameName, string eventName, bool allowBop)
        {
            var allEvents = EventCaller.GetAllInGameManagerList(gameName, new string[] { eventName });
            foreach (var e in allEvents)
            {
                bopRegion.Add(e.beat, allowBop ? 1 : 0);
            }
            bopRegion = bopRegion.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        protected void AddBopRegionEventsInt(string gameName, string eventName, int allowBop)
        {
            var allEvents = EventCaller.GetAllInGameManagerList(gameName, new string[] { eventName });
            foreach (var e in allEvents)
            {
                bopRegion.Add(e.beat, allowBop);
            }
            bopRegion = bopRegion.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        #endregion

        private void OnDestroy()
        {
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        protected void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(17.77695f, 10, 0));
        }
    }
}
