using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeavenStudio.Util
{
    public class MultiSound : MonoBehaviour
    {
        private float startBeat;
        private int index;
        private bool game;
        private bool forcePlay;
        public List<Sound> sounds = new List<Sound>();

        public class Sound
        {
            public string name { get; set; }
            public float beat { get; set; }

            public Sound(string name, float beat)
            {
                this.name = name;
                this.beat = beat;
            }
        }


        public static MultiSound Play(Sound[] snds, bool game = true, bool forcePlay = false)
        {
            List<Sound> sounds = snds.ToList();
            GameObject gameObj = new GameObject();
            MultiSound ms = gameObj.AddComponent<MultiSound>();
            ms.sounds = sounds;
            ms.startBeat = sounds[0].beat;
            ms.game = game;
            ms.forcePlay = forcePlay;
            gameObj.name = "MultiSound";

            GameManager.instance.SoundObjects.Add(gameObj);
            return ms;
        }

        private void Update()
        {
            float songPositionInBeats = Conductor.instance.songPositionInBeats;

            for (int i = 0; i < sounds.Count; i++)
            {
                if (songPositionInBeats >= sounds[i].beat && index == i)
                {
                    if (game)
                        Jukebox.PlayOneShotGame(sounds[i].name, forcePlay:forcePlay);
                    else
                        Jukebox.PlayOneShot(sounds[i].name);

                    index++;
                }
            }

            if (songPositionInBeats >= (sounds[sounds.Count - 1].beat))
            {
                Delete();
            }
        }

        public void Delete()
        {
            GameManager.instance.SoundObjects.Remove(gameObject);
            Destroy(gameObject);
        }
    }
}