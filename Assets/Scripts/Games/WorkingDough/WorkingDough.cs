using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlWorkingDoughLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("workingDough", "Working Dough \n<color=#eb5454>[WIP]</color>", "090909", false, false, new List<GameAction>()
            {
            });
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_WorkingDough;
    public class WorkingDough : Minigame
    {
        [Header("Animators")]
        public Animator bigDoughNPCAnimator; //Idle Animations
        public Animator smallDoughNPCAnimator; //Idle Animations
        public Animator bigDoughPlayerAnimator; //Idle Animations
        public Animator smallDoughPlayerAnimator; //Idle Animations

        public static WorkingDough instance;

        void Awake()
        {
            instance = this;
        }

        void Update()
        {

        }
    }
}
