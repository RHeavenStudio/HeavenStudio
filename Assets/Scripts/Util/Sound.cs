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

        private float startTime;

        public float beat;

        bool playInstant = false;
        int playIndex = 0;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.pitch = pitch;
            audioSource.volume = volume;
            audioSource.loop = looping;

            if (beat == -1 && !scheduled)
            {
                audioSource.PlayScheduled(Time.time);
                playInstant = true;
                playIndex++;
            }
            else
            {
                playInstant = false;
            }

            startTime = Conductor.instance.songPosition;

            if (!scheduled && !looping)
                StartCoroutine(NotRelyOnBeatSound());
        }

        private void Update()
        {
            if (playIndex < 1)
            {
                if (scheduled)
                {
                    if (AudioSettings.dspTime > scheduledTime)
                    {
                        StartCoroutine(NotRelyOnBeatSound());
                        playIndex++;
                    }
                }
                else if (!playInstant)
                {
                    if (Conductor.instance.songPositionInBeats > beat)
                    {
                        audioSource.PlayScheduled(Time.time);
                        playIndex++;
                    }
                }
            }

            if (loopIndex < 1)
            {
                if (looping && loopEndBeat != -1) // Looping sounds play forever unless params are set.
                {
                    if (Conductor.instance.songPositionInBeats > loopEndBeat)
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
                yield return new WaitForSeconds(clip.length);
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
