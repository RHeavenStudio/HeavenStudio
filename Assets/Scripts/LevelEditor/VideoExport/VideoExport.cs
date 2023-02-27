using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFmpegOut;
using UnityEngine.UI;
using HeavenStudio.Editor.Track;
using UnityEngine.UI.Extensions;
using TMPro;
using System;
using Starpelly;
using SFB;
using HeavenStudio.Util;
using System.Linq;

namespace HeavenStudio.Editor.VideoExport
{
    // !!!PLEASE NOTE!!! ========================================================
    // This is blueprint code, meant to be expanded and improved upon in the future.
    // There are some bad practices here, right now Video Export is in the "make it work" phase.
    // https://i.imgur.com/RsVetUg.png
    // ==========================================================================

    public class VideoExport : MonoBehaviour
    {

        public enum ExportFormat
        {
            H264Default = 0,
            H264Nvidia = 1
        }

        public FFmpegPreset ExportFormatToPreset(int index)
        {
            switch (index)
            {
                case 0:
                    return FFmpegPreset.H264Default;
                case 1:
                    return FFmpegPreset.H264Nvidia;
                default:
                    Debug.LogError("Unknown format?");
                    return FFmpegPreset.H264Default;
            }
        }

        [SerializeField] private RawImage exportPreview;
        [SerializeField] private RangeSlider exportRangeSlider;
        [SerializeField] private TMP_Text minText, maxText;

        [Header("Rendering")]
        [SerializeField] private TMP_Text progressPercentage;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private GameObject renderProgressHolder;

        [Header("Properties")]
        [SerializeField] Toggle exportAudio;
        [SerializeField] TMP_Dropdown formatDropdown;
        [SerializeField] TMP_InputField widthInput;
        [SerializeField] TMP_InputField heightInput;
        [SerializeField] TMP_InputField fpsInput;


        #region Private

        private CameraCapture gameCameraCapture;
        private FrameRateController frameRateController;

        private int format => formatDropdown.value;
        private int width => int.Parse(widthInput.text);
        private int height => int.Parse(heightInput.text);
        private int fps => int.Parse(fpsInput.text);

        private float startExportTime => exportRangeSlider.LowValue;
        private float endExportTime => exportRangeSlider.HighValue;
        private DynamicBeatmap.DynamicEntity endEvent;

        private bool isExporting;

        #endregion

        #region MonoBehaviour
        private void Start()
        {
            Tooltip.AddTooltip(exportAudio.transform.parent.parent.gameObject, "<color=red>Not Implemented</color>\n¯\\_(:/)_/¯");
            Tooltip.AddTooltip(fpsInput.transform.parent.parent.gameObject, "Above 60 not recommended");

            var exportFormatNames = Enum.GetNames(typeof(ExportFormat));
            formatDropdown.ClearOptions();
            formatDropdown.AddOptions(exportFormatNames.ToList());
            formatDropdown.value = 0;
        }

        public void Open(DynamicBeatmap.DynamicEntity endEvent)
        {
            this.endEvent = endEvent;
            exportPreview.texture = GameCamera.instance.camera.activeTexture;
            GameManager.instance.CircleCursor.gameObject.SetActive(false);

            if (endEvent != null)
            {
                var maxRange = (float)Conductor.instance.GetSongPosFromBeat(endEvent.beat);
                exportRangeSlider.MaxValue = maxRange;
                exportRangeSlider.HighValue = maxRange;
            }
        }

        private void Update()
        {
            minText.text = TimeSpan.FromSeconds(exportRangeSlider.LowValue).ToString(@"hh\:mm\:ss\:fff");
            maxText.text = TimeSpan.FromSeconds(exportRangeSlider.HighValue).ToString(@"hh\:mm\:ss\:fff");
            renderProgressHolder.SetActive(isExporting);
        }

        public void Close()
        {
            GameManager.instance.CircleCursor.gameObject.SetActive(true);
        }

        public void ExportVideo()
        {
            var extensionFilter = new ExtensionFilter();
            switch (format)
            {
                case 0:
                case 1:
                    extensionFilter = new ExtensionFilter("Video Files", "mp4");
                    break;
            }

            var extensions = new[]
            {
                extensionFilter
            };

            StandaloneFileBrowser.SaveFilePanelAsync("Export Video As", "", "rhexport", extensions, (string path) =>
            {
                if (path != string.Empty)
                {
                    gameCameraCapture = GameCamera.instance.gameObject.AddComponent<CameraCapture>();
                    gameCameraCapture.outputDir = path;
                    gameCameraCapture.preset = ExportFormatToPreset(format);
                    gameCameraCapture.width = width;
                    gameCameraCapture.height = height;
                    gameCameraCapture.frameRate = fps;
                    gameCameraCapture.OnCreateTexture += delegate { exportPreview.texture = GameCamera.instance.camera.activeTexture; };

                    frameRateController = GameCamera.instance.gameObject.AddComponent<FrameRateController>();

                    GameManager.instance.Play(startExportTime, false);
                    isExporting = true;
                    GameManager.instance.SortEventsList();
                    Conductor.instance.isPlaying = true;
                    Conductor.instance.ignoreConductorPlaying = true;

                    gameCameraCapture.OnFramePush += GameCameraCapture_OnFramePush;
                }
            });
        }

        private void GameCameraCapture_OnFramePush(int frameCount, float delta)
        {
            Conductor.instance.songPosition = ((frameCount / (fps / 60f)) / gameCameraCapture.frameRate) + startExportTime - GameManager.instance.Beatmap.firstBeatOffset;
            Conductor.instance.songPositionInBeats = (float)Conductor.instance.GetBeatFromSongPos(Conductor.instance.songPositionAsDouble);

            progressSlider.value = Mathp.Normalize(Conductor.instance.songPosition, startExportTime, endExportTime);
            progressPercentage.text = $"{Mathf.Floor(progressSlider.value * 100f)}%";

            if (Conductor.instance.songPosition >= endExportTime)
            {
                Cancel();
                Jukebox.PlayOneShot("games/mrUpbeat/applause");
            }
        }

        public void Cancel()
        {
            isExporting = false;

            Conductor.instance.isPlaying = false;
            Conductor.instance.ignoreConductorPlaying = false;
            Conductor.instance.songPosition = startExportTime;
            Conductor.instance.songPositionInBeats = (float)Conductor.instance.GetBeatFromSongPos(startExportTime);

            Destroy(frameRateController);
            Destroy(gameCameraCapture);

            GameManager.instance.SetCurrentEventToClosest(Conductor.instance.songPositionInBeats);

            // Editor.instance.SetRenderTextures(Editor.instance.ScreenRenderTexture);
            exportPreview.texture = GameCamera.instance.camera.activeTexture;
        }

        private void OnDisable()
        {
            Cancel();
        }

        private void OnDestroy()
        {
            Cancel();
        }

        #endregion
    }
}

