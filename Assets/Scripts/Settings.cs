using UnityEngine;
using UnityEngine.Audio;

namespace HeavenStudio
{
    public class Settings : MonoBehaviour
    {
        public static AudioMixerGroup GetMusicMixer()
        {
            AudioMixerGroup audioMixerGroup = Resources.Load<AudioMixer>("MainMixer").FindMatchingGroups("Music")[0];
            AudioMixer audioMixer = GetMainMixer();
            // float vol = Mathf.Log10(GetMusicVolume()) * 20;
            // audioMixer.SetFloat("MusicVolume", vol);
            return audioMixerGroup;
        }

        public static AudioMixerGroup GetSFXMixer()
        {
            AudioMixerGroup audioMixerGroup = Resources.Load<AudioMixer>("MainMixer").FindMatchingGroups("SFX")[0];
            AudioMixer audioMixer = GetMainMixer();
            // float vol = Mathf.Log10(GetSFXVolume()) * 20;
            // audioMixer.SetFloat("SFXVolume", vol);
            return audioMixerGroup;
        }

        public static AudioMixer GetMainMixer()
        {
            return Resources.Load<AudioMixer>("MainMixer");
        }
    }

}