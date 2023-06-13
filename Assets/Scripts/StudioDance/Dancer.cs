using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.StudioDance
{
    public class Dancer : MonoBehaviour
    {
        private Animator animator;
        private double lastReportedBeat = 0f;
        private double currentBeat = 0f;

        private bool isDance = false;

        private void Start()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond == null || !cond.isPlaying)
            {
                if (!isDance) return;
                if (currentBeat % 2 != 0)
                {
                    animator.DoScaledAnimationAsync("PoseL");
                }
                else
                {
                    animator.DoScaledAnimationAsync("PoseR");
                }
                isDance = false;
                return;
            }
            isDance = true;
            
            if (cond.ReportBeat(ref lastReportedBeat))
            {
                currentBeat = lastReportedBeat;
            }
            else if (cond.songPositionInBeats < lastReportedBeat)
            {
                lastReportedBeat = Mathf.Round(cond.songPositionInBeats);
            }

            if (currentBeat % 2 != 0)
            {
                animator.DoScaledAnimation("DanceL", currentBeat);
            }
            else
            {
                animator.DoScaledAnimation("DanceR", currentBeat);
            }
        }
    }
}