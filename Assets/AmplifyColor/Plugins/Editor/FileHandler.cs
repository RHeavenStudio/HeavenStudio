// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System.IO;
using UnityEngine;
using UnityEditor;

namespace AmplifyColor
{
	public class FileHandler
	{
		private readonly BackBufferHandler _backBufferHandler;
		private readonly LUTWriter _lutWriter;
		private readonly ToolSettings _settings;
		private readonly CameraCollection _cameraCollection;

		public FileHandler( BackBufferHandler backBufferHandler, LUTWriter lutWriter, ToolSettings settings, CameraCollection cameraCollection )
		{
			_backBufferHandler = backBufferHandler;
			_lutWriter = lutWriter;
			_settings = settings;
			_cameraCollection = cameraCollection;
		}

		public void SaveToFile()
		{
			ImageResult imageResult;
			if ( _backBufferHandler.ReadBackBuffer( out imageResult ) )
			{
				var path = EditorUtility.SaveFilePanel( "Save reference file", _settings.FilePath ?? "", "reference", "png" );
				if ( !string.IsNullOrEmpty( path ) )
				{
					_settings.FilePath = path;

					if ( _settings.AddLut )
					{
						Texture2D luttexture = _cameraCollection.GetCurrentEffectTexture();

						if ( _settings.ApplyColorGrading && luttexture != null )
						{
							if ( !imageResult.AddLUTFromTexture( _settings.LUT, luttexture ) )
							{
								_settings.Message = "Couldnt add the LUT to the image. Try changing the lut size or settings";

								return;
							}
						}
						else
						{
							if ( !imageResult.AddLUT( _settings.LUT ) )
							{
								_settings.Message = "Couldnt add the LUT to the image. Try changing the lut size or settings";

								return;
							}
						}
					}

					var texture = imageResult.GenerateTexture2D();

					if ( texture != null )
					{
						File.WriteAllBytes( _settings.FilePath, texture.EncodeToPNG() );
					};

					Texture2D.DestroyImmediate( texture );
				}
			}
			else
			{
				_settings.Message = "No camera selected";
			}
		}

		public void ReadFromFile()
		{
			var path = EditorUtility.OpenFilePanel( "Load graded file", _settings.FilePath ?? "", "png" );
			if ( !string.IsNullOrEmpty( path ) )
			{
				_settings.FilePath = path;

				if ( File.Exists( _settings.FilePath ) )
				{
					var data = File.ReadAllBytes( _settings.FilePath );

					var screenshottexture = new Texture2D( 16, 16, TextureFormat.ARGB32, false );
					screenshottexture.LoadImage( data );

					var imageResult = ImageResult.FromTexture( screenshottexture );

					if ( imageResult != null )
					{
						LUTResult lutResult = imageResult.GetLUT( _settings.LUT );

						if ( lutResult != null )
						{
							_lutWriter.SaveLUT( lutResult );
						}

						lutResult.Release();
					}

					Texture2D.DestroyImmediate( screenshottexture );
				}
			}
		}

		public void Reload()
		{
			if ( string.IsNullOrEmpty( _settings.FilePath ) )
			{
				ReadFromFile();
				return;
			}

			if ( File.Exists( _settings.FilePath ) )
			{
				var data = File.ReadAllBytes( _settings.FilePath );

				var screenshottexture = new Texture2D( 16, 16, TextureFormat.ARGB32, false );
				screenshottexture.LoadImage( data );

				var imageResult = ImageResult.FromTexture( screenshottexture );

				if ( imageResult != null )
				{
					LUTResult lutResult = imageResult.GetLUT( _settings.LUT );

					if ( lutResult != null )
					{
						_lutWriter.SaveLUT( lutResult );
					}

					lutResult.Release();
				}

				Texture2D.DestroyImmediate( screenshottexture );
			}
			else
			{
				if ( EditorUtility.DisplayDialog( "File doesnt exist", "Target file doesn't exit. Please select a new one.", "ok", "cancel" ) )
				{
					ReadFromFile();
					return;
				}
			}
		}
	}
}
