// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AmplifyColor
{
	public class BackBufferHandler
	{
		private readonly CameraCollection _cameras;

		public BackBufferHandler( CameraCollection cameras )
		{
			_cameras = cameras;
		}

		public bool ReadBackBuffer( out ImageResult imageResult )
		{
			imageResult = null;

			if ( _cameras == null )
			{
				Debug.LogError( "[AmplifyColor] Camera collection is invalid." );
				return false;
			}

			var camera = _cameras.SelectedCamera;

			if ( camera == null )
			{
				Debug.LogError( "[AmplifyColor] Selected camera is invalid." );
				return false;
			}

            AmplifyColorBase component = camera.GetComponent<AmplifyColorBase>();
			Tonemapping prevTonemapper = Tonemapping.Disabled;
			float prevExposure = 1.0f;
			float prevLinearWhitePoint = 11.2f;
			bool prevApplyDithering = false;
			float prevBlendAmount = 0.0f;
			Texture prevLUT = null;

			if ( component != null )
			{
				prevTonemapper = component.Tonemapper;
				prevExposure = component.Exposure;
				prevLinearWhitePoint = component.LinearWhitePoint;
				prevApplyDithering = component.ApplyDithering;
				prevBlendAmount = component.BlendAmount;
				prevLUT = component.LutTexture;

				component.Tonemapper = ToolSettings.Instance.ApplyHDRControl ? component.Tonemapper : Tonemapping.Disabled;
				component.Exposure = ToolSettings.Instance.ApplyHDRControl ? component.Exposure : 1.0f;
				component.LinearWhitePoint = ToolSettings.Instance.ApplyHDRControl ? component.LinearWhitePoint : 11.2f;
				component.ApplyDithering = ToolSettings.Instance.ApplyHDRControl ? component.ApplyDithering : false;
				component.BlendAmount = ToolSettings.Instance.ApplyColorGrading ? component.BlendAmount : 0.0f;
				component.LutTexture = ToolSettings.Instance.ApplyColorGrading ? component.LutTexture : null;
			}

			var width = ToolSettings.Instance.Resolution.TargetWidth;
			var height = ToolSettings.Instance.Resolution.TargetHeight;

			//if (ToolSettings.Instance.Resolution.IsGameWindowSize)
			//{
			//    width = Screen.width;
			//    height = Screen.height;
			//}

			var cameratarget = camera.targetTexture;

			var rt = RenderTexture.GetTemporary( width, height, 24, RenderTextureFormat.ARGB32 );
			camera.targetTexture = rt;
			camera.Render();
			camera.targetTexture = cameratarget;

			var activert = RenderTexture.active;
			RenderTexture.active = rt;
			var text = new Texture2D( width, height, TextureFormat.ARGB32, false );
			text.ReadPixels( new Rect( 0, 0, width, height ), 0, 0 );
			text.Apply();
			RenderTexture.active = activert;
			var colors = text.GetPixels( 0, 0, width, height );
			Texture2D.DestroyImmediate( text );

			var colordata = new Color[ width, height ];

			for ( int i = height - 1; i >= 0; i-- )
			{
				for ( int j = 0; j < width; j++ )
				{
					colordata[ j, ( height - 1 - i ) ] = colors[ i * width + j ];
					colordata[ j, ( height - 1 - i ) ].a = 1;
				}
			}

			if ( component != null )
			{
				component.Tonemapper = prevTonemapper;
				component.Exposure = prevExposure;
				component.LinearWhitePoint = prevLinearWhitePoint;
				component.ApplyDithering = prevApplyDithering;
				component.BlendAmount = prevBlendAmount;
				component.LutTexture = prevLUT;
			}

			imageResult = new ImageResult( colordata );

			return true;
		}
	}
}
