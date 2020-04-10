# FaderPlayableAsset

Unity Timeline 内で Image コンポーネントをフェードさせるためのプレイアブルアセットです。

# チュートリアル

空のシーンから以下を行ってください。

1. 空の *GameObject* を作成し、*Director* コンポーネントを追加してください。
1. Unity プロジェクト内のフォルダー（*StreamingAssets* 配下のフォルダーが推奨されます）内の画像シーケンスをコピーしてください。
   > *StreamingAssets* 配下のフォルダーにコピーすると、これらの画像を Unity にインポートするプロセスを省くことができます（画像の数が多い場合にはこのインポートプロセスに時間が掛かる場合があります）。
1. Timeline ウィンドウが開かれていない場合は、これを開いてください。
1. Timeline ウィンドウ内で、FaderTrack を追加してください。

   ![AddFaderTrack](../images/AddFaderTrack.png)
   
1. Timeline ウィンドウ上で右クリックし「*Add Fader Playable Asset*」をクリックしてください。
 
   ![AddFaderPlayableAsset](../images/AddFaderPlayableAsset.png)
   
1. メニューを GameObject -> UI -> Image の順にクリックして Image オブジェクトを作成してください。

1. FaderTrack のオブジェクトプロパティに Image オブジェクトをドラッグアンドドロップしてください。



Timeline を再生したり Timeline ウィンドウでタイムスライダーをドラッグすると、Image オブジェクトがフェードイン／フェードアウトします。



# インスペクター

![FaderPlayableAsset](../images/FaderPlayableAsset.png)

* **Color**   
  トラックに添付された Image コンポーネントに適用される色です。
* **Fade Type**
  - FadeIn: 見えない状態（alpha=0）から見える状態（alpha=1）へのフェードです。
  - FadeOut: 見える状態（alpha=1）から見えない状態（alpha=0）へのフェードです。





