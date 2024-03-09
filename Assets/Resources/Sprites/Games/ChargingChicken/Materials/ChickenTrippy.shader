Shader "Sprites/ChickenTrippy"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_GradientMap ("Color Map", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Speed ("Color Rotate Speed", Float) = 1
		_Speed1 ("Wobble Speed", Float) = 1
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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

		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
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
				float2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			sampler2D _GradientMap;
			float _Speed;
			float _Speed1;
			float _AlphaSplitEnabled;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

				#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
				#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 input1 = SampleSpriteTexture (IN.texcoord);
				fixed2 wobble = (IN.texcoord + (sin(_Time.y * _Speed1) / 50)) % 1;
				wobble.y = ((wobble.y + (sin((_Time.y * _Speed1) + wobble.x * 5) / 20) + 1)) % 1;
				fixed4 input2 = SampleSpriteTexture (wobble);
				fixed4 input = ((input1 * 2) + (input2 * 3)) / 5;
				float grayscale = (input.r + input.g + input.b) / 3;
				float grayscaleScrolled = (grayscale + ((_Speed / 4) * _Time.y)) % 1;
				fixed4 c = tex2D (_GradientMap, grayscaleScrolled);
				c *= _Color;
				c.a *= input.a;

				return c;
			}
		ENDCG
		}
	}
}
