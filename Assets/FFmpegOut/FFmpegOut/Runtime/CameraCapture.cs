// FFmpegOut - FFmpeg video encoding plugin for Unity
// https://github.com/keijiro/KlakNDI

using UnityEngine;
using System.Collections;

namespace FFmpegOut
{
    // This is a modified CameraCapture
    [AddComponentMenu("FFmpegOut/Camera Capture")]
    public sealed class CameraCapture : MonoBehaviour
    {
        #region Public properties

        public delegate void FramePush(int frameCount, float delta);
        public event FramePush OnFramePush;
        public delegate void CreateTexture();
        public event CreateTexture OnCreateTexture;

        public string outputDir;

        [SerializeField] bool _recordAudio = false;

        public bool recordAudio {
            get { return _recordAudio; }
            set { _recordAudio = value; }
        }

        [SerializeField] int _width = 1920;

        public int width {
            get { return _width; }
            set { _width = value; }
        }

        [SerializeField] int _height = 1080;

        public int height {
            get { return _height; }
            set { _height = value; }
        }

        [SerializeField] FFmpegPreset _preset;

        public FFmpegPreset preset {
            get { return _preset; }
            set { _preset = value; }
        }

        [SerializeField] float _frameRate = 60;

        public float frameRate {
            get { return _frameRate; }
            set { _frameRate = value; }
        }

        public RenderTexture tempRT;

        #endregion

        #region Private members

        FFmpegSession _session;
        GameObject _blitter;

        RenderTextureFormat GetTargetFormat(Camera camera)
        {
            return camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        }

        int GetAntiAliasingLevel(Camera camera)
        {
            return camera.allowMSAA ? QualitySettings.antiAliasing : 1;
        }

        #endregion

        #region Time-keeping variables

        public int _frameCount;
        float _startTime;
        int _frameDropCount;

        float FrameTime {
            get { return _startTime + (_frameCount - 0.5f) / _frameRate; }
        }

        void WarnFrameDrop()
        {
            if (++_frameDropCount != 10) return;

            Debug.LogWarning(
                "Significant frame droppping was detected. This may introduce " +
                "time instability into output video. Decreasing the recording " +
                "frame rate is recommended."
            );
        }

        #endregion

        #region MonoBehaviour implementation

        void OnValidate()
        {
            _width = Mathf.Max(8, _width);
            _height = Mathf.Max(8, _height);
        }

        void OnDisable()
        {
            if (_session != null)
            {
                // Close and dispose the FFmpeg session.
                _session.Close();
                _session.Dispose();
                _session = null;
            }

            if (tempRT != null)
            {
                // Dispose the frame texture.
                GetComponent<Camera>().targetTexture = null;
                Destroy(tempRT);
                tempRT = null;
            }

            if (_blitter != null)
            {
                // Destroy the blitter game object.
                Destroy(_blitter);
                _blitter = null;
            }
        }

        void OnAudioFilterRead(float[] buffer, int channels) {
            if (_session == null || !_session.recordAudio) return;
            _session.PushAudioBuffer(buffer, channels);
        }


        IEnumerator Start()
        {
            // Sync with FFmpeg pipe thread at the end of every frame.
            for (var eof = new WaitForEndOfFrame();;)
            {
                yield return eof;
                _session?.CompletePushFrames();
            }
        }

        void Update()
        {
            var camera = GetComponent<Camera>();

            // Lazy initialization
            if (_session == null)
            {
                // // Give a newly created temporary render texture to the camera
                // // if it's set to render to a screen. Also create a blitter
                // // object to keep frames presented on the screen.
                // tempRT = new RenderTexture(_width, _height, 24, GetTargetFormat(camera));
                // tempRT.antiAliasing = GetAntiAliasingLevel(camera);
                // camera.targetTexture = tempRT;
                _blitter = Blitter.CreateInstance(camera);

                OnCreateTexture?.Invoke();

                // Start an FFmpeg session.
                _session = FFmpegSession.Create(
                    outputDir,
                    camera.targetTexture.width,
                    camera.targetTexture.height,
                    _frameRate, preset, recordAudio
                );

                _startTime = Time.time;
                _frameCount = 0;
                _frameDropCount = 0;
            }

            var gap = Time.time - FrameTime;
            var delta = 1 / _frameRate;

            if (gap < 0)
            {
                // Update without frame data.
                _session.PushFrame(null);

                OnFramePush?.Invoke(_frameCount, delta);
            }
            else if (gap < delta)
            {
                // Single-frame behind from the current time:
                // Push the current frame to FFmpeg.
                _session.PushFrame(camera.targetTexture);
                _frameCount++;

                OnFramePush?.Invoke(_frameCount, delta);
            }
            else if (gap < delta * 2)
            {
                // Two-frame behind from the current time:
                // Push the current frame twice to FFmpeg. Actually this is not
                // an efficient way to catch up. We should think about
                // implementing frame duplication in a more proper way. #fixme
                _session.PushFrame(camera.targetTexture);
                _session.PushFrame(camera.targetTexture);
                _frameCount += 2;

                OnFramePush?.Invoke(_frameCount, delta);
            }
            else
            {
                // Show a warning message about the situation.
                WarnFrameDrop();

                // Push the current frame to FFmpeg.
                _session.PushFrame(camera.targetTexture);

                // Compensate the time delay.
                _frameCount += Mathf.FloorToInt(gap * _frameRate);

                OnFramePush?.Invoke(_frameCount, delta);
            }
        }

        #endregion
    }
}
