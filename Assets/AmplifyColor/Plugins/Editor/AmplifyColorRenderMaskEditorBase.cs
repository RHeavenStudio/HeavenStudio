// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AmplifyColor
{
	[CustomPropertyDrawer( typeof( RenderLayer ) )]
	public class AmplifyColorRenderLayerDrawer : PropertyDrawer
	{
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			EditorGUI.BeginProperty( position, label, property );

			position = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label );

			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			float halfWidth = position.width / 2.0f - 4.0f;
			Rect maskRect = new Rect( position.x, position.y, halfWidth, position.height );
			Rect colorRect = new Rect( position.x + halfWidth + 4.0f, position.y, halfWidth, position.height );

			EditorGUI.PropertyField( maskRect, property.FindPropertyRelative( "mask" ), GUIContent.none );
			EditorGUI.PropertyField( colorRect, property.FindPropertyRelative( "color" ), GUIContent.none );

			EditorGUI.indentLevel = indent;
			EditorGUI.EndProperty();
		}
	}
}

public class AmplifyColorRenderMaskEditorBase : Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		var renderColorMask = target as AmplifyColorRenderMaskBase;
		int length = renderColorMask.RenderLayers.Length;

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.PropertyField( serializedObject.FindProperty( "ClearColor" ) );
		EditorGUILayout.PropertyField( serializedObject.FindProperty( "RenderLayers" ), true );
		EditorGUILayout.PropertyField( serializedObject.FindProperty( "DebugMask" ) );

		if ( EditorGUI.EndChangeCheck() )
		{
			serializedObject.ApplyModifiedProperties();

			if ( renderColorMask.RenderLayers.Length > length )
			{
				for ( int i = length; i <  renderColorMask.RenderLayers.Length; i++ )
				{
					renderColorMask.RenderLayers[ i ] = new AmplifyColor.RenderLayer( 0, Color.white );
				}
			}
		}
	}
 }
