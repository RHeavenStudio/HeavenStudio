Shader "UI/ButtonsSDFShader"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
        _OutColor ("Outline Colour", Color) = (1,1,1,1)
        _Thickness("Thickness", Range(0, 1)) = 0.5
        _Smoothness("Smoothness", Range(0, 1)) = 0.2
        _OutThickness("Outline Thickness", Range(0, 1)) = 0.0
        _OutSmoothness("Outline Smoothness", Range(0, 1)) = 0.1
        _OffBright ("Off Brightness", Range(0, 1)) = 0.5
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
		_ClipRect ("Clip Rect", vector) = (-32767, -32767, 32767, 32767)

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
			};
			
			sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _OutColor;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _OffBright;
            float _Thickness;
            float _Smoothness;
            float _OutThickness;
            float _OutSmoothness;
 
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
 
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
 
                OUT.color = v.color * _Color;
                return OUT;
            }
 
            fixed4 frag(v2f IN) : SV_Target
            {
                fixed a = tex2D(_MainTex, IN.texcoord).g;
                fixed b = tex2D(_MainTex, IN.texcoord).r;
                fixed o = smoothstep(_OutThickness - _OutSmoothness, _OutThickness + _OutSmoothness, b);
                half4 color = _TextureSampleAdd + IN.color;
                color.a = smoothstep(1.0 - _Thickness - _Smoothness, 1.0 - _Thickness + _Smoothness, b);
                color.rgb = lerp(_OutColor.rgb, color.rgb * min(a + _OffBright, 1.0), o);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
 
                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif
 
                return color;
            }
		ENDCG
		}
	}
}