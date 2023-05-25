using System.Collections;
using System.Collections.Generic;
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
        public float loopEndBeat = -1;
        public float fadeTime;
        int loopIndex = 0;

        private AudioSource audioSource;

        private int pauseTimes = 0;

        private double startTime;

        public float beat;
        public float offset;
        public float scheduledPitch = 1f;

        bool playInstant = false;
        bool played = false;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.pitch = pitch;
            audioSource.volume = volume;
            audioSource.loop = looping;
            Conductor cnd = Conductor.instance;

            if (beat == -1 && !scheduled)
            {
                audioSource.Play();
                playInstant = true;
                played = true;
                startTime = cnd.songPositionAsDouble;
                StartCoroutine(NotRelyOnBeatSound());
            }
            else
            {
                playInstant = false;
                scheduledPitch = cnd.SongPitch;
                startTime = (AudioSettings.dspTime + (cnd.GetSongPosFromBeat(beat) - cnd.songPositionAsDouble)/(double)scheduledPitch) - offset;
                audioSource.PlayScheduled(startTime);
            }
        }

        private void Update()
        {
            Conductor cnd = Conductor.instance;
            if (!played)
            {
                if (scheduled)
                {
                    if (scheduledPitch != 0 && AudioSettings.dspTime > scheduledTime)
                    {
                        StartCoroutine(NotRelyOnBeatSound());
                        played = true;
                    }
                }
                else if (!playInstant)
                {
                    if (scheduledPitch != 0 && AudioSettings.dspTime > startTime)
                    {
                        played = true;
                        StartCoroutine(NotRelyOnBeatSound());
                    }
                    else
                    {
                        if (!played && scheduledPitch != cnd.SongPitch)
                        {
                            if (cnd.SongPitch == 0)
                            {
                                scheduledPitch = cnd.SongPitch;
                                audioSource.Pause();
                            }
                            else
                            {
                                if (scheduledPitch == 0)
                                {
                                    audioSource.UnPause();
                                }
                                scheduledPitch = cnd.SongPitch;
                                startTime = (AudioSettings.dspTime + (cnd.GetSongPosFromBeat(beat) - cnd.songPositionAsDouble)/(double)scheduledPitch);
                                audioSource.SetScheduledStartTime(startTime);
                            }
                        }
                    }
                }
            }

            if (loopIndex < 1)
            {
                if (looping && loopEndBeat != -1) // Looping sounds play forever unless params are set.
                {
                    if (cnd.songPositionInBeats > loopEndBeat)
                    {
                        KillLoop(fadeTime);
                        loopIndex++;
                    }
                }
            }
        }

        IEnumerator NotRelyOnBeatSound()
        {
            if (!looping) // Looping sounds are destroyed manually.
            {
                yield return new WaitForSeconds(clip.length / pitch);
                Delete();
            }
        }

        public void SetLoopParams(float endBeat, float fadeTime)
        {
            loopEndBeat = endBeat;
            this.fadeTime = fadeTime;
        }

        public void Stop()
        {
            if (audioSource != null)
                audioSource.Stop();
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

        public void Delete()
        {
            GameManager.instance.SoundObjects.Remove(gameObject);
            Destroy(gameObject);
        }

        #region Bend
        // All of these should only be used with rockers.
        public void BendUp(float bendTime, float bendedPitch)
        {
            this.bendedPitch = bendedPitch;
            StartCoroutine(BendUpLoop(bendTime));
        }

        public void BendDown(float bendTime)
        {
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

        public void KillLoop(float fadeTime)
        {
            StartCoroutine(FadeLoop(fadeTime));
        }

        float loopFadeTimer = 0f;
        IEnumerator FadeLoop(float fadeTime)
        {
            float startingVol = audioSource.volume;

            while (loopFadeTimer < fadeTime)
            {
                loopFadeTimer += Time.deltaTime;
                audioSource.volume = Mathf.Max((1f - (loopFadeTimer / fadeTime)) * startingVol, 0f);
                yield return null;
            }

            Delete();
        }
    }
}
