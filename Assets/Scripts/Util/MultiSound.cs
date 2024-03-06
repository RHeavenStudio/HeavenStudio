using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace HeavenStudio.Util
{
    public class MultiSound : MonoBehaviour
    {
        private double startBeat;
        private bool game;
        private bool forcePlay;
        private bool commited;
        public List<Sound> sounds = new List<Sound>();
        public List<Util.Sound> playingSounds = new List<Util.Sound>();

        public class Sound
        {
            public string name { get; set; }
            public double beat { get; set; }
            public float pitch { get; set; }
            public float volume { get; set; }
            public bool looping { get; set; }
            public double offset { get; set; }

            public Sound(string name, double beat, float pitch = 1f, float volume = 1f, bool looping = false, double offset = 0f)
            {
                this.name = name;
                this.beat = beat;
                this.pitch = pitch;
                this.volume = volume;
                this.looping = looping;
                this.offset = offset;
            }
        }

        public static MultiSound Play(Sound[] sounds, bool game = true, bool forcePlay = false)
        {
            return Play(sounds.ToList(), game, forcePlay);
        }

        public static MultiSound Play(List<Sound> sounds, bool game = true, bool forcePlay = false)
        {
            if (Conductor.instance == null || sounds.Count < 1) return null;

            GameObject go = new GameObject("MultiSound");
            MultiSound ms = go.AddComponent<MultiSound>();

            ms.sounds = sounds;
            ms.startBeat = sounds[0].beat;
            ms.game = game;
            ms.forcePlay = forcePlay;
            ms.commited = false;

            if (Conductor.instance.WaitingForDsp)
            {
                Debug.Log("Multisound waiting for DSP, deferring play");
                ms.PlayDeferred().Forget();
            }
            else
            {
                ms.CommitPlay();
            }

            return ms;
        }

        void CommitPlay()
        {
            for (int i = 0; i < sounds.Count; i++)
            {
                Util.Sound s;
                if (game)
                    s = SoundByte.PlayOneShotGame(sounds[i].name, sounds[i].beat, sounds[i].pitch, sounds[i].volume, sounds[i].looping, forcePlay, sounds[i].offset);
                else
                    s = SoundByte.PlayOneShot(sounds[i].name, sounds[i].beat, sounds[i].pitch, sounds[i].volume, sounds[i].looping, null, sounds[i].offset);
                playingSounds.Add(s);
            }
            commited = true;
        }

        async UniTaskVoid PlayDeferred()
        {
            await UniTask.WaitUntil(() => !Conductor.instance.WaitingForDsp, PlayerLoopTiming.LastUpdate);
            Debug.Log("Multisound DSP ready, playing");
            CommitPlay();
        }

        private void Update()
        {
            if (!commited) return;
            foreach (Util.Sound sound in playingSounds)
            {
                if (sound == null) continue;
                if (!sound.available) return;
            }
            Destroy(gameObject);
        }

        public void Delete()
        {
            foreach (Util.Sound sound in playingSounds)
            {
                GameManager.instance.SoundObjects.Release(sound);
            }
            Destroy(gameObject);
        }
    }
}