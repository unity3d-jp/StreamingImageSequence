# Streaming Image Sequence

## その他の言語
* [English](README.md)
 
## 概要

Streaming Image Sequence は、Unity 2D Sprite を作成せずに Unity の Timeline で一連の画像シーケンスを
簡単に再生するためのパッケージで、下記の要件を満たすように設計されています。

1. [StreamingAssets](https://docs.unity3d.com/ja/current/Manual/StreamingAssets.html) 
   を使用することで、完全にテクスチャのインポート時間を回避できます。
1. 再生モードとタイムライン編集モードの両方で、スムーズに画像を再生できます。   
1. 複数のOSをサポートします。

**Timeline 1.4.x 以降のご利用を推奨いたします。**


## 対応プラットフォーム

1. Windows
2. Mac

## 機能

### [一連の画像を生成する](./Documentation~/jp/StreamingImageSequencePlayableAsset.md)
![StreamingImageSequenceDemo](Documentation~/images/StreamingImageSequenceDemo.gif)

### [Image オブジェクトをフェードさせる](./Documentation~/jp/FaderPlayableAsset.md)
![FaderDemo](Documentation~/images/FaderDemo.gif)


## メモリ

StreamingImageSequence は、スムーズな画像再生を提供するために、物理メモリを確保します。  
この確保は、次の要件を満たすように設定されています:
1. システムの物理メモリの合計の 90％ を超えない。
1. **編集 > 設定** ウィンドウで設定できるメモリの最大量を超えない。

![Preferences](Documentation~/images/Preferences.png)

| 記号    | 用途                                                                    | 
| ------- | ---------------------------------------------------------------------- | 
| A       | 設定できる環境変数                                                       |   
| B       | 現在の設定値                                                            |   
| C       | 適応し、保存する                                                        |  


## プラグイン
* [ビルド](Plugins~/Docs/en/BuildPlugins.md)

## ライセンス
* ソースコード: [Unity Companion License](LICENSE.md)
* サードパーティのソフトウェア　コンポーネント: [Third Party Notices](Third%20Party%20Notices.md) 
* Unity-chan アセット: [Unity-Chan License](https://unity-chan.com/contents/guideline/)  
  このアセットは下記のフォルダーに配置されていますが、これらのみに限定されない。
  - `AE~/Samples`
  - `StreamingImageSequence~/Assets/StreamingAssets`

## チュートリアル動画
- [使用方法を示した動画](https://youtu.be/mlRbwqJ74CM)
- [サンプル](https://youtu.be/4og6rgQdb3c)


