// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//
// Kino/Datamosh - Glitch effect simulating video compression artifacts
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
Shader "Hidden/Kino/Datamosh"
{
    Properties
    {
        _MainTex("", 2D) = ""{}
        _WorkTex("", 2D) = ""{}
        _DispTex("", 2D) = ""{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    sampler2D _WorkTex;
    float4 _WorkTex_TexelSize;

    sampler2D _DispTex;
    float4 _DispTex_TexelSize;

    sampler2D_half _CameraMotionVectorsTexture;
    float4 _CameraMotionVectorsTexture_TexelSize;

    float _BlockSize;
    float _Quality;
    float _Contrast;
    float _Velocity;
    float _Diffusion;

    // PRNG
    float UVRandom(float2 uv)
    {
        float f = dot(float2(12.9898, 78.233), uv);
        return frac(43758.5453 * sin(f));
    }

    // Vertex shader for multi texturing
    struct v2f_multitex
    {
        float4 pos : SV_POSITION;
        float2 uv0 : TEXCOORD0;
        float2 uv1 : TEXCOORD1;
    };

    v2f_multitex vert_multitex(appdata_full v)
    {
        v2f_multitex o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv0 = v.texcoord.xy;
        o.uv1 = v.texcoord.xy;
    #if UNITY_UV_STARTS_AT_TOP
        if (_MainTex_TexelSize.y < 0.0)
            o.uv1.y = 1.0 - v.texcoord.y;
    #endif
        return o;
    }

    // Initialization shader
    half4 frag_init(v2f_img i) : SV_Target
    {
        return 0;
    }

    // Displacement buffer updating shader
    half4 frag_update(v2f_img i) : SV_Target
    {
        float2 uv = i.uv;
        float2 t0 = float2(_Time.y, 0);

        // Random numbers
        float3 rand = float3(
            UVRandom(uv + t0.xy),
            UVRandom(uv + t0.yx),
            UVRandom(uv.yx - t0.xx)
        );

        // Motion vector
        half2 mv = tex2D(_CameraMotionVectorsTexture, uv).rg;
        mv *= _Velocity;

        // Normalized screen space -> Pixel coordinates
        mv = mv * _ScreenParams.xy;

        // Small random displacement (diffusion)
        mv += (rand.xy - 0.5) * _Diffusion;

        // Pixel perfect snapping
        mv = round(mv);

        // Accumulates the amount of motion.
        half acc = tex2D(_MainTex, i.uv).a;
        half mv_len = length(mv);
        // - Simple update
        half acc_update = acc + min(mv_len, _BlockSize) * 0.005;
        acc_update += rand.z * lerp(-0.02, 0.02, _Quality);
        // - Reset to random level
        half acc_reset = rand.z * 0.5 + _Quality;
        // - Reset if the amount of motion is larger than the block size.
        acc = saturate(mv_len > _BlockSize ? acc_reset : acc_update);

        // Pixel coordinates -> Normalized screen space
        mv *= (_ScreenParams.zw - 1);

        // Random number (changing by motion)
        half mrand = UVRandom(uv + mv_len);

        return half4(mv, mrand, acc);
    }

    // Moshing shader
    half4 frag_mosh(v2f_multitex i) : SV_Target
    {
        // Color from the original image
        half4 src = tex2D(_MainTex, i.uv1);

        // Displacement vector (x, y, random, acc)
        half4 disp = tex2D(_DispTex, i.uv0);

        // Color from the working buffer (slightly scaled to make it blurred)
        half3 work = tex2D(_WorkTex, i.uv1 - disp.xy * 0.98).rgb;

        // Generate some pseudo random numbers.
        float4 rand = frac(float4(1, 17.37135, 841.4272, 3305.121) * disp.z);

        // Generate noise patterns that look like DCT bases.
        // - Frequency
        float2 uv = i.uv1 * _DispTex_TexelSize.zw * (rand.x * 80 / _Contrast);
        // - Basis wave (vertical or horizontal)
        float dct = cos(lerp(uv.x, uv.y, 0.5 < rand.y));
        // - Random amplitude (the high freq, the less amp)
        dct *= rand.z * (1 - rand.x) * _Contrast;

        // Conditional weighting
        // - DCT-ish noise: acc > 0.5
        float cw = (disp.w > 0.5) * dct;
        // - Original image: rand < (Q * 0.8 + 0.2) && acc == 1.0
        cw = lerp(cw, 1, rand.w < lerp(0.2, 1, _Quality) * (disp.w > 0.999));
        // - If the conditions above are not met, choose work.

        return half4(lerp(work, src.rgb, cw), src.a);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_init
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_update
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_multitex
            #pragma fragment frag_mosh
            #pragma target 3.0
            ENDCG
        }
    }
}
