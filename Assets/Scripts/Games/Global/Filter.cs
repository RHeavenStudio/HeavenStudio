using System;
using System.Collections.Generic;
using System.IO;
using Starpelly;
using UnityEngine;
using Jukebox;
using Jukebox.Legacy;

namespace HeavenStudio.Games.Global
{
    public class Filter : MonoBehaviour
    {
        private List<RiqEntity> allFilterEvents = new List<RiqEntity>();
        private int lastFilterIndexesCount = 0; // Optimization

        private List<AmplifyColorEffect> amplifies = new List<AmplifyColorEffect>(); // keeps memory of all the filters on the main camera
        private List<Texture2D> amplifyTextures = new List<Texture2D>(); // All available camera filters in texture format

        // Because of how HS serializes enums, we have to number these manually to make sure a level doesn't break if we add more.
        // Backwards compatibility is preserved as long as new elements are appended to the end of this enum. (!DO NOT TRY TO SORT THESE!)
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
            colorshift = 8,
            dawn = 9,
            deepfry = 10,
            deuteranopia = 11,
            exposed = 12,
            friend = 13,
            friend_diffusion = 14,
            gamebob = 15,
            gamebob_2 = 16,
            gameboy = 17,
            gameboy_color = 18,
            glare = 19,
            grayscale = 20,
            grayscale_invert = 21,
            invert = 22,
            iso_blue = 23,
            iso_cyan = 24,
            iso_green = 25,
            iso_highlights = 26,
            iso_magenta = 27,
            iso_mid = 28,
            iso_red = 29,
            iso_shadows = 30,
            iso_yellow = 31,
            maritime = 32,
            moonlight = 33,
            nightfall = 34,
            polar = 35,
            poster = 36,
            protanopia = 37,
            redder = 38,
            sanic = 39,
            sepia = 40,
            sepier = 41,
            sepiest = 42,
            shareware = 43,
            shift_behind = 44,
            shift_left = 45,
            shift_right = 46,
            tina = 47,
            tiny_palette = 48,
            toxic = 49,
            tritanopia = 50,
            vibrance = 51,
            winter = 52,
            blackwhite = 53,
            blackwhite_2 = 54,
        }

        #region MonoBehaviour

        private void Start()
        {
            foreach (var filt in Enum.GetNames(typeof(FilterType)))
                amplifyTextures.Add(Resources.Load<Texture2D>(Path.Combine("Filters/", filt)));

            GameManager.instance.onBeatChanged += OnBeatChanged;

            // Don't use this because of serialization, add to the "FilterType" enum above manually.
            // GenerateFilterTypeEnum();
        }

        #endregion

        #region Filter

        private void OnBeatChanged(double beat)
        {
            allFilterEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "filter" });
        }

        private void Update()
        {
            var songPosBeat = Conductor.instance.songPositionInBeatsAsDouble;

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
                    var fadeInTime = filter["fadein"] * 0.01f;
                    var fadeOutTime = filter["fadeout"] * 0.01f;

                    var intensity = filter["inten"];
                    intensity = Mathf.Lerp(1, 0, Mathp.Normalize(intensity, 0, 100));
                    var blendAmount = intensity;

                    var endFadeInTime = Mathf.Lerp((float) filter.beat, (float) filter.beat + filter.length, fadeInTime);
                    var startFadeOutTime = (float) filter.beat + (Mathf.Lerp(0, filter.length, Mathf.Lerp(1, 0, fadeOutTime)));

                    if (songPosBeat < endFadeInTime)
                    {
                        var normalizedFadeIn = Mathp.Normalize((float) songPosBeat, (float) filter.beat, endFadeInTime);
                        blendAmount = Mathf.Lerp(1f, intensity, normalizedFadeIn);
                    }
                    else if (songPosBeat >= startFadeOutTime)
                    {
                        var normalizedFadeOut = Mathf.Clamp01(Mathp.Normalize((float) songPosBeat, startFadeOutTime, (float) filter.beat + filter.length));
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
        /// Logs C# code of the "FilterType" enum.
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
