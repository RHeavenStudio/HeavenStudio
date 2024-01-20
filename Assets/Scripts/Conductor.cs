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
        double absTime, absTimeAdjust, lastAbsTime;
        double dspSizeSeconds;
        double dspMargin = 128 / 44100.0;
        bool deferTimeKeeping = false;
        public bool WaitingForDsp => deferTimeKeeping;

        // the dspTime we started at
        private double dspStart;
        private float dspStartTime => (float)dspStart;
        public double dspStartTimeAsDouble => dspStart;
        DateTime startTime, lastMixTime;

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
        public float TimelinePitch { get => timelinePitch; }
        private float musicScheduledPitch = 1f;
        private double musicScheduledTime = 0;

        // volume modifier
        private float timelineVolume = 1f;
        private float minigameVolume = 1f;

        public void SetTimelinePitch(float pitch)
        {
            if (isPaused) return;
            if (pitch != 0 && pitch != timelinePitch)
            {
                Debug.Log("added pitch change " + pitch * minigamePitch + " at" + absTime);
                addedPitchChanges.Add(new AddedPitchChange { time = absTime, pitch = pitch * minigamePitch });
            }

            timelinePitch = pitch;
            if (musicSource != null && musicSource.clip != null)
            {
                musicSource.pitch = SongPitch;
            }
        }

        public void SetMinigamePitch(float pitch)
        {
            if (isPaused || !isPlaying) return;
            if (pitch != 0 && pitch != minigamePitch)
            {
                Debug.Log("added pitch change " + pitch * timelinePitch + " at" + absTime);
                addedPitchChanges.Add(new AddedPitchChange { time = absTime, pitch = pitch * timelinePitch });
            }

            minigamePitch = pitch;
            if (musicSource != null && musicSource.clip != null)
            {
                musicSource.pitch = SongPitch;
            }
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

        public void PlaySetup(double beat)
        {
            deferTimeKeeping = true;
            songPosBeat = beat;
        }

        public void Play(double beat)
        {
            if (isPlaying) return;
            addedPitchChanges.Clear();
            addedPitchChanges.Add(new AddedPitchChange { time = 0, pitch = timelinePitch });
            minigamePitch = 1f;

            if (isPaused)
            {
                Util.SoundByte.UnpauseOneShots();
            }
            else
            {
                AudioConfiguration config = AudioSettings.GetConfiguration();
                dspSizeSeconds = config.dspBufferSize / (double)config.sampleRate;
                Debug.Log($"dsp size: {dspSizeSeconds}");
                dspMargin = 2 * dspSizeSeconds;

                SetMinigameVolume(1f);
            }

            RiqBeatmap chart = GameManager.instance.Beatmap;
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
                    musicScheduledTime = dspTime + (musicStartDelay / timelinePitch) + 2*dspSizeSeconds;
                    dspStart = dspTime + 2*dspSizeSeconds;
                }
                else
                {
                    musicScheduledTime = dspTime + 2*dspSizeSeconds;
                    dspStart = dspTime + 2*dspSizeSeconds;
                }
                musicSource.PlayScheduled(musicScheduledTime);
                musicScheduledPitch = timelinePitch;
                musicSource.pitch = timelinePitch;
                Debug.Log($"playback scheduled for dsptime {dspStart}");
            }
            if (musicSource.clip == null)
            {
                dspStart = dspTime;
            }

            songPosBeat = beat;
            startBeat = songPosBeat;
            _metronomeTally = 0;

            startTime = DateTime.Now;
            absTimeAdjust = 0;
            lastAbsTime = 0;
            deferTimeKeeping = musicSource.clip != null;

            isPlaying = true;
            isPaused = false;
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            // don't actually do anything with the data

            // wait until we get a dsp update before starting to keep time
            double dsp = AudioSettings.dspTime;
            if (deferTimeKeeping && dsp >= dspStart - dspSizeSeconds)
            {
                deferTimeKeeping = false;
                Debug.Log($"dsptime: {dsp}, deferred timekeeping for {DateTime.Now - startTime} seconds (delta dsp {dsp - dspStart})");
                startTime += TimeSpan.FromSeconds(dsp - dspStart);
                absTimeAdjust = 0;
                dspStart = dsp;
            }
            lastMixTime = DateTime.Now;
        }

        public void Pause()
        {
            if (!isPlaying) return;
            isPlaying = false;
            isPaused = true;
            deferTimeKeeping = false;
            SetMinigamePitch(1f);

            musicSource.Stop();
            Util.SoundByte.PauseOneShots();
        }

        public void Stop(double beat)
        {
            if (absTimeAdjust != 0)
            {
                Debug.Log($"Last playthrough had a dsp (audio) drift of {absTimeAdjust}.\nConsider increasing audio buffer size if audio distortion was present.");
            }
            songPosBeat = beat;

            time = GetSongPosFromBeat(beat);
            songPos = time;

            absTimeAdjust = 0;

            isPlaying = false;
            isPaused = false;
            deferTimeKeeping = false;
            SetMinigamePitch(1f);

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

        Coroutine fadeOutAudioCoroutine;
        public void FadeMinigameVolume(double startBeat, double durationBeats = 1f, float targetVolume = 0f)
        {
            if (fadeOutAudioCoroutine != null)
            {
                StopCoroutine(fadeOutAudioCoroutine);
            }
            fadeOutAudioCoroutine = StartCoroutine(FadeMinigameVolumeCoroutine(startBeat, durationBeats, targetVolume));
        }

        IEnumerator FadeMinigameVolumeCoroutine(double startBeat, double durationBeats, float targetVolume)
        {
            float startVolume = minigameVolume;
            float endVolume = targetVolume;
            double startTime = startBeat;
            double endTime = startBeat + durationBeats;

            while (songPositionInBeatsAsDouble < endTime)
            {
                if (!NotStopped()) yield break;
                double t = (songPositionInBeatsAsDouble - startTime) / durationBeats;
                SetMinigameVolume(Mathf.Lerp(startVolume, endVolume, (float)t));
                yield return null;
            }

            SetMinigameVolume(endVolume);
        }

        public void SetTimelineVolume(float volume)
        {
            timelineVolume = volume;
            musicSource.volume = timelineVolume * minigameVolume;
        }

        public void SetMinigameVolume(float volume)
        {
            minigameVolume = volume;
            musicSource.volume = timelineVolume * minigameVolume;
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
            double dsp = AudioSettings.dspTime;
            if (isPlaying && !(isPaused || deferTimeKeeping))
            {
                //dspTime to sync with audio thread in case of drift
                dspTime = dsp - dspStart;

                absTime = (DateTime.Now - startTime).TotalSeconds;

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
            if (isPlaying)
            {
                if (songPositionInBeatsAsDouble >= Math.Ceiling(startBeat) + _metronomeTally)
                {
                    if (metronome) metronomeSound = Util.SoundByte.PlayOneShot("metronome", Math.Ceiling(startBeat) + _metronomeTally);
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

        public float GetLoopPositionFromBeat(float beatOffset, float length, bool beatClamp = true)
        {
            float beat = songPositionInBeats;
            if (beatClamp)
            {
                beat = Mathf.Max(beat, 0);
            }
            return Mathf.Repeat((beat / length) + beatOffset, 1);
        }

        public float GetPositionFromBeat(double startBeat, double length, bool beatClamp = true)
        {
            float beat = songPositionInBeats;
            if (beatClamp)
            {
                beat = Mathf.Max(beat, 0);
            }
            float a = Mathp.Normalize(beat, (float)startBeat, (float)(startBeat + length));
            return a;
        }

        private List<RiqEntity> GetSortedTempoChanges()
        {
            GameManager.instance.SortEventsList();
            return GameManager.instance.Beatmap.TempoChanges;
        }

        public float GetBpmAtBeat(double beat, out float swingRatio)
        {
            swingRatio = 0.5f;
            var chart = GameManager.instance.Beatmap;
            if (chart.TempoChanges.Count == 0)
                return 120f;
            float bpm = chart.TempoChanges[0]["tempo"];
            swingRatio = chart.TempoChanges[0]["swing"] + 0.5f;

            foreach (RiqEntity t in chart.TempoChanges)
            {
                if (t.beat > beat)
                {
                    break;
                }
                bpm = t["tempo"];
                swingRatio = t["swing"] + 0.5f;
            }

            return bpm;
        }

        public float GetBpmAtBeat(double beat)
        {
            return GetBpmAtBeat(beat, out _);
        }

        public float GetSwingRatioAtBeat(double beat)
        {
            float swingRatio;
            GetBpmAtBeat(beat, out swingRatio);
            return swingRatio;
        }

        public double GetSwungBeat(double beat, float ratio)
        {
            return beat + GetSwingOffset(beat, ratio);
        }

        public double GetSwingOffset(double beatFrac, float ratio)
        {
            beatFrac %= 1;
            if (beatFrac <= 0.5)
            {
                return 0.5 / ratio * beatFrac;
            }
            else
            {
                return 0.5 + (0.5 / (1f - ratio) * (beatFrac - ratio));
            }
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
            SetTimelineVolume(percent / 100f);
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
            return Conductor.instance.isPlaying || Conductor.instance.isPaused;
        }
    }
}