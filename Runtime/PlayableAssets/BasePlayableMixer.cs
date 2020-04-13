using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {

// A PlayableBehaviour that is attached to a Track via CreateTrackMixer() 
internal abstract class BasePlayableMixer<T> : PlayableBehaviour where T: PlayableAsset {

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
        
        GetActiveTimelineClipInto(out TimelineClip clip, out T activePlayableAsset);
        if (null == clip)
            return;
        
        ProcessActiveClipV(activePlayableAsset, m_playableDirector.time, clip);
        m_boundGameObject.SetActive(true);
        
    }

//----------------------------------------------------------------------------------------------------------------------

    public void GetActiveTimelineClipInto( out TimelineClip outClip, out T outAsset) {
        
        double directorTime = m_playableDirector.time;
        foreach (TimelineClip clip in m_clips) {
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
        m_clips = clips;

        InitInternalV(go);
    }

//----------------------------------------------------------------------------------------------------------------------

    protected abstract void InitInternalV(GameObject boundGameObject);
    protected abstract void ProcessActiveClipV(T asset, double directorTime, TimelineClip activeClip);

//----------------------------------------------------------------------------------------------------------------------
    protected GameObject GetBoundGameObject() { return m_boundGameObject; }
    protected PlayableDirector GetPlayableDirector() { return m_playableDirector; }
    protected IEnumerable<TimelineClip> GetClips() { return m_clips; }

//----------------------------------------------------------------------------------------------------------------------

    private GameObject m_boundGameObject;
    private PlayableDirector m_playableDirector;
    private IEnumerable<TimelineClip> m_clips;

}

} //end namespace