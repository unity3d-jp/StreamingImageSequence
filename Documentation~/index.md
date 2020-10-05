# Streaming Image Sequence

Streaming Image Sequence is a package for playing sequential image sequences in Unity Timeline 
easily without making Unity 2D Sprites.  
It is designed with the following principles in mind:

1. Can avoid texture importing time entirely by using 
   [StreamingAssets](https://docs.unity3d.com/Manual/StreamingAssets.html).
1. Offers smooth image playback, both in play mode and in timeline editing mode.
1. Supports multiple OS.

**Using Timeline 1.4.x or above is recommended.**

## Supported Platforms

1. Windows
2. Mac


## Features

1. #### [Playing Sequential Images](en/StreamingImageSequencePlayableAsset.md)

   ![StreamingImageSequenceDemo](images/StreamingImageSequenceDemo.gif)

2. #### [Caching Render Results](en/RenderCachePlayableAsset.md)

   ![RenderCacheDemo](images/RenderCacheDemo.gif)

3. #### [Fading Image objects](en/FaderPlayableAsset.md)

   ![FaderDemo](images/FaderDemo.gif)

## Memory

StreamingImageSequence allocates physical memory to ensure smooth image playback.    
This allocation is set to satisfy the following requirements:
1. Does not exceed 90% of the total physical memory of the system.
2. Does not exceed the maximum amount of memory, which can be configured on the
**Edit > Preferences** window.

![Preferences](images/Preferences.png)

| Legend  | Use                                                                                       | 
| ------- | ---------------------------------------------------------------------- | 
| A       | Preferences to configure                                               |   
| B       | Currently applied values                                               |   
| C       | Apply and save                                                         |  


## Other Languages
- [日本語](jp/index.md)





