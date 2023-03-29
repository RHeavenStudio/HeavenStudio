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
using UnityEditor;

namespace Kino
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Feedback))]
    public class FeedbackEditor : Editor
    {
        SerializedProperty _color;
        SerializedProperty _offsetX;
        SerializedProperty _offsetY;
        SerializedProperty _rotation;
        SerializedProperty _scale;
        SerializedProperty _jaggies;

        void OnEnable()
        {
            _color = serializedObject.FindProperty("_color");
            _offsetX = serializedObject.FindProperty("_offsetX");
            _offsetY = serializedObject.FindProperty("_offsetY");
            _rotation = serializedObject.FindProperty("_rotation");
            _scale = serializedObject.FindProperty("_scale");
            _jaggies = serializedObject.FindProperty("_jaggies");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_color);
            EditorGUILayout.PropertyField(_offsetX);
            EditorGUILayout.PropertyField(_offsetY);
            EditorGUILayout.PropertyField(_rotation);
            EditorGUILayout.PropertyField(_scale);
            EditorGUILayout.PropertyField(_jaggies);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
