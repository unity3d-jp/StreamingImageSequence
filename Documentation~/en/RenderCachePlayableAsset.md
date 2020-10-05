# RenderCachePlayableAsset

A playable asset for caching render results to image files for playback by 
[StreamingImageSequencePlayableAsset](StreamingImageSequencePlayableAsset.md).

RenderCachePlayableAsset works together with RenderCapturer components, which execute the actual capturing process and
decide what gets rendered into image files. Currently, StreamingImageSequence provides: 
1. **CameraRenderCapturer** component.   
   Caches the render result of a Camera component.
1. **BaseRenderCapturer** class.  
   An extensible abstract class, which can be extended to customize the capturing process.




# Tutorial 

From an empty scene, do the following:

1. Create an animation in Timeline, for example: by referring to  
   [Creating Keyframed Animation in Timeline](https://learn.unity.com/tutorial/creating-keyframed-animation-in-timeline) tutorial.

1. Open the Timeline window and add a **RenderCacheTrack**.
 
   ![AddRenderCacheTrack](../images/AddRenderCacheTrack.png)
   
1. Right click on the **RenderCacheTrack** and click *Add Render Cache Playable Asset*
 
   ![AddRenderCachePlayableAsset](../images/AddRenderCachePlayableAsset.png)
   
1. Create a GameObject and add **CameraRenderCapturer** component.

1. Drag and drop the GameObject to the object property of the **RenderCacheTrack**.

   ![AssignRenderCapturer](../images/AssignRenderCapturer.png)

1. Select the **RenderCachePlayableAsset** and click *Update Render Cache* in the inspector.

# Inspector

![RenderCachePlayableAsset](../images/RenderCachePlayableAssetInspector.png)

* **Resolution**   
  The resolution of the output images. Modify the size of the Game window to change this property.
* **Cache Output folder**  
  Where the cached render results are stored.
* **Show Frame Markers**  
  [FramerMarkes](FrameMarkers.md) are used to customize which frames to capture. 
* **Lock Frames**  
  Turn the [FramerMarkes](FrameMarkers.md) to lock mode in order to prevent certain frames 
  from being rewritten, which is useful to maintain custom manipulation 
  to previous cached images.  
  ![RenderCache_LockFrames](../images/RenderCache_LockFrames.png)

* **Update Render Cache**  
  To update the images by rendering and caching the results as images.





