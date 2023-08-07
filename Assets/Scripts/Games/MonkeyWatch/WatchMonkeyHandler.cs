using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using UnityEngine.Rendering;

namespace HeavenStudio.Games.Scripts_MonkeyWatch
{
    public class WatchMonkeyHandler : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private WatchMonkey yellowMonkeyRef;
        [SerializeField] private WatchMonkey pinkMonkeyRef;

        [Header("Properties")]
        [SerializeField] private int maxMonkeys = 30;

        private MonkeyWatch game;

        private int startMinute = 0;
        private int watchHoleIndex = 0;
        private List<Transform> watchHoles = new();
        private List<WatchMonkey> currentMonkeys = new();

        private void Awake()
        {
            game = MonkeyWatch.instance;
            for (int i = 0; i < 60; i++)
            {
                watchHoles.Add(transform.GetChild(i));
            }
        }

        public void Init(int minute)
        {
            startMinute = minute;
            watchHoleIndex = startMinute;
        }
        private Transform GetNextAvailableWatchHole()
        {
            int currentIndex = watchHoleIndex;
            watchHoleIndex++;
            if (watchHoleIndex >= watchHoles.Count)
            {
                watchHoleIndex = 0;
            }
            return watchHoles[currentIndex];
        }

        public void SpawnMonkey(double beat, bool isPink, bool instant)
        {
            if (GetMonkeyAtBeat(beat) != null) return;
            int index = watchHoleIndex;
            var hole = GetNextAvailableWatchHole();
            WatchMonkey spawnedMonkey = Instantiate(isPink ? pinkMonkeyRef : yellowMonkeyRef, hole);
            spawnedMonkey.Appear(beat, instant, hole.GetComponent<Animator>(), GetMonkeyAngle(index));
            spawnedMonkey.transform.eulerAngles = Vector3.zero;
            var sortingGroup = spawnedMonkey.GetComponent<SortingGroup>();
            if (index <= 30) sortingGroup.sortingOrder = 50 + watchHoleIndex;
            else sortingGroup.sortingOrder = 50 + watchHoleIndex - (watchHoleIndex - 29);
            currentMonkeys.Add(spawnedMonkey);
            if (currentMonkeys.Count > maxMonkeys)
            {
                currentMonkeys[0].Disappear(Conductor.instance.songPositionInBeats);
                currentMonkeys.Remove(currentMonkeys[0]);
            }
        }

        public WatchMonkey GetMonkeyAtBeat(double beat)
        {
            return currentMonkeys.Find(x => x.monkeyBeat == beat);
        }

        private int GetMonkeyAngle(int monkey)
        {
            return monkey switch
            {
                >= 4 and <= 12 => 2,
                >= 12 and <= 19 => 3,
                >= 19 and <= 27 => 4,
                >= 27 and <= 34 => 5,
                >= 34 and <= 42 => 6,
                >= 42 and <= 49 => 7,
                >= 49 and <= 56 => 8,
                _ => 1,
            };
        }
    }
}

