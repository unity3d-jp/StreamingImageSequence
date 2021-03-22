# Changelog

## [0.9.2-preview] - 2021-03-22
* chore: use com.unity.film-internal-utilities@0.8.4-preview 

## [0.9.1-preview] - 2021-03-17
* fix: set texture for HDRP/Unlit materials when playing StreamingImageSequence 
* fix: used frame for SISPlayableAsset when the clipIn value is more than 0
* fix: FPS calculation of SISPlayableAsset 
* fix: auto-fill the camera field of CameraRenderCapturer 

## [0.9.0-preview] - 2021-03-10
* feat: load images as Texture2D directly if they are regular Unity assets 
* feat: make the SIS curve applicable in runtime 
* feat: partially support EXR for SISPlayableAsset and RenderCachePlayableAsset 
* fix: reset curves for Timeline 1.5.x and up
* fix: errors when moving RenderCachePlayableAsset around in Timeline 
* fix: errors when opening older SISPlayableAsset with no curves inside ClipData
* fix: allow RenderCapturer classes to define their own Blit Material to screen
* fix: errors when updating RenderCache while changing resolution 
* chore: use com.unity.film-internal-utilities@0.8.1-preview
* chore: update dependency to com.unity.film-internal-utilities@0.8.0-preview
* chore: Add V to indicate virtual functions in BaseRenderCapturer 

## [0.8.4-preview] - 2021-02-12
* feat: add extrapolation capability to FaderPlayableAsset 
* fix: undo for StreamingImageSequencePlayableAsset and RenderCachePlayableAsset 
* chore: handle warnings when using com.unity.timeline@1.5.0 and up
* doc: add Gap Extrapolation section for RenderCachePlayableAsset

## [0.8.3-preview] - 2021-02-05
* fix: errors when using HDRP on Unity 2019.4.18 and up

## [0.8.2-preview] - 2021-02-05
* fix: error when showing the inspector of SISPlayableAsset which is not loaded in TimelineWindow 
* chore: make sure to open internals to VisualCompositor 

## [0.8.1-preview] - 2021-02-04
* feat: add Edit action when right clicking on the FrameMarker of SISPlayableAsset 
* feat: show OOM log warning when there is not enough memory for loading images
* fix: deserialize older versions of SISPlayableAsset (MovieProxy) successfully
* chore: update the VisualCompositor's assembly name 

## [0.8.0-preview] - 2021-02-03
* chore: use com.unity.film-internal-utilities@0.6.0-preview

## [0.7.0-preview] - 2020-12-24
* feat: add extrapolation support for SISPlayableAsset 
* feat: specify frames when updating RenderCache 
* fix: specify alpha channel and use CatmullRom filter when creating preview images 
* fix: handle RenderCache file output error 
* fix: package dependencies to ensure the package works in isolation 
* fix: set a fixed height value for the preview images 
* fix: null static textures after reopening an existing scene 
* fix: "The object of type 'PlayableDirector' has been destroyed but you are still trying to access it." in BasePlayableMixer 
* fix: show the requested image of StreamingImageSequencePlayableAsset after successfully loaded 
* fix: don't show previously loaded image of StreamingImageSequencePlayableAsset if the requested one is not successfully loaded
* fix: error when there is no material in the MeshRenderer of the GameObject bound to StreamingImageSequenceTrack
* doc: add a section about gap extrapolation for StreamingImageSequence clips 
* doc: add an item about capturing specified frames in RenderCachePlayableAsset 
* doc: update the curve section of SISPlayableAsset 
* doc: arrange docs and put features as the focus instead of PlayableAsset types
* opt: minor optimization in loading images by removing memset when allocating memory for them 

## [0.6.3-preview] - 2020-10-27
* chore: remove debug logs when updating package for 2020.2 and above

## [0.6.2-preview] - 2020-10-27
* chore: add debug logs when updating package for 2020.2 and above

## [0.6.1-preview] - 2020-10-27
* chore: republish 0.6.0-preview as 0.6.1-preview

## [0.6.0-preview] - 2020-10-26

* feat: add default StreamingImageSequencePlayableAsset FPS setting in Preferences 
* feat: enable the setting of background color in Timeline for SISPlayableAsset
* feat: save Preferences immediately when the setting is changed 
* feat: add FPS field in the inspector of StreamingImageSequencePlayableAsset to change its length
* feat: support TGA on Mac by loading TGA images on all platforms using stb
* feat: support Linux (PNG, TGA)
* feat: Add a notifier to restart Unity if the package is updated
* fix: crash when trying to load a preview of an image that is not available
* fix: memory leak in loading PNG on Windows when an out of memory situation happened.
* opt: optimize performance when loading PNG on Mac by using stb
* doc: update that TGA is now supported on Windows and Mac 


## [0.5.1-preview] - 2020-10-13

* chore: update dependency to com.unity.anime-toolbox@0.2.1-preview

## [0.5.0-preview] - 2020-10-12
* feat: set background color in the Game View when updating RenderCache 
* feat: set the background color of RenderCachePlayableAsset clips in Timeline 
* feat: add a texture blitter for URP and use it when updating RenderCache in URP projects
* feat: show the updating of RenderCache in Game View for HDRP projects 
* fix: add blending in LinearToGamma shader
* fix: apply linear to gamma conversion for drawing preview textures if the project is using linear color space 
* fix: RenderCachePlayable path errors when reloading, and show the path as a normalized path if possible 
* fix: try to create the saved path of RenderCachePlayableAsset if it is invalid 
* fix: set StreamingImageSequencePlayableAsset's texture properly for MeshRenderer in HDRP
* fix: play sequential images in runtime builds 
* doc: fix broken anchor links in the doc to import images, and add missing BaseRenderCapturer API docs

## [0.4.0-preview] - 2020-10-06

* feat: add RenderCache and RenderCapturer, which work together to cache render results to image files. Currently provides:
  ** CameraRenderCapturer: to cache the render results of a Camera component.
  ** BaseRenderCapturer: an extensible class that a user can use to customize the capturing process.
* feat: lock frames via FrameMarkers when capturing RenderCache to prevent them from being overwritten
* feat: skipping frames when caching render results if specified by the FrameMarkers
* feat: add a shortcut for locking and edit a frame of RenderCachePlayableAsset using the assigned image application.
* feat: auto hide/show FrameMarkers when the TimelineWindow is zoomed out/in
* feat: add user notes in FrameMarkers
* feat: check if we should reload the folder of StreamingImageSequencePlayableAsset or RenderCachePlayableAsset when the Editor Application is back in focus
* fix: check if an image file exists before queuing to load it 
* fix: import images directly for StramingImageSequencePlayableAsset when the source folder is already under StreamingAssets 
* fix: prevent the internal texture from being destroyed when TimelineEditor is refreshed
* fix: time rounding errors when calculating the image index of StreamingImageSequencePlayableAsset
* fix: compile error in Unity 2020.2 
* fix: open internals to com.unity.visual-compositor package
* refactor: rename UseImageMarkers to FrameMarkers 
* chore: change the plugin library name from Loader to StreamingImageSequence
* chore: use com.unity.anime-toolbox@0.2.0-preview 
* chore: change namespace to be exactly the same with the assembly name
* doc: update about package installation in the Readme files on Github.
* doc: add package badge
* doc: add RenderCache and FrameMarker documentations
* doc: update StreamingImageSequencePlayableAsset's doc

## [0.3.3-preview] - 2020-09-08
* fix: disable renderer component when showing/hiding gameObject instead of enabling/disabling
* fix: fix errors when displaying small number of images in the import window

## [0.3.2-preview] - 2020-08-13
* fix: fix memory allocation blocking on Mac

## [0.3.1-preview] - 2020-07-14

* feat: add support for SkinnedMeshRenderer and copy StreamingImageSequencePlayableAsset output to a target RenderTexture, if set 
* feat: add user settings to set maximum memory allocation for images

* fix: fix errors when building app build
* fix: fix errors on FaderTrack when there is no Image bound to the track
* fix: fix rounding errors when calculating imageIndex in StreamingImageSequencePlayableAsset
* fix: fix UseImageMarker when the timeScale of the clip is not one

* chore: remove unused/unsupported sub-menu (#133)
* chore: add Third Party Notices

## [0.3.0-preview] - 2020-07-14

* feat: reuse previously allocated memory for loading images when there is less than 10% free physical memory on the system.

* opt: performance improvement when resizing the length of TimelineClip containing StreamingImageSequencePlayableAsset in TimelineWindow
* opt: optimize the performance of loading images on the plugin side (locking, using full images for loading preview images, etc)
* opt: optimize the performance of loading images on the C# side (preventing duplicate tasks, limiting the number of pending tasks, etc)
* opt: optimize the ScrollView in ImageSequenceImportWindow 
* opt: optimize string-related operations in StreamingImageSequencePlayableAsset 
* opt: reduce StreamingImageSequence properties which are inspected by AnimatedParameterUtility.HasAnyAnimatableParameters 

* fix: hide buttons to reset Timeline-related properties of StreamingImageSequencePlayableAsset in non-timeline context 
* fix: render preview images correctly when clipIn is not 0 
* fix: prevent jittering preview images at the end of a long clip
* fix: prevent saving temporary StreamingImageSequencePlayableAsset texture in scenes 
* fix: make sure the image dimension ratio in StreamingImageSequencePlayableAsset is calculated correctly
* fix: fix errors that occurred when interacting with StreamingImageSequencePlayableAsset GUI
* fix: fix StreamingImageSequencePlayableAsset texture leak when closing or opening a new scene
* fix: fix the memory leak of preview textures when closing or opening a new scene
* fix: fix thread-related crashes when loading images
* fix: handle image loading process correctly when switching to playmode/editmode, or opening a new scene
* fix: reset image loading processes when opening a scene in single mode

* chore: delete unused StreamingImageSequenceWindow

* doc: Add Table of Contents and a link to the plugin building doc in the top Readme


## [0.2.0-preview] - 2020-06-12
* feat: deallocate unused images when there is not enough memory, and stop allocating memory if there is not enough memory for the current frame
* fix: preload images starting from the active frame as the center instead of always from frame 0
* chore: cleanup the code to preload images 
* chore: delete DrawOverWindow project and DLL
* chore: convert the plugin building process to cmake and add unit tests for the plugin

## [0.1.5-preview] - 2020-06-01
* chore: depend on com.unity.ext.nunit 
* chore: make it explicit that StreamingImageSequence depends on ugui 

## [0.1.4-preview] - 2020-05-14

* fix: inaccuracies in placing preview icon positions 
* fix: bug when stretching PlayableAsset
* fix: crash bug when entering play mode on Windows 
* fix: remove invalid alerts in StreamingImageSequenceTrack and FaderTrack 
* fix: Override ToString() in StreamingImageSequenceTrack
* chore: update yamato npm registry (#62)

## [0.1.3-preview] - 2020-04-15
* fix: crash caused by performing graphics operation when g_ThreadedGfxDevice is not ready after deserialization	

## [0.1.2-preview] - 2020-04-14
* fix: errors caused by StreamingImageSequenceTrack::GetActivePlayableAsset() when TimelineWindow is not in focus
* fix: keep processing StreamingImageSequencePlayableAsset even if there is no bound GameObject in the track, as the output texture is still required

## [0.1.1-preview] - 2020-04-10
* api: open StreamingImageSequenceTrack to public 
* api: open StreamingImageSequencePlayableAsset::GetTexture() to public 
* docs: Update Japanese docs.

## [0.1.0-preview] - 2020-04-06
* feat: markers to indicate the use/skipping of image in StreamingImageSequencePlayableAsset 

## [0.0.4-preview] - 2020-04-03
* fix: runtime build errors 
* fix: avoid tests from modifying the project assets

## [0.0.3-preview] - 2020-03-27
* fix: update DLLs to avoid the requirement of installing VCRUNTIME140_1.DLL 
* docs: Add Japanese docs

## [0.0.2-preview.3] - 2020-03-16
* docs: Updating img tag to MD

## [0.0.2-preview.2] - 2020-03-05

* feat: Fader imporvements. Reverse FadeOut and FadeIn, and a color to highlight FaderPlayableAsset
* fix: reverse the parameter to copy images to StreamingAssets
* fix: Hide UseImageMarker and use Timeline 1.4.0's ClipCaps.AutoScale 
* fix: StreamingImageSequencePlayableAsset stability issues. 
* fix: Support folder D&D for StreamingImageSequencePlayableAsset from folders which are not under "StreamingAssets"
* fix: Test assembly definitions.

## [0.0.2-preview.1] - 2020-03-02

- Renaming to *Streaming Image Sequence \<com.unity.streaming-image-sequence\>*.
- feat: folder drag and drop support.
- feat: preview icons


## [0.0.1-preview] - 2019-10-10

The first release of *Movie Proxy \<com.unity.movie-proxy\>*.

