using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games
{
    public class SoundEffects : MonoBehaviour
    {

        public enum CountNumbers { One, Two, Three, Four }
        public static string[] countNames = { "one", "two", "three", "four" };
        public static void Count(int type, bool alt)
        {
            string sound = countNames[type];
            if (!alt)
                sound += "1";
            else
                sound += "2";
            SoundByte.PlayOneShot("count-ins/" + sound);
        }

        public enum CountInType { Normal, Alt, Cowbell }
        public static string[] GetCountInSounds(string[] sounds, CountInType type)
        {
            for (int i = 0; i < sounds.Length; i++)
            {
                switch (type)
                {
                    case CountInType.Normal:
                        sounds[i] += "1";
                        break;
                    case CountInType.Alt:
                        sounds[i] += "2";
                        break;
                    case CountInType.Cowbell:
                        sounds[i] = "cowbell";
                        break;
                }
            }
            return sounds;
        }
        public static void FourBeatCountIn(double beat, float length, int type)
        {
            string[] sounds = { "one", "two", "three", "four" };
            sounds = GetCountInSounds(sounds, (CountInType)type);
            
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("count-ins/" + sounds[0], beat),
                new MultiSound.Sound("count-ins/" + sounds[1], beat + 1f * length),
                new MultiSound.Sound("count-ins/" + sounds[2], beat + 2f * length),
                new MultiSound.Sound("count-ins/" + sounds[3], beat + 3f * length)
            }, false);
        }

        public static void EightBeatCountIn(double beat, float length, int type)
        {
            string[] sounds = { "one", "two", "one", "two", "three", "four" };
            sounds = GetCountInSounds(sounds, (CountInType)type);
            
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("count-ins/" + sounds[0], beat),
                new MultiSound.Sound("count-ins/" + sounds[1], beat + 2f * length),
                new MultiSound.Sound("count-ins/" + sounds[2], beat + 4f * length),
                new MultiSound.Sound("count-ins/" + sounds[3], beat + 5f * length),
                new MultiSound.Sound("count-ins/" + sounds[4], beat + 6f * length),
                new MultiSound.Sound("count-ins/" + sounds[5], beat + 7f * length)
            }, false);
        }

        public static void Cowbell()
        {
            SoundByte.PlayOneShot("count-ins/cowbell");
        }

        public static void Ready(double beat, float length)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("count-ins/ready1", beat),
                new MultiSound.Sound("count-ins/ready2", beat + 1f * length),
            }, false);
        }

        public static void And()
        {
            SoundByte.PlayOneShot("count-ins/and");
        }

        public static void Go(bool alt)
        {
            string sound = "count-ins/go";
            if (!alt)
                sound += "1";
            else
                sound += "2";
            SoundByte.PlayOneShot(sound);
        }
    }

}