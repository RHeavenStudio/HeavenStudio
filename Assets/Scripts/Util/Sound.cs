using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Util
{
    public class Sound : MonoBehaviour
    {
        public AudioClip clip;
        public float pitch = 1;
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
                audioSource.PlayScheduled(AudioSettings.dspTime);
                playInstant = true;
                played = true;
                startTime = cnd.songPositionAsDouble;
                StartCoroutine(NotRelyOnBeatSound());
            }
            else
            {
                playInstant = false;
                scheduledPitch = cnd.SongPitch;
                startTime = (AudioSettings.dspTime + (cnd.GetSongPosFromBeat(beat) - cnd.songPositionAsDouble)/(double)scheduledPitch);
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
                    if (AudioSettings.dspTime > scheduledTime)
                    {
                        StartCoroutine(NotRelyOnBeatSound());
                        played = true;
                    }
                }
                else if (!playInstant)
                {
                    if (AudioSettings.dspTime > startTime)
                    {
                        played = true;
                        StartCoroutine(NotRelyOnBeatSound());
                    }
                    else
                    {
                        if (!played && scheduledPitch != cnd.SongPitch)
                        {
                            scheduledPitch = cnd.SongPitch;
                            startTime = (AudioSettings.dspTime + (cnd.GetSongPosFromBeat(beat) - cnd.songPositionAsDouble)/(double)scheduledPitch);
                            audioSource.SetScheduledStartTime(startTime);
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

        public void Delete()
        {
            GameManager.instance.SoundObjects.Remove(gameObject);
            Destroy(gameObject);
        }

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
