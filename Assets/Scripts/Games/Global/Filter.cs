using System;
using System.Collections.Generic;
using System.IO;
using Starpelly;
using UnityEngine;

namespace HeavenStudio.Games.Global
{
    public class Filter : MonoBehaviour
    {
        private List<DynamicBeatmap.DynamicEntity> allFilterEvents = new List<DynamicBeatmap.DynamicEntity>();
        private int lastFilterIndexesCount = 0; // Optimization

        private List<AmplifyColorEffect> amplifies = new List<AmplifyColorEffect>(); // keeps memory of all the filters on the main camera
        private List<Texture2D> amplifyTextures = new List<Texture2D>(); // All available camera filters in texture format

        // Because of how HS serializes enums, we have to number these manually to make sure a level doesn't break if we add more.
        public enum FilterType
        {
            accent = 0,
            air = 1,
            atri = 2,
            bleach = 3,
            bleak = 4,
            blockbuster = 5,
            cinecold = 6,
            cinewarm = 7,
            dawn = 8,
            exposed = 9,
            friend = 10,
            friend_diffusion = 11,
            gamebob_1 = 12,
            gamebob_2 = 13,
            gameboy = 14,
            gameboy_color = 15,
            glare = 16,
            grayscale = 17,
            grayscale_invert = 18,
            invert = 19,
            iso_blue = 20,
            iso_cyan = 21,
            iso_green = 22,
            iso_highlights = 23,
            iso_magenta = 24,
            iso_mid = 25,
            iso_red = 26,
            iso_shadows = 27,
            iso_yellow = 28,
            maritime = 29,
            moonlight = 30,
            pelly = 31,
            polar = 32,
            poster = 33,
            redder = 34,
            sanic = 35,
            shareware = 36,
            shift_behind = 37,
            shift_left = 38,
            shift_right = 39,
            tiny_palette = 40,
            toxic = 41,
            tritanopia = 42,
            vibrance = 43,
            winter = 44,
        }

        #region MonoBehaviour

        private void Start()
        {
            foreach (var filt in Enum.GetNames(typeof(FilterType)))
                amplifyTextures.Add(Resources.Load<Texture2D>(Path.Combine("Filters/", filt)));

            GameManager.instance.onBeatChanged += OnBeatChanged;

            // GenerateFilterTypeEnum();
        }

        #endregion

        #region Filter

        private void OnBeatChanged(float beat)
        {
            allFilterEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "filter" });
        }

        private void Update()
        {
            var songPosBeat = Conductor.instance.songPositionInBeats;

            var filterIndexes = new List<int>();
            for (int i = 0; i < allFilterEvents.Count; i++)
            {
                var filter = allFilterEvents[i];

                if (filter.beat > songPosBeat)
                    break;
                if (filter.beat + filter.length < songPosBeat)
                    continue;

                filterIndexes.Add(i);
            }

            bool indexCountChanged = filterIndexes.Count != lastFilterIndexesCount;

            if (indexCountChanged)
            {
                for (int i = 0; i < amplifies.Count; i++)
                {
                    Destroy(amplifies[i]);
                }
                amplifies.Clear();
            }

            for (int i = 0; i < filterIndexes.Count; i++)
            {
                var filter = allFilterEvents[filterIndexes[i]];

                if (filter.beat <= songPosBeat && filter.beat + filter.length >= songPosBeat)
                {
                    var fadeInTime = filter["fadein"];
                    var fadeOutTime = filter["fadeout"];

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

                    var newAmplify = 
                        (indexCountChanged) ? 
                        GameManager.instance.GameCamera.gameObject.AddComponent<AmplifyColorEffect>() :
                        (AmplifyColorEffect)GameManager.instance.GameCamera.GetComponents(typeof(AmplifyColorEffect))[i];

                    var texIndex = (int)filter["filter"];
                    newAmplify.LutTexture = amplifyTextures[texIndex];
                    newAmplify.BlendAmount = blendAmount;

                    amplifies.Add(newAmplify);
                }
            }

            lastFilterIndexesCount = filterIndexes.Count;
        }

        /// <summary>
        /// Used to generate the "FilterType" enum, you probably shouldn't use this, though.
        /// </summary>
        private void GenerateFilterTypeEnum()
        {
            var allFilterTypes = Resources.LoadAll("Filters");
            var filterEnum = string.Empty;
            filterEnum += "public enum FilterType\n{\n";
            for (int i = 0; i < allFilterTypes.Length; i += 2)
            {
                filterEnum += $"    {allFilterTypes[i].name} = {i / 2},\n";
            }
            filterEnum += "}";
            Debug.Log(filterEnum);
        }

        #endregion
    }
}
