// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

#if UNITY_5_6_OR_NEWER
#define MANUAL_DETECT_SINGLE_PASS
#endif

using System;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.Rendering;
using UnityEngine.XR;
#elif UNITY_5_6_OR_NEWER
using UnityEngine.Rendering;
using UnityEngine.VR;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AmplifyColor
{
	[Serializable]
	public struct RenderLayer
	{
		public LayerMask mask;
		public Color color;

		public RenderLayer( LayerMask mask, Color color )
		{
			this.mask = mask;
			this.color = color;
		}
	}
}

[ExecuteInEditMode]
[RequireComponent( typeof( Camera ) )]
[AddComponentMenu( "" )]
public class AmplifyColorRenderMaskBase : MonoBehaviour
{
	[FormerlySerializedAs( "clearColor" )]
	public Color ClearColor = Color.black;

	[FormerlySerializedAs( "renderLayers" )]
	public AmplifyColor.RenderLayer[] RenderLayers = new AmplifyColor.RenderLayer[ 0 ];

	[FormerlySerializedAs( "debug" )]
	public bool DebugMask;

	private Camera referenceCamera;
	private Camera maskCamera;
	private AmplifyColorBase colorEffect;
	private int width, height;
	private RenderTexture maskTexture;
	private Shader colorMaskShader;
	private bool singlePassStereo = false;

	void OnEnable()
	{
		if ( maskCamera == null )
		{
			var go = new GameObject( "Mask Camera", typeof( Camera ) ) { hideFlags = HideFlags.HideAndDontSave };
			go.transform.parent = gameObject.transform;
			maskCamera = go.GetComponent<Camera>();
		}

		referenceCamera = GetComponent<Camera>();
		colorEffect = GetComponent<AmplifyColorBase>();

		colorMaskShader = Shader.Find( "Hidden/RenderMask" );
	}

	void OnDisable()
	{
		DestroyCamera();
		DestroyRenderTextures();
	}

	void DestroyCamera()
	{
		if ( maskCamera != null )
		{
			DestroyImmediate( maskCamera.gameObject );
			maskCamera = null;
		}
	}

	void DestroyRenderTextures()
	{
		if ( maskTexture != null )
		{
			RenderTexture.active = null;
			DestroyImmediate( maskTexture );
			maskTexture = null;
		}
	}

	void UpdateRenderTextures( bool singlePassStereo )
	{
		int width = referenceCamera.pixelWidth;
		int height = referenceCamera.pixelHeight;

		if ( maskTexture == null || this.width != width || this.height != height || !maskTexture.IsCreated() || this.singlePassStereo != singlePassStereo )
		{
			this.width = width;
			this.height = height;

			DestroyRenderTextures();

		#if UNITY_2017_2_OR_NEWER
			if ( XRSettings.enabled )
			{
				width = XRSettings.eyeTextureWidth * ( singlePassStereo ? 2 : 1 );
				height = XRSettings.eyeTextureHeight;
			}
		#endif

			if ( maskTexture == null )
			{
				maskTexture = new RenderTexture( width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB ) { hideFlags = HideFlags.HideAndDontSave, name = "MaskTexture" };
				maskTexture.name = "AmplifyColorMaskTexture";
				#if UNITY_5_6_OR_NEWER
					bool allowMSAA = maskCamera.allowMSAA;
				#else
					bool allowMSAA = true;
				#endif
				maskTexture.antiAliasing = ( allowMSAA && QualitySettings.antiAliasing > 0 ) ? QualitySettings.antiAliasing : 1;
			}

			maskTexture.Create();

			this.singlePassStereo = singlePassStereo;
		}

		if ( colorEffect != null )
		{
			colorEffect.MaskTexture = maskTexture;
		}
	}

	void UpdateCameraProperties()
	{
		maskCamera.CopyFrom( referenceCamera );
		maskCamera.targetTexture = maskTexture;
		maskCamera.clearFlags = CameraClearFlags.Nothing;
		maskCamera.renderingPath = RenderingPath.VertexLit;
		maskCamera.pixelRect = new Rect( 0, 0, width, height );
		maskCamera.depthTextureMode = DepthTextureMode.None;
	#if UNITY_5_6_OR_NEWER
		maskCamera.allowHDR = false;
	#else
		maskCamera.hdr = false;
	#endif
		maskCamera.enabled = false;
	}

	void OnPreRender()
	{
		if ( maskCamera != null )
		{
			RenderBuffer prevColor = Graphics.activeColorBuffer;
			RenderBuffer prevDepth = Graphics.activeDepthBuffer;

			// single pass not supported in RenderWithShader() as of Unity 2018.1; do multi-pass
			bool singlePassStereo = false;
		#if UNITY_2017_2_OR_NEWER
			if ( referenceCamera.stereoEnabled )
			{
				singlePassStereo = ( XRSettings.eyeTextureDesc.vrUsage == VRTextureUsage.TwoEyes );
				maskCamera.SetStereoViewMatrix( Camera.StereoscopicEye.Left, referenceCamera.GetStereoViewMatrix( Camera.StereoscopicEye.Left ) );
				maskCamera.SetStereoViewMatrix( Camera.StereoscopicEye.Right, referenceCamera.GetStereoViewMatrix( Camera.StereoscopicEye.Right ) );
				maskCamera.SetStereoProjectionMatrix( Camera.StereoscopicEye.Left, referenceCamera.GetStereoProjectionMatrix( Camera.StereoscopicEye.Left ) );
				maskCamera.SetStereoProjectionMatrix( Camera.StereoscopicEye.Right, referenceCamera.GetStereoProjectionMatrix( Camera.StereoscopicEye.Right ) );
			}
		#endif

			UpdateRenderTextures( singlePassStereo );
			UpdateCameraProperties();

			Graphics.SetRenderTarget( maskTexture );
			GL.Clear( true, true, ClearColor );

		#if UNITY_2017_2_OR_NEWER
			if ( singlePassStereo )
			{
				maskCamera.worldToCameraMatrix = referenceCamera.GetStereoViewMatrix( Camera.StereoscopicEye.Left );
				maskCamera.projectionMatrix = referenceCamera.GetStereoProjectionMatrix( Camera.StereoscopicEye.Left );
				maskCamera.rect = new Rect( 0, 0, 0.5f, 1 );
			}
		#endif

			foreach ( var layer in RenderLayers )
			{
				Shader.SetGlobalColor( "_COLORMASK_Color", layer.color );
				maskCamera.cullingMask = layer.mask;
				maskCamera.RenderWithShader( colorMaskShader, "RenderType" );
			}

		#if UNITY_2017_2_OR_NEWER
			if ( singlePassStereo )
			{
				maskCamera.worldToCameraMatrix = referenceCamera.GetStereoViewMatrix( Camera.StereoscopicEye.Right );
				maskCamera.projectionMatrix = referenceCamera.GetStereoProjectionMatrix( Camera.StereoscopicEye.Right );
				maskCamera.rect = new Rect( 0.5f, 0, 0.5f, 1 );

				foreach ( var layer in RenderLayers )
				{
					Shader.SetGlobalColor( "_COLORMASK_Color", layer.color );
					maskCamera.cullingMask = layer.mask;
					maskCamera.RenderWithShader( colorMaskShader, "RenderType" );
				}
			}
		#endif

			Graphics.SetRenderTarget( prevColor, prevDepth );
		}
	}

#if UNITY_EDITOR
	void OnGUI()
	{
		if ( DebugMask )
		{
			GUI.DrawTexture( new Rect( 0, 0, Screen.width, Screen.height ), maskTexture );
		}
	}
#endif
}
