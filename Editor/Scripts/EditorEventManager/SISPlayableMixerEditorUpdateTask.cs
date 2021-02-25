

using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence.Editor
{
internal class SISPlayableMixerEditorUpdateTask : IUpdateTask { 

    public SISPlayableMixerEditorUpdateTask(StreamingImageSequencePlayableMixer mixer) : base() {
        m_mixer = mixer;
    }

//----------------------------------------------------------------------------------------------------------------------    
    public void Reset() {
    }


//----------------------------------------------------------------------------------------------------------------------    

    //[Note-sin: 2021-2-5] There should be only one task that accesses the same SISPlayableMixer
    //So, overwrite equals to help that check    
    public static bool operator== (SISPlayableMixerEditorUpdateTask obj1, SISPlayableMixerEditorUpdateTask obj2) {
               
        Assert.IsNotNull(obj1);
        Assert.IsNotNull(obj2);
                
        return (obj1.m_mixer == obj2.m_mixer); 
    }
    
    public static bool operator!= (SISPlayableMixerEditorUpdateTask obj1, SISPlayableMixerEditorUpdateTask obj2) {
        Assert.IsNotNull(obj1);
        Assert.IsNotNull(obj2);
                
        return (obj1.m_mixer != obj2.m_mixer); 
    }    
    
    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;

        SISPlayableMixerEditorUpdateTask otherTask = obj as SISPlayableMixerEditorUpdateTask;
        Assert.IsNotNull(otherTask);
        return (this.m_mixer == otherTask.m_mixer); 
    }
    
    public override int GetHashCode() {
        unchecked {
            return m_mixer.GetHashCode();
        }
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

