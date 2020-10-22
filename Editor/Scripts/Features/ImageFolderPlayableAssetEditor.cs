using UnityEditor.Timeline;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence.Editor {

internal abstract class ImageFolderPlayableAssetEditor<T> : ClipEditor where T: ImageFolderPlayableAsset{

    //Called when a clip is changed by the Editor. (TrimStart, TrimEnd, etc)    
    public override void OnClipChanged(TimelineClip clip) {       
        base.OnClipChanged(clip);
                        
        T imageFolderPlayableAsset = clip.asset as T;
        Assert.IsNotNull(imageFolderPlayableAsset);
        imageFolderPlayableAsset.RefreshPlayableFrames();            
    }

    protected abstract void DrawPreviewImageV(ref PreviewDrawInfo drawInfo, TimelineClip clip,
        T playableAsset); 

}

}
