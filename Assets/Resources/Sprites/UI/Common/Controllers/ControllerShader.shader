// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/ControllerShader"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
        _BodyColor ("Body Colour", Color) = (1,1,1,1)
        _BtnColor ("Button Colour", Color) = (1,1,1,1)
        _LGripColor ("Left Grip Colour", Color) = (1,1,1,1)
        _RGripColor ("Right Grip Colour", Color) = (1,1,1,1)
		
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
			
			sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _BodyColor;
			fixed4 _BtnColor;
			fixed4 _LGripColor;
			fixed4 _RGripColor;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
 
            struct v2f
			{
				half2 texcoord  : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float3 localPos : TEXCOORD2;
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
			};
			
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				float4 scaleVertex = float4(IN.vertex.xyz, 0); //By setting the last value to 0 it ignores the flipping ( loses relative position if sprite is flipped :( )
				float4 wP = mul(unity_ObjectToWorld, scaleVertex); //Get the object to world vertex and store it
				OUT.worldPos = wP.xyz; //For use in fragment shader
				float4 lP = mul(unity_WorldToObject, scaleVertex); //Get the world to object vertex and store it
				OUT.localPos = lP.xyz; //For use in fragment shader
				
				OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
				OUT.color = IN.color;
				
				return OUT;
			}
 
            fixed4 frag(v2f IN) : SV_Target
            {
				fixed4 t =  tex2D(_MainTex, IN.texcoord)*IN.color;
				//Calculate relative position
				fixed2 relativeWorld = fixed2(IN.worldPos.x + IN.localPos.x, IN.worldPos.y + IN.localPos.y);
			
				//This becomes the UV for the texture I want to apply to the sprite ( using the sprites width and height )
				fixed2 relativePos = fixed2((relativeWorld.x + _MainTex_TexelSize.z), (relativeWorld.y + _MainTex_TexelSize.w));

                fixed r = tex2D(_MainTex, IN.texcoord).r;
                fixed g = tex2D(_MainTex, IN.texcoord).g;
				fixed b = tex2D(_MainTex, IN.texcoord).b;
				fixed lg = 0.0;
				fixed rg = 0.0;
				if (relativePos.x <= 0.5)
				{
					lg = b;
				}
				else
				{
					rg = b;
				}

				half4 color = _TextureSampleAdd + IN.color;
                color.rgb = (r * _BodyColor.rgb) + (g * _BtnColor.rgb) + (lg * _LGripColor.rgb) + (rg * _RGripColor.rgb);
				color.a = tex2D(_MainTex, IN.texcoord).a;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPos.xy, _ClipRect);
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