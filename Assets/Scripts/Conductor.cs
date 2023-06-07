using System;
using System.Collections.Generic;
using UnityEngine;

using Starpelly;
using Jukebox;
using Jukebox.Legacy;

namespace HeavenStudio
{
    // [RequireComponent(typeof(AudioSource))]
    public class Conductor : MonoBehaviour
    {
        // Song beats per minute
        // This is determined by the song you're trying to sync up to
        public float songBpm;

        // The number of seconds for each song beat
        public float secPerBeat => (float)secPerBeatAsDouble;
        public double secPerBeatAsDouble;

        // The number of seconds for each song beat, inversely scaled to song pitch (higer pitch = shorter time)
        public float pitchedSecPerBeat => (float)pitchedSecPerBeatAsDouble;
        public double pitchedSecPerBeatAsDouble => (secPerBeat / SongPitch);

        // Current song position, in seconds
        private double songPos; // for Conductor use only
        public float songPosition => (float) songPos;
        public double songPositionAsDouble => songPos;

        // Current song position, in beats
        public double songPosBeat; // for Conductor use only
        public float songPositionInBeats => (float) songPosBeat;
        public double songPositionInBeatsAsDouble => songPosBeat;

        // Current time of the song
        private double time;

        double lastAbsTime;

        // the dspTime we started at
        private double dspStart;
        private float dspStartTime => (float)dspStart;
        public double dspStartTimeAsDouble => dspStart;

        //the beat we started at
        private double startBeat;
        public double startBeatAsDouble => startBeat;

        // an AudioSource attached to this GameObject that will play the music.
        public AudioSource musicSource;

        // The offset to the first beat of the song in seconds
        public double firstBeatOffset;

        // Conductor instance
        public static Conductor instance;

        // Conductor is currently playing song
        public bool isPlaying;
        
        // Conductor is currently paused, but not fully stopped
        public bool isPaused;

        // Last reported beat based on song position
        private double lastReportedBeat = 0f;

        // Metronome tick sound enabled
        public bool metronome = false;
        Util.Sound metronomeSound;

        // pitch values
        private float timelinePitch = 1f;
        private float minigamePitch = 1f;
        public float SongPitch { get => isPaused ? 0f : (timelinePitch * minigamePitch); }

        public void SetTimelinePitch(float pitch)
        {
            timelinePitch = pitch;
            musicSource.pitch = SongPitch;
        }

        public void SetMinigamePitch(float pitch)
        {
            minigamePitch = pitch;
            musicSource.pitch = SongPitch;
        }
        

        void Awake()
        {
            instance = this;
        }

        public void SetBeat(double beat)
        {
            double secFromBeat = GetSongPosFromBeat(beat);

            if (musicSource.clip != null)
            {
                if (secFromBeat < musicSource.clip.length)
                    musicSource.time = (float) secFromBeat;
                else
                    musicSource.time = 0;
            }

            GameManager.instance.SetCurrentEventToClosest((float) beat);
            songPosBeat = beat;
        }

        public void Play(double beat)
        {
            GameManager.instance.SortEventsList();
            bool negativeOffset = firstBeatOffset < 0f;
            bool negativeStartTime = false;

            // Debug.Log("starting playback @ beat " + beat + ", offset is " + firstBeatOffset);

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
            
            //TODO: make this take into account past tempo changes
            songPosBeat = GetBeatFromSongPos(time - firstBeatOffset);
            // Debug.Log("corrected starting playback @ beat " + songPosBeat);

            isPlaying = true;
            isPaused = false;

            if (SongPosLessThanClipLength(startPos))
            {
                if (negativeOffset)
                {
                    var musicStartTime = startPos + firstBeatOffset;

                    if (musicStartTime < 0f)
                    {
                        musicSource.time = (float) startPos;
                        musicSource.PlayScheduled(AudioSettings.dspTime - firstBeatOffset / SongPitch);
                    }
                    else
                    {
                        musicSource.time = (float) musicStartTime;
                        musicSource.PlayScheduled(AudioSettings.dspTime);
                    }
                }
                else
                {
                    if (negativeStartTime)
                    {
                        musicSource.time = (float) startPos;
                    }  
                    else
                    {
                        musicSource.time = (float) (startPos + firstBeatOffset);
                    }

                    musicSource.PlayScheduled(AudioSettings.dspTime);
                }
            }
            lastAbsTime = Time.realtimeSinceStartupAsDouble;
            dspStart = AudioSettings.dspTime;
            startBeat = beat;

            // GameManager.instance.SetCurrentEventToClosest(songPositionInBeats);
        }

        public void Pause()
        {
            isPlaying = false;
            isPaused = true;

            musicSource.Pause();
        }

        public void Stop(double time)
        {
            this.time = time;

            songPosBeat = 0;

            isPlaying = false;
            isPaused = false;

            musicSource.Stop();
        }
        float test;

        public void Update()
        {
            if (isPlaying)
            {
                double absTime = Time.realtimeSinceStartupAsDouble;
                double dt = (absTime - lastAbsTime) * SongPitch;
                lastAbsTime = absTime;

                time += dt;

                songPos = time;

                songPosBeat = GetBeatFromSongPos(songPos - firstBeatOffset);
            }
        }

        public void LateUpdate()
        {
            if (metronome && isPlaying)
            {
                if (ReportBeat(ref lastReportedBeat))
                {
                    metronomeSound = Util.SoundByte.PlayOneShot("metronome", lastReportedBeat);
                }
                else if (songPositionInBeats < lastReportedBeat)
                {
                    lastReportedBeat = Mathf.Round(songPositionInBeats);
                }
            }
            else
            {
                if (metronomeSound != null)
                {
                    metronomeSound.Delete();
                    metronomeSound = null;
                }
            }
        }

        public bool ReportBeat(ref double lastReportedBeat, double offset = 0, bool shiftBeatToOffset = true)
        {
            bool result = songPositionInBeats + (shiftBeatToOffset ? offset : 0f) >= (lastReportedBeat) + 1f;
            if (result)
            {
                lastReportedBeat += 1f;
                if (lastReportedBeat < songPositionInBeats)
                {
                    lastReportedBeat = Mathf.Round(songPositionInBeats);
                }
            }
            return result;
        }

        public float GetLoopPositionFromBeat(float beatOffset, float length)
        {
            return Mathf.Repeat((songPositionInBeats / length) + beatOffset, 1);
        }

        public float GetPositionFromBeat(double startBeat, double length)
        {
            float a = Mathp.Normalize(songPositionInBeats, (float)startBeat, (float)(startBeat + length));
            return a;
        }

        public float GetBeatFromPosition(float position, float startBeat, float length)
        {
            return Mathp.DeNormalize(position, (float)startBeat, (float)(startBeat + length));
        }

        public float GetPositionFromMargin(float targetBeat, float margin)
        {
            return GetPositionFromBeat(targetBeat - margin, margin);
        }

        public float GetBeatFromPositionAndMargin(float position, float targetBeat, float margin)
        {
            return GetBeatFromPosition(position, targetBeat - margin, margin);
        }

        private List<RiqEntity> GetSortedTempoChanges()
        {
            GameManager.instance.SortEventsList();
            return GameManager.instance.Beatmap.TempoChanges;
        }

        public float GetBpmAtBeat(double beat)
        {
            var chart = GameManager.instance.Beatmap;
            if (chart.TempoChanges.Count == 0)
                return 120f;
            float bpm = chart.TempoChanges[0]["tempo"];

            foreach (RiqEntity t in chart.TempoChanges)
            {
                if (t.beat > beat)
                {
                    break;
                }
                bpm = t["tempo"];
            }

            return bpm;
        }

        public double GetSongPosFromBeat(double beat)
        {
            var chart = GameManager.instance.Beatmap;
            float bpm = 120f;

            double counter = 0f;

            double lastTempoChangeBeat = 0f;

            foreach (RiqEntity t in chart.TempoChanges)
            {
                if (t.beat > beat)
                {
                    break;
                }

                counter += (t.beat - lastTempoChangeBeat) * 60/bpm;
                bpm = t["tempo"];
                lastTempoChangeBeat = t.beat;
            }

            counter += (beat - lastTempoChangeBeat) * 60/bpm;

            return counter;
        }

        //thank you @wooningcharithri#7419 for the psuedo-code
            public double BeatsToSecs(double beats, float bpm)
            {
                return beats / bpm * 60f;
            }
            public double SecsToBeats(double s, float bpm)
            {
                return s / 60f * bpm;
            }

            public double GetBeatFromSongPos(double seconds)
            {
                double lastTempoChangeBeat = 0f;
                double counterSeconds = -firstBeatOffset;
                float lastBpm = 120f;
                
                foreach (RiqEntity t in GameManager.instance.Beatmap.TempoChanges)
                {
                    double beatToNext = t.beat - lastTempoChangeBeat;
                    double secToNext = BeatsToSecs(beatToNext, lastBpm);
                    double nextSecs = counterSeconds + secToNext;

                    if (nextSecs >= seconds)
                        break;
                    
                    lastTempoChangeBeat = t.beat;
                    lastBpm = t["tempo"];
                    counterSeconds = nextSecs;
                }
                return lastTempoChangeBeat + SecsToBeats(seconds - counterSeconds, lastBpm);
            }
        //

        // convert real seconds to beats
        public double GetRestFromRealTime(double seconds)
        {
            return seconds/pitchedSecPerBeat;
        }

        public void SetBpm(float bpm)
        {
            this.songBpm = bpm;
            secPerBeatAsDouble = 60.0 / songBpm;
        }

        public void SetVolume(float percent)
        {
            musicSource.volume = percent / 100f;
        }

        public float SongLengthInBeats()
        {
            if (!musicSource.clip) return 0;
            return (float) GetBeatFromSongPos(musicSource.clip.length);
        }

        public bool SongPosLessThanClipLength(float t)
        {
            if (musicSource.clip != null)
                return t < musicSource.clip.length;
            else
                return false;
        }

        public bool SongPosLessThanClipLength(double t)
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