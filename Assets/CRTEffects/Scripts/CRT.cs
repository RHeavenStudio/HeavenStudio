using System;
using UnityEngine;
// UnityEngine.Rendering.PostProcessingをusing
using UnityEngine.Rendering.PostProcessing;


[Serializable] // 必ずSerializableアトリビュートを付ける
[PostProcess(typeof(CRTRenderer), PostProcessEvent.AfterStack, "Custom/CRT", true)]
public sealed class CRT : PostProcessEffectSettings
{
    [Range(0f, 1f)]
    public FloatParameter distort = new FloatParameter { value = 0.0f };
    [Range(0f, 1f)]
    public FloatParameter RGBBlend = new FloatParameter { value = 1f };
    [Range(0f, 1f)]
    public FloatParameter BottomCollapse = new FloatParameter { value = 0f };
    [Range(0f, 1f)]
    public FloatParameter NoiseAmount = new FloatParameter { value = 0f };


    /*// 有効化する条件はこうやって指定する（ちゃんとやっておいたほうがパフォーマンスにつながりそう）
    public override bool IsEnabledAndSupported(PostProcessRenderContext context)
    {
        //Debug.Log(base.IsEnabledAndSupported(context));
        return base.IsEnabledAndSupported(context) || distort != 0;
        //return true;
    }*/
}