using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

using Starpelly;

namespace HeavenStudio
{
    // [RequireComponent(typeof(AudioSource))]
    public class Conductor : MonoBehaviour
    {
        // Song beats per minute
        // This is determined by the song you're trying to sync up to
        public float songBpm;

        // The number of seconds for each song beat
        public float secPerBeat;

        // Current song position, in seconds
        private float songPos; // for Conductor use only
        public float songPosition;

        // Current song position, in beats
        private float songPosBeat; // for Conductor use only
        public float songPositionInBeats;

        // Current time of the song
        private float time;

        // an AudioSource attached to this GameObject that will play the music.
        public AudioSource musicSource;

        // The offset to the first beat of the song in seconds
        public float firstBeatOffset;

        // Conductor instance
        public static Conductor instance;

        // Conductor is currently playing song
        public bool isPlaying;
        
        // Conductor is currently paused, but not fully stopped
        public bool isPaused;

        // Last reported beat based on song position
        private float lastReportedBeat = 0f;

        // Metronome tick sound enabled
        public bool metronome = false;

        public float timeSinceLastTempoChange = 0;

        private bool beat;

        // private AudioDspTimeKeeper timeKeeper;

        void Awake()
        {
            instance = this;
        }

        public void SetBeat(float beat)
        {
            float secFromBeat = GetSongPosFromBeat(beat);

            if (musicSource.clip != null)
            {
                if (secFromBeat < musicSource.clip.length)
                    musicSource.time = secFromBeat;
                else
                    musicSource.time = 0;
            }

            GameManager.instance.SetCurrentEventToClosest(beat);
            songPosBeat = beat;
            songPositionInBeats = songPosBeat;
        }

        public void Play(float beat)
        {
            bool negativeOffset = firstBeatOffset < 0f;
            bool negativeStartTime = false;

            var startPos = GetSongPosFromBeat(beat);
            if (negativeOffset)
            {
                time = startPos;
            }
            else
            {
                negativeStartTime = startPos - firstBeatOffset < 0f;

                if (negativeStartTime)
                    time = startPos - firstBeatOffset;
                else
                    time = startPos;
            }

            songPosBeat = time / secPerBeat; 

            isPlaying = true;
            isPaused = false;

            if (SongPosLessThanClipLength(startPos))
            {
                if (negativeOffset)
                {
                    var musicStartTime = startPos + firstBeatOffset;

                    if (musicStartTime < 0f)
                    {
                        musicSource.time = startPos;
                        musicSource.PlayScheduled(AudioSettings.dspTime - firstBeatOffset / musicSource.pitch);
                    }
                    else
                    {
                        musicSource.time = musicStartTime;
                        musicSource.PlayScheduled(AudioSettings.dspTime);
                    }
                }
                else
                {
                    if (negativeStartTime)
                    {
                        musicSource.time = startPos;
                    }  
                    else
                    {
                        musicSource.time = startPos + firstBeatOffset;
                    }

                    musicSource.PlayScheduled(AudioSettings.dspTime);
                }
            }

            // GameManager.instance.SetCurrentEventToClosest(songPositionInBeats);
        }

        public void Pause()
        {
            isPlaying = false;
            isPaused = true;

            musicSource.Pause();
        }

        public void Stop(float time)
        {
            this.time = time;

            songPosBeat = 0;
            songPositionInBeats = 0;

            isPlaying = false;
            isPaused = false;

            musicSource.Stop();
        }
        float test;

        public void Update()
        {
            secPerBeat = 60f / songBpm;

            if (isPlaying)
            {
                var dt = Time.unscaledDeltaTime * musicSource.pitch;

                time += dt;

                songPos = time;
                songPosition = songPos;

                songPosBeat += (dt / secPerBeat);
                songPositionInBeats = songPosBeat;
                // songPositionInBeats = Time.deltaTime / secPerBeat;

                if (metronome)
                {
                    if (ReportBeat(ref lastReportedBeat))
                    {
                        Util.Jukebox.PlayOneShot("metronome");
                    }
                    else if (songPosition <= lastReportedBeat)
                    {
                        lastReportedBeat = (songPosition - (songPosition % secPerBeat));
                    }
                }
            }
        }

        public bool ReportBeat(ref float lastReportedBeat, float offset = 0, bool shiftBeatToOffset = false)
        {
            bool result = songPosition > (lastReportedBeat + offset) + secPerBeat;
            if (result == true)
            {
                lastReportedBeat = (songPosition - (songPosition % secPerBeat));

                if (!shiftBeatToOffset)
                    lastReportedBeat += offset;
            }
            return result;
        }

        public float GetLoopPositionFromBeat(float beatOffset, float length)
        {
            return Mathf.Repeat((songPositionInBeats / length) + beatOffset, 1);
        }

        public float GetPositionFromBeat(float startBeat, float length)
        {
            float a = Mathp.Normalize(songPositionInBeats, startBeat, startBeat + length);
            return a;
        }

        public float GetBeatFromPosition(float position, float startBeat, float length)
        {
            return Mathp.DeNormalize(position, startBeat, startBeat + length);
        }

        public float GetPositionFromMargin(float targetBeat, float margin)
        {
            return GetPositionFromBeat(targetBeat - margin, margin);
        }

        public float GetBeatFromPositionAndMargin(float position, float targetBeat, float margin)
        {
            return GetBeatFromPosition(position, targetBeat - margin, margin);
        }

        public float GetSongPosFromBeat(float beat)
        {
            return secPerBeat * beat;
        }

        public void SetBpm(float bpm)
        {
            this.songBpm = bpm;
            secPerBeat = 60f / songBpm;
        }

        public float SongLengthInBeats()
        {
            if (!musicSource.clip) return 0;
            return musicSource.clip.length / secPerBeat;
        }

        public bool SongPosLessThanClipLength(float t)
        {
            if (musicSource.clip != null)
                return t < musicSource.clip.length;
            else
                return false;
        }

        public bool NotStopped()
        {
            return Conductor.instance.isPlaying == true || Conductor.instance.isPaused == true;
        }
    }
}