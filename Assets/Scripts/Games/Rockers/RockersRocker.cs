using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using System;

namespace HeavenStudio.Games.Scripts_Rockers
{
    public class RockersRocker : MonoBehaviour
    {
        private Sound[] stringSounds = new Sound[6];
        private Sound chordSound;
        private Animator anim;
        public int[] lastPitches = new int[6];
        public int lastBendPitch;

        [SerializeField] private GameObject strumEffect;

        [SerializeField] private bool JJ;

        [NonSerialized] public bool muted;
        private bool strumming;
        private bool bending;
        [NonSerialized] public bool together;

        [SerializeField] List<Sprite> bluSprites = new List<Sprite>();
        [SerializeField] List<Sprite> yelSprites = new List<Sprite>();
        [SerializeField] List<Sprite> normalSprites = new List<Sprite>();

        [SerializeField] List<SpriteRenderer> lightningLefts = new List<SpriteRenderer>();
        [SerializeField] List<SpriteRenderer> lightningRights = new List<SpriteRenderer>();

        private void Awake()
        {
            anim = GetComponent<Animator>();
        }

        private void OnDestroy()
        {
            StopSounds();
        }

        private void StopSounds()
        {
            foreach (var sound in stringSounds)
            {
                if (sound != null)
                {
                    sound.KillLoop(0);
                }
            }
            if (chordSound != null)
            {
                chordSound.KillLoop(0);
            }
        }

        public void PrepareTogether(bool forceMute = false)
        {
            together = true;
            if ((PlayerInput.Pressing() && !JJ) || forceMute)
            {
                DoScaledAnimationAsync("ComeOnPrepare", 0.5f);
                if (forceMute) Mute(true, true);
            }
            else
            {
                DoScaledAnimationAsync("ComeOnPrepareNoMute", 0.5f);
                if (strumming) strumEffect.GetComponent<Animator>().Play("StrumRight", 0, 0);
            }
        }

        public void Miss()
        {
            if (strumming) return;
            DoScaledAnimationAsync(together ? "Miss" : "MissComeOn", 0.5f);
        }

        public void ReturnBack()
        {
            together = false;
            if (JJ)
            {
                muted = false;
                DoScaledAnimationAsync("Return", 0.5f);
            }
            else
            {
                if (strumming) strumEffect.GetComponent<Animator>().Play("StrumIdle", 0, 0);
                if (PlayerInput.Pressing() || (GameManager.instance.autoplay && muted))
                {
                    DoScaledAnimationAsync("Crouch", 0.5f);
                }
                else
                {
                    muted = false;
                    DoScaledAnimationAsync("Return", 0.5f);
                }
            }
        }

        public void StrumStrings(bool gleeClub, int[] pitches, Rockers.PremadeSamples sample, int sampleTones, bool disableStrumEffect = false, bool jump = false, bool barely = false)
        {
            muted = false;
            strumming = true;
            StopSounds();
            if (sample == Rockers.PremadeSamples.None)
            {
                lastPitches = pitches;
                for (int i = 0; i < pitches.Length; i++)
                {
                    if (pitches[i] == -1) continue;
                    float pitch = Jukebox.GetPitchFromSemiTones(pitches[i], true);
                    float volume = GetVolumeBasedOnAmountOfStrings(pitches.Length);
                    string soundName = "rockers/strings/" + (gleeClub ? "gleeClub/" : "normal/" + (i + 1));
                    Debug.Log("Pitch: " + pitch + " Volume: " + volume + " Name: " + soundName);
                    stringSounds[i] = Jukebox.PlayOneShotGame(soundName, -1, pitch, volume, true);
                }
            }
            else
            {
                float pitch = Jukebox.GetPitchFromSemiTones(sampleTones, true);
                string soundName = sample switch
                {
                    Rockers.PremadeSamples.None => "",
                    Rockers.PremadeSamples.BendG5 => "rockers/BendG5",
                    Rockers.PremadeSamples.BendC6 => "rockers/BendC6",
                    Rockers.PremadeSamples.ChordA => "rockers/rocker/ChordA",
                    Rockers.PremadeSamples.ChordAsus4 => "rockers/rocker/ChordAsus4",
                    Rockers.PremadeSamples.ChordBm => "rockers/rocker/ChordBm",
                    Rockers.PremadeSamples.ChordCSharpm7 => "rockers/rocker/ChordC#m7",
                    Rockers.PremadeSamples.ChordDmaj7 => "rockers/rocker/ChordDmaj7",
                    Rockers.PremadeSamples.ChordDmaj9 => "rockers/rocker/ChordDmaj9",
                    Rockers.PremadeSamples.ChordFSharp5 => "rockers/rocker/ChordF#5",
                    Rockers.PremadeSamples.ChordG => "rockers/rocker/ChordG",
                    Rockers.PremadeSamples.ChordG5 => "rockers/rocker/ChordG5",
                    Rockers.PremadeSamples.ChordGdim7 => "rockers/rocker/ChordGdim7",
                    Rockers.PremadeSamples.ChordGm => "rockers/rocker/ChordGm",
                    Rockers.PremadeSamples.NoteASharp4 => "rockers/rocker/NoteA#4",
                    Rockers.PremadeSamples.NoteA5 => "rockers/rocker/NoteA5",
                    Rockers.PremadeSamples.PracticeChordD => "rockers/rocker/PracticeChordD",
                    Rockers.PremadeSamples.Remix6ChordA => "rockers/rocker/Remix6ChordA",
                    Rockers.PremadeSamples.Remix10ChordD => "rockers/rocker/Remix10ChordD",
                    Rockers.PremadeSamples.Remix10ChordFSharpm => "rockers/rocker/Remix10ChordF#m",
                    Rockers.PremadeSamples.DoremiChordA7 => "rockers/doremi/ChordA7",
                    Rockers.PremadeSamples.DoremiChordAm7 => "rockers/doremi/ChordAm7",
                    Rockers.PremadeSamples.DoremiChordC => "rockers/doremi/ChordC",
                    Rockers.PremadeSamples.DoremiChordC7 => "rockers/doremi/ChordC7",
                    Rockers.PremadeSamples.DoremiChordCadd9 => "rockers/doremi/ChordCadd9",
                    Rockers.PremadeSamples.DoremiChordDm => "rockers/doremi/ChordDm",
                    Rockers.PremadeSamples.DoremiChordDm7 => "rockers/doremi/ChordDm7",
                    Rockers.PremadeSamples.DoremiChordEm => "rockers/doremi/ChordEm",
                    Rockers.PremadeSamples.DoremiChordF => "rockers/doremi/ChordF",
                    Rockers.PremadeSamples.DoremiChordFadd9 => "rockers/doremi/ChordFadd9",
                    Rockers.PremadeSamples.DoremiChordFm => "rockers/doremi/ChordFm",
                    Rockers.PremadeSamples.DoremiChordG => "rockers/doremi/ChordG",
                    Rockers.PremadeSamples.DoremiChordG7 => "rockers/doremi/ChordG7",
                    Rockers.PremadeSamples.DoremiChordGm => "rockers/doremi/ChordGm",
                    Rockers.PremadeSamples.DoremiChordGsus4 => "rockers/doremi/ChordGsus4",
                    Rockers.PremadeSamples.DoremiNoteA2 => "rockers/doremi/NoteA2",
                    Rockers.PremadeSamples.DoremiNoteE2 => "rockers/doremi/NoteE2",
                    _ => throw new System.NotImplementedException(),
                };
                chordSound = Jukebox.PlayOneShotGame(soundName, -1, pitch, 1, true);
            }

            if (together)
            {
                DoScaledAnimationAsync(jump ? "Jump" : "ComeOnStrum", 0.5f);
                if (disableStrumEffect) return;
                strumEffect.SetActive(true);
                bool strumLeft = JJ && jump;
                strumEffect.GetComponent<Animator>().Play(strumLeft ? "StrumStartLeft" : "StrumStartRIght", 0, 0);
            }
            else
            {
                DoScaledAnimationAsync("Strum", 0.5f);
                if (disableStrumEffect) return;
                strumEffect.SetActive(true);
                strumEffect.GetComponent<Animator>().Play("StrumStart", 0, 0);
            }

            if (!JJ)
            {
                if (barely)
                {
                    bool useYel = UnityEngine.Random.Range(1, 3) == 1;
                    for (int i = 0; i < 3; i++)
                    {
                        if (lightningRights[i].gameObject.activeSelf) lightningRights[i].sprite = useYel ? yelSprites[i] : bluSprites[i];
                        if (lightningLefts[i].gameObject.activeSelf) lightningLefts[i].sprite = useYel ? yelSprites[i] : bluSprites[i];
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (lightningRights[i].gameObject.activeSelf) lightningRights[i].sprite = normalSprites[i];
                        if (lightningLefts[i].gameObject.activeSelf) lightningLefts[i].sprite = normalSprites[i];
                    }
                }
            }
        }

        public void BendUp(int pitch)
        {
            if (bending || !strumming) return;
            bending = true;
            lastBendPitch = pitch;
            if (chordSound != null)
            {
                chordSound.BendUp(0.05f, Jukebox.GetPitchFromSemiTones(Jukebox.GetSemitonesFromPitch(chordSound.pitch) + pitch, true));
            }
            else
            {
                for (int i = 0; i < stringSounds.Length; i++)
                {
                    if (stringSounds[i] != null)
                    {
                        stringSounds[i].BendUp(0.05f, Jukebox.GetPitchFromSemiTones(Jukebox.GetSemitonesFromPitch(stringSounds[i].pitch) + pitch, true));
                    }
                }
            }

            Jukebox.PlayOneShotGame("rockers/bendUp");
            DoScaledAnimationAsync("Bend", 0.5f);
        }


        public void BendDown()
        {
            if (!bending) return;
            bending = false;
            foreach (var sound in stringSounds)
            {
                if (sound != null)
                {
                    sound.BendDown(0.05f);
                }
            }
            if (chordSound != null)
            {
                chordSound.BendDown(0.05f);
            }
            Jukebox.PlayOneShotGame("rockers/bendDown");
            DoScaledAnimationAsync("Unbend", 0.5f);
        }

        private float GetVolumeBasedOnAmountOfStrings(int stringAmount)
        {

            switch (stringAmount)
            {
                default:
                    return 1;
                case 3:
                    return 0.893f;
                case 4:
                    return 0.75f;
                case 5:
                    return 0.686f;
                case 6:
                    return 0.62f;
            }
        }

        public void Mute(bool soundExists = true, bool noAnim = false)
        {
            strumming = false;
            strumEffect.SetActive(false);
            bending = false;
            StopSounds();
            if (soundExists) Jukebox.PlayOneShotGame("rockers/mute");
            if (!noAnim) DoScaledAnimationAsync(together ? "ComeOnMute" : "Crouch", 0.5f);
            muted = true;
        }

        public void UnHold()
        {
            if (!muted) return;
            muted = false;
            if (!together) DoScaledAnimationAsync("UnCrouch", 0.5f);
        }

        private void DoScaledAnimationAsync(string name, float time)
        {
            anim.DoScaledAnimationAsync((JJ ? "JJ" : "") + name, time);
        }
    }
}

