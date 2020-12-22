# Fading Image objects

1. [Quick Start](#quick-start)
1. [FaderPlayableAsset](#faderplayableasset)

# Quick Start

From an empty scene, do the following:

1. Create an empty *GameObject* and add a *Director* component to it
1. Copy the sequential images in a folder inside the Unity project, preferably under *StreamingAssets*
   > Copying to a folder under *StreamingAssets* will save us from the process to import those images in Unity, which may take a long time if there are a lot of images
1. Open the Timeline window, if not opened yet
1. Add a FaderTrack in the Timeline Window

   ![AddFaderTrack](../images/AddFaderTrack.png)
   
1. Right click on the timeline window and click *Add Fader Playable Asset*
 
   ![AddFaderPlayableAsset](../images/AddFaderPlayableAsset.png)
   
1. Create an Image object by clicking the menu: GameObject -> UI -> Image

1. Drag and drop the Image object to the object property of the FaderTrack 


The Image object will be faded in/out as we play the Timeline or drag the time slider of the Timeline window.



# FaderPlayableAsset

FaderPlayableAsset is a type of 
[PlayableAsset](https://docs.unity3d.com/ScriptReference/Playables.PlayableAsset.html)
which is used for fading Image component in
[Unity Timeline](https://docs.unity3d.com/Packages/com.unity.timeline@latest).
We can view or modify the following properties through the inspector.

* **Color**   
  The color to be applied to the Image component attached to the track.
* **Fade Type**  
  - FadeIn : from invisible (alpha=0) to visible   (alpha=1) 
  - FadeOut: from visible   (alpha=1) to invisible (alpha=0) 





