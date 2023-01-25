using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using HeavenStudio.Util;
using Starpelly;

namespace HeavenStudio.Games
{

    public class PlayerActionEvent : PlayerActionObject
    {
        public static bool EnableAutoplayCheat = false;
        public delegate void ActionEventCallback(PlayerActionEvent caller);
        public delegate void ActionEventCallbackState(PlayerActionEvent caller, float state);

        public ActionEventCallbackState OnHit; //Function to trigger when an input has been done perfectly
        public ActionEventCallback OnMiss; //Function to trigger when an input has been missed
        public ActionEventCallback OnBlank; //Function to trigger when an input has been recorded while this is pending

        public ActionEventCallback OnDestroy; //Function to trigger whenever this event gets destroyed. /!\ Shouldn't be used for a minigame! Use OnMiss instead /!\

        public float startBeat;
        public float timer;

        public bool canHit  = true; //Indicates if you can still hit the cue or not. If set to false, it'll guarantee a miss
        public bool enabled = true; //Indicates if the PlayerActionEvent is enabled. If set to false, it'll not trigger any events and destroy itself AFTER it's not relevant anymore

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

        public void Enable()  { enabled = true; }
        public void Disable() { enabled = false; }

        public void CanHit(bool canHit)
        {
            this.canHit = canHit;
        }


        public void Update()
        {
            if(!Conductor.instance.NotStopped()){CleanUp();} // If the song is stopped entirely in the editor, destroy itself as we don't want duplicates

            if (noAutoplay && autoplayOnly) autoplayOnly = false;
            if (noAutoplay && triggersAutoplay){ triggersAutoplay = false; }

            double normalizedTime = GetNormalizedTime();
            double stateProg = ((normalizedTime - Minigame.PerfectTime()) / (Minigame.LateTime() - Minigame.PerfectTime()) - 0.5f) * 2;
            StateCheck(normalizedTime);

            //BUGFIX: ActionEvents destroyed too early
            if (normalizedTime > Minigame.EndTime()) Miss();


            if (IsCorrectInput() && !autoplayOnly)
            {
                if (state.perfect)
                {
                    Hit(stateProg, normalizedTime);
                }
                else if (state.early && !perfectOnly)
                {
                    Hit(-1f, normalizedTime);
                }
                else if (state.late && !perfectOnly) 
                {
                    Hit(1f, normalizedTime);
                }
                else
                {
                    Blank();
                }
            }
        }

        public bool IsExpectingInputNow()
        {
            double normalizedBeat = GetNormalizedTime();
            return normalizedBeat > Minigame.EarlyTime() && normalizedBeat < Minigame.EndTime();
        }

        double GetNormalizedTime()
        {
            var cond = Conductor.instance;
            double currTime = cond.GetSongPosFromBeat(cond.songPositionInBeatsAsDouble);
            double targetTime = cond.GetSongPosFromBeat(startBeat + timer);
            double min = targetTime - 1f;
            double max = targetTime + 1f;
            return 1f + (((currTime - min) / (max - min))-0.5f)*2;
        }

        public bool IsCorrectInput()
        {
            // This one is a mouthful but it's an evil good to detect the correct input
            // Forgive me for those input type names
            return (
                        //General inputs, both down and up
                        (PlayerInput.Pressed()              && inputType.HasFlag(InputType.STANDARD_DOWN))        ||
                        (PlayerInput.AltPressed()           && inputType.HasFlag(InputType.STANDARD_ALT_DOWN))    ||
                        (PlayerInput.GetAnyDirectionDown()  && inputType.HasFlag(InputType.DIRECTION_DOWN))       ||
                        (PlayerInput.PressedUp()            && inputType.HasFlag(InputType.STANDARD_UP))          ||
                        (PlayerInput.AltPressedUp()         && inputType.HasFlag(InputType.STANDARD_ALT_UP))      ||
                        (PlayerInput.GetAnyDirectionUp()    && inputType.HasFlag(InputType.DIRECTION_UP))         ||
                        //Specific directional inputs
                        (PlayerInput.GetSpecificDirectionDown(PlayerInput.DOWN)     && inputType.HasFlag(InputType.DIRECTION_DOWN_DOWN))  ||
                        (PlayerInput.GetSpecificDirectionDown(PlayerInput.UP)       && inputType.HasFlag(InputType.DIRECTION_UP_DOWN))    ||
                        (PlayerInput.GetSpecificDirectionDown(PlayerInput.LEFT)     && inputType.HasFlag(InputType.DIRECTION_LEFT_DOWN))  ||
                        (PlayerInput.GetSpecificDirectionDown(PlayerInput.RIGHT)    && inputType.HasFlag(InputType.DIRECTION_RIGHT_DOWN)) ||

                        (PlayerInput.GetSpecificDirectionUp(PlayerInput.DOWN)       && inputType.HasFlag(InputType.DIRECTION_DOWN_UP))    ||
                        (PlayerInput.GetSpecificDirectionUp(PlayerInput.UP)         && inputType.HasFlag(InputType.DIRECTION_UP_UP))      ||
                        (PlayerInput.GetSpecificDirectionUp(PlayerInput.LEFT)       && inputType.HasFlag(InputType.DIRECTION_LEFT_UP))    ||
                        (PlayerInput.GetSpecificDirectionUp(PlayerInput.RIGHT)      && inputType.HasFlag(InputType.DIRECTION_RIGHT_UP))
                   );
        }


        //For the Autoplay
        public override void OnAce()
        {
            if (EnableAutoplayCheat)
            {
                Hit(0f, 1f);
            }
            else
            {
                double normalizedBeat = GetNormalizedTime();
                double stateProg = ((normalizedBeat - Minigame.PerfectTime()) / (Minigame.LateTime() - Minigame.PerfectTime()) - 0.5f) * 2;
                Hit(stateProg, normalizedBeat);
            }
        }

        //The state parameter is either -1 -> Early, 0 -> Perfect, 1 -> Late
        public void Hit(double state, double time) 
        {
            if (OnHit != null && enabled)
            {
                if(canHit)
                {
                    double normalized = time - 1f;
                    int offset = Mathf.CeilToInt((float)normalized * 1000);
                    GameManager.instance.AvgInputOffset = offset;
                    OnHit(this, (float) state);

                    CleanUp();
                    if (countsForAccuracy && !(noAutoplay || autoplayOnly))
                        GameManager.instance.ScoreInputAccuracy(TimeToAccuracy(time), time > 1.0, 1.0);
                } else
                {
                   Blank();
                }
            }
        }

        double TimeToAccuracy(double time)
        {
            if (time >= Minigame.AceStartTime() && time <= Minigame.AceEndTime())
            {
                // Ace
                Debug.Log("Accuracy (Ace): " + 1.0);
                return 1.0;
            }

            double state = 0;
            if (time >= Minigame.PerfectTime() && time <= Minigame.LateTime())
            {
                // Good Hit
                if (time > 1.0)
                {
                    // late half of timing window
                    state = 1.0 - ((time - Minigame.AceEndTime()) / (Minigame.LateTime() - Minigame.AceEndTime()));
                    state *= 1.0 - Minigame.rankHiThreshold;
                    state += Minigame.rankHiThreshold;
                    Debug.Log("Accuracy (Late): " + state);
                }
                else
                {
                    //early half of timing window
                    state = ((time - Minigame.PerfectTime()) / (Minigame.AceStartTime() - Minigame.PerfectTime()));
                    state *= 1.0 - Minigame.rankHiThreshold;
                    state += Minigame.rankHiThreshold;
                    Debug.Log("Accuracy (Early): " + state);
                }
            }
            else
            {
                if (time > 1.0)
                {
                    // late half of timing window
                    state = 1.0 - ((time - Minigame.LateTime()) / (Minigame.EndTime() - Minigame.LateTime()));
                    state *= Minigame.rankOkThreshold;
                    Debug.Log("Accuracy (Late NG): " + state);
                }
                else
                {
                    //early half of timing window
                    state = ((time - Minigame.PerfectTime()) / (Minigame.AceStartTime() - Minigame.PerfectTime()));
                    state *= Minigame.rankOkThreshold;
                    Debug.Log("Accuracy (Early NG): " + state);
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
                GameManager.instance.ScoreInputAccuracy(0, true, 1.0);
        }

        public void Blank()
        {
            if(OnBlank != null && enabled && !autoplayOnly)
            {
                OnBlank(this);
            }
        }

        public void CleanUp()
        {
            OnDestroy(this);
            Destroy(this.gameObject);
        }

    }
}