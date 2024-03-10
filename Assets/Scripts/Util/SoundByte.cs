using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HeavenStudio.Util
{
    public class SoundByte
    {
        static GameObject oneShotAudioSourceObject;
        static AudioSource oneShotAudioSource;
        static int soundIdx = 0;

        public static Dictionary<string, AudioClip> audioClips { get; private set; } = new Dictionary<string, AudioClip>();

        public enum AudioType
        {
            OGG,
            WAV
        }

        public static Sound GetAvailableScheduledSound()
        {
            // soundIdx++;
            // soundIdx %= GameManager.instance.SoundObjects.Count;
            // GameManager.instance.SoundObjects[soundIdx].Stop();
            // return GameManager.instance.SoundObjects[soundIdx];

            return GameManager.instance.SoundObjects.Get();
        }

        /// <summary>
        /// Ensures that the jukebox and one-shot audio source exist.
        /// </summary>
        public static void BasicCheck()
        {
            if (oneShotAudioSourceObject == null)
            {
                oneShotAudioSourceObject = new GameObject("OneShot Audio Source");
                oneShotAudioSource = oneShotAudioSourceObject.AddComponent<AudioSource>();
                UnityEngine.Object.DontDestroyOnLoad(oneShotAudioSourceObject);
            }
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
        ///    Pauses all currently playing sounds.
        /// </summary>
        public static void PauseOneShots()
        {
            if (oneShotAudioSource != null)
            {
                oneShotAudioSource.Pause();
            }
        }

        /// <summary>
        ///    Unpauses all currently playing sounds.
        /// </summary>
        public static void UnpauseOneShots()
        {
            if (oneShotAudioSource != null)
            {
                oneShotAudioSource.UnPause();
            }
        }

        public static void PreloadGameAudioClips(Minigames.Minigame inf)
        {
            if (inf.usesAssetBundle)
            {
                var cmnAb = inf.GetCommonAssetBundle();
                if (cmnAb != null)
                {
                    cmnAb.LoadAllAssetsAsync<AudioClip>().completed += (op) =>
                    {
                        foreach (var clip in (op as AssetBundleRequest).allAssets.Cast<AudioClip>())
                        {
                            OnResourceLoaded(clip, $"games/{inf.name}/{clip.name}");
                        }
                    };
                }
                var locAb = inf.GetLocalizedAssetBundle();
                if (locAb != null)
                {
                    locAb.LoadAllAssetsAsync<AudioClip>().completed += (op) =>
                    {
                        foreach (var clip in (op as AssetBundleRequest).allAssets.Cast<AudioClip>())
                        {
                            OnResourceLoaded(clip, $"games/{inf.name}/{clip.name}");
                        }
                    };
                }
            }
            else
            {
                string path = $"Sfx/games/{inf.name}";
                var clips = Resources.LoadAll<AudioClip>(path);
                foreach (var clip in clips)
                {
                    OnResourceLoaded(clip, $"games/{inf.name}/{clip.name}");
                }
            }
        }

        public static void PreloadGameAudioClips(string game)
        {
            var inf = GameManager.instance.GetGameInfo(game);
            PreloadGameAudioClips(inf);
        }

        public static void PreloadAudioClipAsync(string name, string game)
        {
            var inf = GameManager.instance.GetGameInfo(game);
            if (inf != null)
            {
                name = $"games/{name}";
            }
            if (audioClips.ContainsKey(name)) return;
            if (inf.usesAssetBundle)
            {
                var cmnAb = inf.GetCommonAssetBundle();
                if (cmnAb != null && cmnAb.Contains(name))
                {
                    var request = cmnAb.LoadAssetAsync<AudioClip>(name);
                    request.completed += (op) =>
                    {
                        OnResourceLoaded((op as ResourceRequest).asset as AudioClip, $"{game}/{name}");
                    };
                }
                else
                {
                    var locAb = inf.GetLocalizedAssetBundle();
                    if (locAb != null && locAb.Contains(name))
                    {
                        var request = locAb.LoadAssetAsync<AudioClip>(name);
                        request.completed += (op) =>
                        {
                            OnResourceLoaded((op as ResourceRequest).asset as AudioClip, $"{game}/{name}");
                        };
                    }
                }
            }
            else
            {
                PreloadAudioClipAsync($"{game}/{name}");
            }
        }

        public static void PreloadAudioClipAsync(string name)
        {
            if (audioClips.ContainsKey(name)) return;
            string path = $"Sfx/{name}";
            ResourceRequest request = Resources.LoadAsync<AudioClip>(path);
            request.completed += (op) =>
            {
                OnResourceLoaded((op as ResourceRequest).asset as AudioClip, name);
            };
        }

        static void OnResourceLoaded(AudioClip clip, string name)
        {
            if (audioClips.ContainsKey(name))
            {
                audioClips[name] = clip;
            }
            else
            {
                audioClips.Add(name, clip);
            }
        }

        public static void UnloadAudioClips(params string[] names)
        {
            foreach (string s in names)
            {
                if (audioClips.ContainsKey(s)) audioClips.Remove(s);
            }
            Resources.UnloadUnusedAssets();
        }

        public static void UnloadAudioClips()
        {
            audioClips.Clear();
            Resources.UnloadUnusedAssets();
        }

        public static void UnloadAudioClips(string game)
        {
            string[] split;
            foreach (string s in audioClips.Where(x =>
            {
                split = x.Key.Split('/');
                return split.Length > 2 && split[0] == "games" && split[1] == game;
            }).Select(x => x.Key).ToList())
            {
                audioClips.Remove(s);
            }
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        ///    Gets the length of an audio clip
        /// </summary>
        public static double GetClipLength(string name, float pitch = 1f, string game = null)
        {
            AudioClip clip = null;
            string soundName = name.Split('/')[^1];
            if (game != null)
            {
                string cachedName = $"games/{game}/{soundName}";
                if (audioClips.ContainsKey(cachedName))
                {
                    clip = audioClips[cachedName];
                }
                else
                {
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
            }

            //can't load from assetbundle, load from resources
            if (clip == null)
            {
                if (audioClips.ContainsKey(name))
                {
                    clip = audioClips[name];
                }
                else
                {
                    // Debug.Log("Jukebox loading sound " + name + " from resources");
                    clip = Resources.Load<AudioClip>($"Sfx/{name}");
                }
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
        public static Sound PlayOneShot(string name, double beat = -1, float pitch = 1f, float volume = 1f, bool looping = false, string game = null, double offset = 0f)
        {
            AudioClip clip = null;
            string soundName = name.Split('/')[^1];
            if (game != null)
            {
                string cachedName = $"games/{game}/{soundName}";
                if (audioClips.ContainsKey(cachedName))
                {
                    clip = audioClips[cachedName];
                }
                else
                {
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
            }

            //can't load from assetbundle, load from resources
            if (clip == null)
            {
                if (audioClips.ContainsKey(name))
                {
                    clip = audioClips[name];
                }
                else
                {
                    // Debug.Log("Jukebox loading sound " + name + " from resources");
                    clip = Resources.Load<AudioClip>($"Sfx/{name}");
                }
            }

            if (looping || beat != -1 || pitch != 1f)
            {
                Sound snd = GetAvailableScheduledSound();

                snd.clip = clip;
                snd.beat = beat;
                snd.pitch = pitch;
                snd.volume = volume;
                snd.looping = looping;
                snd.offset = offset;
                snd.Play();

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
            Sound snd = GetAvailableScheduledSound();
            AudioClip clip = null;
            string soundName = name.Split('/')[^1];
            if (game != null)
            {
                string cachedName = $"games/{game}/{soundName}";
                if (audioClips.ContainsKey(cachedName))
                {
                    clip = audioClips[cachedName];
                }
                else
                {
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
            }

            //can't load from assetbundle, load from resources
            if (clip == null)
            {
                if (audioClips.ContainsKey(name))
                {
                    clip = audioClips[name];
                }
                else
                {
                    // Debug.Log("Jukebox loading sound " + name + " from resources");
                    clip = Resources.Load<AudioClip>($"Sfx/{name}");
                }
            }

            // abort if no clip found

            snd.clip = clip;
            snd.pitch = pitch;
            snd.volume = volume;
            snd.looping = looping;

            snd.scheduled = true;
            snd.scheduledTime = targetTime;
            snd.Play();

            return snd;
        }

        /// <summary>
        ///    Fires a one-shot sound located in minigame resources.
        ///    Unpitched, non-scheduled, non-looping sounds are played using a global One-Shot audio source that doesn't create a Sound object.
        ///    Looped sounds return their created Sound object so they can be canceled after creation.
        /// </summary>
        public static Sound PlayOneShotGame(string name, double beat = -1, float pitch = 1f, float volume = 1f, bool looping = false, bool forcePlay = false, double offset = 0f)
        {
            string gameName = name.Split('/')[0];
            var inf = GameManager.instance.GetGameInfo(gameName);
            if (GameManager.instance.currentGame == gameName || forcePlay)
            {
                return PlayOneShot($"games/{name}", beat, pitch, volume, looping, inf.usesAssetBundle ? gameName : null, offset);
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
            var newSemitones = Mathf.Pow(2f, (1f / 12f) * semiTones);
            if (pitchToMusic) newSemitones *= Conductor.instance.musicSource.pitch;
            return newSemitones;
        }
        /// <summary>
        /// Returns the semitones from a pitch.
        /// </summary>
        /// <param name="pitch">The pitch of the sound.</param>
        public static int GetSemitonesFromPitch(float pitch, bool pitchToMusic)
        {
            if (pitchToMusic) return (int)((12f * Mathf.Log(pitch, 2)) / Conductor.instance.musicSource.pitch);
            return (int)(12f * Mathf.Log(pitch, 2));
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