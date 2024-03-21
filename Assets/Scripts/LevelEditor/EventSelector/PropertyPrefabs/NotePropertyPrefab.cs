using System;
using System.Collections;
using System.Collections.Generic;
using HeavenStudio;
using HeavenStudio.Editor;
using HeavenStudio.Util;
using TMPro;
using UnityEngine;

public class NotePropertyPrefab : NumberPropertyPrefab
{
    public TMP_Text noteLabel;
    
    private Sound previewAudioSource;

    public override void SetProperties(string propertyName, object type, string caption)
    {
        base.SetProperties(propertyName, type, caption);

        EntityTypes.Note note = (EntityTypes.Note)type;

        slider.minValue = note.min;
        slider.maxValue = note.max;
        _defaultValue = note.val;

        slider.wholeNumbers = true;

        int offsetFromC = 3 - note.sampleNote;

        slider.value = Convert.ToSingle(parameterManager.entity[propertyName]) - offsetFromC;
        inputField.text = slider.value.ToString();
        noteLabel.text = GetNoteText(note, (int)slider.value + offsetFromC);

        slider.onValueChanged.AddListener(
            _ =>
            {
                int trueSemitones = (int)slider.value + offsetFromC;
                inputField.text = slider.value.ToString();
                parameterManager.entity[propertyName] = trueSemitones;
                if (slider.value != _defaultValue)
                {
                    this.caption.text = _captionText + "*";
                }
                else
                {
                    this.caption.text = _captionText;
                }

                noteLabel.text = GetNoteText(note, trueSemitones);
                
                PlayPreview(note, trueSemitones);
            }
        );

        inputField.onSelect.AddListener(
            _ =>
                Editor.instance.editingInputField = true
        );

        inputField.onEndEdit.AddListener(
            _ =>
            {
                int trueSemitones = (int)slider.value + offsetFromC;

                slider.value = Convert.ToSingle(inputField.text);
                parameterManager.entity[propertyName] = trueSemitones;
                Editor.instance.editingInputField = false;
                if (slider.value != _defaultValue)
                {
                    this.caption.text = _captionText + "*";
                }
                else
                {
                    this.caption.text = _captionText;
                }

                noteLabel.text = GetNoteText(note, trueSemitones);
                
                PlayPreview(note, trueSemitones);
            }
        );
    }

    private void PlayPreview(EntityTypes.Note note, int currentSemitones)
    {
        if (note.sampleName.Equals("")) return;
        
        if(previewAudioSource) previewAudioSource.Stop();
        
        float pitch = SoundByte.GetPitchFromSemiTones(currentSemitones, true);
        previewAudioSource = SoundByte.PlayOneShotGame(note.sampleName, pitch: pitch, volume: 0.75f, forcePlay: true, ignoreConductorPause: true);
    }

    private static readonly string[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };

    private static string GetNoteText(EntityTypes.Note note, int currentSemitones)
    {
        int noteIndex = (note.sampleNote + currentSemitones) % 12;
        if (noteIndex < 0)
        {
            noteIndex += 12;
        }

        int octaveOffset = (note.sampleNote + currentSemitones) / 12;
        int octave = note.sampleOctave + octaveOffset;

        if ((note.sampleNote + currentSemitones) % 12 < 0)
        {
            octave--;
        }

        return notes[noteIndex] + octave;
    }
}
