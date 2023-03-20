using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Editor;
using HeavenStudio.Editor.Track;

namespace HeavenStudio.Games
{
    public class PlayerActionObject : MonoBehaviour
    {
        public bool inList = false;
        public Minigame.Eligible state = new Minigame.Eligible();

        public List<Minigame.Eligible> eligibleHitsList = new List<Minigame.Eligible>();

        //the variables below seem to be mostly unused (they are never used in any meaningful way)
        public int aceTimes; //always set to 0 no matter what (also, the one time it's used doesn't seem to make sense)
        public bool isEligible = true;
        private bool autoPlayEnabledOnStart; //value never used for anything

        public bool triggersAutoplay = true;

        public void PlayerActionInit(GameObject g, float createBeat)
        {
            state.gameObject = g;
            state.createBeat = createBeat;

            autoPlayEnabledOnStart = GameManager.instance.autoplay;
        }

        private void CheckForAce(double normalizedBeat, bool autoPlay = false)
        {
            if (aceTimes == 0)
            {
                if (triggersAutoplay && (GameManager.instance.autoplay || autoPlay) && GameManager.instance.canInput && normalizedBeat >= 1f - (Time.deltaTime*0.5f))
                {
                    OnAce();
                    if (!autoPlay)
                        AceVisuals();
                    // aceTimes++;
                }
            }
        }

        public void ResetAce()
        {
            aceTimes = 0;
        }

        public void ResetState()
        {
            ResetAce();
        }

        // could possibly add support for custom early, perfect, and end times if needed.
        public void StateCheck(double normalizedBeat, bool autoPlay = false)
        {
            CheckForAce(normalizedBeat, autoPlay);
            if (normalizedBeat > Minigame.EarlyTime() && normalizedBeat < Minigame.PerfectTime())
            {
                MakeEligible(true, false, false);
            }
            // Perfect State
            else if (normalizedBeat > Minigame.PerfectTime() && normalizedBeat < Minigame.LateTime())
            {
                MakeEligible(false, true, false);
            }
            // Late State
            else if (normalizedBeat > Minigame.LateTime() && normalizedBeat < Minigame.EndTime())
            {
                MakeEligible(false, false, true);
            }
            else if (normalizedBeat < Minigame.EarlyTime() || normalizedBeat > Minigame.EndTime())
            {
                MakeInEligible();
            }
        }

        public void MakeEligible(bool early, bool perfect, bool late)
        {
            if (!inList)
            {
                state.early = early;
                state.perfect = perfect;
                state.late = late;

                eligibleHitsList.Add(state);
                inList = true;
            }
            else
            {
                Minigame.Eligible es = eligibleHitsList[eligibleHitsList.IndexOf(state)];
                es.early = early;
                es.perfect = perfect;
                es.late = late;
            }
        }

        public void MakeInEligible()
        {
            state.early = false;
            state.perfect = false;
            state.late = false;
            if (!inList) return;

            eligibleHitsList.Remove(state);
            inList = false;
        }

        public void RemoveObject(int currentHitInList, bool destroyObject = false)
        {
            if (currentHitInList < eligibleHitsList.Count)
            {
                eligibleHitsList.Remove(eligibleHitsList[currentHitInList]);
                currentHitInList++;
                if (destroyObject) Destroy(this.gameObject);
            }
        }
        
        // No list
        public void StateCheckNoList(float normalizedBeat)
        {
            CheckForAce(normalizedBeat);
            if (normalizedBeat > Minigame.EarlyTime() && normalizedBeat < Minigame.PerfectTime())
            {
                ModifyState(true, false, false);
            }
            // Perfect State
            else if (normalizedBeat > Minigame.PerfectTime() && normalizedBeat < Minigame.LateTime())
            {
                ModifyState(false, true, false);
            }
            // Late State
            else if (normalizedBeat > Minigame.LateTime() && normalizedBeat < Minigame.EndTime())
            {
                ModifyState(false, false, true);
            }
            else if (normalizedBeat < Minigame.EarlyTime() || normalizedBeat > Minigame.EndTime())
            {
                // ineligible
            }
        }

        public virtual void OnAce()
        {
        }

        private void AceVisuals()
        {
            if (Timeline.instance != null && Editor.Editor.instance != null && !Editor.Editor.instance.fullscreen)
            {
                Timeline.instance.AutoplayBTN.GetComponent<Animator>().Play("Ace", 0, 0);
            }
        }

        private void ModifyState(bool early, bool perfect, bool late)
        {
            state.early = early;
            state.perfect = perfect;
            state.late = late;
        }

        private void OnDestroy()
        {
            MakeInEligible();
        }
    }
}
