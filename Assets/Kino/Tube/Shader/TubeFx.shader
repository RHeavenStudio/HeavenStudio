// KinoTube - Old TV/video artifacts simulation
// https://github.com/keijiro/KinoTube

Shader "Hidden/Kino/Tube"
{
    Properties
    {
        _MainTex("", 2D) = ""{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    uint _BleedTaps;
    float _BleedDelta;
    float _FringeDelta;
    float _Scanline;

    half3 RGB2YIQ(fixed3 rgb)
    {
        rgb = saturate(rgb);
#ifndef UNITY_COLORSPACE_GAMMA
        rgb = LinearToGammaSpace(rgb);
#endif
        return mul(half3x3(0.299,  0.587,  0.114,
                           0.596, -0.274, -0.322,
                           0.211, -0.523,  0.313), rgb);
    }

    fixed3 YIQ2RGB(half3 yiq)
    {
        half3 rgb = mul(half3x3(1,  0.956,  0.621,
                                1, -0.272, -0.647,
                                1, -1.106,  1.703), yiq);
        rgb = saturate(rgb);
#ifndef UNITY_COLORSPACE_GAMMA
        rgb = GammaToLinearSpace(rgb);
#endif
        return rgb;
    }

    half3 SampleYIQ(float2 uv, float du)
    {
        uv.x += du;
        return RGB2YIQ(tex2D(_MainTex, uv).rgb);
    }

    fixed4 frag(v2f_img input) : SV_Target
    {
        float2 uv = input.uv;
        half3 yiq = SampleYIQ(uv, 0);

        // Bleeding
        for (uint i = 0; i < _BleedTaps; i++)
        {
            yiq.y += SampleYIQ(uv, -_BleedDelta * i).y;
            yiq.z += SampleYIQ(uv, +_BleedDelta * i).z;
        }
        yiq.yz /= _BleedTaps + 1;

        // Fringing
        half y1 = SampleYIQ(uv, -_FringeDelta).x;
        half y2 = SampleYIQ(uv, +_FringeDelta).x;
        yiq.yz += y2 - y1;

        // Scanline
        half scan = sin(uv.y * 500 * UNITY_PI);
        scan = lerp(1, (scan + 1) / 2, _Scanline);

        return fixed4(YIQ2RGB(yiq * scan), 1);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            ENDCG
        }
    }
}
