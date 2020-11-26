using System.Collections.Generic;
using Unity.AnimeToolbox;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence {

// A PlayableBehaviour that is attached to a Track via CreateTrackMixer() 
internal class StreamingImageSequencePlayableMixer : BasePlayableMixer<StreamingImageSequencePlayableAsset> {

    public StreamingImageSequencePlayableMixer() {
        m_sisRenderer = null;
    }

    
//----------------------------------------------------------------------------------------------------------------------

#region IPlayableBehaviour interfaces

    public override void OnPlayableCreate(Playable playable) {
#if UNITY_EDITOR            
        m_editorUpdateTask = new SISPlayableMixerEditorUpdateTask(this);
        EditorUpdateManager.AddEditorUpdateTask( m_editorUpdateTask);
#endif //UNITY_EDITOR          
       
    }

    public override void OnPlayableDestroy(Playable playable) {
        
        IEnumerable<KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset>> clipAssets = GetClipAssets();
        foreach (KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset> kv in clipAssets) {
            StreamingImageSequencePlayableAsset sisAsset = kv.Value;
            sisAsset.OnPlayableDestroy(playable);
        }
        
        base.OnPlayableDestroy(playable);
        
#if UNITY_EDITOR            
        EditorUpdateManager.RemoveEditorUpdateTask( m_editorUpdateTask);        
#endif //UNITY_EDITOR          
    }

    
//----------------------------------------------------------------------------------------------------------------------
    public override void OnGraphStart(Playable playable) {
       
        IEnumerable<KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset>> clipAssets = GetClipAssets();
        foreach (KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset> kv in clipAssets) {
            StreamingImageSequencePlayableAsset sisAsset = kv.Value;                
            sisAsset.OnGraphStart(playable);                
        }
    }

//----------------------------------------------------------------------------------------------------------------------
    public override void OnGraphStop(Playable playable) {
        
        IEnumerable<KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset>> clipAssets = GetClipAssets();
        foreach (KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset> kv in clipAssets) {
            StreamingImageSequencePlayableAsset sisAsset = kv.Value;                
            sisAsset.OnGraphStop(playable);
        }
        
    }
    
//----------------------------------------------------------------------------------------------------------------------
    public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
        
        base.ProcessFrame(playable, info, playerData); // Calls ProcessActiveClipV()

#if UNITY_EDITOR            
        if (!Application.isPlaying) {
            return;
        }
#endif            
        
        //Preload images here only in play mode
        double directorTime = GetPlayableDirector().time;
        
        IEnumerable<KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset>> clipAssets = GetClipAssets();
        foreach (KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset> kv in clipAssets) {
            TimelineClip clip = kv.Key;
            StreamingImageSequencePlayableAsset sisAsset = kv.Value;

            int numImages  = sisAsset.GetNumImages();
            if (numImages <= 0|| null == clip.parentTrack)
                continue;

            double startTime = clip.start;
            double endTime = clip.end;
            double loadStartOffsetTime = 1.0f + numImages * 0.1f;

            //Start to preload images before the clip is active
            if ( directorTime>= startTime - loadStartOffsetTime && directorTime < endTime) {
                sisAsset.ContinuePreloadingImages();                    
            }

        }

    }

#endregion        


//---------------------------------------------------------------------------------------------------------------------
    protected override void ProcessActiveClipV(StreamingImageSequencePlayableAsset asset,
        double directorTime, TimelineClip activeClip) 
    {
        int numImages  = asset.GetNumImages();
        if (numImages <=0 || null == activeClip.parentTrack)
            return;
        
        int index = asset.GlobalTimeToImageIndex(activeClip, directorTime);
        Texture2D tex  = asset.RequestLoadImage(index);
        if (!tex.IsNullRef()) {
            m_sisRenderer.UpdateRendererTexture(tex);
        }

    }

//---------------------------------------------------------------------------------------------------------------------

    protected override void InitInternalV(GameObject gameObject) {
        if (null!=gameObject) {
            m_sisRenderer = gameObject.GetComponent<StreamingImageSequenceRenderer>();
            m_sisRenderer.InitRenderers();
        }
    }

//---------------------------------------------------------------------------------------------------------------------
    protected override void ShowObjectV(bool show) {
        if (m_sisRenderer.IsNullRef())
            return;
        
        m_sisRenderer.ShowRenderer(show);
    }
    
//---------------------------------------------------------------------------------------------------------------------
   
    private StreamingImageSequenceRenderer m_sisRenderer = null;
   
#if UNITY_EDITOR
    SISPlayableMixerEditorUpdateTask m_editorUpdateTask;
#endif

}

} //end namespace