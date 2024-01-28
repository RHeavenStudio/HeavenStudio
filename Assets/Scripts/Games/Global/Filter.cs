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

            for (int i = 0; i < 10; i++)
            {
                var newAmplify = GameManager.instance.GameCamera.gameObject.AddComponent<AmplifyColorEffect>();
                amplifies.Add(newAmplify);
            }

            // Don't use this because of serialization, add to the "FilterType" enum above manually.
            // GenerateFilterTypeEnum();
        }

        #endregion

        #region Filter

        private void OnBeatChanged(double beat)
        {
            allFilterEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "filter" });
            allFilterEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
            foreach (var a in amplifies)
            {
                a.LutTexture = null;
                a.BlendAmount = 1;
            }
        }

        private void Update()
        {
            var songPosBeat = Math.Max(Conductor.instance.songPositionInBeatsAsDouble, 0);

            foreach (var e in allFilterEvents)
            {
                if (e.beat > songPosBeat) continue;
                var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);
                int slot = e["slot"];
                float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(e.beat, e.length));

                float intensity = func(1 - e["start"], 1 - e["end"], normalizedBeat);

                amplifies[slot - 1].LutTexture = amplifyTextures[(int)e["filter"]];
                amplifies[slot - 1].BlendAmount = intensity;
            }
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
