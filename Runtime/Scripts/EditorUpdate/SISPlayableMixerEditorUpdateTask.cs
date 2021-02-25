
#if UNITY_EDITOR        

using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence
{
internal class SISPlayableMixerEditorUpdateTask : IUpdateTask { 

    public SISPlayableMixerEditorUpdateTask(StreamingImageSequencePlayableMixer mixer) : base() {
        m_mixer = mixer;
    }

//----------------------------------------------------------------------------------------------------------------------    
    public void Reset() {
    }

//----------------------------------------------------------------------------------------------------------------------    
    public void Execute() {

        //Only continue preloading images when we are not in play mode 
        if (Application.isPlaying)
            return;
        
        Assert.IsNotNull(m_mixer);

        var  clipAssets   = m_mixer.GetClipAssets();
        bool needsRefresh = false;
        foreach (KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset> kv in clipAssets) {
            StreamingImageSequencePlayableAsset sisAsset = kv.Value;
            sisAsset.ContinuePreloadingImages();

            if (sisAsset.UpdateTextureWithRequestedImage()) {
                needsRefresh = true;
            }
        }

        if (needsRefresh) {
            TimelineEditor.Refresh(RefreshReason.ContentsModified);            
        }

    }

    
//----------------------------------------------------------------------------------------------------------------------

    readonly StreamingImageSequencePlayableMixer  m_mixer;

}

} //end namespace

#endif //UNITY_EDITOR