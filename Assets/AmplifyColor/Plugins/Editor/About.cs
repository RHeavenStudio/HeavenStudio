// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmplifyColor
{
	public class About : EditorWindow
	{
		private const string AboutImagePath = "AmplifyColor/Textures/About.png";
		private Vector2 m_scrollPosition = Vector2.zero;
		private Texture2D m_aboutImage;

		[MenuItem( "Window/Amplify Color/About...", false, 20 )]
		static void Init()
		{
			About window = ( About ) EditorWindow.GetWindow( typeof( About ) , true , "About Amplify Color" );
			window.minSize = new Vector2( 502, 290 );
			window.maxSize = new Vector2( 502, 290 );
			window.Show();
		}

		public void OnFocus()
		{
			string[] guids = AssetDatabase.FindAssets( "About t:Texture" );
			string asset = "";

			foreach ( string guid in guids )
			{
				string path = AssetDatabase.GUIDToAssetPath( guid );
				if ( path.EndsWith( AboutImagePath ) )
				{
					asset = path;
					break;
				}
			}

			if ( !string.IsNullOrEmpty( asset ) )
			{
				TextureImporter importer = AssetImporter.GetAtPath( asset ) as TextureImporter;

				if ( importer.textureType != TextureImporterType.GUI )
				{
					importer.textureType = TextureImporterType.GUI;
					AssetDatabase.ImportAsset( asset );
				}

				m_aboutImage = AssetDatabase.LoadAssetAtPath( asset, typeof( Texture2D ) ) as Texture2D;
			}
			else
				Debug.LogWarning( "[AmplifyColor] About image not found at " + AboutImagePath );
		}

		public void OnGUI()
		{
			m_scrollPosition = GUILayout.BeginScrollView( m_scrollPosition );

			GUILayout.BeginVertical();

			GUILayout.Space( 10 );

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Box( m_aboutImage, GUIStyle.none );

			if ( Event.current.type == EventType.MouseUp && GUILayoutUtility.GetLastRect().Contains( Event.current.mousePosition ) )
				Application.OpenURL( "http://www.amplify.pt" );

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUIStyle labelStyle = new GUIStyle( EditorStyles.label );
			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.wordWrap = true;

			GUILayout.Label( "\nAmplify Color " + VersionInfo.StaticToString(), labelStyle, GUILayout.ExpandWidth( true ) );

			GUILayout.Label( "\nCopyright (c) Amplify Creations, Lda. All rights reserved.\n", labelStyle, GUILayout.ExpandWidth( true ) );

			GUILayout.EndVertical();

			GUILayout.EndScrollView();
		}
	}
}
