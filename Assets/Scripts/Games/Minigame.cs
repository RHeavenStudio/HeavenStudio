using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.Common;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games
{
    public class Minigame : MonoBehaviour
    {
        public static double ngEarlyTime = 0.075f, justEarlyTime = 0.06f, aceEarlyTime = 0.01f, aceLateTime = 0.01f, justLateTime = 0.06f, ngLateTime = 0.075f;
        public static float rankHiThreshold = 0.8f, rankOkThreshold = 0.6f;
        [SerializeField] public SoundSequence.SequenceKeyValue[] SoundSequences;

        public List<Minigame.Eligible> EligibleHits = new List<Minigame.Eligible>();

        [System.Serializable]
        public class Eligible
        {
            public GameObject gameObject;
            public bool early;
            public bool perfect;
            public bool late;
            public bool notPerfect() { return early || late; }
            public bool eligible() { return early || perfect || late; }
            public float createBeat;
        }

        #region Premade Input Actions
        const int IAEmptyCat = -1;
        const int IAPressCat = 0;
        const int IAReleaseCat = 1;
        const int IAFlickCat = 2;

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
            return PlayerInput.GetTouchUp(InputController.ActionsTouch.Tap, out dt);
        }
        protected static bool IA_BatonBasicRelease(out double dt)
        {
            return PlayerInput.GetBatonUp(InputController.ActionsBaton.Face, out dt);
        }

        protected static bool IA_TouchFlick(out double dt)
        {
            return PlayerInput.GetFlick(out dt);
        }

        public static PlayerInput.InputAction InputAction_BasicPress =
            new("BasicPress", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadBasicPress, IA_TouchBasicPress, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_BasicRelease =
            new("BasicPress", new int[] { IAReleaseCat, IAReleaseCat, IAReleaseCat },
            IA_PadBasicRelease, IA_TouchBasicRelease, IA_BatonBasicRelease);

        public static PlayerInput.InputAction InputAction_FlickPress =
            new("BasicPress", new int[] { IAPressCat, IAFlickCat, IAPressCat },
            IA_PadBasicPress, IA_TouchFlick, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_FlickRelease =
            new("BasicPress", new int[] { IAReleaseCat, IAFlickCat, IAReleaseCat },
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

            GameObject evtObj = new GameObject("ActionEvent" + (startBeat + timer));
            evtObj.AddComponent<PlayerActionEvent>();

            PlayerActionEvent evt = evtObj.GetComponent<PlayerActionEvent>();

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
            return evt;
        }

        /// <summary>
        /// Schedule an Input for a later time in the minigame. Executes the methods put in parameters
        /// </summary>
        /// <param name="startBeat">When the scheduling started (in beats)</param>
        /// <param name="timer">How many beats later should the input be expected</param>
        /// <param name="inputType">The type of the input that's expected (Press, Release, A, B, Directions>)</param>
        /// <param name="OnHit">Method to run if the Input has been Hit</param>
        /// <param name="OnMiss">Method to run if the Input has been Missed</param>
        /// <param name="OnBlank">Method to run whenever there's an Input while this is Scheduled (Shouldn't be used too much)</param>
        /// <returns></returns>
        public PlayerActionEvent ScheduleInput(
            double startBeat,
            double timer,
            InputType inputType,
            PlayerActionEvent.ActionEventCallbackState OnHit,
            PlayerActionEvent.ActionEventCallback OnMiss,
            PlayerActionEvent.ActionEventCallback OnBlank,
            PlayerActionEvent.ActionEventHittableQuery HittableQuery = null
            )
        {

            GameObject evtObj = new GameObject("ActionEvent" + (startBeat + timer));
            evtObj.AddComponent<PlayerActionEvent>();

            PlayerActionEvent evt = evtObj.GetComponent<PlayerActionEvent>();

            evt.startBeat = startBeat;
            evt.timer = timer;
            evt.inputType = inputType;
            evt.OnHit = OnHit;
            evt.OnMiss = OnMiss;
            evt.OnBlank = OnBlank;
            evt.IsHittable = HittableQuery;

            evt.OnDestroy = RemoveScheduledInput;

            evt.canHit = true;
            evt.enabled = true;

            evt.transform.parent = this.transform.parent;

            evtObj.SetActive(true);

            scheduledInputs.Add(evt);

            return evt;
        }

        public PlayerActionEvent ScheduleAutoplayInput(double startBeat,
            double timer,
            InputType inputType,
            PlayerActionEvent.ActionEventCallbackState OnHit,
            PlayerActionEvent.ActionEventCallback OnMiss,
            PlayerActionEvent.ActionEventCallback OnBlank)
        {
            PlayerActionEvent evt = ScheduleInput(startBeat, timer, inputType, OnHit, OnMiss, OnBlank);
            evt.autoplayOnly = true;
            return evt;
        }

        public PlayerActionEvent ScheduleUserInput(double startBeat,
            double timer,
            InputType inputType,
            PlayerActionEvent.ActionEventCallbackState OnHit,
            PlayerActionEvent.ActionEventCallback OnMiss,
            PlayerActionEvent.ActionEventCallback OnBlank,
            PlayerActionEvent.ActionEventHittableQuery HittableQuery = null)
        {
            PlayerActionEvent evt = ScheduleInput(startBeat, timer, inputType, OnHit, OnMiss, OnBlank, HittableQuery);
            evt.noAutoplay = true;
            return evt;
        }

        //Clean up method used whenever a PlayerActionEvent has finished
        public void RemoveScheduledInput(PlayerActionEvent evt)
        {
            scheduledInputs.Remove(evt);
        }

        //Get the scheduled input that should happen the **Soonest**
        //Can return null if there's no scheduled inputs
        // remark: need a check for specific button(s)
        public PlayerActionEvent GetClosestScheduledInput(InputType input = InputType.ANY)
        {
            PlayerActionEvent closest = null;

            foreach (PlayerActionEvent toCompare in scheduledInputs)
            {
                // ignore inputs that are for sequencing in autoplay
                if (toCompare.autoplayOnly) continue;

                if (closest == null)
                {
                    if (input == InputType.ANY || (toCompare.inputType & input) != 0)
                        closest = toCompare;
                }
                else
                {
                    double t1 = closest.startBeat + closest.timer;
                    double t2 = toCompare.startBeat + toCompare.timer;

                    // Debug.Log("t1=" + t1 + " -- t2=" + t2);

                    if (t2 < t1)
                    {
                        if (input == InputType.ANY || (toCompare.inputType & input) != 0)
                            closest = toCompare;
                    }
                }
            }

            return closest;
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

        //Hasn't been tested yet. *Should* work.
        //Can be used to detect if the user is expected to input something now or not
        //Useful for strict call and responses games like Tambourine
        public bool IsExpectingInputNow(InputType wantInput = InputType.ANY)
        {
            PlayerActionEvent input = GetClosestScheduledInput(wantInput);
            if (input == null) return false;
            return input.IsExpectingInputNow();
        }

        public bool IsExpectingInputNow(int[] wantActionCategory)
        {
            PlayerActionEvent input = GetClosestScheduledInput(wantActionCategory);
            if (input == null) return false;
            return input.IsExpectingInputNow();
        }

        // now should fix the fast bpm problem
        public static double NgEarlyTime()
        {
            return 1f - ngEarlyTime;
        }

        public static double JustEarlyTime()
        {
            return 1f - justEarlyTime;
        }

        public static double JustLateTime()
        {
            return 1f + justLateTime;
        }

        public static double NgLateTime()
        {
            return 1f + ngLateTime;
        }

        public static double AceEarlyTime()
        {
            return 1f - aceEarlyTime;
        }

        public static double AceLateTime()
        {
            return 1f + aceLateTime;
        }

        public virtual void OnGameSwitch(double beat)
        {
            //Below is a template that can be used for handling previous entities.
            //section below is if you only want to look at entities that overlap the game switch
            /*
            List<RiqEntity> prevEntities = GameManager.instance.Beatmap.Entities.FindAll(c => c.beat <= beat && c.datamodel.Split(0) == [insert game name]);
            foreach(RiqEntity entity in prevEntities)
            {
                if(entity.beat + entity.length >= beat)
                {
                    EventCaller.instance.CallEvent(entity, true);
                }
            }
            */
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

        public int MultipleEventsAtOnce()
        {
            int sameTime = 0;
            for (int i = 0; i < EligibleHits.Count; i++)
            {
                float createBeat = EligibleHits[i].createBeat;
                if (EligibleHits.FindAll(c => c.createBeat == createBeat).Count > 0)
                {
                    sameTime += 1;
                }
            }

            if (sameTime == 0 && EligibleHits.Count > 0)
                sameTime = 1;

            return sameTime;
        }

        public static MultiSound PlaySoundSequence(string game, string name, double startBeat, params SoundSequence.SequenceParams[] args)
        {
            Minigames.Minigame gameInfo = GameManager.instance.GetGameInfo(game);
            foreach (SoundSequence.SequenceKeyValue pair in gameInfo.LoadedSoundSequences)
            {
                if (pair.name == name)
                {
                    Debug.Log($"Playing sound sequence {pair.name} at beat {startBeat}");
                    return pair.sequence.Play(startBeat);
                }
            }
            Debug.LogWarning($"Sound sequence {name} not found in game {game} (did you build AssetBundles?)");
            return null;
        }

        public void ScoreMiss(double weight = 1f)
        {
            GameManager.instance.ScoreInputAccuracy(0, true, NgLateTime(), weight, false);
            if (weight > 0)
            {
                GoForAPerfect.instance.Miss();
                SectionMedalsManager.instance.MakeIneligible();
            }
        }

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
