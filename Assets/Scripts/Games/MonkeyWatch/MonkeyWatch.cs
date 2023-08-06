using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlMonkeyWatchLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("monkeyWatch", "Monkey Watch", "f0338d", false, false, new List<GameAction>()
            {

            });
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_MonkeyWatch;
    public class MonkeyWatch : Minigame
    {
        public static MonkeyWatch instance;

        [Header("Components")]
        [SerializeField] private Transform cameraAnchor;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform cameraMoveable;

        private void Awake()
        {
            instance = this;
            CameraUpdate();
        }

        private void Update()
        {
            CameraUpdate();
        }

        private void CameraUpdate()
        {
            cameraMoveable.position = -cameraTransform.position;
        }
    }
}
