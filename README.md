## Other Languages
* [日本語](README_JP.md)

# Streaming Image Sequence

[![](https://badge-proxy.cds.internal.unity3d.com/44fe9b4e-feeb-409f-8fcd-d86e42d09b98)](https://badges.cds.internal.unity3d.com/packages/com.unity.streaming-image-sequence/build-info?branch=dev&testWorkflow=package-isolation)
[![](https://badge-proxy.cds.internal.unity3d.com/f9703ad6-4a57-4861-8125-4cec53ece26f)](https://badges.cds.internal.unity3d.com/packages/com.unity.streaming-image-sequence/dependencies-info?branch=dev&testWorkflow=updated-dependencies)
[![](https://badge-proxy.cds.internal.unity3d.com/221d27f5-9807-40c1-8fde-50a1757801b4)](https://badges.cds.internal.unity3d.com/packages/com.unity.streaming-image-sequence/dependants-info)
[![](https://badge-proxy.cds.internal.unity3d.com/1f8f02a0-6e03-417e-9c92-22f978f96c69)](https://badges.cds.internal.unity3d.com/packages/com.unity.streaming-image-sequence/warnings-info?branch=dev)

![ReleaseBadge](https://badge-proxy.cds.internal.unity3d.com/2fe2fc7a-99f7-4bde-b23d-c3358e846fac)
![ReleaseBadge](https://badge-proxy.cds.internal.unity3d.com/84b887b2-1e62-4962-848e-9d6c07023710)

Streaming Image Sequence is a package for playing sequential image sequences in 
Unity Timeline easily without making Unity 2D Sprites.  

**Using Timeline 1.4.x or above is recommended.**

Streaming Image Sequence is designed with the following principles in mind:

1. Can avoid texture importing time entirely by using 
   [StreamingAssets](https://docs.unity3d.com/Manual/StreamingAssets.html).
1. Offers smooth image playback, both in play mode and in timeline editing mode.
1. Supports multiple OS.


Streaming Image Sequence is currently a preview package and the steps to install it 
differ based on the version of Unity.

* Unity 2019.x  
  ![PackageManager2019](Documentation~/images/PackageManager2019.png)
  1. Open [Package Manager](https://docs.unity3d.com/Manual/upm-ui.html) 
  2. Ensure that **Show preview packages** is checked. 
  3. Search for *Streaming Image Sequence*.
  
* Unity 2020.1  
  ![PackageManager2020](Documentation~/images/PackageManager2020.1.png)
  1. Open [Package Manager](https://docs.unity3d.com/Manual/upm-ui.html) 
  2. Click the **+** button, and choose **Add package from git URL** 
  3. Type in `com.unity.streaming-image-sequence@` followed by the version.  
     For example: `com.unity.streaming-image-sequence@0.3.2-preview`
  
## Supported Platforms

1. Windows
2. Mac

## Features

1. ##### [Playing Sequential Images](./Documentation~/en/StreamingImageSequencePlayableAsset.md)

   ![StreamingImageSequenceDemo](Documentation~/images/StreamingImageSequenceDemo.gif)

2. ##### [Caching Render Results](./Documentation~/en/RenderCachePlayableAsset.md)

   ![RenderCacheDemo](Documentation~/images/RenderCacheDemo.gif)

3. ##### [Fading Image objects](./Documentation~/en/FaderPlayableAsset.md)

   ![FaderDemo](Documentation~/images/FaderDemo.gif)

## Memory

StreamingImageSequence allocates physical memory to ensure smooth image playback.  
This allocation is set to satisfy the following requirements:
1. Does not exceed 90% of the total physical memory of the system.
2. Does not exceed the maximum amount of memory, which can be configured on the
**Edit > Preferences** window.

![Preferences](Documentation~/images/Preferences.png)

| Legend  | Use                                                                                       | 
| ------- | ---------------------------------------------------------------------- | 
| A       | Preferences to configure                                               |   
| B       | Currently applied values                                               |   
| C       | Apply and save                                                         |  

## Plugins
* [Building](Plugins~/Docs/en/BuildPlugins.md)

## License
* Source Code: [Unity Companion License](LICENSE.md)
* Third-Party Software Components: [Third Party Notices](Third%20Party%20Notices.md)
* Unity-chan Assets: [Unity-Chan License](http://unity-chan.com/contents/guideline_en/)  
  These assets can be located under, but not limited to, the following folder:
  - `AE~/Samples`
  - `StreamingImageSequence~/Assets/StreamingAssets`  

# Tutorial Videos
* [Usage Video](https://youtu.be/mlRbwqJ74CM)
* [Example](https://youtu.be/4og6rgQdb3c)


