using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Starpelly;
using Jukebox;
using HeavenStudio.Util;
using System.Data.Common;

namespace HeavenStudio
{
    // [RequireComponent(typeof(AudioSource))]
    public class Conductor : MonoBehaviour
    {
        public struct AddedPitchChange
        {
            public double time;
            public float pitch;
        }

        public List<AddedPitchChange> addedPitchChanges = new List<AddedPitchChange>();

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
        public float songPosition => (float)songPos;
        public double songPositionAsDouble => songPos;

        // Current song position, in beats
        public double songPosBeat; // for Conductor use only
        public float songPositionInBeats => (float)songPosBeat;
        public double songPositionInBeatsAsDouble => songPosBeat;

        // Current time of the song
        private double time;
        double dspTime;
        double absTime, absTimeAdjust;
        double dspSizeSeconds;
        double dspMargin = 128 / 44100.0;

        // the dspTime we started at
        private double dspStart;
        private float dspStartTime => (float)dspStart;
        public double dspStartTimeAsDouble => dspStart;
        DateTime startTime;

        //the beat we started at
        private double startPos;
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

        // Metronome tick sound enabled
        public bool metronome = false;
        Util.Sound metronomeSound;
        private int _metronomeTally = 0;

        // pitch values
        private float timelinePitch = 1f;
        private float minigamePitch = 1f;
        public float SongPitch { get => isPaused ? 0f : (timelinePitch * minigamePitch); }
        private float musicScheduledPitch = 1f;
        private double musicScheduledTime = 0;

        public void SetTimelinePitch(float pitch)
        {
            if (pitch != 0 && pitch * minigamePitch != SongPitch)
            {
                Debug.Log("added pitch change " + pitch * minigamePitch + " at" + absTime);
                addedPitchChanges.Add(new AddedPitchChange { time = absTime, pitch = pitch * minigamePitch });
            }

            timelinePitch = pitch;
            musicSource.pitch = SongPitch;

        }

        public void SetMinigamePitch(float pitch)
        {
            if (pitch != 0 && pitch * timelinePitch != SongPitch)
            {
                Debug.Log("added pitch change " + pitch * timelinePitch + " at" + absTime);
                addedPitchChanges.Add(new AddedPitchChange { time = absTime, pitch = pitch * timelinePitch });
            }

            minigamePitch = pitch;
            musicSource.pitch = SongPitch;
        }

        public void SetMinigamePitch(float pitch, double beat)
        {
            BeatAction.New( this,
                new List<BeatAction.Action> {
                    new BeatAction.Action(beat, delegate {
                        SetMinigamePitch(pitch);
                    }),
                }
            );
        }

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            musicSource.priority = 0;
            AudioConfiguration config = AudioSettings.GetConfiguration();
            dspSizeSeconds = config.dspBufferSize / (double)config.sampleRate;
            dspMargin = 2 * dspSizeSeconds;
            addedPitchChanges.Clear();
        }

        public void SetBeat(double beat)
        {
            var chart = GameManager.instance.Beatmap;
            double offset = chart.data.offset;
            startPos = GetSongPosFromBeat(beat);

            double dspTime = AudioSettings.dspTime;

            time = startPos;
            firstBeatOffset = offset;

            SeekMusicToTime(startPos, offset);

            songPosBeat = GetBeatFromSongPos(time);

            GameManager.instance.SetCurrentEventToClosest(beat);
        }

        public void Play(double beat)
        {
            if (isPlaying) return;

            if (!isPaused)
            {
                AudioConfiguration config = AudioSettings.GetConfiguration();
                dspSizeSeconds = config.dspBufferSize / (double)config.sampleRate;
                Debug.Log($"dsp size: {dspSizeSeconds}");
                dspMargin = 2 * dspSizeSeconds;
                addedPitchChanges.Clear();
                addedPitchChanges.Add(new AddedPitchChange { time = 0, pitch = SongPitch });
            }

            var chart = GameManager.instance.Beatmap;
            double offset = chart.data.offset;
            double dspTime = AudioSettings.dspTime;

            startPos = GetSongPosFromBeat(beat);
            firstBeatOffset = offset;
            time = startPos;

            if (musicSource.clip != null && startPos < musicSource.clip.length - offset)
            {
                SeekMusicToTime(startPos, offset);
                double musicStartDelay = -offset - startPos;
                if (musicStartDelay > 0)
                {
                    musicScheduledTime = dspTime + musicStartDelay / SongPitch;
                    dspStart = dspTime;
                }
                else
                {
                    musicScheduledTime = dspTime + dspMargin;
                    dspStart = dspTime + dspMargin;
                }
                musicScheduledPitch = SongPitch;
                musicSource.PlayScheduled(musicScheduledTime);
            }
            if (musicSource.clip == null)
            {
                dspStart = dspTime;
            }

            songPosBeat = GetBeatFromSongPos(time);
            startBeat = songPosBeat;
            _metronomeTally = 0;

            startTime = DateTime.Now;
            absTimeAdjust = 0;

            isPlaying = true;
            isPaused = false;
        }

        public void Pause()
        {
            if (!isPlaying) return;
            isPlaying = false;
            isPaused = true;

            musicSource.Pause();
        }

        public void Stop(double time)
        {
            if (absTimeAdjust != 0)
            {
                Debug.Log($"Last playthrough had a dsp (audio) drift of {absTimeAdjust}.\nConsider increasing audio buffer size if audio distortion was present.");
            }
            this.time = time;

            songPos = time;
            songPosBeat = 0;
            absTimeAdjust = 0;

            isPlaying = false;
            isPaused = false;

            musicSource.Stop();
        }

        /// <summary>
        /// stops playback of the audio without stopping beatkeeping
        /// </summary>
        public void StopOnlyAudio()
        {
            musicSource.Stop();
        }

        /// <summary>
        /// fades out the audio over a duration
        /// </summary>
        /// <param name="duration">duration of the fade</param>
        public void FadeOutAudio(float duration)
        {
            StartCoroutine(FadeOutAudioCoroutine(duration));
        }

        IEnumerator FadeOutAudioCoroutine(float duration)
        {
            float startVolume = musicSource.volume;
            float endVolume = 0f;
            float startTime = Time.time;
            float endTime = startTime + duration;

            while (Time.time < endTime)
            {
                float t = (Time.time - startTime) / duration;
                musicSource.volume = Mathf.Lerp(startVolume, endVolume, t);
                yield return null;
            }

            musicSource.volume = endVolume;
            StopOnlyAudio();
        }

        void SeekMusicToTime(double fStartPos, double offset)
        {
            if (musicSource.clip != null && fStartPos < musicSource.clip.length - offset)
            {
                // https://www.desmos.com/calculator/81ywfok6xk
                double musicStartDelay = -offset - fStartPos;
                if (musicStartDelay > 0)
                {
                    musicSource.timeSamples = 0;
                }
                else
                {
                    int freq = musicSource.clip.frequency;
                    int samples = (int)(freq * (fStartPos + offset));

                    musicSource.timeSamples = samples;
                }
            }
        }

        public void Update()
        {
            if (isPlaying)
            {
                if (AudioSettings.dspTime < musicScheduledTime && musicScheduledPitch != SongPitch)
                {
                    if (SongPitch == 0f)
                    {
                        musicSource.Pause();
                    }
                    else
                    {
                        if (musicScheduledPitch == 0f)
                            musicSource.UnPause();
                        musicScheduledPitch = SongPitch;

                        musicScheduledTime = (AudioSettings.dspTime + (-GameManager.instance.Beatmap.data.offset - songPositionAsDouble) / (double)SongPitch);
                        musicSource.SetScheduledStartTime(musicScheduledTime);
                    }
                }

                absTime = (DateTime.Now - startTime).TotalSeconds;

                //dspTime to sync with audio thread in case of drift
                dspTime = AudioSettings.dspTime - dspStart;
                if (Math.Abs(absTime + absTimeAdjust - dspTime) > dspMargin)
                {
                    int i = 0;
                    while (Math.Abs(absTime + absTimeAdjust - dspTime) > dspMargin)
                    {
                        i++;
                        absTimeAdjust = (dspTime - absTime + absTimeAdjust) * 0.5;
                        if (i > 8) break;
                    }
                }

                time = MapTimeToPitchChanges(absTime + absTimeAdjust);

                songPos = startPos + time;
                songPosBeat = GetBeatFromSongPos(songPos);
            }
        }

        double MapTimeToPitchChanges(double time)
        {
            double counter = 0;
            double lastChangeTime = 0;
            float pitch = addedPitchChanges[0].pitch;
            foreach (var pch in addedPitchChanges)
            {
                double changeTime = pch.time;
                if (changeTime > time)
                {
                    break;
                }

                counter += (changeTime - lastChangeTime) * pitch;
                lastChangeTime = changeTime;
                pitch = pch.pitch;
            }

            counter += (time - lastChangeTime) * pitch;
            return counter;
        }

        public void LateUpdate()
        {
            if (metronome && isPlaying)
            {
                if (songPositionInBeatsAsDouble >= Math.Ceiling(startBeat) + _metronomeTally)
                {
                    metronomeSound = Util.SoundByte.PlayOneShot("metronome", Math.Ceiling(startBeat) + _metronomeTally);
                    _metronomeTally++;
                }
            }
            else
            {
                if (metronomeSound != null)
                {
                    metronomeSound.Stop();
                    metronomeSound = null;
                }
            }
        }

        [Obsolete("Conductor.ReportBeat is deprecated. Please use the OnBeatPulse callback instead.")]
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

                counter += (t.beat - lastTempoChangeBeat) * 60 / bpm;
                bpm = t["tempo"];
                lastTempoChangeBeat = t.beat;
            }

            counter += (beat - lastTempoChangeBeat) * 60 / bpm;

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
            double counterSeconds = 0;
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
            return seconds / pitchedSecPerBeat;
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
            return (float)SongLengthInBeatsAsDouble();
        }

        public double SongLengthInBeatsAsDouble()
        {
            if (!musicSource.clip) return 0;
            return GetBeatFromSongPos(musicSource.clip.length - firstBeatOffset);
        }

        public bool SongPosLessThanClipLength(double t)
        {
            if (musicSource.clip != null)
                return t < musicSource.clip.length - firstBeatOffset;
            else
                return false;
        }

        public bool NotStopped()
        {
            return Conductor.instance.isPlaying == true || Conductor.instance.isPaused == true;
        }
    }
}