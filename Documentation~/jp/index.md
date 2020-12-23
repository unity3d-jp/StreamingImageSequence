# Streaming Image Sequence

Streaming Image Sequence は、Unity 2D Sprite を作成せずに Unity の Timeline で連番画像を
簡単に再生するためのパッケージで、下記の要件を満たすように設計されています。

1. [StreamingAssets](https://docs.unity3d.com/ja/current/Manual/StreamingAssets.html) 
   を使用することで、完全にテクスチャのインポート時間を回避できます。
1. 再生モードとタイムライン編集モードの両方で、スムーズに画像を再生できます。   
1. 複数のOSをサポートします。

**Timeline 1.4.x 以降のご利用を推奨いたします。**

## 対応プラットフォーム

1. Windows
2. Mac
3. Linux


## 機能

1. #### [連番画像を再生する](FeaturePlayingSequentialImages.md)

   ![StreamingImageSequenceDemo](../images/StreamingImageSequenceDemo.gif)

2. #### [描画の結果のキャッシュを生成する](FeatureCachingRenderResults.md)

   ![RenderCacheDemo](../images/RenderCacheDemo.gif)

3. #### [Image オブジェクトをフェードさせる](FeatureFadingImages.md)

   ![FaderDemo](../images/FaderDemo.gif)

## エディタでのメモリ使用量

StreamingImageSequence は、スムーズな画像再生を提供するために、エディタで物理メモリを確保します。  
この確保は、次の要件を満たすように設定されています:
1. システムの物理メモリの合計の 90％ を超えない。
1. [環境設定](Preferences.md)で設定できるメモリの最大量を超えない。


## その他の言語
- [English](../index.md)





