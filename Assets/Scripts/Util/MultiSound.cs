using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeavenStudio.Util
{
    public class MultiSound : MonoBehaviour
    {
        private float startBeat;
        private bool game;
        private bool forcePlay;
        public List<Sound> sounds = new List<Sound>();
        public List<Util.Sound> playingSounds = new List<Util.Sound>();

        public class Sound
        {
            public string name { get; set; }
            public float beat { get; set; }
            public float pitch { get; set; }
            public float volume { get; set; }
            public bool looping { get; set; }
            public float offset { get; set; }

            public Sound(string name, float beat, float pitch = 1f, float volume = 1f, bool looping = false, float offset = 0f)
            {
                this.name = name;
                this.beat = beat;
                this.pitch = pitch;
                this.volume = volume;
                this.looping = looping;
                this.offset = offset;
            }
        }


        public static MultiSound Play(Sound[] snds, bool game = true, bool forcePlay = false)
        {
            List<Sound> sounds = snds.ToList();
            GameObject go = new GameObject("MultiSound");
            MultiSound ms = go.AddComponent<MultiSound>();
            ms.sounds = sounds;
            ms.startBeat = sounds[0].beat;
            ms.game = game;
            ms.forcePlay = forcePlay;

            for (int i = 0; i < sounds.Count; i++)
            {
                Util.Sound s;
                if (game)
                    s = Jukebox.PlayOneShotGame(sounds[i].name, sounds[i].beat, sounds[i].pitch, sounds[i].volume, sounds[i].looping, forcePlay, sounds[i].offset);
                else
                    s = Jukebox.PlayOneShot(sounds[i].name, sounds[i].beat, sounds[i].pitch, sounds[i].volume, sounds[i].looping, null, sounds[i].offset);
                ms.playingSounds.Add(s);
            }

            GameManager.instance.SoundObjects.Add(go);
            return ms;
        }

        private void Update()
        {
            foreach (Util.Sound sound in playingSounds)
            {
                if (sound != null)
                    return;
            }
            Delete();
        }

        public void Delete()
        {
            GameManager.instance.SoundObjects.Remove(gameObject);
            Destroy(gameObject);
        }
    }
}