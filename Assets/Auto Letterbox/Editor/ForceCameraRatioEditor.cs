using UnityEngine;
using UnityEditor;
using System.Collections;
using AutoLetterbox;

namespace AutoLetterbox
{

    /* ForceCameraRatioEditor.cs
     *
     * An Editor script for ForceCameraRatio
     *
     * Originally made by Milo Keeble, source can be found on Github @ https://github.com/Ragepyro/EditorFileMaker */

    [CustomEditor(typeof(ForceCameraRatio))]
    public class ForceCameraRatioEditor : Editor
    {
        /// <summary>
        /// Create a ForceCameraRatio Object in the current scene
        /// </summary>
        [MenuItem("GameObject/Create Force Camera Ratios Object", false, 11)]
        public static void ForceCameraRatios () {
            ForceCameraRatio myCameraForcer = new GameObject("Force Camera Ratios").AddComponent<ForceCameraRatio>();
            Selection.activeGameObject = myCameraForcer.gameObject;
            Undo.RegisterCreatedObjectUndo(myCameraForcer.gameObject, "Created A ForceCameraRatios Manager Obejct");
        }

        /// <summary>
        /// Display an editor friendly interface for the ForceCameraRatio Component
        /// </summary>
        public override void OnInspectorGUI () {

            ForceCameraRatio myTarget = (ForceCameraRatio)target;     
            Undo.RecordObject(myTarget, "Force Camera Ratio");

            myTarget.ratio = EditorGUILayout.Vector2Field(new GUIContent("Target Viewport Ratio", "The ratio that the Letterbox will display at"), myTarget.ratio);
            myTarget.forceRatioOnAwake = EditorGUILayout.Toggle(new GUIContent("Ratio on Awake", "Enable the Letterbox effect automatically on Awake"), myTarget.forceRatioOnAwake);
            myTarget.listenForWindowChanges = EditorGUILayout.Toggle(new GUIContent("Ratio in Realtime", "Recalculate the Letterbox effect every time the game window is resized"), myTarget.listenForWindowChanges);

            EditorGUILayout.Separator();

            // Letterbox Camera
            myTarget.createCameraForLetterBoxRendering = EditorGUILayout.Toggle(new GUIContent("Create Border Camera", "Generate a Camera which renders the Letterbox borders"), myTarget.createCameraForLetterBoxRendering);
            if (myTarget.createCameraForLetterBoxRendering) {
                myTarget.letterBoxCameraColor = EditorGUILayout.ColorField(new GUIContent("Border Color", "The color of the Letterbox borders"), myTarget.letterBoxCameraColor);
            } else {
                myTarget.letterBoxCamera = (Camera)EditorGUILayout.ObjectField("Letterbox Camera", myTarget.letterBoxCamera, typeof(Camera), true);
                if (myTarget.letterBoxCamera == null) {
                    EditorGUILayout.HelpBox("Without a Letterbox Border Camera, things may render but never clear in the letterbox borders. A generated Border Camera would occupy a Camera depth of -100", MessageType.Warning);
                }
            }

            EditorGUILayout.Separator();

            // Registered Cameras for ratio calculation
            myTarget.findCamerasAutomatically = EditorGUILayout.Toggle(new GUIContent("Auto seek Cameras", "If true, will automatically find all cameras in scene on Awake"), myTarget.findCamerasAutomatically);

            if (myTarget.findCamerasAutomatically) {
                EditorGUILayout.HelpBox("Any Cameras that exist in the scene when this script Awakes will be forced to the given aspect ratio", MessageType.Info);
            } else {

                if (GUILayout.Button("Find All Cameras in scene")) {
                    myTarget.FindAllCamerasInScene();
                }

                // If there is no array of cameras (as in null, empty arrays don't count) create one with the cameras in scene
                if (myTarget.GetCameras() == null) {
                    myTarget.FindAllCamerasInScene();
                }

                // If ther is STILL no array of cameras, just create an empty one
                if (myTarget.GetCameras() == null) {
                    myTarget.cameras = new System.Collections.Generic.List<CameraRatio>();
                }

                // Display Array of Cameras
                SerializedObject serialObj = new SerializedObject(myTarget);
                SerializedProperty cams = serialObj.FindProperty("cameras");
                EditorGUILayout.PropertyField(cams, true);
                serialObj.ApplyModifiedProperties();

                EditorGUILayout.HelpBox("Only the Cameras in this array will be forced to the given aspect ratio", MessageType.Info);
            }
        }

    }
}