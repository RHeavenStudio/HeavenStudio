using System.Collections.Generic;
using Starpelly;
using UnityEngine;

namespace HeavenStudio.Games.Global.PostProcessing
{
    public class Vignette : PostProcessingController
    {
        private void Start()
        {
            GameManager.instance.onBeatChanged += OnBeatChanged;
        }

        private void OnBeatChanged(float beat)
        {
            AllEvents = EventCaller.GetAllInGameManagerList("postProcessing", new string[] { "vignette" });
        }

        private void Update()
        {
            var songPosBeat = Conductor.instance.songPositionInBeats;

            var indexes = new List<int>();
            for (int i = 0; i < AllEvents.Count; i++)
            {
                var ppEvent = AllEvents[i];

                if (ppEvent.beat > songPosBeat)
                    break;
                if (ppEvent.beat + ppEvent.length < songPosBeat)
                    continue;

                indexes.Add(i);
            }

            bool indexCountChanged = indexes.Count != LastIndexCount;

            for (int i = 0; i < indexes.Count; i++)
            {
                var filter = AllEvents[indexes[i]];

                if (filter.beat <= songPosBeat && filter.beat + filter.length >= songPosBeat)
                {
                    var fadeInTime = filter["fadein"] * 0.01f;
                    var fadeOutTime = filter["fadeout"] * 0.01f;

                    var intensity = filter["inten"];
                    intensity = Mathf.Lerp(1, 0, Mathp.Normalize(intensity, 0, 100));
                    var blendAmount = intensity;

                    var endFadeInTime = Mathf.Lerp(filter.beat, filter.beat + filter.length, fadeInTime);
                    var startFadeOutTime = filter.beat + (Mathf.Lerp(0, filter.length, Mathf.Lerp(1, 0, fadeOutTime)));

                    if (songPosBeat < endFadeInTime)
                    {
                        var normalizedFadeIn = Mathp.Normalize(songPosBeat, filter.beat, endFadeInTime);
                        blendAmount = Mathf.Lerp(1f, intensity, normalizedFadeIn);
                    }
                    else if (songPosBeat >= startFadeOutTime)
                    {
                        var normalizedFadeOut = Mathf.Clamp01(Mathp.Normalize(songPosBeat, startFadeOutTime, filter.beat + filter.length));
                        blendAmount = Mathf.Lerp(intensity, 1f, normalizedFadeOut);
                    }
                }
            }

            LastIndexCount = indexes.Count;
        }
    }
}
