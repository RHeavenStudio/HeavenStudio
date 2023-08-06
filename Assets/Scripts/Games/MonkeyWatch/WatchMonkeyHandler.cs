using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_MonkeyWatch
{
    public class WatchMonkeyHandler : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private WatchMonkey yellowMonkeyRef;
        [SerializeField] private WatchMonkey pinkMonkeyRef;

        private MonkeyWatch game;

        private int startMinute = 0;
        private double startBeat = 0;

        private void Awake()
        {
            game = MonkeyWatch.instance;
        }

        public void Init(double beat, int minute)
        {
            startBeat = beat;
            startMinute = minute;
        }
    }
}

