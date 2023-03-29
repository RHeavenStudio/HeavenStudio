//
// Kino/Feedback - framebuffer feedback effect for Unity
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
Shader "Hidden/Kino/Feedback"
{
    Properties
    {
        _MainTex("", 2D) = "white"{}
        _Color("", Color) = (1, 1, 1)
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;

    half4 _Color;
    float2 _Offset;
    float4 _Rotation;
    float _Scale;

    v2f_img vert_feedback(appdata_img v)
    {
        v2f_img o;
        o.pos.xy = v.vertex.xy * float2(2, _ProjectionParams.x < 0 ? -2 : 2);
    #ifdef UNITY_REVERSED_Z
        o.pos.z = 0;
    #else
        o.pos.z = 1;
    #endif
        o.pos.w = 1;
        o.uv = v.texcoord;
        return o;
    }

    half4 frag_feedback(v2f_img i) : SV_Target
    {
        float2 uv = (i.uv - 0.5) * _Scale;
        uv = float2(dot(uv, _Rotation.xy), dot(uv, _Rotation.zw));
        uv += 0.5 + _Offset;
        half4 col = tex2D(_MainTex, uv);
        return col * _Color;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            ZTest LEqual
            Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_feedback
            #pragma fragment frag_feedback
            ENDCG
        }
    }
}
