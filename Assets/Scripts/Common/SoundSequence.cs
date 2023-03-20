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

        public MultiSound Play(float startBeat, params SequenceParams[] args)
        {
            List<MultiSound.Sound> sounds = new List<MultiSound.Sound>();
            Dictionary<string, string> paramMaps = new Dictionary<string, string>();

            foreach (SequenceClip clipdat in clips)
            {
                string clip = clipdat.clip;
                float beat = clipdat.beat;
                float pitch = clipdat.pitch;
                float volume = clipdat.volume;
                bool looping = clipdat.looping;
                float offset = clipdat.offset;
                if (args != null && clipdat.parameters != null && clipdat.parameters.Length > 0)
                {
                    paramMaps.Clear();
                    // map param names to overrides
                    foreach (SequenceParams prm in clipdat.parameters)
                    {
                        if (!paramMaps.ContainsKey(prm.name))
                            paramMaps.Add(prm.name, prm.map);
                    }
                    // apply overrides
                    foreach (SequenceParams prm in args)
                    {
                        if (paramMaps.ContainsKey(prm.name))
                        {
                            string map = paramMaps[prm.name];
                            switch (map)
                            {
                                case "beat":
                                    beat = prm.value;
                                    break;
                                case "pitch":
                                    pitch = prm.value;
                                    break;
                                case "volume":
                                    volume = prm.value;
                                    break;
                                case "offset":
                                    offset = prm.value;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                sounds.Add(new MultiSound.Sound(clip, startBeat + beat, pitch, volume, looping, offset));
            }

            return MultiSound.Play(sounds.ToArray(), game, force);
        }

        [Serializable]
        public struct SequenceClip
        {
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

            [Tooltip("Set of possible value overrides for clip data")]
            public SequenceParams[] parameters;
        }

        [Serializable]
        public struct SequenceKeyValue
        {
            [Tooltip("Name of sequence (game scripts will call sequences to play using this name")]
            public string name;
            [Tooltip("Sequence to play")]
            public SoundSequence sequence;
        }

        [Serializable]
        public struct SequenceParams
        {
            //SequenceParams used in minigame code
            public SequenceParams(string name, float value)
            {
                this.map = "";
                this.name = name;
                this.value = value;
            }

            [Tooltip("Inspector use only; Sequence Clip value to override")]
            public string map;

            [Tooltip("Name of parameter")]
            public string name;

            [NonSerialized]
            public float value;
        }
    }
}