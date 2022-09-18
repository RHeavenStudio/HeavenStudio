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

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat,timer);
            // allows ace detection with this new system
            float stateProg = ((normalizedBeat - Minigame.PerfectTime()) / (Minigame.LateTime() - Minigame.PerfectTime()) - 0.5f) * 2;
            StateCheck(normalizedBeat);

            //BUGFIX: ActionEvents destroyed too early
            if (normalizedBeat > Minigame.EndTime()) Miss();


            if (IsCorrectInput() && !autoplayOnly)
            {
                if (state.perfect)
                {
                    Hit(stateProg);
                }
                else if (state.early && !perfectOnly)
                {
                    Hit(-1f);
                }
                else if (state.late && !perfectOnly) 
                {
                    Hit(1f);
                }
                else
                {
                    Blank();
                }
            }
        }

        public bool IsExpectingInputNow()
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, timer);
            return normalizedBeat > Minigame.EarlyTime() && normalizedBeat < Minigame.EndTime();
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
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat,timer);
            // allows ace detection with this new system
            float stateProg = ((normalizedBeat - Minigame.PerfectTime()) / (Minigame.LateTime() - Minigame.PerfectTime()) - 0.5f) * 2;
            Hit(stateProg);
        }

        //The state parameter is either -1 -> Early, 0 -> Perfect, 1 -> Late
        public void Hit(float state) 
        {
            if (OnHit != null && enabled)
            {
                if(canHit)
                {
                    OnHit(this, state);
                    CleanUp();
                } else
                {
                   Blank();
                }
            }
    
        }

        public void Miss()
        {
            if (OnMiss != null && enabled && !autoplayOnly)
            {
                OnMiss(this);
            }

            CleanUp();
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