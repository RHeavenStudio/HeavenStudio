using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Starpelly;
using Newtonsoft.Json;
using HeavenStudio.Games;

namespace HeavenStudio.Editor
{
    public class PropController : MonoBehaviour
    {
        public static PropController instance { get; private set; }

        public Properties Properties = new Properties();

        public void LoadProperties(string json = "")
        {
            SortPropertyList();

            if (json != "")
            {
                Properties = JsonConvert.DeserializeObject<Properties>(json);
            }
            else
            {
                NewProperties();
            }
            Conductor.instance.SetBpm(Beatmap.bpm);
            Conductor.instance.SetVolume(Beatmap.musicVolume);
            Conductor.instance.firstBeatOffset = Beatmap.firstBeatOffset;
            Stop(0);
            SetCurrentEventToClosest(0);

        }

        public void SortPropertyList()
        {
            Beatmap.entities.Sort((x, y) => x.beat.CompareTo(y.beat));
            Beatmap.tempoChanges.Sort((x, y) => x.beat.CompareTo(y.beat));
        }
    }
}