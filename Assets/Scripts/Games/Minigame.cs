using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.Common;

namespace HeavenStudio.Games
{
    public class Minigame : MonoBehaviour
    {
        public static double earlyTime = 0.075f, perfectTime = 0.06f, aceEarlyTime = 0.01f, aceLateTime = 0.01f, lateTime = 0.06f, endTime = 0.075f;
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

        public List<PlayerActionEvent> scheduledInputs = new List<PlayerActionEvent>();

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
            float startBeat,
            float timer,
            InputType inputType,
            PlayerActionEvent.ActionEventCallbackState OnHit,
            PlayerActionEvent.ActionEventCallback OnMiss,
            PlayerActionEvent.ActionEventCallback OnBlank)
        {

            GameObject evtObj = new GameObject("ActionEvent" + (startBeat+timer));
            evtObj.AddComponent<PlayerActionEvent>();

            PlayerActionEvent evt = evtObj.GetComponent<PlayerActionEvent>();

            evt.startBeat = startBeat;
            evt.timer = timer;
            evt.inputType = inputType;
            evt.OnHit = OnHit;
            evt.OnMiss = OnMiss;
            evt.OnBlank = OnBlank;

            evt.OnDestroy = RemoveScheduledInput;

            evt.canHit = true;
            evt.enabled = true;

            evt.transform.parent = this.transform.parent;

            evtObj.SetActive(true);

            scheduledInputs.Add(evt);

            return evt;
        }

        public PlayerActionEvent ScheduleAutoplayInput(float startBeat,
            float timer,
            InputType inputType,
            PlayerActionEvent.ActionEventCallbackState OnHit,
            PlayerActionEvent.ActionEventCallback OnMiss,
            PlayerActionEvent.ActionEventCallback OnBlank)
        {
            PlayerActionEvent evt = ScheduleInput(startBeat, timer, inputType, OnHit, OnMiss, OnBlank);
            evt.autoplayOnly = true;
            return evt;
        }

        public PlayerActionEvent ScheduleUserInput(float startBeat,
            float timer,
            InputType inputType,
            PlayerActionEvent.ActionEventCallbackState OnHit,
            PlayerActionEvent.ActionEventCallback OnMiss,
            PlayerActionEvent.ActionEventCallback OnBlank)
        {
            PlayerActionEvent evt = ScheduleInput(startBeat, timer, inputType, OnHit, OnMiss, OnBlank);
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

            foreach(PlayerActionEvent toCompare in scheduledInputs)
            {
                // ignore inputs that are for sequencing in autoplay
                if (toCompare.autoplayOnly) continue;

                if(closest == null)
                {
                    if (input == InputType.ANY || toCompare.inputType.HasFlag(input))
                        closest = toCompare;
                } else
                {
                    float t1 = closest.startBeat + closest.timer;
                    float t2 = toCompare.startBeat + toCompare.timer;

                    // Debug.Log("t1=" + t1 + " -- t2=" + t2);

                    if (t2 < t1)
                    {
                        if (input == InputType.ANY || toCompare.inputType.HasFlag(input))
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

        // now should fix the fast bpm problem
        public static double EarlyTime()
        {
            return 1f - earlyTime;
        }

        public static double PerfectTime()
        {
            return 1f - perfectTime;
        }

        public static double LateTime()
        {
            return 1f + lateTime;
        }

        public static double EndTime()
        {
            return 1f + endTime;
        }

        public static double AceStartTime()
        {
            return 1f - aceEarlyTime;
        }

        public static double AceEndTime()
        {
            return 1f + aceLateTime;
        }

        // DEPRECATED: scales timing windows to the BPM in an ""intelligent"" manner
        // only left for historical reasons
        static float ScaleTimingMargin(float f)
        {
            float bpm = Conductor.instance.songBpm * Conductor.instance.musicSource.pitch;
            float a = bpm / 120f;
            float b = (Mathf.Log(a) + 2f) * 0.5f;
            float r = Mathf.Lerp(a, b, 0.25f);
            return r * f;
        }

        public int firstEnable = 0;

        public virtual void OnGameSwitch(float beat)
        {
            //Below is a template that can be used for handling previous entities.
            //section below is if you only want to look at entities that overlap the game switch
            /*
            List<Beatmap.Entity> prevEntities = GameManager.instance.Beatmap.entities.FindAll(c => c.beat <= beat && c.datamodel.Split(0) == [insert game name]);
            foreach(Beatmap.Entity entity in prevEntities)
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

        public virtual void OnPlay(float beat)
        {

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

        public static MultiSound PlaySoundSequence(string game, string name, float startBeat, params SoundSequence.SequenceParams[] args)
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
            GameManager.instance.ScoreInputAccuracy(0, true, EndTime(), weight, false);
            if (weight > 0)
            {
                GoForAPerfect.instance.Miss();
                SectionMedalsManager.instance.MakeIneligible();
            }
        }

        private void OnDestroy() {
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        protected void OnDrawGizmos() {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(17.77695f, 10, 0));
        }
    }
}
