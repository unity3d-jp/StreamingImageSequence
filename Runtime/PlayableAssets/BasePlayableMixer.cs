using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {

// A PlayableBehaviour that is attached to a Track via CreateTrackMixer() 
internal abstract class BasePlayableMixer<T> : PlayableBehaviour where T: PlayableAsset {

    #region IPlayableBehaviour interfaces
    
    public override void OnPlayableDestroy(Playables.Playable playable) {
        base.OnPlayableDestroy(playable);
        Destroy();
    }
    
    public override void PrepareFrame(Playable playable, FrameData info) {
        base.PrepareFrame(playable, info);
        if (null == m_boundGameObject)
            return;

        m_boundGameObject.SetActive(false); //Always hide first, and show it later 
    }
    

//----------------------------------------------------------------------------------------------------------------------
    // Called each frame while the state is set to Play
    public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
        int inputCount = playable.GetInputCount<Playable>();
        if (inputCount == 0 ) {
            return; // it doesn't work as mixer.
        }

        if (m_boundGameObject== null ) {
            return;
        }
        
        GetActiveTimelineClipInto(m_clips, m_playableDirector.time, out TimelineClip clip, out T activePlayableAsset);
        if (null == clip)
            return;
        
        ProcessActiveClipV(activePlayableAsset, m_playableDirector.time, clip);
        m_boundGameObject.SetActive(true);
        
    }

    #endregion IPlayableBehaviour interfaces
    
//----------------------------------------------------------------------------------------------------------------------

    public static void GetActiveTimelineClipInto( IEnumerable<TimelineClip> clips, double directorTime, 
        out TimelineClip outClip, out T outAsset) {
        
        foreach (TimelineClip clip in clips) {
            T asset = clip.asset as T;
            if (null == asset)
                continue;

            if ( directorTime >= clip.start && directorTime <= clip.end) {
                outClip = clip;
                outAsset = asset;
                return;
            }
        }

        outClip = null;
        outAsset = null;
    }

//----------------------------------------------------------------------------------------------------------------------

    internal void Init(GameObject go, PlayableDirector director, IEnumerable<TimelineClip> clips) {
        m_boundGameObject = go;
        m_playableDirector = director;
        
        m_clips = new List<TimelineClip>(clips);
        m_clipAssets = new Dictionary<TimelineClip, T>();
        foreach (var clip in m_clips) {
            T clipAsset = clip.asset as T;
            Assert.IsNotNull(clipAsset);
            m_clipAssets.Add(clip, clipAsset);
        }
        

        InitInternalV(go);
    }
    
//----------------------------------------------------------------------------------------------------------------------
    internal void Destroy() {
        m_clips.Clear();
        m_clipAssets.Clear();
        
    }

//----------------------------------------------------------------------------------------------------------------------
    internal double GetDirectorTime() { return m_playableDirector.time; }
    
//----------------------------------------------------------------------------------------------------------------------

    protected abstract void InitInternalV(GameObject boundGameObject);
    protected abstract void ProcessActiveClipV(T asset, double directorTime, TimelineClip activeClip);

//----------------------------------------------------------------------------------------------------------------------
    protected GameObject GetBoundGameObject() { return m_boundGameObject; }
    protected PlayableDirector GetPlayableDirector() { return m_playableDirector; }
    protected IEnumerable<TimelineClip> GetClips() { return m_clips; }
    internal IEnumerable<KeyValuePair<TimelineClip,T>> GetClipAssets() { return m_clipAssets; }

//----------------------------------------------------------------------------------------------------------------------

    private GameObject m_boundGameObject;
    private PlayableDirector m_playableDirector;
    private List<TimelineClip> m_clips;
    private Dictionary<TimelineClip, T> m_clipAssets;

}

} //end namespace