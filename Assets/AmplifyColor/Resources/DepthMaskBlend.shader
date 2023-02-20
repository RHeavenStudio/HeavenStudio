// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/Amplify Color/DepthMaskBlend"
{
	Properties
	{
		_MainTex ( "Base (RGB)", Any ) = "" {}
		_RgbTex ( "LUT (RGB)", 2D ) = "" {}
		_MaskTex ( "Mask (RGB)", Any ) = "" {}
		_RgbBlendCacheTex ( "RgbBlendCache (RGB)", 2D ) = "" {}
		_DepthCurveLut ( "Depth Curve LUT (RGB)", 2D ) = "" {}
	}

	CGINCLUDE
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0

		#include "Common.cginc"

		inline float4 apply_grading( v2f i, float4 color, const bool mobile )
		{
			float depth = Linear01Depth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv01Stereo.zw ) );
			float mask = tex2D( _DepthCurveLut, depth.xx ).a;
			return lerp( color, apply_blend( color, mobile ), mask );
		}

		inline float4 frag_ldr_gamma( v2f i, const bool mobile )
		{
			init_frag( i );
			float4 color = fetch_process_ldr_gamma( i, mobile );
			color = apply_grading( i, color, mobile );
			return output_ldr_gamma( color );
		}

		inline float4 frag_ldr_linear( v2f i, const bool mobile )
		{
			init_frag( i );
			float4 color = fetch_process_ldr_linear( i, mobile );
			color = apply_grading( i, color, mobile );
			return output_ldr_linear( color );
		}

		inline float4 frag_hdr_gamma( v2f i, bool mobile, const bool dithering, const int tonemapper )
		{
			init_frag( i );
			float4 color = fetch_process_hdr_gamma( i, mobile, dithering, tonemapper );
			color = apply_grading( i, color, mobile );
			return output_hdr_gamma( color );
		}

		inline float4 frag_hdr_linear( v2f i, bool mobile, const bool dithering, const int tonemapper )
		{
			init_frag( i );
			float4 color = fetch_process_hdr_linear( i, mobile, dithering, tonemapper );
			color = apply_grading( i, color, mobile );
			return output_hdr_linear( color );
		}
	ENDCG

	Subshader
	{
		ZTest Always Cull Off ZWrite Off Blend Off Fog { Mode off }

		// -- QUALITY NORMAL --------------------------------------------------------------
		// 0 => LDR GAMMA
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_ldr_gamma( i, false ); } ENDCG }

		// 1 => LDR LINEAR
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_ldr_linear( i, false ); } ENDCG }

		// 2-5 => HDR GAMMA / DITHERING: OFF
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_gamma( i, false, false, TONEMAPPING_DISABLED ); } ENDCG }
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_gamma( i, false, false, TONEMAPPING_PHOTO ); } ENDCG }
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_gamma( i, false, false, TONEMAPPING_HABLE ); } ENDCG }
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_gamma( i, false, false, TONEMAPPING_ACES ); } ENDCG }

		// 6-9 => HDR GAMMA / DITHERING: ON
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_gamma( i, false, true, TONEMAPPING_DISABLED ); } ENDCG }
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_gamma( i, false, true, TONEMAPPING_PHOTO ); } ENDCG }
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_gamma( i, false, true, TONEMAPPING_HABLE ); } ENDCG }
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_gamma( i, false, true, TONEMAPPING_ACES ); } ENDCG }

		// 10-13 => HDR LINEAR / DITHERING: OFF
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_linear( i, false, false, TONEMAPPING_DISABLED ); } ENDCG }
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_linear( i, false, false, TONEMAPPING_PHOTO ); } ENDCG }
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_linear( i, false, false, TONEMAPPING_HABLE ); } ENDCG }
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_linear( i, false, false, TONEMAPPING_ACES ); } ENDCG }

		// 14-17 => HDR LINEAR / DITHERING: ON
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_linear( i, false, true, TONEMAPPING_DISABLED ); } ENDCG }
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_linear( i, false, true, TONEMAPPING_PHOTO ); } ENDCG }
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_linear( i, false, true, TONEMAPPING_HABLE ); } ENDCG }
		Pass{ CGPROGRAM float4 frag( v2f i ) : SV_Target{ return frag_hdr_linear( i, false, true, TONEMAPPING_ACES ); } ENDCG }

		// -- QUALITY MOBILE --------------------------------------------------------------
		// 18 => LDR GAMMA
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_ldr_gamma( i, true ); } ENDCG }

		// 19 => LDR LINEAR
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_ldr_linear( i, true ); } ENDCG }

		// 20-23 => HDR GAMMA / DITHERING: OFF
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_gamma( i, true, false, TONEMAPPING_DISABLED ); } ENDCG }
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_gamma( i, true, false, TONEMAPPING_PHOTO ); } ENDCG }
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_gamma( i, true, false, TONEMAPPING_HABLE ); } ENDCG }
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_gamma( i, true, false, TONEMAPPING_ACES ); } ENDCG }

		// 24-27 => HDR GAMMA / DITHERING: ON
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_gamma( i, true, true, TONEMAPPING_DISABLED ); } ENDCG }
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_gamma( i, true, true, TONEMAPPING_PHOTO ); } ENDCG }
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_gamma( i, true, true, TONEMAPPING_HABLE ); } ENDCG }
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_gamma( i, true, true, TONEMAPPING_ACES ); } ENDCG }

		// 28-31 => HDR LINEAR / DITHERING: OFF
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_linear( i, true, false, TONEMAPPING_DISABLED ); } ENDCG }
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_linear( i, true, false, TONEMAPPING_PHOTO ); } ENDCG }
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_linear( i, true, false, TONEMAPPING_HABLE ); } ENDCG }
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_linear( i, true, false, TONEMAPPING_ACES ); } ENDCG }

		// 32-35 => HDR LINEAR / DITHERING: ON
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_linear( i, true, true, TONEMAPPING_DISABLED ); } ENDCG }
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_linear( i, true, true, TONEMAPPING_PHOTO ); } ENDCG }
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_linear( i, true, true, TONEMAPPING_HABLE ); } ENDCG }
		Pass { CGPROGRAM float4 frag( v2f i ) : SV_Target { return frag_hdr_linear( i, true, true, TONEMAPPING_ACES ); } ENDCG }
	}

	Fallback Off
}
