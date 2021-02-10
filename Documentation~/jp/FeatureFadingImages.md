# Image オブジェクトをフェードさせる

1. [クイックスタート](#クイックスタート)
1. [ギャップの外挿](#ギャップの外挿)
1. [FaderPlayableAsset](#faderplayableasset)

## クイックスタート

空のシーンから、次の手順を実行して下さい。

1. 空の **GameObject** を作成し、**Director** コンポーネントを追加する。
1. Unity プロジェクト内のフォルダー（*StreamingAssets* 配下のフォルダーが推奨されます）内の画像シーケンスをコピーする。
   > *StreamingAssets* 配下のフォルダーにコピーすると、これらの画像を Unity にインポートするプロセスを省くことができます（画像の数が多い場合にはこのインポートプロセスに時間が掛かる場合があります）。
1. Timeline ウィンドウを開く。
1. Timeline ウィンドウ内で、**FaderTrack** を追加する。

   ![AddFaderTrack](../images/AddFaderTrack.png)
   
1. Timeline ウィンドウ上で右クリックし「*Add Fader Playable Asset*」をクリックする。
 
   ![AddFaderPlayableAsset](../images/AddFaderPlayableAsset.png)
   
1. メニューを GameObject -> UI -> Image の順にクリックして **Image** オブジェクトを作成する。

1. FaderTrack のオブジェクトプロパティに **Image** オブジェクトをドラッグアンドドロップする。

Timeline を再生したり、Timeline ウィンドウでタイムスライダーをドラッグすると、
Image オブジェクトがフェードイン／フェードアウトします。

## ギャップの外挿

![StreamingImageSequencePlayableAssetExtrapolation](../images/StreamingImageSequencePlayableAssetExtrapolation.png)

FaderPlayableAsset クリップの前後のギャップの動作は、
[Animation クリップのギャップの外挿の設定](https://docs.unity3d.com/ja/Packages/com.unity.timeline@1.5/manual/clp_gap_extrap.html)のと同様、
下記のオプションで設定できます：
1. **None** (デフォルト): **Renderer** コンポーネントを非アクティブにし、バインドされたオブジェクトを非表示にする。
1. **Hold**: ギャップ内に最初、または最後のフェードの状態を表示し続ける。
1. **Loop**: 同じクリップの長さで、フェードをループする。
1. **Ping Pong**: 同じクリップの長さで、フェードを逆方向にループし、次に順方向にループする。
1. **Continue**: **Hold** と同じ。

デフォルトとして、FaderPlayableAsset は Pre-Extrapolate と Post-Extrapolate プロパティの両方を **None** 
に設定します。

## FaderPlayableAsset

FaderPlayableAsset は、
[Unity Timeline](https://docs.unity3d.com/Packages/com.unity.timeline@latest) で
Image コンポーネントをフェードさせるための
[PlayableAsset](https://docs.unity3d.com/ScriptReference/Playables.PlayableAsset.html) です。  
インスペクターで下記のプロパティを確認または変更することができます。

![FaderPlayableAsset](../images/FaderPlayableAsset.png)

* **Color**   
  トラックに添付された Image コンポーネントに適用される色です。
* **Fade Type**
  - FadeIn: 見えない状態（alpha=0）から見える状態（alpha=1）へのフェードです。
  - FadeOut: 見える状態（alpha=1）から見えない状態（alpha=0）へのフェードです。





