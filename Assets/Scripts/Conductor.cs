using System;
using System.Collections.Generic;
using UnityEngine;

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

        // The number of seconds for each song beat, inversely scaled to song pitch (higer pitch = shorter time)
        public float pitchedSecPerBeat => (secPerBeat / musicSource.pitch);

        // Current song position, in seconds
        private double songPos; // for Conductor use only
        public float songPosition => (float) songPos;

        // Current song position, in beats
        private double songPosBeat; // for Conductor use only
        public float songPositionInBeats => (float) songPosBeat;

        // Current time of the song
        private double time;

        double lastAbsTime;

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

        public float timeSinceLastTempoChange = Single.MinValue;

        private bool beat;

        // private AudioDspTimeKeeper timeKeeper;

        void Awake()
        {
            instance = this;
        }

        public void SetBeat(float beat)
        {
            float secFromBeat = (float) GetSongPosFromBeat(beat);

            if (musicSource.clip != null)
            {
                if (secFromBeat < musicSource.clip.length)
                    musicSource.time = secFromBeat;
                else
                    musicSource.time = 0;
            }

            GameManager.instance.SetCurrentEventToClosest(beat);
            songPosBeat = beat;
        }

        public void Play(float beat)
        {
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
                        musicSource.PlayScheduled(AudioSettings.dspTime - firstBeatOffset / musicSource.pitch);
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
                        musicSource.time = (float) startPos + firstBeatOffset;
                    }

                    musicSource.PlayScheduled(AudioSettings.dspTime);
                }
            }
            lastAbsTime = Time.realtimeSinceStartupAsDouble;

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
                double absTime = Time.realtimeSinceStartupAsDouble;
                float dt = (float) (absTime - lastAbsTime) * musicSource.pitch;
                lastAbsTime = absTime;

                time += dt;

                songPos = time;

                songPosBeat = GetBeatFromSongPos(songPos - firstBeatOffset);
                // songPositionInBeats = Time.deltaTime / secPerBeat;

                if (metronome)
                {
                    if (ReportBeat(ref lastReportedBeat))
                    {
                        Util.Jukebox.PlayOneShot("metronome");
                    }
                    else if (songPositionInBeats < lastReportedBeat)
                    {
                        lastReportedBeat = Mathf.Round(songPositionInBeats);
                    }
                }
            }
        }

        public bool ReportBeat(ref float lastReportedBeat, float offset = 0, bool shiftBeatToOffset = true)
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

        private List<Beatmap.TempoChange> GetSortedTempoChanges(Beatmap chart)
        {
            //iterate over all tempo changes, adding to counter
            List<Beatmap.TempoChange> tempoChanges = chart.tempoChanges;
            tempoChanges.Sort((x, y) => x.beat.CompareTo(y.beat)); //sorts all tempo changes by ascending time (GameManager already does this but juste en cas...)
            return tempoChanges;
        }

        private List<DynamicBeatmap.TempoChange> GetSortedTempoChanges(DynamicBeatmap chart)
        {
            //iterate over all tempo changes, adding to counter
            List<DynamicBeatmap.TempoChange> tempoChanges = chart.tempoChanges;
            tempoChanges.Sort((x, y) => x.beat.CompareTo(y.beat)); //sorts all tempo changes by ascending time (GameManager already does this but juste en cas...)
            return tempoChanges;
        }

        public double GetSongPosFromBeat(float beat)
        {
            var chart = GameManager.instance.Beatmap;
            SetBpm(chart.bpm);

            //initial counter
            double counter = 0f;

            //time of last tempo change, to know how much to add to counter
            float lastTempoChangeBeat = 0f;

            //iterate over all tempo changes, adding to counter
            var tempoChanges = GetSortedTempoChanges(chart);
            foreach (var t in tempoChanges)
            {
                if (t.beat > beat)
                {
                    // this tempo change is past our requested time, abort
                    break;
                }
                // Debug.Log("tempo change at " + t.beat);

                counter += (t.beat - lastTempoChangeBeat) * secPerBeat;
                // Debug.Log("counter is now " + counter);

                // now update to new bpm
                SetBpm(t.tempo);
                lastTempoChangeBeat = t.beat;
            }

            //passed all past tempo changes, now extrapolate from last tempo change until requested position
            counter += (beat - lastTempoChangeBeat) * secPerBeat;

            // Debug.Log("GetSongPosFromBeat returning " + counter);
            return counter;
        }

        //thank you @wooningcharithri#7419 for the psuedo-code
            private double BeatsToSecs(double beats, float bpm)
            {
                // Debug.Log("BeatsToSecs returning " + beats / bpm * 60);
                return beats / bpm * 60f;
            }
            private double SecsToBeats(double s, float bpm)
            {
                // Debug.Log("SecsToBeats returning " + s / 60f / bpm);
                return s / 60f * bpm;
            }

            public double GetBeatFromSongPos(double seconds)
            {
                // Debug.Log("Getting beat of seconds " + seconds);
                var chart = GameManager.instance.Beatmap;
                double lastTempoChangeBeat = 0f;
                double counterSeconds = -firstBeatOffset;
                float lastBpm = chart.bpm;
                
                var tempoChanges = GetSortedTempoChanges(chart);
                foreach (var t in tempoChanges)
                {
                    double beatToNext = t.beat - lastTempoChangeBeat;
                    double secToNext = BeatsToSecs(beatToNext, lastBpm);
                    double nextSecs = counterSeconds + secToNext;

                    // Debug.Log("nextSecs is " + nextSecs + ", seconds " + seconds);
                    if (nextSecs >= seconds)
                        break;
                    
                    lastTempoChangeBeat = t.beat;
                    lastBpm = t.tempo;
                    counterSeconds = nextSecs;
                }

                // Debug.Log("lastTempoChangeBeat is " + lastTempoChangeBeat + ", counterSeconds is " + counterSeconds);

                return lastTempoChangeBeat + SecsToBeats(seconds - counterSeconds, lastBpm);
            }
        //

        // convert real seconds to beats
        public float GetRestFromRealTime(float seconds)
        {
            return seconds/pitchedSecPerBeat;
        }

        public void SetBpm(float bpm)
        {
            this.songBpm = bpm;
            secPerBeat = 60f / songBpm;
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