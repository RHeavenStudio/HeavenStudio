using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games
{
    public class SoundEffects : MonoBehaviour
    {
        public enum CountNumbers { One, Two, Three, Four }
        // public readonly static string[] countNames = { "one", "two", "one", "two", "three", "four" };
        public readonly static string[] countNames = { "one", "two", "one", "two", "three", "four" };
        public readonly static float[] timings     = {  0f,    2f,    4f,    5f,    6f,      7f    };

        public enum CountInType { Normal, Alt, Cowbell }
        public static string GetCountInSound(int type)
        {
            return (CountInType)type switch {
                CountInType.Normal => "1",
                CountInType.Alt    => "2",
                CountInType.Cowbell or _ => "cowbell",
            };
        }

        public static void PreloadCounts()
        {
            foreach (string load in new string[] { "one", "two", "three", "four", "cowbell", "ready1", "ready2" })
            {
                SoundByte.PreloadAudioClipAsync(load);
            }
        }

        public static void CountIn(double beat, float length, bool alt, bool go)
        {
            PreloadCounts();
            string countType = alt ? "2" : "1";
            double startBeat = beat + length - 8;

            List<MultiSound.Sound> sfx = new();
            for (int i = 0; i < countNames.Length; i++) {
                if (startBeat + timings[i] >= beat) {
                    sfx.Add(new MultiSound.Sound("count-ins/" + countNames[i] + countType, startBeat + timings[i]));
                }
            }
            if (go) sfx[^1].name = "count-ins/go" + countType;
            MultiSound.Play(sfx, false);
        }

        public static void FourBeatCountIn(double beat, float length, int type)
        {
            PreloadCounts();
            string countType = GetCountInSound(type);
            
            List<MultiSound.Sound> sfx = new();
            for (int i = 0; i < 4; i++) {
                sfx.Add(new MultiSound.Sound("count-ins/" + countNames[i + 2] + countType, beat + (i * length)));
            }
            MultiSound.Play(sfx, false);
        }

        public static void EightBeatCountIn(double beat, float length, int type)
        {
            PreloadCounts();
            string[] sounds = { "one", "two", "one", "two", "three", "four" };
            string sound = GetCountInSound(type);
            
            List<MultiSound.Sound> sfx = new();
            for (int i = 0; i < sounds.Length; i++) {
                sfx.Add(new MultiSound.Sound("count-ins/" + sounds[i] + sound, beat + (timings[i] * length)));
            }
            MultiSound.Play(sfx, false);
        }

        public static void Count(int type, bool alt)
        {
            SoundByte.PlayOneShot("count-ins/" + (CountNumbers)type + (!alt ? "1" : "2"));
        }

        public static void Cowbell()
        {
            SoundByte.PlayOneShot("count-ins/cowbell");
        }

        public static void Ready(double beat, float length)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("count-ins/ready1", beat),
                new MultiSound.Sound("count-ins/ready2", beat + (1f * length)),
            }, false);
        }

        public static void And()
        {
            SoundByte.PlayOneShot("count-ins/and");
        }

        public static void Go(bool alt)
        {
            SoundByte.PlayOneShot("count-ins/go" + (!alt ? "1" : "2"));
        }
    }

}