using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using HeavenStudio;
using HeavenStudio.Editor;
using HeavenStudio.Editor.Track;
using TMPro;

public class VolumeDialog : Dialog
{
    VolumeTimelineObj volumeObj;

    [SerializeField] Button deleteButton;
    [SerializeField] Slider volumeSlider;
    [SerializeField] TMP_InputField volumeInput;

    public void SwitchVolumeDialog()
    {
        if (dialog.activeSelf)
        {
            volumeObj = null;
            dialog.SetActive(false);
            Editor.instance.inAuthorativeMenu = false;
        }
        else
        {
            Editor.instance.inAuthorativeMenu = true;
            ResetAllDialogs();
            dialog.SetActive(true);

            volumeSlider.maxValue = 100;
            volumeSlider.minValue = 0;
        }
    }

    void Update()
    {
        if (volumeObj != null && volumeObj.first)
        {
            volumeInput.text = volumeObj.chartEntity["volume"].ToString("F");
        }
    }

    public void SetVolumeObj(VolumeTimelineObj volumeObj)
    {
        this.volumeObj = volumeObj;
        deleteButton.gameObject.SetActive(!volumeObj.first);

        volumeSlider.value = volumeObj.chartEntity["volume"];
        volumeInput.text = volumeObj.chartEntity["volume"].ToString("F");
    }

    public void DeleteVolume()
    {
        if (volumeObj != null)
        {
            volumeObj.Remove();
        }
        if (dialog.activeSelf)
        {
            SwitchVolumeDialog();
        }
    }

    public void VolumeSliderUpdate()
    {
        if (volumeObj != null)
        {
            volumeObj.SetVolume(volumeSlider.value);
            volumeInput.text = volumeSlider.value.ToString("F");
        }
    }

    public void SetVolume()
    {
        if (volumeObj != null)
        {
            float volume = float.Parse(volumeInput.text);
            volumeObj.SetVolume(volume);
            volumeSlider.value = volume;
        }
    }
}
