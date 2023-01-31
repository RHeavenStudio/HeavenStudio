// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System.IO;
using UnityEditor;
using UnityEngine;

namespace AmplifyColor
{
	public class LUTWriter
	{
		private string _texturePath;
		private bool _overwrite = false;

		public string TexturePath
		{
			get { return _texturePath; }
			set { _texturePath = value; }
		}

		public bool Overwrite
		{
			get { return _overwrite; }
			set { _overwrite = value; }
		}

		public void SaveLUT( LUTResult lutResult )
		{
			if ( lutResult == null )
			{
				ToolSettings.Instance.Message = "Error while reading LUT data.";
				return;
			}

			var assetpath = _texturePath;

			bool justBrowsed = false;

			if ( string.IsNullOrEmpty( assetpath ) )
			{
				if ( EditorUtility.DisplayDialog( "Browse?", "There is no current path to save the file to.", "Browse", "Cancel" ) )
				{
					var path = EditorUtility.SaveFilePanelInProject( "Save as", Path.GetFileName( _texturePath ), "png", "Please enter a file name to save the texture to" );

					justBrowsed = true;

					if ( string.IsNullOrEmpty( path ) )
					{
						return;
					}

					_texturePath = path;
				}
				else
				{
					return;
				}
			}

			if ( File.Exists( _texturePath ) && !justBrowsed && !_overwrite )
			{
				if ( !EditorUtility.DisplayDialog( "Overwrite?", "File already exists. This action will overwrite the current file. Do you want to continue?", "Overwrite", "Cancel" ) )
					return;
			}

			File.WriteAllBytes( _texturePath, lutResult.Texture.EncodeToPNG() );
			AssetDatabase.Refresh();
			var text = AssetDatabase.LoadAssetAtPath( _texturePath, typeof( Texture2D ) ) as Texture2D;
			if ( text != null )
			{
				text.wrapMode = TextureWrapMode.Clamp;
				text.filterMode = FilterMode.Bilinear;
			}

			TextureImporter tImporter = AssetImporter.GetAtPath( _texturePath ) as TextureImporter;
			if ( tImporter != null )
			{
				tImporter.mipmapEnabled = false;
				tImporter.isReadable = false;

				tImporter.filterMode = FilterMode.Bilinear;
				tImporter.anisoLevel = 0;

			#if UNITY_5_6_OR_NEWER
				tImporter.textureType = TextureImporterType.Default;
				tImporter.textureCompression = TextureImporterCompression.Uncompressed;
				tImporter.sRGBTexture = false;
			#else
				tImporter.textureType = TextureImporterType.Advanced;
				tImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				tImporter.linearTexture = true;
			#endif
				tImporter.wrapMode = TextureWrapMode.Clamp;
				tImporter.maxTextureSize = 1024;
				AssetDatabase.ImportAsset( _texturePath, ImportAssetOptions.ForceUpdate );
			}
		}
	}
}
