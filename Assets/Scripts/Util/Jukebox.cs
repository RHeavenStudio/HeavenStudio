using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Util
{
    public class Jukebox
    {
        static GameObject oneShotAudioSourceObject;
        static AudioSource oneShotAudioSource;

        public enum AudioType
        {
            OGG,
            WAV
        }

        /// <summary>
        /// Ensures that the jukebox and one-shot audio source exist.
        /// </summary>
        public static void BasicCheck()
        {
            if (FindJukebox() == null)
            {
                GameObject Jukebox = new GameObject("Jukebox");
                Jukebox.AddComponent<AudioSource>();
                Jukebox.tag = "Jukebox";

                
            }
            if (oneShotAudioSourceObject == null)
            {
                oneShotAudioSourceObject = new GameObject("OneShot Audio Source");
                oneShotAudioSource = oneShotAudioSourceObject.AddComponent<AudioSource>();
                UnityEngine.Object.DontDestroyOnLoad(oneShotAudioSourceObject);
            }
        }

        public static GameObject FindJukebox()
        {
            if (GameObject.FindGameObjectWithTag("Jukebox") != null)
                return GameObject.FindGameObjectWithTag("Jukebox");
            else
                return null;
        }

        /// <summary>
        ///    Stops all currently playing sounds.
        /// </summary>
        public static void KillOneShots()
        {
            if (oneShotAudioSource != null)
            {
                oneShotAudioSource.Stop();
            }
        }

        /// <summary>
        ///    Gets the length of an audio clip
        /// </summary>
        public static double GetClipLength(string name, float pitch = 1f, string game = null)
        {
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

            if (clip == null)
            {
                Debug.LogError($"Could not load clip {name}");
                return double.NaN;
            }
            return clip.length / pitch;
        }

        /// <summary>
        ///    Gets the length of an audio clip
        ///    Audio clip is fetched from minigame resources
        /// </summary>
        public static double GetClipLengthGame(string name, float pitch = 1f)
        {
            string gameName = name.Split('/')[0];
            var inf = GameManager.instance.GetGameInfo(gameName);
            if (inf != null)
            {
                return GetClipLength($"games/{name}", pitch, inf.usesAssetBundle ? gameName : null);
            }

            return double.NaN;
        }

        /// <summary>
        ///    Fires a one-shot sound.
        ///    Unpitched, non-scheduled, non-looping sounds are played using a global One-Shot audio source that doesn't create a Sound object.
        ///    Looped sounds return their created Sound object so they can be canceled after creation.
        /// </summary>
        public static Sound PlayOneShot(string name, float beat = -1, float pitch = 1f, float volume = 1f, bool looping = false, string game = null)
        {
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

            if (looping || beat != -1 || pitch != 1f)
            {
                GameObject oneShot = new GameObject("oneShot");

                AudioSource audioSource = oneShot.AddComponent<AudioSource>();
                //audioSource.outputAudioMixerGroup = Settings.GetSFXMixer();
                audioSource.playOnAwake = false;

                Sound snd = oneShot.AddComponent<Sound>();

                snd.clip = clip;
                snd.beat = beat;
                snd.pitch = pitch;
                snd.volume = volume;
                snd.looping = looping;
                // snd.pitch = (clip.length / Conductor.instance.secPerBeat);

                GameManager.instance.SoundObjects.Add(oneShot);

                return snd;
            }
            else
            {
                if (oneShotAudioSourceObject == null)
                {
                    oneShotAudioSourceObject = new GameObject("OneShot Audio Source");
                    oneShotAudioSource = oneShotAudioSourceObject.AddComponent<AudioSource>();
                    UnityEngine.Object.DontDestroyOnLoad(oneShotAudioSourceObject);
                }

                oneShotAudioSource.PlayOneShot(clip, volume);
                return null;
            }
        }

        /// <summary>
        ///    Schedules a sound to be played at a specific time in seconds.
        /// </summary>
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

        /// <summary>
        ///    Fires a one-shot sound located in minigame resources.
        ///    Unpitched, non-scheduled, non-looping sounds are played using a global One-Shot audio source that doesn't create a Sound object.
        ///    Looped sounds return their created Sound object so they can be canceled after creation.
        /// </summary>
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

        /// <summary>
        ///    Schedules a sound to be played at a specific time in seconds.
        ///    Audio clip is fetched from minigame resources
        /// </summary>
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

        /// <summary>
        /// Stops a looping Sound
        /// </summary>
        public static void KillLoop(Sound source, float fadeTime)
        {
            // Safeguard against previously-destroyed sounds.
            if (source == null)
                return;

            source.KillLoop(fadeTime);
        }

        /// <summary>
        /// Gets a pitch multiplier from semitones.
        /// </summary>
        public static float GetPitchFromSemiTones(int semiTones, bool pitchToMusic)
        {
            if (pitchToMusic)
            {
                return Mathf.Pow(2f, (1f / 12f) * semiTones) * Conductor.instance.musicSource.pitch;
            }
            else
            {
                return Mathf.Pow(2f, (1f / 12f) * semiTones);
            }
        }

        /// <summary>
        /// Gets a pitch multiplier from cents.
        /// </summary>
        public static float GetPitchFromCents(int cents, bool pitchToMusic)
        {
            if (pitchToMusic)
            {
                return Mathf.Pow(2f, (1f / 12f) * (cents / 100)) * Conductor.instance.musicSource.pitch;
            }
            else
            {
                return Mathf.Pow(2f, (1f / 12f) * (cents / 100));
            }
        }
    }

}