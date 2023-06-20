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
        /// <summary>
        /// Returns true if animName is currently playing on animator
        /// </summary>
        /// <param name="anim">Animator to check</param>
        /// <param name="animName">name of animation to look out for</param>
        public static bool IsPlayingAnimationName(this Animator anim, string animName) 
        {
            float compare = anim.GetCurrentAnimatorStateInfo(0).speed;
            return anim.GetCurrentAnimatorStateInfo(0).IsName(animName) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < compare;
        }

        /// <summary>
        /// Sets animator's progress on an animation based on current song beat between startTime and length
        /// function must be called in actor's Update loop to update properly
        /// </summary>
        /// <param name="anim">Animator to update</param>
        /// <param name="animName">name of animation to play</param>
        /// <param name="startTime">reference start time of animation (progress 0.0)</param>
        /// <param name="length">duration of animation (progress 1.0)</param>
        /// <param name="timeScale">multiplier for animation progress (smaller values make animation slower)</param>
        /// <param name="animLayer">animator layer to play animation on</param>
        public static void DoScaledAnimation(this Animator anim, string animName, double startTime, float length = 1f, float timeScale = 1f, int animLayer = -1)
        {
            float pos = Conductor.instance.GetPositionFromBeat(startTime, length) * timeScale;
            anim.Play(animName, animLayer, pos);
            anim.speed = 1f; //not 0 so these can still play their script events
        }

        /// <summary>
        /// Sets animator progress on an animation according to pos
        /// </summary>
        /// <param name="anim">Animator to update</param>
        /// <param name="animName">name of animation to play</param>
        /// <param name="pos">position to set animation progress to (0.0 - 1.0)</param>
        /// <param name="animLayer">animator layer to play animation on</param>
        public static void DoNormalizedAnimation(this Animator anim, string animName, float pos = 0f, int animLayer = -1)
        {
            anim.Play(animName, animLayer, pos);
            anim.speed = 1f;
        }

        /// <summary>
        /// Plays animation on animator, scaling speed to song BPM
        /// call this funtion once, when playing an animation
        /// </summary>
        /// <param name="anim">Animator to play animation on</param>
        /// <param name="animName">name of animation to play</param>
        /// <param name="timeScale">multiplier for animation speed</param>
        /// <param name="startPos">starting progress of animation</param>
        /// <param name="animLayer">animator layer to play animation on</param>
        public static void DoScaledAnimationAsync(this Animator anim, string animName, float timeScale = 1f, float startPos = 0f, int animLayer = -1)
        {
            anim.Play(animName, animLayer, startPos);
            anim.speed = (1f / Conductor.instance.pitchedSecPerBeat) * timeScale;
        }

        /// <summary>
        /// Plays animation on animator, at default speed
        /// this is the least nessecary function here lol
        /// </summary>
        /// <param name="anim">Animator to play animation on</param>
        /// <param name="animName">name of animation to play</param>
        /// <param name="startPos">starting progress of animation</param>
        /// <param name="animLayer">animator layer to play animation on</param>
        public static void DoUnscaledAnimation(this Animator anim, string animName, float startPos = 0f, int animLayer = -1)
        {
            anim.Play(animName, animLayer, startPos);
            anim.speed = 1f;
        }
    }
}