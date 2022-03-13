using UnityEngine;

namespace HeavenStudio.Util
{
    public static class AnimationHelpers
    {
        public static bool IsAnimationNotPlaying(this Animator anim)
        {
            float compare = anim.GetCurrentAnimatorStateInfo(0).speed;
            return anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= compare && !anim.IsInTransition(0);
        }
    }
}