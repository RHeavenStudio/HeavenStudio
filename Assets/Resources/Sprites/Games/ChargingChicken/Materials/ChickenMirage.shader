Shader "Sprites/ChickenMirage"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color1 ("Top Color", Color) = (1,1,1,1)
		_Color ("Bottom Color", Color) = (1,1,1,1)
		_Speed1 ("Wobble Speed", Float) = 1
		_Alpha ("Alpha", Float) = 1
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
				float2 texcoord : TEXCOORD0;
				float2 screenPos: TEXCOORD2;
			};
			
			fixed4 _Color1;
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				OUT.screenPos = ComputeScreenPos(IN.vertex);
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _Alpha;
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
				fixed2 wobble = (IN.texcoord);
				wobble.x = ((wobble.x + (sin((_Time.y * _Speed1 * 3) + wobble.y * 100) / 1000) + 1)) % 1;
				fixed4 input2 = SampleSpriteTexture (wobble);
				float screenPosAdjusted = clamp((IN.screenPos.y / 30) + 0.5, 0, 1);
				fixed4 ColorAdjusted = ((_Color) * screenPosAdjusted) + ((_Color1) * (1 - screenPosAdjusted));
				input1 *= ColorAdjusted;
				input2 *= ColorAdjusted;
				fixed4 c = (input1 + input2) / 1.5;
				c *= ColorAdjusted;
				c.a *= _Alpha;
				return c;
			}
		ENDCG
		}
	}
}
