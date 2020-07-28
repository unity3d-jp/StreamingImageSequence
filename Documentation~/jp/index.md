# Streaming Image Sequence

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

### [一連の画像を再生する](StreamingImageSequencePlayableAsset.md)
![StreamingImageSequenceDemo](../images/StreamingImageSequenceDemo.gif)


### [Image オブジェクトをフェードさせる](FaderPlayableAsset.md)
![FaderDemo](../images/FaderDemo.gif)

## メモリ

StreamingImageSequence は、スムーズな画像再生を提供するために、物理メモリを確保します。  
この確保は、次の要件を満たすように設定されています:
1. システムの物理メモリの合計の 90％ を超えない。
1. **編集 > 設定** ウィンドウで設定できるメモリの最大量を超えない。

![Preferences](../images/Preferences.png)

| 記号    | 用途                                                                    | 
| ------- | ---------------------------------------------------------------------- | 
| A       | 設定できる環境変数                                                       |   
| B       | 現在の設定値                                                            |   
| C       | 適応し、保存する                                                        |  

## その他の言語
- [English](../index.md)





