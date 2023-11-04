using System;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Common;

namespace HeavenStudio.Games
{

    public class PlayerActionEvent : MonoBehaviour
    {
        static List<PlayerActionEvent> allEvents = new List<PlayerActionEvent>();
        public static bool EnableAutoplayCheat = true;
        public delegate void ActionEventCallback(PlayerActionEvent caller);
        public delegate void ActionEventCallbackState(PlayerActionEvent caller, float state);
        public delegate bool ActionEventHittableQuery();

        public ActionEventCallbackState OnHit; //Function to trigger when an input has been done perfectly
        public ActionEventCallback OnMiss; //Function to trigger when an input has been missed
        public ActionEventCallback OnBlank; //Function to trigger when an input has been recorded while this is pending
        public ActionEventHittableQuery IsHittable; //Checks if an input can be hit. Returning false will skip button checks.

        public ActionEventCallback OnDestroy; //Function to trigger whenever this event gets destroyed. /!\ Shouldn't be used for a minigame! Use OnMiss instead /!\

        public PlayerInput.InputAction InputAction;

        public double startBeat;
        public double timer;

        public bool isEligible = true;
        public bool canHit = true; //Indicates if you can still hit the cue or not. If set to false, it'll guarantee a miss
        public bool enabled = true; //Indicates if the PlayerActionEvent is enabled. If set to false, it'll not trigger any events and destroy itself AFTER it's not relevant anymore
        public bool triggersAutoplay = true;
        bool lockedByEvent = false;
        bool markForDeletion = false;

        public bool autoplayOnly = false; //Indicates if the input event only triggers when it's autoplay. If set to true, NO Miss or Blank events will be triggered when you're not autoplaying.

        public bool noAutoplay = false; //Indicates if this PlayerActionEvent is recognized by the autoplay. /!\ Overrides autoPlayOnly /!\

        public InputType inputType; //The type of input. Check the InputType class to see a list of all of them

        public bool perfectOnly = false; //Indicates that the input only recognize perfect inputs.

        public bool countsForAccuracy = true; //Indicates if the input counts for the accuracy or not. If set to false, it'll not be counted in the accuracy calculation

        public void setHitCallback(ActionEventCallbackState OnHit)
        {
            this.OnHit = OnHit;
        }

        public void setMissCallback(ActionEventCallback OnMiss)
        {
            this.OnMiss = OnMiss;
        }

        public void setHittableQuery(ActionEventHittableQuery IsHittable)
        {
            this.IsHittable = IsHittable;
        }

        public void Enable() { enabled = true; }
        public void Disable() { enabled = false; }
        public void QueueDeletion() { markForDeletion = true; }

        public bool IsCorrectInput(out double dt)
        {
            dt = 0;
            if (InputAction != null)
            {
                return PlayerInput.GetIsAction(InputAction, out dt);
            }
            return (
                //General inputs, both down and up
                (PlayerInput.Pressed(out dt) && inputType.HasFlag(InputType.STANDARD_DOWN)) ||
                (PlayerInput.AltPressed(out dt) && inputType.HasFlag(InputType.STANDARD_ALT_DOWN)) ||
                (PlayerInput.GetAnyDirectionDown(out dt) && inputType.HasFlag(InputType.DIRECTION_DOWN)) ||
                (PlayerInput.PressedUp(out dt) && inputType.HasFlag(InputType.STANDARD_UP)) ||
                (PlayerInput.AltPressedUp(out dt) && inputType.HasFlag(InputType.STANDARD_ALT_UP)) ||
                (PlayerInput.GetAnyDirectionUp(out dt) && inputType.HasFlag(InputType.DIRECTION_UP)) ||
                //Specific directional inputs
                (PlayerInput.GetSpecificDirectionDown(PlayerInput.DOWN, out dt) && inputType.HasFlag(InputType.DIRECTION_DOWN_DOWN)) ||
                (PlayerInput.GetSpecificDirectionDown(PlayerInput.UP, out dt) && inputType.HasFlag(InputType.DIRECTION_UP_DOWN)) ||
                (PlayerInput.GetSpecificDirectionDown(PlayerInput.LEFT, out dt) && inputType.HasFlag(InputType.DIRECTION_LEFT_DOWN)) ||
                (PlayerInput.GetSpecificDirectionDown(PlayerInput.RIGHT, out dt) && inputType.HasFlag(InputType.DIRECTION_RIGHT_DOWN)) ||

                (PlayerInput.GetSpecificDirectionUp(PlayerInput.DOWN, out dt) && inputType.HasFlag(InputType.DIRECTION_DOWN_UP)) ||
                (PlayerInput.GetSpecificDirectionUp(PlayerInput.UP, out dt) && inputType.HasFlag(InputType.DIRECTION_UP_UP)) ||
                (PlayerInput.GetSpecificDirectionUp(PlayerInput.LEFT, out dt) && inputType.HasFlag(InputType.DIRECTION_LEFT_UP)) ||
                (PlayerInput.GetSpecificDirectionUp(PlayerInput.RIGHT, out dt) && inputType.HasFlag(InputType.DIRECTION_RIGHT_UP))
            );
        }

        public void CanHit(bool canHit)
        {
            this.canHit = canHit;
        }

        public void Start()
        {
            allEvents.Add(this);
        }

        public void Update()
        {
            if (markForDeletion) CleanUp();
            if (!Conductor.instance.NotStopped()) CleanUp(); // If the song is stopped entirely in the editor, destroy itself as we don't want duplicates

            if (noAutoplay && autoplayOnly) autoplayOnly = false;
            if (noAutoplay && triggersAutoplay) triggersAutoplay = false;
            if (!enabled) return;

            double normalizedTime = GetNormalizedTime();
            if (GameManager.instance.autoplay)
            {
                AutoplayInput(normalizedTime);
                return;
            }

            //BUGFIX: ActionEvents destroyed too early
            if (normalizedTime > Minigame.NgLateTime()) Miss();

            if (lockedByEvent)
            {
                return;
            }
            if (!CheckEventLock())
            {
                return;
            }

            if (!autoplayOnly && (IsHittable == null || IsHittable != null && IsHittable()) && IsCorrectInput(out double dt))
            {
                normalizedTime -= dt;
                if (IsExpectingInputNow())
                {
                    // if (InputAction != null)
                    // {
                    //     Debug.Log("Hit " + InputAction.name);
                    // }
                    double stateProg = ((normalizedTime - Minigame.JustEarlyTime()) / (Minigame.JustLateTime() - Minigame.JustEarlyTime()) - 0.5f) * 2;
                    Hit(stateProg, normalizedTime);
                }
                else
                {
                    Blank();
                }
            }
        }

        public void LateUpdate()
        {
            if (markForDeletion)
            {
                allEvents.Remove(this);
                OnDestroy(this);
                Destroy(this.gameObject);
            }
            foreach (PlayerActionEvent evt in allEvents)
            {
                evt.lockedByEvent = false;
            }
        }

        private bool CheckEventLock()
        {
            foreach (PlayerActionEvent toCompare in allEvents)
            {
                if (toCompare == this) continue;
                if (toCompare.autoplayOnly) continue;
                if (InputAction != null)
                {
                    if (toCompare.InputAction == null) continue;
                    int catIdx = (int)PlayerInput.CurrentControlStyle;
                    if (toCompare.InputAction != null
                        && toCompare.InputAction.inputLockCategory[catIdx] != InputAction.inputLockCategory[catIdx]) continue;
                }
                else
                {
                    if ((toCompare.inputType & this.inputType) == 0) continue;
                    if (!toCompare.IsExpectingInputNow()) continue;
                }

                double t1 = this.startBeat + this.timer;
                double t2 = toCompare.startBeat + toCompare.timer;
                double songPos = Conductor.instance.songPositionInBeatsAsDouble;

                // compare distance between current time and the events
                // events that happen at the exact same time with the exact same inputs will return true
                if (Math.Abs(t1 - songPos) > Math.Abs(t2 - songPos))
                    return false;
                else if (t1 != t2)  // if they are the same time, we don't want to lock the event
                    toCompare.lockedByEvent = true;
            }
            return true;
        }

        private void AutoplayInput(double normalizedTime, bool autoPlay = false)
        {
            if (triggersAutoplay && (GameManager.instance.autoplay || autoPlay) && GameManager.instance.canInput && normalizedTime >= 1f - (Time.deltaTime * 0.5f))
            {
                AutoplayEvent();
                if (!autoPlay)
                    TimelineAutoplay();
            }
        }

        // TODO: move this to timeline code instead
        private void TimelineAutoplay()
        {
            if (Editor.Editor.instance == null) return;
            if (Editor.Track.Timeline.instance != null && !Editor.Editor.instance.fullscreen)
            {
                Editor.Track.Timeline.instance.AutoplayBTN.GetComponent<Animator>().Play("Ace", 0, 0);
            }
        }

        public bool IsExpectingInputNow()
        {
            if (IsHittable != null)
            {
                if (!IsHittable()) return false;
            }
            if (!enabled) return false;
            if (!isEligible) return false;

            double normalizedBeat = GetNormalizedTime();
            return normalizedBeat > Minigame.NgEarlyTime() && normalizedBeat < Minigame.NgLateTime();
        }

        double GetNormalizedTime()
        {
            var cond = Conductor.instance;
            double currTime = cond.songPositionAsDouble;
            double targetTime = cond.GetSongPosFromBeat(startBeat + timer);

            // HS timing window uses 1 as the middle point instead of 0
            return 1 + (currTime - targetTime);
        }

        //For the Autoplay
        public void AutoplayEvent()
        {
            if (EnableAutoplayCheat)
            {
                Hit(0f, 1f);
            }
            else
            {
                double normalizedBeat = GetNormalizedTime();
                double stateProg = ((normalizedBeat - Minigame.JustEarlyTime()) / (Minigame.JustLateTime() - Minigame.JustEarlyTime()) - 0.5f) * 2;
                Hit(stateProg, normalizedBeat);
            }
        }

        //The state parameter is either -1 -> Early, 0 -> Perfect, 1 -> Late
        public void Hit(double state, double time)
        {
            if (OnHit != null && enabled)
            {
                if (canHit)
                {
                    double normalized = time - 1f;
                    int offset = Mathf.CeilToInt((float)normalized * 1000);
                    GameManager.instance.AvgInputOffset = offset;
                    state = System.Math.Max(-1.0, System.Math.Min(1.0, state));
                    OnHit(this, (float)state);

                    CleanUp();
                    if (countsForAccuracy && !(noAutoplay || autoplayOnly) && isEligible)
                    {
                        GameManager.instance.ScoreInputAccuracy(TimeToAccuracy(time), time > 1.0, time);
                        if (state >= 1f || state <= -1f)
                        {
                            GoForAPerfect.instance.Miss();
                            SectionMedalsManager.instance.MakeIneligible();
                        }
                        else
                        {
                            GoForAPerfect.instance.Hit();
                        }
                    }
                }
                else
                {
                    Blank();
                }
            }
        }

        double TimeToAccuracy(double time)
        {
            if (time >= Minigame.AceEarlyTime() && time <= Minigame.AceLateTime())
            {
                // Ace
                return 1.0;
            }

            double state = 0;
            if (time >= Minigame.JustEarlyTime() && time <= Minigame.JustLateTime())
            {
                // Good Hit
                if (time > 1.0)
                {
                    // late half of timing window
                    state = 1.0 - ((time - Minigame.AceLateTime()) / (Minigame.JustLateTime() - Minigame.AceLateTime()));
                    state *= 1.0 - Minigame.rankHiThreshold;
                    state += Minigame.rankHiThreshold;
                }
                else
                {
                    //early half of timing window
                    state = ((time - Minigame.JustEarlyTime()) / (Minigame.AceEarlyTime() - Minigame.JustEarlyTime()));
                    state *= 1.0 - Minigame.rankHiThreshold;
                    state += Minigame.rankHiThreshold;
                }
            }
            else
            {
                if (time > 1.0)
                {
                    // late half of timing window
                    state = 1.0 - ((time - Minigame.JustLateTime()) / (Minigame.NgLateTime() - Minigame.JustLateTime()));
                    state *= Minigame.rankOkThreshold;
                }
                else
                {
                    //early half of timing window
                    state = ((time - Minigame.JustEarlyTime()) / (Minigame.AceEarlyTime() - Minigame.JustEarlyTime()));
                    state *= Minigame.rankOkThreshold;
                }
            }
            return state;
        }

        public void Miss()
        {
            if (OnMiss != null && enabled && !autoplayOnly)
            {
                OnMiss(this);
            }

            CleanUp();
            if (countsForAccuracy && !(noAutoplay || autoplayOnly))
            {
                GameManager.instance.ScoreInputAccuracy(0, true, 2.0, 1.0, false);
                GoForAPerfect.instance.Miss();
                SectionMedalsManager.instance.MakeIneligible();
            }
        }

        public void Blank()
        {
            if (OnBlank != null && enabled && !autoplayOnly)
            {
                OnBlank(this);
            }
        }

        public void CleanUp()
        {
            if (markForDeletion) return;
            markForDeletion = true;
        }
    }
}