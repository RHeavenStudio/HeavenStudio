using System;
using System.Collections;
using UnityEngine;
using Starpelly;

namespace HeavenStudio.Util
{
    public class Sound : MonoBehaviour
    {
        public AudioClip clip;
        public float pitch = 1;
        public float bendedPitch = 1; //only used with rockers
        public float volume = 1;

        // For use with PlayOneShotScheduled
        public bool scheduled;
        public double scheduledTime;

        public bool looping;
        public double loopEndBeat = -1;
        public double fadeTime;
        bool loopDone;

        private AudioSource audioSource;
        private Conductor cond;


        private double startTime;

        public double beat;
        public double offset;
        public float scheduledPitch = 1f;

        bool playInstant = false;
        bool played = false;
        bool paused = false;
        bool queued = false;
        public bool available = true;

        const double PREBAKE_TIME = 0.5;

        private void Start()
        {
        }

        public void LerpVolume(double beat, double length, float volumeStart, float volumeEnd)
        {
            if (!gameObject.activeSelf) return;
            StartCoroutine(LerpVolumeCo(beat, length, volumeStart, volumeEnd));
        }

        private IEnumerator LerpVolumeCo(double beat, double length, float volumeStart, float volumeEnd)
        {
            float normalized = 0;
            while (normalized <= 1)
            {
                normalized = cond.GetPositionFromBeat(beat, length);
                audioSource.volume = Mathf.Lerp(volumeStart, volumeEnd, normalized);
                yield return null;
            }
        }

        public void Play()
        {
            if (!available)
            {
                GameManager.instance.SoundObjects.Release(this);
                return;
            }

            audioSource = GetComponent<AudioSource>();
            cond = Conductor.instance;

            available = false;
            audioSource.clip = clip;
            audioSource.pitch = pitch;
            audioSource.volume = volume;
            audioSource.loop = looping;

            loopEndBeat = -1;
            loopDone = false;
            audioSource.Stop();

            if (beat == -1 && !scheduled)
            {
                playInstant = true;
                played = true;
                startTime = AudioSettings.dspTime;
                audioSource.PlayScheduled(startTime);
            }
            else
            {
                playInstant = false;
                if (cond != null)
                {
                    scheduledPitch = cond.SongPitch;
                    startTime = (AudioSettings.dspTime + (cond.GetSongPosFromBeat(beat) - cond.songPositionAsDouble) / (double)scheduledPitch) - offset;
                }

                if (scheduledPitch != 0 && AudioSettings.dspTime >= startTime)
                {
                    audioSource.PlayScheduled(startTime);
                    queued = true;
                }
            }
        }

        private void Update()
        {
            cond = Conductor.instance;
            double dspTime = AudioSettings.dspTime;
            if (!(available || played))
            {
                if (!(cond.isPlaying || cond.isPaused))
                {
                    GameManager.instance.SoundObjects.Release(this);
                    return;
                }
                if (scheduled)
                {
                    if (!queued && dspTime > scheduledTime - PREBAKE_TIME)
                    {
                        audioSource.PlayScheduled(scheduledTime);
                        queued = true;
                    }
                    if (scheduledPitch != 0 && dspTime > scheduledTime)
                    {
                        played = true;
                    }
                }
                else if (!playInstant)
                {
                    if (!queued && dspTime > startTime - PREBAKE_TIME)
                    {
                        startTime = (dspTime + (cond.GetSongPosFromBeat(beat) - cond.songPositionAsDouble) / (double)scheduledPitch) - offset;
                        audioSource.PlayScheduled(startTime);
                        queued = true;
                    }
                    if (scheduledPitch != 0 && dspTime > startTime)
                    {
                        played = true;
                    }
                    else
                    {
                        if (!played && scheduledPitch != cond.SongPitch)
                        {
                            if (cond.SongPitch == 0)
                            {
                                scheduledPitch = cond.SongPitch;
                                if (queued)
                                    audioSource.Pause();
                            }
                            else
                            {
                                if (scheduledPitch == 0)
                                {
                                    audioSource.UnPause();
                                }
                                scheduledPitch = cond.SongPitch;
                                startTime = (dspTime + (cond.GetSongPosFromBeat(beat) - cond.songPositionAsDouble) / (double)scheduledPitch);
                                if (queued)
                                    audioSource.SetScheduledStartTime(startTime);
                            }
                        }
                    }
                }
            }

            if (played)
            {
                if (!(cond.isPlaying || cond.isPaused))
                {
                    GameManager.instance.SoundObjects.Release(this);
                    return;
                }
                if (!(looping || paused || audioSource.isPlaying))
                {
                    GameManager.instance.SoundObjects.Release(this);
                    return;
                }

                if (cond.isPaused || cond.SongPitch == 0)
                {
                    if (!paused)
                    {
                        // Debug.Log($"Pausing {gameObject.name}");
                        audioSource.Pause();
                        paused = true;
                    }
                }
                else
                {
                    if (paused)
                    {
                        // Debug.Log($"Unpausing {gameObject.name}");
                        audioSource.UnPause();
                        paused = false;
                    }
                }
            }

            if (!loopDone)
            {
                if (looping && loopEndBeat != -1) // Looping sounds play forever unless params are set.
                {
                    if (cond.songPositionInBeatsAsDouble > loopEndBeat)
                    {
                        KillLoop(fadeTime);
                        loopDone = true;
                    }
                }
            }
        }

        public void SetLoopParams(double endBeat, double fadeTime)
        {
            loopEndBeat = endBeat;
            this.fadeTime = fadeTime;
        }

        public void Pause()
        {
            if (audioSource != null)
                audioSource.Pause();
        }

        public void UnPause()
        {
            if (audioSource != null)
                audioSource.UnPause();
        }

        public void Stop()
        {
            available = true;
            played = false;
            paused = false;
            queued = false;
            playInstant = false;
            looping = false;
            scheduled = false;
            beat = 0;
            loopEndBeat = -1;
            loopDone = false;
            startTime = 0;

            audioSource.loop = false;
            audioSource.Stop();

            gameObject.SetActive(false);
        }

        #region Bend
        // All of these should only be used with rockers.
        // minenice: consider doing these in the audio thread so they can work per-sample?
        public void BendUp(float bendTime, float bendedPitch)
        {
            if (!gameObject.activeSelf) return;
            this.bendedPitch = bendedPitch;
            StartCoroutine(BendUpLoop(bendTime));
        }

        public void BendDown(float bendTime)
        {
            if (!gameObject.activeSelf) return;
            StartCoroutine(BendDownLoop(bendTime));
        }

        IEnumerator BendUpLoop(float bendTime)
        {
            float startingPitch = audioSource.pitch;
            float bendTimer = 0f;

            while (bendTimer < bendTime)
            {
                bendTimer += Time.deltaTime;
                float normalizedProgress = Mathp.Normalize(bendTimer, 0, bendTime);
                float currentPitch = Mathf.Lerp(startingPitch, bendedPitch, normalizedProgress);
                audioSource.pitch = Mathf.Min(currentPitch, bendedPitch);
                yield return null;
            }
        }

        IEnumerator BendDownLoop(float bendTime)
        {
            float bendTimer = 0f;
            float startingPitch = pitch;

            while (bendTimer < bendTime)
            {
                bendTimer += Time.deltaTime;
                float normalizedProgress = Mathp.Normalize(bendTimer, 0, bendTime);
                float currentPitch = Mathf.Lerp(startingPitch, bendedPitch, 1 - normalizedProgress);
                audioSource.pitch = Mathf.Max(currentPitch, pitch);
                yield return null;
            }
        }

        #endregion

        public void KillLoop(double fadeTime)
        {
            if (!gameObject.activeSelf) return;
            StartCoroutine(FadeLoop(fadeTime));
        }

        double loopFadeTimer = 0f;
        IEnumerator FadeLoop(double fadeTime)
        {
            float startingVol = audioSource.volume;

            while (loopFadeTimer < fadeTime)
            {
                loopFadeTimer += Time.deltaTime;
                audioSource.volume = (float)Math.Max((1f - (loopFadeTimer / fadeTime)) * startingVol, 0.0);
                yield return null;
            }
            yield return null;
            GameManager.instance.SoundObjects.Release(this);
        }
    }
}
