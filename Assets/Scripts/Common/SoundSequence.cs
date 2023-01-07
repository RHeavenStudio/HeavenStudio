using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace HeavenStudio.Util
{
    /// <summary>
    /// MultiSound that is serializable in the inspector, etc.
    /// </summary>
    [Serializable]
    public class SoundSequence
    {
        [Tooltip("Should sequence use game specific-sounds?")]
        [SerializeField] bool game;
        [Tooltip("Should sequence force playback even if corresponding game is not loaded?")]
        [SerializeField] bool force;
        [Tooltip("Clips to play in the sequence")]
        [SerializeField] private List<SequenceClip> clips = new List<SequenceClip>();

        public SoundSequence(bool game, bool force, params SequenceClip[] clips)
        {
            this.game = game;
            this.force = force;
            this.clips = new List<SequenceClip>(clips);
        }

        public MultiSound Play(float startBeat)
        {
            List<MultiSound.Sound> sounds = new List<MultiSound.Sound>();

            foreach (SequenceClip clip in clips)
            {
                sounds.Add(new MultiSound.Sound(clip.clip, startBeat + clip.beat, clip.pitch, clip.volume, clip.looping, clip.offset));
            }

            return MultiSound.Play(sounds.ToArray(), game, force);
        }

        [Serializable]
        public struct SequenceClip
        {
            public SequenceClip(string clip, float beat, float pitch = 1f, float volume = 1f, bool looping = false, float offset = 0f)
            {
                this.clip = clip;
                this.beat = beat;
                this.pitch = pitch;
                this.volume = volume;
                this.looping = looping;
                this.offset = offset;
            }

            [Tooltip("Filename of clip to use (will look in assetbundles before resources)")]
            public string clip;
            [Tooltip("Beat to play clip at relative to start of sequence")]
            public float beat;
            [Tooltip("Pitch to play clip at")]
            [DefaultValue(1f)]
            public float pitch;
            [Tooltip("Volume to play clip at")]
            [DefaultValue(1f)]
            public float volume;
            [Tooltip("Whether to loop the clip")]
            public bool looping;
            [Tooltip("Offset to start playing clip")]
            public float offset;
        }

        [Serializable]
        public struct SequenceKeyValue
        {
            [Tooltip("Name of sequence (game scripts will call sequences to play using this name")]
            public string name;
            [Tooltip("Sequence to play")]
            public SoundSequence sequence;
        }
    }
}