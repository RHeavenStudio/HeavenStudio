using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlDoubleDateLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("doubleDate", "Double Date \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
            {
                new GameAction("SoccerBall", "Soccer Ball")
                {
                    function = delegate { DoubleDate.instance.SoccerBallCue(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2,
                },
                new GameAction("Basketball", "Basketball")
                {
                    function = delegate { DoubleDate.instance.BasketballCue(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2,
                },
                new GameAction("Football", "Football")
                {
                    function = delegate { DoubleDate.instance.FootballCue(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2,
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_DoubleDate;

    public class DoubleDate : Minigame
    {
        [Header("Animators")]
        public Animator BoyAnim;
        public Animator soccerBallAnim;
        public Animator basketballAnim;
        public Animator footballAnim;

        [Header("References")]
        public GameObject Boy;
        public GameObject Girl;
        public GameObject SoccerBall;
        public GameObject Basketball;
        public GameObject Football;
        
        private float lastReportedBeat = 0f;

        public static DoubleDate instance;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat))
            {
                BoyAnim.Play("Bop", 0, 0);
            }
        }

        public void SoccerBallCue(float beat)
        {
            Jukebox.PlayOneShotGame("doubleDate/soccer_ball_bounce");

            Ball Object = Instantiate(SoccerBall).GetComponent<Ball>();
            Object.startBeat = beat;
        }

        public void BasketballCue(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound("doubleDate/basketball_bounce", beat), 
                    new MultiSound.Sound("doubleDate/basketball_bounce", beat + 0.75f),
                });
            
            Ball Object = Instantiate(Basketball).GetComponent<Ball>();
            Object.startBeat = beat;
        }

        public void FootballCue(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound("doubleDate/football_bounce", beat), 
                    new MultiSound.Sound("doubleDate/football_bounce", beat + 0.75f),
                });

            Football Object = Instantiate(Football).GetComponent<Football>();
            Object.startBeat = beat;
        }
    }

}