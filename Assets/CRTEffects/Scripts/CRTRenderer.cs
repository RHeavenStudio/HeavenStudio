using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class CRTRenderer : PostProcessEffectRenderer<CRT>
{
    // 初期化時の処理
    public override void Init()
    {
        base.Init();
    }

    public override void Render(PostProcessRenderContext context)
    {
        // 内部的にプールされているMaterialPropertyBlockが保存されているPropertySheetを取得
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/CRT"));

        // MaterialPropertyBlockに対してプロパティをセット
        sheet.properties.SetFloat("_Distort", settings.distort);
        sheet.properties.SetFloat("_RGBBlend", settings.RGBBlend);
        sheet.properties.SetFloat("_BottomCollapse", settings.BottomCollapse);
        sheet.properties.SetFloat("_NoiseAmount", settings.NoiseAmount);
        sheet.properties.SetFloat("_ScreenWidth", Screen.width);
        sheet.properties.SetFloat("_ScreenHeight", Screen.height);

        // CommandBufferのBlitFullscreenTriangleを使って描画
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }

    // 破棄時の処理
    public override void Release()
    {
        base.Release();
    }
}