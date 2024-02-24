using UnityEngine;

namespace HeavenStudio.Games.Scripts_Spaceball
{
    public class Alien : MonoBehaviour
    {
        private Animator anim;

        private double showBeat = 0;
        private bool isShowing = false;
        private bool isHiding = false;

        const string IdleAnim = "AlienIdle";
        const string SwingAnim = "AlienSwing";
        const string ShowAnim = "AlienShow";

        private void Awake()
        {
            anim = GetComponent<Animator>();
            anim.Play(IdleAnim, 0, 0);
        }

        private void Update()
        {
            if (Conductor.instance.isPlaying && !isShowing && !isHiding)
            {
                anim.Play(SwingAnim, 0, Conductor.instance.GetLoopPositionFromBeat(0, 1f));
                anim.speed = 0;
            }
            else if (!Conductor.instance.isPlaying && !isHiding)
            {
                anim.Play(IdleAnim, 0, 0);
            }

            if (isShowing)
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(showBeat, 1f);
                if (!isHiding) anim.Play(ShowAnim, 0, normalizedBeat);
                anim.speed = 0;

                if (normalizedBeat >= 2)
                {
                    isShowing = false;
                }
            }
        }

        public void Show(double showBeat, bool hide)
        {
            isShowing = true;
            this.showBeat = showBeat;
            isHiding = hide;
            if (hide) anim.Play("AlienHide", 0, 0);
        }
    }
}