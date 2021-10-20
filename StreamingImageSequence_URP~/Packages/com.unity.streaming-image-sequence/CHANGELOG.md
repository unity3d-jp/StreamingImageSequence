# Changelog

## [0.14.1-preview] - 2021-10-21

### Changed
* deps: update dependency to com.unity.film-internal-utilities@0.11.1-preview

## [0.14.0-preview] - 2021-10-11

### Changed
* use the timelineAsset FPS settings when creating a new StreamingImageSequencePlayableAsset 

### Fixed
* fix: fix the incorrect extra addition of curve key when dragging a folder to StreamingImageSequenceTrack 
* fix: modify curve directly when changing the FPS of StreamingImageSequencePlayableAsset
* fix: use half rounding down to get the image index of StreamingImageSequencePlayableAsset 

## [0.13.2-preview] - 2021-09-22

### Fixed
* fix: related files were deleted when updating RenderCache

## [0.13.1-preview] - 2021-09-21

### Fixed
* fix: put SIS-related menu items at the bottom
* doc: newline and image fixes 
* doc: remove Japanese translation 
* doc: rearrange the location of md files 

## [0.13.0-preview] - 2021-09-14

### Changed
* deps: update dependency to com.unity.film-internal-utilities@0.11.0-preview 

### Fixed
* fix: errors when inspecting an empty SISPlayableAsset
* fix: lingering sprite when duplicating an Image which has a StreamingImageSequenceRenderer
* fix: better error handling when attempting to load non-Texture2D images as regular Unity assets 

## [0.12.0-preview] - 2021-08-17

### Added
* add an option to use the last loaded image when loading for SISRenderer
* doc: add a section about StreamingImageSequenceRenderer 

### Changed
* deps: update dependencies to com.unity.film-internal-utilities@0.10.2-preview
* convert the changelog format to semantics versioning

### Fixed
* support the official Pencil+4 Line package, instead of the temporary one

## [0.11.0-preview] - 2021-07-02

### Added
* hide scale indicators for SISPlayableAsset when using Timeline 1.6.x 

### Changed
* change the default StreamingImageSequence TimelineClip fps to 24
* deps: use com.unity.film-internal-utilities@0.10.1-preview

## [0.10.0-preview] - 2021-04-22

### Changed
* stop autoscaling by default when changing the length of SISPlayableAsset
* deps: depend on com.unity.film-internal-utilities@0.9.0-preview

## [0.9.3-preview] - 2021-03-29

### Fixed
* fix: build error

## [0.9.2-preview] - 2021-03-22

### Changed
* deps: use com.unity.film-internal-utilities@0.8.4-preview 

## [0.9.1-preview] - 2021-03-17

### Fixed
* fix: set texture for HDRP/Unlit materials when playing StreamingImageSequence 
* fix: used frame for SISPlayableAsset when the clipIn value is more than 0
* fix: FPS calculation of SISPlayableAsset 
* fix: auto-fill the camera field of CameraRenderCapturer 

## [0.9.0-preview] - 2021-03-10

### Added
* load images as Texture2D directly if they are regular Unity assets 
* make the SIS curve applicable in runtime 
* partially support EXR for SISPlayableAsset and RenderCachePlayableAsset 

### Changed
* allow RenderCapturer classes to define their own Blit Material to screen
* deps: use com.unity.film-internal-utilities@0.8.1-preview
* chore: Add V to indicate virtual functions in BaseRenderCapturer 

### Fixed
* reset curves for Timeline 1.5.x and up
* fix errors when moving RenderCachePlayableAsset around in Timeline 
* fix errors when opening older SISPlayableAsset with no curves inside ClipData
* fix errors when updating RenderCache while changing resolution 


## [0.8.4-preview] - 2021-02-12

### Added
* add extrapolation capability to FaderPlayableAsset 
* doc: add Gap Extrapolation section for RenderCachePlayableAsset

### Fixed
* fix undo for StreamingImageSequencePlayableAsset and RenderCachePlayableAsset 
* handle warnings when using com.unity.timeline@1.5.0 and up

## [0.8.3-preview] - 2021-02-05

### Fixed
* fix errors when using HDRP on Unity 2019.4.18 and up

## [0.8.2-preview] - 2021-02-05

### Changed
* make sure to open internals to VisualCompositor 

### Fixed
* fix errors when showing the inspector of SISPlayableAsset which is not loaded in TimelineWindow 

## [0.8.1-preview] - 2021-02-04

### Added
* add Edit action when right clicking on the FrameMarker of SISPlayableAsset 
* show OOM log warning when there is not enough memory for loading images

### Changed
* update the VisualCompositor's assembly name 

### Fixed
* fix: deserialize older versions of SISPlayableAsset (MovieProxy) successfully

## [0.8.0-preview] - 2021-02-03

### Changed
* deps: use com.unity.film-internal-utilities@0.6.0-preview

## [0.7.0-preview] - 2020-12-24

### Added
* add extrapolation support for SISPlayableAsset 
* specify frames when updating RenderCache 
* doc: add a section about gap extrapolation for StreamingImageSequence clips 
* doc: add an item about capturing specified frames in RenderCachePlayableAsset 

### Changed
* show the requested image of StreamingImageSequencePlayableAsset after successfully loaded 
* don't show previously loaded image of StreamingImageSequencePlayableAsset if the requested one is not successfully loaded
* doc: update the curve section of SISPlayableAsset 
* doc: arrange docs and put features as the focus instead of PlayableAsset types
* opt: minor optimization in loading images by removing memset when allocating memory for them 

### Fixed
* specify alpha channel and use CatmullRom filter when creating preview images 
* handle RenderCache file output error 
* fix package dependencies to ensure the package works in isolation 
* set a fixed height value for the preview images 
* fix null static textures after reopening an existing scene 
* fix "The object of type 'PlayableDirector' has been destroyed but you are still trying to access it." in BasePlayableMixer 
* fix errors when there is no material in the MeshRenderer of the GameObject bound to StreamingImageSequenceTrack

## [0.6.3-preview] - 2020-10-27

### Changed
* remove debug logs when updating package for 2020.2 and above

## [0.6.2-preview] - 2020-10-27

### Changed
* add debug logs when updating package for 2020.2 and above

## [0.6.1-preview] - 2020-10-27

### Changed
* republish 0.6.0-preview as 0.6.1-preview

## [0.6.0-preview] - 2020-10-26

### Added
* add default StreamingImageSequencePlayableAsset FPS setting in Preferences 
* enable the setting of background color in Timeline for SISPlayableAsset
* add FPS field in the inspector of StreamingImageSequencePlayableAsset to change its length
* support TGA on Mac by loading TGA images on all platforms using stb
* support Linux (PNG, TGA)
* add a notifier to restart Unity if the package is updated

### Changed
* save Preferences immediately when the setting is changed 
* opt: optimize performance when loading PNG on Mac by using stb
* doc: update that TGA is now supported on Windows and Mac 

### Fixed
* fix crash when trying to load a preview of an image that is not available
* fix memory leak in loading PNG on Windows when an out of memory situation happened.

## [0.5.1-preview] - 2020-10-13

### Changed
* deps: update dependency to com.unity.anime-toolbox@0.2.1-preview

## [0.5.0-preview] - 2020-10-12

### Added
* set background color in the Game View when updating RenderCache 
* set the background color of RenderCachePlayableAsset clips in Timeline 
* add a texture blitter for URP and use it when updating RenderCache in URP projects
* show the updating of RenderCache in Game View for HDRP projects 

### Changed
* add blending in LinearToGamma shader

### Fixed
* apply linear to gamma conversion for drawing preview textures if the project is using linear color space 
* fix RenderCachePlayable path errors when reloading, and show the path as a normalized path if possible 
* fix: try to create the saved path of RenderCachePlayableAsset if it is invalid 
* fix: set StreamingImageSequencePlayableAsset's texture properly for MeshRenderer in HDRP
* fix: play sequential images in runtime builds 
* doc: fix broken anchor links in the doc to import images, and add missing BaseRenderCapturer API docs

## [0.4.0-preview] - 2020-10-06

### Added
* feat: add RenderCache and RenderCapturer, which work together to cache render results to image files. Currently provides:
  ** CameraRenderCapturer: to cache the render results of a Camera component.
  ** BaseRenderCapturer: an extensible class that a user can use to customize the capturing process.
* feat: lock frames via FrameMarkers when capturing RenderCache to prevent them from being overwritten
* feat: skipping frames when caching render results if specified by the FrameMarkers
* feat: add a shortcut for locking and edit a frame of RenderCachePlayableAsset using the assigned image application.
* feat: auto hide/show FrameMarkers when the TimelineWindow is zoomed out/in
* feat: add user notes in FrameMarkers
* feat: check if we should reload the folder of StreamingImageSequencePlayableAsset or RenderCachePlayableAsset when the Editor Application is back in focus
* doc: add package badge
* doc: add RenderCache and FrameMarker documentations

### Changed
* import images directly for StreamingImageSequencePlayableAsset when the source folder is already under StreamingAssets 
* open internals to com.unity.visual-compositor package
* rename UseImageMarkers to FrameMarkers 
* change the plugin library name from Loader to StreamingImageSequence
* deps: use com.unity.anime-toolbox@0.2.0-preview 
* change namespace to be exactly the same with the assembly name
* doc: update about package installation in the Readme files on Github.
* doc: update StreamingImageSequencePlayableAsset's doc

### Fixed
* fix: check if an image file exists before queuing to load it 
* fix: prevent the internal texture from being destroyed when TimelineEditor is refreshed
* fix time rounding errors when calculating the image index of StreamingImageSequencePlayableAsset
* fix compile errors in Unity 2020.2 

## [0.3.3-preview] - 2020-09-08

### Fixed
* fix: disable renderer component when showing/hiding gameObject instead of enabling/disabling
* fix errors when displaying small number of images in the import window

## [0.3.2-preview] - 2020-08-13

### Fixed
* fix memory allocation blocking on Mac

## [0.3.1-preview] - 2020-07-14

### Added
* add support for SkinnedMeshRenderer and copy StreamingImageSequencePlayableAsset output to a target RenderTexture, if set 
* add user settings to set maximum memory allocation for images
* doc: add Third Party Notices

### Fixed
* fix errors when building app build
* fix errors on FaderTrack when there is no Image bound to the track
* fix rounding errors when calculating imageIndex in StreamingImageSequencePlayableAsset
* fix UseImageMarker when the timeScale of the clip is not one

### Removed
* chore: remove unused/unsupported sub-menu (#133)

## [0.3.0-preview] - 2020-07-14

### Added
* reuse previously allocated memory for loading images when there is less than 10% free physical memory on the system.
* doc: Add Table of Contents and a link to the plugin building doc in the top Readme

### Changed
* opt: performance improvement when resizing the length of TimelineClip containing StreamingImageSequencePlayableAsset in TimelineWindow
* opt: optimize the performance of loading images on the plugin side (locking, using full images for loading preview images, etc)
* opt: optimize the performance of loading images on the C# side (preventing duplicate tasks, limiting the number of pending tasks, etc)
* opt: optimize the ScrollView in ImageSequenceImportWindow 
* opt: optimize string-related operations in StreamingImageSequencePlayableAsset 
* opt: reduce StreamingImageSequence properties which are inspected by AnimatedParameterUtility.HasAnyAnimatableParameters 

### Fixed
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

### Removed
* delete unused StreamingImageSequenceWindow

## [0.2.0-preview] - 2020-06-12

### Added
* deallocate unused images when there is not enough memory, and stop allocating memory if there is not enough memory for the current frame

### Changed
* chore: cleanup the code to preload images 
* chore: convert the plugin building process to cmake and add unit tests for the plugin
* preload images starting from the active frame as the center instead of always from frame 0

### Removed
* chore: delete DrawOverWindow project and DLL

## [0.1.5-preview] - 2020-06-01

### Changed
* deps: depend on com.unity.ext.nunit 
* deps: make it explicit that StreamingImageSequence depends on ugui 

## [0.1.4-preview] - 2020-05-14

### Changed
* chore: update yamato npm registry (#62)

### Fixed
* fix inaccuracies in placing preview icon positions 
* fix bugs when stretching PlayableAsset
* fix: crash bug when entering play mode on Windows 
* remove invalid alerts in StreamingImageSequenceTrack and FaderTrack 
* fix: Override ToString() in StreamingImageSequenceTrack

## [0.1.3-preview] - 2020-04-15

### Fixed
* fix: crash caused by performing graphics operation when g_ThreadedGfxDevice is not ready after deserialization	

## [0.1.2-preview] - 2020-04-14

### Fixed
* fix errors caused by StreamingImageSequenceTrack::GetActivePlayableAsset() when TimelineWindow is not in focus
* fix: keep processing StreamingImageSequencePlayableAsset even if there is no bound GameObject in the track, as the output texture is still required

## [0.1.1-preview] - 2020-04-10

### Changed
* api: open StreamingImageSequenceTrack to public 
* api: open StreamingImageSequencePlayableAsset::GetTexture() to public 
* docs: Update Japanese docs.

## [0.1.0-preview] - 2020-04-06

### Added
* markers to indicate the use/skipping of image in StreamingImageSequencePlayableAsset 

## [0.0.4-preview] - 2020-04-03

### Fixed
* fix runtime build errors 
* fix: avoid tests from modifying the project assets

## [0.0.3-preview] - 2020-03-27

### Added
* docs: Add Japanese docs

### Fixed
* fix: update DLLs to avoid the requirement of installing VCRUNTIME140_1.DLL 

## [0.0.2-preview.3] - 2020-03-16

### Changed
* docs: Updating img tag to MD

## [0.0.2-preview.2] - 2020-03-05

### Added
* feat: Fader imporvements. Reverse FadeOut and FadeIn, and a color to highlight FaderPlayableAsset

### Changed
* reverse the parameter to copy images to StreamingAssets
* Hide UseImageMarker and use Timeline 1.4.0's ClipCaps.AutoScale 
* change Test assembly definitions.

### Fixed
* fix StreamingImageSequencePlayableAsset stability issues. 
* fix: Support folder D&D for StreamingImageSequencePlayableAsset from folders which are not under "StreamingAssets"

## [0.0.2-preview.1] - 2020-03-02

### Added
* folder drag and drop support.
* preview icons

### Changed
* Renaming to *Streaming Image Sequence \<com.unity.streaming-image-sequence\>*.


## [0.0.1-preview] - 2019-10-10

### Added
The first release of *Movie Proxy \<com.unity.movie-proxy\>*.

