using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AutoLetterbox;

namespace AutoLetterbox
{
    /* ForceCameraRatio.cs
     *
     * Forces the assigned Cameras to a given Aspect Ratio by Letterboxing them horizontally or vertically
     *
     * 2015
     * Written by Tom Elliott */

    // A class for tracking individual Cameras and their Viewports
    [System.Serializable]
    public class CameraRatio
    {
        public enum CameraAnchor
        {
            Center,
            Top,
            Bottom,
            Left,
            Right,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        [Tooltip("The Camera assigned to have an automatically calculated Viewport Ratio")]
        public Camera camera;
        [Tooltip("When a Camera Viewport is shrunk to fit a ratio, it will anchor the new Viewport Rectangle at the given point (relative to the original, unshrunk Viewport)")]
        public CameraAnchor anchor = CameraAnchor.Center;

        [HideInInspector]
        public Vector2 vectorAnchor;
        private Rect originViewPort;

        public CameraRatio (Camera _camera, Vector2 _anchor) {
            camera = _camera;
            vectorAnchor = _anchor;
            originViewPort = camera.rect;
        }

        /// <summary>
        /// Sets the Camera's current Viewport as the viewport measurements to fill on resizing
        /// </summary>
        public void ResetOriginViewport () {
            originViewPort = camera.rect;
            SetAnchorBasedOnEnum(anchor);
        }

        /// <summary>
        /// Sets the Anchor for this Camera when it is resized based on a given enum description
        /// </summary>
        /// <param name="_anchor"></param>
        public void SetAnchorBasedOnEnum (CameraAnchor _anchor) {
            switch (_anchor) {
                case CameraAnchor.Center:
                    vectorAnchor = new Vector2(0.5f, 0.5f);
                    break;
                case CameraAnchor.Top:
                    vectorAnchor = new Vector2(0.5f, 1f);
                    break;
                case CameraAnchor.Bottom:
                    vectorAnchor = new Vector2(0.5f, 0f);
                    break;
                case CameraAnchor.Left:
                    vectorAnchor = new Vector2(0f, 0.5f);
                    break;
                case CameraAnchor.Right:
                    vectorAnchor = new Vector2(1f, 0.5f);
                    break;
                case CameraAnchor.TopLeft:
                    vectorAnchor = new Vector2(0f, 1f);
                    break;
                case CameraAnchor.TopRight:
                    vectorAnchor = new Vector2(1f, 1f);
                    break;
                case CameraAnchor.BottomLeft:
                    vectorAnchor = new Vector2(0f, 0f);
                    break;
                case CameraAnchor.BottomRight:
                    vectorAnchor = new Vector2(1f, 0f);
                    break;
            }
        }

        /// <summary>
        /// Forces a camera to render at a given ratio
        /// Creates a letter box effect if the new viewport does not match the current Window ratio
        /// </summary>
        /// <param name="_targetAspect"></param>
        /// <param name="_currentAspect"></param>
        public void CalculateAndSetCameraRatio (float _width, float _height, bool _horizontalLetterbox) {

            Rect localViewPort = new Rect();

            // Force the viewport to a width and height accurate to the target ratio
            if (_horizontalLetterbox) { // current aspect is wider than target aspect so shorten down height of the viewport
                localViewPort.height = _height;
                localViewPort.width = 1;

            } else { // current aspect is taller than target aspect so thin down width of the viewport
                localViewPort.height = 1f;
                localViewPort.width = _width;
            }

            // Resize and position the viewport to fit in it's original position on screen (adhering to a given anchor point)
            Rect screenViewPortHorizontal = new Rect();
            Rect screenViewPortVertical = new Rect();

            // Calculate both a horizontally and vertically resized viewport
            screenViewPortHorizontal.width = originViewPort.width;
            screenViewPortHorizontal.height = originViewPort.width * (localViewPort.height / localViewPort.width);
            screenViewPortHorizontal.x = originViewPort.x;
            screenViewPortHorizontal.y = Mathf.Lerp(originViewPort.y, originViewPort.y + (originViewPort.height - screenViewPortHorizontal.height), vectorAnchor.y);

            screenViewPortVertical.width = originViewPort.height * (localViewPort.width / localViewPort.height);
            screenViewPortVertical.height = originViewPort.height;
            screenViewPortVertical.x = Mathf.Lerp(originViewPort.x, originViewPort.x + (originViewPort.width - screenViewPortVertical.width), vectorAnchor.x);
            screenViewPortVertical.y = originViewPort.y;

            // Use the best fitting of the two
            if (screenViewPortHorizontal.height >= screenViewPortVertical.height && screenViewPortHorizontal.width >= screenViewPortVertical.width) {
                if (screenViewPortHorizontal.height <= originViewPort.height && screenViewPortHorizontal.width <= originViewPort.width) {
                    camera.rect = screenViewPortHorizontal;
                } else {
                    camera.rect = screenViewPortVertical;
                }
            } else {
                if (screenViewPortVertical.height <= originViewPort.height && screenViewPortVertical.width <= originViewPort.width) {
                    camera.rect = screenViewPortVertical;
                } else {
                    camera.rect = screenViewPortHorizontal;
                }
            }
        }
    }

    // A class for tracking all cameras in a scene
    [System.Serializable]
    public class ForceCameraRatio : MonoBehaviour
    {
        public Vector2 ratio = new Vector2(16, 9);
        public bool forceRatioOnAwake = true;
        public bool listenForWindowChanges = true;
        public bool createCameraForLetterBoxRendering = true;
        public bool findCamerasAutomatically = true;
        public Color letterBoxCameraColor = new Color(0, 0, 0, 1);

        public List<CameraRatio> cameras;

        public Camera letterBoxCamera;

        private void Start () {
            // If no cameras have been assigned in editor, search for cameras in the scene
            if (findCamerasAutomatically) {
                FindAllCamerasInScene();
            } else if (cameras == null || cameras.Count == 0) {
                cameras = new List<CameraRatio>();
            }

            ValidateCameraArray();

            // Set the origin viewport space for each Camera
            for (int i = 0; i < cameras.Count; i++) {
                cameras[i].ResetOriginViewport();
            }

            // If requested, a Camera will be generated that renders a letter box Color
            if (createCameraForLetterBoxRendering) {
                letterBoxCamera = new GameObject().AddComponent<Camera>();
                letterBoxCamera.backgroundColor = letterBoxCameraColor;
                letterBoxCamera.cullingMask = 0;
                letterBoxCamera.depth = -100;
                letterBoxCamera.farClipPlane = 1;
                letterBoxCamera.useOcclusionCulling = false;
                letterBoxCamera.allowHDR = false;
                letterBoxCamera.clearFlags = CameraClearFlags.Color;
                letterBoxCamera.name = "Letter Box Camera";

                for (int i = 0; i < cameras.Count; i++) {
                    if (cameras[i].camera.depth == -100) {
                        Debug.LogError(cameras[i].camera.name + " has a depth of -100 and may conflict with the Letter Box Camera in Forced Camera Ratio!");
                    }
                }
            }

            if (forceRatioOnAwake) {
                CalculateAndSetAllCameraRatios();
            }
        }

        private void Update () {
            if (listenForWindowChanges) {
                // Recalculate the viewport size if the window size has changed
                CalculateAndSetAllCameraRatios();
                if (letterBoxCamera != null) {
                    letterBoxCamera.backgroundColor = letterBoxCameraColor;
                }
            }
        }

        /// <summary>
        /// Returns the container class for a Camera and it's ratio by the _camera it contains
        /// Returns null if the given _camera is not being tracked
        /// </summary>
        /// <param name="_camera"></param>
        /// <returns></returns>
        private CameraRatio GetCameraRatioByCamera (Camera _camera) {
            if (cameras == null) {
                return null;
            }

            for (int i = 0; i < cameras.Count; i++) {
                if (cameras[i] != null && cameras[i].camera == _camera) {
                    return cameras[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Removes any null elements from the CameraRatio Array
        /// </summary>
        private void ValidateCameraArray() {
            for (int i = cameras.Count - 1; i >= 0; i--) {
                if (cameras[i].camera == null) {
                    cameras.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Populates the tracked Camera Array with every Camera currently in the scene
        /// </summary>
        public void FindAllCamerasInScene () {
            Camera[] allCameras = FindObjectsOfType<Camera>();
            cameras = new List<CameraRatio>();

            for (int i = 0; i < allCameras.Length; i++) {
                if ((createCameraForLetterBoxRendering || allCameras[i] != letterBoxCamera)) { // Ignore the Custom LetterBox Camera
                    cameras.Add(new CameraRatio(allCameras[i], new Vector2(0.5f, 0.5f)));
                }
            }
        }

        /// <summary>
        /// Loops through all cameras in scene (or that have been set in editor)
        /// Forces each camera to render at a given ratio
        /// Creates a letter box effect if the new viewport does not match the current Window ratio
        /// </summary>
        public void CalculateAndSetAllCameraRatios () {
            float targetAspect = ratio.x / ratio.y;
            float currentAspect = ((float)Screen.width) / ((float)Screen.height);

            bool horizontalLetterbox = false;
            float fullWidth = targetAspect / currentAspect;
            float fullHeight = currentAspect / targetAspect;

            if (currentAspect > targetAspect) {
                horizontalLetterbox = false;
            }

            for (int i = 0; i < cameras.Count; i++) {
                cameras[i].SetAnchorBasedOnEnum(cameras[i].anchor);
                cameras[i].CalculateAndSetCameraRatio(fullWidth, fullHeight, horizontalLetterbox);
            }
        }

        /// <summary>
        /// Set the anchor for a given Camera
        /// </summary>
        /// <param name="_camera"></param>
        /// <param name="_anchor"></param>
        public void SetCameraAnchor (Camera _camera, Vector2 _anchor) {
            CameraRatio camera = GetCameraRatioByCamera(_camera);
            if (camera != null) {
                camera.vectorAnchor = _anchor;
            }
        }

        public CameraRatio[] GetCameras () {
            if (cameras == null) {
                cameras = new List<CameraRatio>();
            }
            return cameras.ToArray();
        }
    }
}