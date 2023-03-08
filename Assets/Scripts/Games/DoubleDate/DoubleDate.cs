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
                new GameAction("soccer", "Soccer Ball")
                {
                    function = delegate { var e = eventCaller.currentEntity; DoubleDate.instance.SpawnSoccerBall(e.beat); },
                    defaultLength = 2f,
                },
                new GameAction("basket", "Basket Ball")
                {
                    function = delegate { var e = eventCaller.currentEntity; DoubleDate.instance.SpawnBasketBall(e.beat); },
                    defaultLength = 2f,
                },
                new GameAction("football", "Football")
                {
                    function = delegate { var e = eventCaller.currentEntity; DoubleDate.instance.SpawnFootBall(e.beat); },
                    defaultLength = 2.5f,
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_DoubleDate;

    public class DoubleDate : Minigame
    {
        [Header("Prefabs")]
        [SerializeField] SoccerBall soccer;
        [SerializeField] BasketBall basket;
        [SerializeField] Football football;
        public static DoubleDate instance;
        
        private void Awake()
        {
            instance = this;
        }

        void Update()
        {
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                Jukebox.PlayOneShotGame("doubleDate/kick_whiff");
            }
        }

        public void SpawnSoccerBall(float beat)
        {
            SoccerBall spawnedBall = Instantiate(soccer, instance.transform);
            spawnedBall.Init(beat);
            Jukebox.PlayOneShotGame("doubleDate/soccerBounce", beat);
        }

        public void SpawnBasketBall(float beat)
        {
            BasketBall spawnedBall = Instantiate(basket, instance.transform);
            spawnedBall.Init(beat);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("doubleDate/basketballBounce", beat),
                new MultiSound.Sound("doubleDate/basketballBounce", beat + 0.75f),
            });
        }

        public void SpawnFootBall(float beat)
        {
            Football spawnedBall = Instantiate(football, instance.transform);
            spawnedBall.Init(beat);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("doubleDate/footballBounce", beat),
                new MultiSound.Sound("doubleDate/footballBounce", beat + 0.75f),
            });
        }
    }

}