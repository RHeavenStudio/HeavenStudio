//
// Kino/Feedback - framebuffer feedback effect for Unity
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using UnityEngine.Rendering;

namespace Kino
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class Feedback : MonoBehaviour
    {
        #region Editable properties

        /// Feedback color
        public Color color {
            get { return _color; }
            set { _color = value; }
        }

        [SerializeField]
        Color _color = Color.white;

        /// Horizontal offset for feedback
        public float offsetX {
            get { return _offsetX; }
            set { _offsetX = value; }
        }

        [SerializeField, Range(-1, 1)]
        float _offsetX = 0;

        /// Vertical offset for feedback
        public float offsetY {
            get { return _offsetY; }
            set { _offsetY = value; }
        }

        [SerializeField, Range(-1, 1)]
        float _offsetY = 0;

        /// Center-axis rotation for feedback
        public float rotation {
            get { return _rotation; }
            set { _rotation = value; }
        }

        [SerializeField, Range(-5, 5)]
        float _rotation = 0;

        /// Scale factor for feedback
        public float scale {
            get { return _scale; }
            set { _scale = value; }
        }

        [SerializeField, Range(0.95f, 1.05f)]
        float _scale = 1;

        /// Disables bilinear filter
        public bool jaggies {
            get { return _jaggies; }
            set { _jaggies = value; }
        }

        [SerializeField]
        bool _jaggies = false;

        #endregion

        #region Private members

        [SerializeField] Shader _shader;
        [SerializeField] Mesh _mesh;

        Material _material;
        RenderTexture _delayBuffer;
        CommandBuffer _command;

        // 2D rotation matrix
        Vector4 rotationMatrixAsVector {
            get {
                var angle = -Mathf.Deg2Rad * _rotation;
                var sin = Mathf.Sin(angle);
                var cos = Mathf.Cos(angle);
                return new Vector4(cos, sin, -sin, cos);
            }
        }

        // Initialize the delay buffer and the feedback command.
        void StartFeedback()
        {
            var camera = GetComponent<Camera>();

            // Initialize the delay buffer.
            _delayBuffer = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight);
            _delayBuffer.wrapMode = TextureWrapMode.Clamp;
            _material.SetTexture("_MainTex", _delayBuffer);

            // Create a feedback command and attach it to the camera.
            if (_command == null) {
                _command = new CommandBuffer();
                _command.name = "Kino.Feedback";
            }
            _command.Clear();
            _command.DrawMesh(_mesh, Matrix4x4.identity, _material, 0, 0);
            camera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, _command);
        }

        #endregion

        #region MonoBehaviour Functions

        void OnEnable()
        {
            // Initialize the shader and the temporary material.
            if (_material == null) {
                _material = new Material(Shader.Find("Hidden/Kino/Feedback"));
                _material.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        void OnDisable()
        {
            // Do nothing if it hasn't been initialized.
            if (_delayBuffer == null) return;

            // Release the delay buffer.
            RenderTexture.ReleaseTemporary(_delayBuffer);
            _delayBuffer = null;

            // Detach the command buffer from the camera.
            var camera = GetComponent<Camera>();
            camera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, _command);
        }

        void OnDestroy()
        {
            if (Application.isPlaying)
                Destroy(_material);
            else
                DestroyImmediate(_material);
        }

        void Update()
        {
            // Do nothing if it hasn't been initialized.
            if (_delayBuffer == null) return;

            // Release temporary objects when the screen resolution is changed.
            var camera = GetComponent<Camera>();
            if (camera.pixelWidth != _delayBuffer.width ||
                camera.pixelHeight != _delayBuffer.height)
            {
                OnDisable();
                return;
            }

            // Update the shader/texture properties.
            _material.SetColor("_Color", _color);
            _material.SetVector("_Offset", new Vector2(_offsetX, _offsetY) * -0.05f);
            _material.SetVector("_Rotation", rotationMatrixAsVector);
            _material.SetFloat("_Scale", 2 - _scale);
            _delayBuffer.filterMode = _jaggies ? FilterMode.Point : FilterMode.Bilinear;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // Lazy initialization of the delay buffer.
            if (_delayBuffer == null) StartFeedback();

            // Copy the content of the frame to the delay buffer.
            Graphics.Blit(source, _delayBuffer);
            Graphics.Blit(source, destination);
        }

        #endregion
    }
}
