# CRTEffects
post processing stack v2 にて使えるブラウン管風のエフェクトです。

## 使い方
このファイルをAssetに追加し、Post Processing Stack Volumeで"Custom/CRT"を追加すると適応されます。
Unity2019.1.1f1にて動作を確認しています。

## パラメータ
- Distort - レンズ歪みの強さ
- RGB Blend - 0だとRGBが完全に分離、1だと通常のRGB
- Bottom Collapse - 画面下部の映像が圧縮された部分の大きさ
- Noise Amount - 画面下部のノイズの量

## 参考にさせていただいたサイト樣
[notargs.com "ブラウン管風シェーダーを作った"](http://wordpress.notargs.com/blog/blog/2016/01/09/unity3d%e3%83%96%e3%83%a9%e3%82%a6%e3%83%b3%e7%ae%a1%e9%a2%a8%e3%82%b7%e3%82%a7%e3%83%bc%e3%83%80%e3%83%bc%e3%82%92%e4%bd%9c%e3%81%a3%e3%81%9f/)

[おもちゃラボ "シェーダで作るノイズ５種盛り"](http://nn-hokuson.hatenablog.com/entry/2017/01/27/195659#fBm%E3%83%8E%E3%82%A4%E3%82%BA)

[LIGHT11 "Post Processingで自作のポストエフェクトを実装する"](http://light11.hatenadiary.com/entry/2019/03/31/225111#FXAA%E3%82%92%E4%BD%BF%E3%81%86%E5%A0%B4%E5%90%88%E3%81%AF%E6%9B%B8%E3%81%8D%E6%96%B9%E3%81%AB%E6%B3%A8%E6%84%8F%E3%81%99%E3%82%8B)
