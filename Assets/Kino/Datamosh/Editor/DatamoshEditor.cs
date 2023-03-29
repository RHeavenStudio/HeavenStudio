//
// Kino/Datamosh - Glitch effect simulating video compression artifacts
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
using UnityEditor;

namespace Kino
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Datamosh))]
    public class DatamoshEditor : Editor
    {
        SerializedProperty _blockSize;
        SerializedProperty _entropy;
        SerializedProperty _noiseContrast;
        SerializedProperty _velocityScale;
        SerializedProperty _diffusion;

        void OnEnable()
        {
            _blockSize = serializedObject.FindProperty("_blockSize");
            _entropy = serializedObject.FindProperty("_entropy");
            _noiseContrast = serializedObject.FindProperty("_noiseContrast");
            _velocityScale = serializedObject.FindProperty("_velocityScale");
            _diffusion = serializedObject.FindProperty("_diffusion");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_blockSize);
            EditorGUILayout.PropertyField(_entropy);
            EditorGUILayout.PropertyField(_noiseContrast);
            EditorGUILayout.PropertyField(_velocityScale);
            EditorGUILayout.PropertyField(_diffusion);

            serializedObject.ApplyModifiedProperties();

            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);

            if (GUILayout.Button("Glitch!"))
                foreach (Datamosh d in targets) d.Glitch(); 

            if (GUILayout.Button("Clear"))
                foreach (Datamosh d in targets) d.Reset();

            EditorGUI.EndDisabledGroup ();
        }
    }
}
