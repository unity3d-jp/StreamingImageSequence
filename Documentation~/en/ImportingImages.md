# Importing Images

There are a couple of ways to import images into StreamingImageSequencePlayableAsset

1. [Folder Drag and Drop to the track](#Folder Drag and Drop to the track)
1. [Importing a file exported from Adobe After Effects using our StreamingImageSequence script for Adobe After Effects](Using StreamingImageSequence plugin for Adobe After Effects)

# Folder Drag and Drop to the track

This is done by doing drag and drop of the sequential images folder under *StreamingAssets* to the StreamingImageSequenceTrack.

<img src="../images/DragAndDropStreamingAssets.png" width=960>  


# Using StreamingImageSequence script for Adobe After Effects

1. In Adobe After Effects, Run [StreamingImageSequence script for Adobe After Effects](https://github.com/unity3d-jp/StreamingImageSequence/tree/dev/AE~/Plugins) to export the images into a folder.
2. In Unity, click the menu: Assets -> StreamingImageSequence -> Import AE Timeline, and select the *jstimeline* file in the exported folder.

> This import will also create/setup the required Director and Image objects in the Assets/{jstimeline_name} folder.  
Importing a *jstimeline* which has the same file name as a previously imported *jstimeline* will overwrite the existing assets in the folder, 
so it is recommended to move these assets into another folder.






