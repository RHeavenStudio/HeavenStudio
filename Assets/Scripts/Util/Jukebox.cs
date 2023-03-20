using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Util
{
    public class Jukebox
    {
        public enum AudioType
        {
            OGG,
            WAV
        }

        /// <summary>
        /// This is me just idiot-proofing.
        /// </summary>
        public static void BasicCheck()
        {
            if (FindJukebox() == null)
            {
                GameObject Jukebox = new GameObject("Jukebox");
                Jukebox.AddComponent<AudioSource>();
                Jukebox.tag = "Jukebox";
            }
        }

        public static GameObject FindJukebox()
        {
            if (GameObject.FindGameObjectWithTag("Jukebox") != null)
                return GameObject.FindGameObjectWithTag("Jukebox");
            else
                return null;
        }

        public static void SetVolume(float volume)
        {
            BasicCheck();
            FindJukebox().GetComponent<AudioSource>().volume = volume;
        }

        public static Sound PlayOneShot(string name, float beat = -1, float pitch = 1f, float volume = 1f, bool looping = false, string game = null)
        {
            GameObject oneShot = new GameObject("oneShot");

            AudioSource audioSource = oneShot.AddComponent<AudioSource>();
            //audioSource.outputAudioMixerGroup = Settings.GetSFXMixer();
            audioSource.playOnAwake = false;

            Sound snd = oneShot.AddComponent<Sound>();
            AudioClip clip = null;
            if (game != null)
            {
                string soundName = name.Split('/')[2];
                var inf = GameManager.instance.GetGameInfo(game);
                //first try the game's common assetbundle
                // Debug.Log("Jukebox loading sound " + soundName + " from common");
                clip = inf.GetCommonAssetBundle()?.LoadAsset<AudioClip>(soundName);
                //then the localized one
                if (clip == null)
                {
                    // Debug.Log("Jukebox loading sound " + soundName + " from locale");
                    clip = inf.GetLocalizedAssetBundle()?.LoadAsset<AudioClip>(soundName);
                }
            }

            //can't load from assetbundle, load from resources
            if (clip == null)
            {
                // Debug.Log("Jukebox loading sound " + name + " from resources");
                clip = Resources.Load<AudioClip>($"Sfx/{name}");
            }

            snd.clip = clip;
            snd.beat = beat;
            snd.pitch = pitch;
            snd.volume = volume;
            snd.looping = looping;
            // snd.pitch = (clip.length / Conductor.instance.secPerBeat);

            GameManager.instance.SoundObjects.Add(oneShot);

            return snd;
        }

        public static Sound PlayOneShotScheduled(string name, double targetTime, float pitch = 1f, float volume = 1f, bool looping = false, string game = null)
        {
            GameObject oneShot = new GameObject("oneShotScheduled");

            var audioSource = oneShot.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            var snd = oneShot.AddComponent<Sound>();
            AudioClip clip = null;
            if (game != null)
            {
                string soundName = name.Split('/')[2];
                var inf = GameManager.instance.GetGameInfo(game);
                //first try the game's common assetbundle
                // Debug.Log("Jukebox loading sound " + soundName + " from common");
                clip = inf.GetCommonAssetBundle()?.LoadAsset<AudioClip>(soundName);
                //then the localized one
                if (clip == null)
                {
                    // Debug.Log("Jukebox loading sound " + soundName + " from locale");
                    clip = inf.GetLocalizedAssetBundle()?.LoadAsset<AudioClip>(soundName);
                }
            }

            //can't load from assetbundle, load from resources
            if (clip == null)
                clip = Resources.Load<AudioClip>($"Sfx/{name}");

            audioSource.clip = clip;
            snd.clip = clip;
            snd.pitch = pitch;
            snd.volume = volume;
            snd.looping = looping;
            
            snd.scheduled = true;
            snd.scheduledTime = targetTime;
            audioSource.PlayScheduled(targetTime);
            
            GameManager.instance.SoundObjects.Add(oneShot);

            return snd;
        }

        public static Sound PlayOneShotGame(string name, float beat = -1, float pitch = 1f, float volume = 1f, bool looping = false, bool forcePlay = false)
        {
            string gameName = name.Split('/')[0];
            var inf = GameManager.instance.GetGameInfo(gameName);
            if (GameManager.instance.currentGame == gameName || forcePlay)
            {
                return PlayOneShot($"games/{name}", beat, pitch, volume, looping, inf.usesAssetBundle ? gameName : null);
            }

            return null;
        }

        public static Sound PlayOneShotScheduledGame(string name, double targetTime, float pitch = 1f, float volume = 1f, bool looping = false, bool forcePlay = false)
        {
            string gameName = name.Split('/')[0];
            var inf = GameManager.instance.GetGameInfo(gameName);
            if (GameManager.instance.currentGame == gameName || forcePlay)
            {
                return PlayOneShotScheduled($"games/{name}", targetTime, pitch, volume, looping, inf.usesAssetBundle ? gameName : null);
            }

            return null;
        }

        public static void KillLoop(Sound source, float fadeTime)
        {
            // Safeguard against previously-destroyed sounds.
            if (source == null)
                return;

            source.KillLoop(fadeTime);
        }
    }

}