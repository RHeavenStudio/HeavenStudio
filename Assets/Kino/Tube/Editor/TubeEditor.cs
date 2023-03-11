// KinoTube - Old TV/video artifacts simulation
// https://github.com/keijiro/KinoTube

using UnityEngine;
using UnityEditor;

namespace Kino
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Tube))]
    public class TubeEditor : Editor
    {
        SerializedProperty _bleeding;
        SerializedProperty _fringing;
        SerializedProperty _scanline;

        void OnEnable()
        {
            _bleeding = serializedObject.FindProperty("_bleeding");
            _fringing = serializedObject.FindProperty("_fringing");
            _scanline = serializedObject.FindProperty("_scanline");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_bleeding);
            EditorGUILayout.PropertyField(_fringing);
            EditorGUILayout.PropertyField(_scanline);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
