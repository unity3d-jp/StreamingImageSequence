# 連番画像を再生する

1. [クイックスタート](#クイックスタート)
1. [サポートされている画像形式](#サポートされている画像形式)
1. [フレームマーカー](#フレームマーカー)
1. [ギャップの外挿](#ギャップの外挿)
1. [曲線の編集　（エディターのみ）](#曲線の編集エディターのみ)
1. [StreamingImageSequencePlayableAsset](#streamingimagesequenceplayableasset)


## クイックスタート

空のシーンから、次の手順を実行して下さい。

1. 空の **GameObject** を作成し、**Director** コンポーネントを追加する。
1. Unity プロジェクト内のフォルダー（*StreamingAssets* 配下のフォルダーが推奨されます）内の画像シーケンスをコピーする。
   > *StreamingAssets* 配下のフォルダーにコピーすると、これらの画像を Unity にインポートするプロセスを省くことができます（画像の数が多い場合にはこのインポートプロセスに時間が掛かる場合があります）。
1. [Timeline](https://docs.unity3d.com/Packages/com.unity.timeline@latest) 
   ウィンドウを開く。
1. Timeline ウィンドウに **StreamingImageSequenceTrack** を追加する。

   ![AddStreamingImageSequenceTrack](../images/AddStreamingImageSequenceTrack.png)
   
1. 追加した **StreamingImageSequenceTrack** に画像シーケンスの入ったフォルダーをドラッグアンドドロップする。
   これにより、フォルダー内の画像を使った 
   [StreamingImageSequencePlayableAsset](#streamingimagesequenceplayableasset) が自動的に生成されます。
 
   ![DragAndDropFolder](../images/DragAndDropFolder.png)
   
1. メニューを GameObject > UI > Image の順にクリックして **Image** オブジェクトを作成する。

1. 作成した **Image** オブジェクトを、**StreamingImageSequenceTrack** のオブジェクトプロパティーにドラッグアンドドロップし
   *Create StreamingImageSequenceRenderer on Image* をクリックする。

   ![CreateStreamingImageSequenceNativeRenderer](../images/CreateStreamingImageSequenceRenderer.png)


フォルダー内の画像シーケンスが **Image** オブジェクトの中に表示されます。
Timeline を再生したり、Timeline ウィンドウのタイムスライダーをドラッグすると、
**Image** オブジェクトの **Renderer** コンポネントが更新されます。


画像をインポートする他の方法については、
[画像をインポート](ImportingImages.md) を参照してください。

## サポートされている画像形式

|             | Windows            | Mac                | Linux              |
| ----------- | ------------------ | ------------------ | ------------------ |
| png         | :white_check_mark: | :white_check_mark: | :white_check_mark: |       
| tga         | :white_check_mark: | :white_check_mark: | :white_check_mark: |    

## フレームマーカー

すべてのフレームが [フレームマーカー](FrameMarkers.md) を持っています。
これは特定のフレームに割り当てられた画像をスキップし、
そのフレームの直前に使われた画像を代わりに表示するために使います。

![FrameMarker](../images/StreamingImageSequence_FrameMarker.png)

詳細に関しては[フレームマーカー](FrameMarkers.md)を参照してください。

## ギャップの外挿

![StreamingImageSequencePlayableAssetExtrapolation](../images/StreamingImageSequencePlayableAssetExtrapolation.png)

StreamingImageSequence クリップの前後のギャップの動作は、
[Animation クリップのギャップの外挿を設定する](https://docs.unity3d.com/Packages/com.unity.timeline@1.5/manual/clp_gap_extrap.html)のと同様、
下記のオプションで設定できます：
1. **None** (デフォルト): **Renderer** コンポーネントを非アクティブにし、バインドされたオブジェクトを非表示にする。
1. **Hold**: ギャップ内に連番の最初、または最後のフレームを表示し続ける。
1. **Loop**: 同じクリップの長さで、連番をループする。
1. **Ping Pong**: 同じクリップの長さで、連番を逆方向にループし、次に順方向にループする。
1. **Continue**: **Hold** と同じ。

デフォルトでは、StreamingImageSequence は Pre-Extrapolate と Post-Extrapolate プロパティの両方を **None** に設定します。

## 曲線の編集（エディターのみ）

エディターでは、再生のタイミングを次のように変更できます。
1. 曲線のセクションを開く
2. 曲線を右クリックして、キーを追加する
3. 追加されたキーを動かす  

現状、この機能はエディターでのみサポートされており、実行時の再生のタイミングは常にリニアになります。

![StreamingImageSequenceCurve](../images/StreamingImageSequenceCurve.png)

## StreamingImageSequencePlayableAsset

StreamingImageSequencePlayableAsset は、
[Unity Timeline](https://docs.unity3d.com/Packages/com.unity.timeline@latest) で
連番画像を再生する為に使われる
[PlayableAsset](https://docs.unity3d.com/ScriptReference/Playables.PlayableAsset.html) です。  
インスペクターで下記のプロパティを確認または変更することができます。

![StreamingImageSequencePlayableAsset](../images/StreamingImageSequencePlayableAssetInspector.png)

* **Resolution**（読み取り専用）  
  フォルダー内の最初の画像の幅と高さを表示します。
* **Folder**  
  画像ファイルが入っているフォルダーです。
* **Images**  
  フォルダー内の画像です。
  画像ファイルの名前を上下にドラッグアンドドロップして並び替えることができます。
* **Show FrameMarkers**  
  各フレームのフレームマーカーの表示・非表示を切り替えます。
  * **Reset**  
    フレームマーカーの編集をリセットします。
* **Background Colors**.  
  * **In Timeline Window**  
    Timeline ウィンドウのプレビュー画像の背景色。

* **Reset Curve**。
  プレイアブルアセット内の曲線のタイミングをリニアにリセットします。



