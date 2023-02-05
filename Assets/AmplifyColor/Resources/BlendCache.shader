// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/Amplify Color/BlendCache"
{
	Properties
	{
		_MainTex ( "Base (RGB)", Any ) = "" {}
		_RgbTex ( "LUT (RGB)", 2D ) = "" {}
		_LerpRgbTex ( "LerpRGB (RGB)", 2D ) = "" {}
	}

	Subshader
	{
		ZTest Always Cull Off ZWrite Off Blend Off Fog { Mode off }

		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "Common.cginc"

				float4 frag( v2f i ) : SV_Target
				{
					float4 lut1 = tex2D( _RgbTex, i.uv01.xy );
					float4 lut2 = tex2D( _LerpRgbTex, i.uv01.xy );
					return lerp( lut1, lut2, _LerpAmount );
				}
			ENDCG
		}
	}

	Fallback Off
}
