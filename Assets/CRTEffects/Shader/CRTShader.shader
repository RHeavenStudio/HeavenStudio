Shader "Hidden/Custom/CRT"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            
            const float PI = 3.14159265;

            #pragma vertex VertDefault
            #pragma fragment Frag
            
            #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
            
            float rand(float2 st) {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453);
            }
            float2 random2(float2 st){
                st = float2( dot(st,float2(127.1,311.7)),
                           dot(st,float2(269.5,183.3)) );
                return -1.0 + 2.0*frac(sin(st)*43758.5453123);
            }
            float perlinNoise(float2 st) 
            {
                float2 p = floor(st);
                float2 f = frac(st);
                float2 u = f*f*(3.0-2.0*f);

                float v00 = random2(p+float2(0,0));
                float v10 = random2(p+float2(1,0));
                float v01 = random2(p+float2(0,1));
                float v11 = random2(p+float2(1,1));

                return lerp( lerp( dot( v00, f - float2(0,0) ), dot( v10, f - float2(1,0) ), u.x ),
                         lerp( dot( v01, f - float2(0,1) ), dot( v11, f - float2(1,1) ), u.x ), 
                         u.y)+0.5f;
            }

            TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
            
            float _Distort;
            float _ScreenWidth;
            float _ScreenHeight;
            float _RGBBlend;
            float _BottomCollapse;
            float _NoiseAmount;

            float4 Frag(VaryingsDefault i) : SV_Target
            {
                //レンズ歪み
                float2 distcoord = i.texcoord;
                distcoord -= 0.5;
                distcoord /= 1 - length(distcoord) * _Distort;
                distcoord += 0.5;
                
                //画面のズレ
                float2 linecoord = distcoord; 
                //linecoord.x += (sin(_Time.r * 1.5 + linecoord.y * 0.7) > 0.9) * 0.05;
                float linedistsin = sin(_Time.g + linecoord.y * 2 * PI);
                float linedistwidth = 0.995;
                linecoord.x += (linedistsin > linedistwidth) * (linedistsin - linedistwidth);
                linecoord.x += (sin(_Time.a * 100 + linecoord.y * 10)) * 0.0005;
                
                //下部の圧縮された部分
                linecoord.x -= (linecoord.y < _BottomCollapse) * rand(float2(_Time.a,linecoord.y)) * 0.1;
                linecoord.y = linecoord.y < _BottomCollapse ? linecoord.y * (1 / _BottomCollapse) : linecoord.y;
                
                //rgbずれ
                float4 color;
                color.r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, linecoord + float2(0.002,0)).r ;
                color.g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, linecoord + float2(0,0)).g;
                color.b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, linecoord + float2(-0.002,0)).b;
                
                //下部のノイズ
                float noisevalue = perlinNoise(linecoord * float2(5,500) + rand(_Time) + _Time.ba);
                float noiseCrit = (1 - _NoiseAmount) + max(linecoord.y - _BottomCollapse, linecoord.y < _BottomCollapse) * 2;
                color.r = (noisevalue > noiseCrit) ? rand(linecoord + float2(0,1)) : color.r;
                color.g = (noisevalue > noiseCrit) ? rand(linecoord + float2(1,2)) : color.g;
                color.b = (noisevalue > noiseCrit) ? rand(linecoord + float2(3,4)) : color.b;
                
                //rgb配列
                float rgbmod = fmod((i.texcoord.x) * _ScreenWidth, 3);
                color.r *= max(rgbmod < 1, _RGBBlend);
                color.g *= max(1 < rgbmod && rgbmod < 2, _RGBBlend);
                color.b *= max(2 < rgbmod, _RGBBlend);
                
                rgbmod = fmod((i.texcoord.y) * _ScreenHeight, 4);
                color.rgb *= rgbmod >= 1;
                
                //レンズ歪みの外側
                color.rgb *= 1 - (distcoord.x < 0 || distcoord.x > 1 || distcoord.y < 0 || distcoord.y > 1);
                return color;
            }
            ENDHLSL
        }
    }
}