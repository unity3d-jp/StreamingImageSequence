# Playing Sequential Images

1. [Quick Start](#quick-start)
1. [Supported Image Formats](#supported-image-formats)
1. [FrameMarker](#framemarker)
1. [Gap Extrapolation](#gap-extrapolation)
1. [Curve Editing (Editor only)](#curve-editing-editor-only)
1. [StreamingImageSequencePlayableAsset](#streamingimagesequenceplayableasset)


## Quick Start

From an empty scene, do the following:

1. Create an empty **GameObject** and add a **Director** component to it.
1. Copy the sequential images in a folder inside the Unity project, preferably under *StreamingAssets*.
   > Copying to a folder under *StreamingAssets* will save us from the process to import those images in Unity, which may take a long time if there are a lot of images.
1. Open the [Timeline](https://docs.unity3d.com/Packages/com.unity.timeline@latest) window.
1. Add a **StreamingImageSequenceTrack** in the Timeline Window.

   ![AddStreamingImageSequenceTrack](../images/AddStreamingImageSequenceTrack.png)
   
1. Drag and drop the sequential images folder to the **StreamingImageSequenceTrack** added previously. 
   This will automatically create a [StreamingImageSequencePlayableAsset](#streamingimagesequenceplayableasset)
   using the images in the folder.
 
   ![DragAndDropFolder](../images/DragAndDropFolder.png)
   
1. Create an **Image** object by clicking the menu: GameObject > UI > Image.

1. Drag and drop the **Image** object to the object property of the **StreamingImageSequenceTrack**, 
   and click *Create StreamingImageSequenceRenderer on Image*.

   ![CreateStreamingImageSequenceNativeRenderer](../images/CreateStreamingImageSequenceRenderer.png)


The image sequences in the folder will then be shown inside the **Image** object, 
and the **Renderer** component of the **Image** object will be updated
as we play the Timeline or drag the time slider of the Timeline window.


For other ways for importing images, see [ImportingImages](ImportingImages.md).

## Supported Image Formats

|             | Windows            | Mac                | Linux              |
| ----------- | ------------------ | ------------------ | ------------------ |
| png         | :white_check_mark: | :white_check_mark: | :white_check_mark: |       
| tga         | :white_check_mark: | :white_check_mark: | :white_check_mark: |    

## FrameMarker

Each frame has a [FrameMarker](FrameMarkers.md), 
which can be used to skip the image assigned to that particular frame, 
and show the last used image instead.

![FrameMarker](../images/StreamingImageSequence_FrameMarker.png)

Refer to [FrameMarkers](FrameMarkers.md) for more details. 

## Gap Extrapolation

![StreamingImageSequencePlayableAssetExtrapolation](../images/StreamingImageSequencePlayableAssetExtrapolation.png)

The behaviour of a gap before or after a StreamingImageSequence clip can be set in a similar way to 
[setting gap extrapolation for Animation clips](https://docs.unity3d.com/Packages/com.unity.timeline@1.5/manual/clp_gap_extrap.html)
using one of the following options:
1. **None** (default): hide the bound object by deactivating its **Renderer** component.
1. **Hold**: hold and show the first/last frame of the image sequence in the gap.
1. **Loop**: loop the entire image sequence with the same clip duration.
1. **Ping Pong**: loop the entire image sequence backwards, then forwards, and so forth, with the same clip duration.
1. **Continue**: same as **Hold**

By default, StreamingImageSequence sets both Pre-Extrapolate and Post-Extrapolate properties to **None**.


## Curve Editing (Editor only)

In the editor, we can modify the timing of the playback by 
1. opening the curve section
2. right clicking on the curve to add keys
3. moving the keys accordingly

This is currently only supported in the editor, 
and the playback timing will always be linear in runtime.

![StreamingImageSequenceCurve](../images/StreamingImageSequenceCurve.png)

## StreamingImageSequencePlayableAsset

StreamingImageSequencePlayableAsset is a type of 
[PlayableAsset](https://docs.unity3d.com/ScriptReference/Playables.PlayableAsset.html)
which is used for playing sequential image sequences in 
[Unity Timeline](https://docs.unity3d.com/Packages/com.unity.timeline@latest).  
We can view or modify the following properties through the inspector.

![StreamingImageSequencePlayableAsset](../images/StreamingImageSequencePlayableAssetInspector.png)

* **Resolution** (Read Only)  
  Shows the width and height of the first image in the folder
* **Folder**  
  The folder where the image files are located
* **Images**  
  The images inside the folder. 
  Can be reordered by dragging the image file name up/down.
* **Show FrameMarkers**.  
  Show/hide the [FrameMarker](FrameMarkers.md) of each frame.
  * **Reset**  
    Reset edits to FrameMarkers.
* **Background Colors**.  
  * **In Timeline Window**  
    The background color of the preview images in the Timeline window.    
* **Reset Curve**.  
  Reset the curve timing in the PlayableAsset to be linear.



