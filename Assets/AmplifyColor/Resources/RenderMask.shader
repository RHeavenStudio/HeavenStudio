// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/RenderMask"
{
	Properties
	{
		_MainTex ("", 2D) = "white" {}
		_Cutoff ("", Float) = 0.5
		_COLORMASK_Color ("", Color) = (1,1,1,1)
	}

	CGINCLUDE
		#pragma multi_compile _ PIXELSNAP_ON
		#pragma multi_compile _ STEREO_INSTANCING_ON
		#pragma multi_compile _ UNITY_SINGLE_PASS_STEREO
		#include "UnityCG.cginc"
		#include "Common.cginc"

		fixed _Cutoff;
		fixed4 _COLORMASK_Color;

		struct v2f_mask
		{
			float4 pos  : SV_POSITION;
			float2 uv : TEXCOORD0;
		#if UNITY_VERSION >= 550
			UNITY_VERTEX_OUTPUT_STEREO
		#endif
		};

		v2f_mask vert( appdata_base v )
		{
			v2f_mask o;
		#if UNITY_VERSION >= 550
			UNITY_SETUP_INSTANCE_ID( v );
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
		#endif
			o.pos = CustomObjectToClipPos( v.vertex );
			o.uv = TRANSFORM_TEX( v.texcoord, _MainTex );
		#ifdef PIXELSNAP_ON
			o.pos = UnityPixelSnap( o.pos );
		#endif
			return o;
		}
	ENDCG

	SubShader
	{
		Tags { "Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" }
		Blend Off Lighting Off Fog { Mode Off  }
		ColorMask RGB
		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				fixed4 frag( v2f_mask i ) : SV_Target
				{
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );
					return _COLORMASK_Color;
				}
			ENDCG
		}
	}
	SubShader
	{
		Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
		Blend Off Lighting Off Fog { Mode Off  }
		ColorMask RGB
		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				fixed4 frag( v2f_mask i ) : SV_Target
				{
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );
					clip( UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, i.uv ).a - _Cutoff );
					return _COLORMASK_Color;
				}
			ENDCG
		}
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off ZWrite Off Lighting Off Fog { Mode Off  }
		ColorMask RGB

		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				fixed4 frag( v2f_mask i ) : SV_Target
				{
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );
					return fixed4( _COLORMASK_Color.rgb, UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, i.uv ).a );
				}
            ENDCG
		}
	}
}
