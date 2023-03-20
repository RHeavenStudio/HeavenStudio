// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Net.Sockets;

namespace AmplifyColor
{
	public class PhotoshopHandler
	{
		private readonly PhotoshopProtocol _photoshopProtocol;
		private readonly BackBufferHandler _backBufferHandler;
		private readonly LUTWriter _lutWriter;
		private readonly ToolSettings _settings;
		private readonly CameraCollection _cameraCollection;

		public PhotoshopHandler( PhotoshopProtocol photoshopProtocol, BackBufferHandler backBufferHandler, LUTWriter lutWriter, ToolSettings settings, CameraCollection cameraCollection )
		{
			_photoshopProtocol = photoshopProtocol;
			_backBufferHandler = backBufferHandler;
			_lutWriter = lutWriter;
			_settings = settings;
			_cameraCollection = cameraCollection;
		}

		public void SendToPhotoshop()
		{
			ImageResult imageResult;

			if ( _backBufferHandler.ReadBackBuffer( out imageResult ) )
			{
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

				if ( SendImage( imageResult ) )
				{
					_photoshopProtocol.SendJSCommand( "app.activeDocument.bitsPerChannel = BitsPerChannelType.SIXTEEN;" +
								"takeSnapshot();" +
								" function takeSnapshot ()" +
								" { var desc = new ActionDescriptor();" +
								" var sref = new ActionReference(); sref.putClass(charIDToTypeID(\"SnpS\"));" +
								"desc.putReference(charIDToTypeID(\"null\"), sref);" +
								"var fref = new ActionReference();" +
								" fref.putProperty(charIDToTypeID(\"HstS\")," +
								" charIDToTypeID(\"CrnH\")); " +
								"desc.putReference(charIDToTypeID(\"From\"), fref );" +
								" executeAction(charIDToTypeID(\"Mk  \"), desc, DialogModes.NO );} " );

					ToolSettings.Instance.Message = "Sent";
				}
				else
				{
					ToolSettings.Instance.Message = "Error sending the image to Photoshop";
				}
			}
			else
			{
				_settings.Message = "No camera selected";
			}
		}

		public void ReadFromPhotoshopTools()
		{
			LUTResult lutResult;

			if ( ReadLUT( out lutResult ) )
			{
				_lutWriter.SaveLUT( lutResult );
				lutResult.Release();
			}
		}

		public bool ReadLUT( out LUTResult lutResult )
		{
			_photoshopProtocol.SendJSCommand( "takeSnapshot();" +
												 " function takeSnapshot ()" +
												 " { var desc = new ActionDescriptor();" +
												 " var sref = new ActionReference(); sref.putClass(charIDToTypeID(\"SnpS\"));" +
												 "desc.putReference(charIDToTypeID(\"null\"), sref);" +
												 "var fref = new ActionReference();" +
												 " fref.putProperty(charIDToTypeID(\"HstS\")," +
												 " charIDToTypeID(\"CrnH\")); " +
												 "desc.putReference(charIDToTypeID(\"From\"), fref );" +
												 " executeAction(charIDToTypeID(\"Mk  \"), desc, DialogModes.NO );} " );

			var rulerunits = _photoshopProtocol.SendJSCommand( "app.preferences.rulerUnits;" );
			_photoshopProtocol.SendJSCommand( "app.preferences.rulerUnits = Units.PIXELS;" );
			_photoshopProtocol.SendJSCommand( string.Format( "app.activeDocument.crop(new Array(0,0,{0},{1}), 0, {0}, {1})", ToolSettings.Instance.LUT.Width, ToolSettings.Instance.LUT.Height ) );

			ImageResult imageData;
			_photoshopProtocol.ReceiveImage( "", out imageData );

			_photoshopProtocol.SendJSCommand( "revertToLastSnapshot(); " +
												 "function revertToLastSnapshot() " +
												 "{ var docRef = app.activeDocument; " +
												 "var hsObj = docRef.historyStates; " +
												 "var hsLength = hsObj.length; " +
												 "for (var i=hsLength - 1;i>-1;i--) { " +
												 "if (hsObj[i].snapshot) { " +
												 "docRef.activeHistoryState = docRef.historyStates[i]; break; } } }" );

			_photoshopProtocol.SendJSCommand( string.Format( "app.preferences.rulerUnits = {0};", rulerunits ) );

			lutResult = null;

			if ( imageData != null )
			{
				lutResult = imageData.GetLUT( _settings.LUT );
				ToolSettings.Instance.Message = "Done.";
				return true;
			}

			ToolSettings.Instance.Message = "Error reading LUT from Photoshop Image.";
			return false;
		}

		public bool SendImage( ImageResult imageResult )
		{
			if ( imageResult == null )
			{
				return false;
			}

			_photoshopProtocol.SendImage( imageResult );

			return true;
		}
	}
}
