Shader "Hidden/PaletteSwapper" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader {

        Pass {
            CGPROGRAM
            #pragma vertex vp
            #pragma fragment fp

            #include "UnityCG.cginc"

            struct VertexData {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vp(VertexData v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            Texture2D _ColorPalette;
            float4 _ColorPalette_TexelSize;
            SamplerState point_clamp_sampler;
            int _Invert;

            fixed4 fp(v2f i) : SV_Target {
                /* Assumes _MainTex is grayscale and quantized */
                float2 uv = float2(tex2D(_MainTex, i.uv).r, 0.5f);

                uv.x = _Invert == 1 ? 1 - uv.x : uv.x;

                float4 firstColor = _ColorPalette.Sample(point_clamp_sampler, float2(floor(_ColorPalette_TexelSize.z * uv.x) / _ColorPalette_TexelSize.z, 0.5f));
                float4 secondColor = _ColorPalette.Sample(point_clamp_sampler, float2(ceil(_ColorPalette_TexelSize.z * uv.x) / _ColorPalette_TexelSize.z, 0.5f));
            
                return lerp(firstColor, secondColor, frac(_ColorPalette_TexelSize.z * uv.x));
            }
            ENDCG
        }
    }
}