# StreamingImageSequencePlayableAsset

一連の画像シーケンスを Unity Timeline で再生するためのプレイアブルアセットです。

# 対応の画像形式


|             | Windows            | Mac                |
| ----------- | ------------------ | ------------------ |
| png         | :white_check_mark: | :white_check_mark: |    
| tga         | :white_check_mark: |                    |


# チュートリアル

空のシーンから以下を行ってください。

1. 空の *GameObject* を作成し、*Director* コンポーネントを追加してください。
1. Unity プロジェクト内のフォルダー（*StreamingAssets* 配下のフォルダーが推奨されます）内の画像シーケンスをコピーしてください。
   > *StreamingAssets* 配下のフォルダーにコピーすると、これらの画像を Unity にインポートするプロセスを省くことができます（画像の数が多い場合にはこのインポートプロセスに時間が掛かる場合があります）。
1. Timeline ウィンドウが開かれていない場合は、これを開いてください。
1. Timeline ウィンドウに StreamingImageSequenceTrack を追加してください。

   ![AddStreamingImageSequenceTrack](../images/AddStreamingImageSequenceTrack.png)
   
1. 追加した StreamingImageSequenceTrack に画像シーケンスの入ったフォルダーをドラッグアンドドロップしてください。
 
   ![DragAndDropFolder](../images/DragAndDropFolder.png)
   
1. メニューを GameObject -> UI -> Image の順にクリックして Image オブジェクトを作成してください。

1. 作成した Image オブジェクトを、StreamingImageSequenceTrack のオブジェクトプロパティーにドラッグアンドドロップし「*Create StreamingImageSequenceNativeRenderer on Image*」をクリックしてください。

   ![CreateStreamingImageSequenceNativeRenderer](../images/CreateStreamingImageSequenceNativeRenderer.png)


フォルダー内の画像シーケンスが Image オブジェクトの中に表示されます。Timeline を再生したり、Timeline ウィンドウのタイムスライダーをドラッグすると、Image オブジェクトが再生／有効化／無効化されます。


画像をインポートするこの他の方法は、[ImportingImages](ImportingImages.md) をご覧ください。


# 曲線

アセットの曲線を調整してプレイバックのタイミングを変更することも可能です。   
これを行うには、曲線のセクションを開き、曲線の上で右クリックするとキーの追加を開始できます。

![StreamingImageSequenceCurve](../images/StreamingImageSequenceCurve.png)

# インスペクター
![StreamingImageSequencePlayableAsset](../images/StreamingImageSequencePlayableAsset.png)

* **Resolution**（読み取り専用）  
  フォルダー内の最初の画像の幅と高さを表示します。
* **Folder**  
  画像ファイルが入っているフォルダーです。
* **Images**  
  フォルダー内の画像です。
  画像ファイルの名前を上下にドラッグアンドドロップして画像を並び替えることができます。
* **Reset Curve**.  
  PlayableAsset 内の曲線のタイミングをリニアにリセットします。



