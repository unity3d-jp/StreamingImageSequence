using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence
{

// A behaviour that is attached to a playable
internal class FaderPlayableMixer : PlayableBehaviour {


#if false //PlayableBehaviour's functions that can be overridden

    // Called when the owning graph starts playing
    public override void OnGraphStart(Playable playable) {
    }

    // Called when the owning graph stops playing
    public override void OnGraphStop(Playable playable) {
    }

    // Called when the state of the playable is set to Play
    public override void OnBehaviourPlay(Playable playable, FrameData info) {

    }

    // Called when the state of the playable is set to Paused
    public override void OnBehaviourPause(Playable playable, FrameData info) {

    }

#endif
//----------------------------------------------------------------------------------------------------------------------

    // Called each frame while the state is set to Play
    public override void PrepareFrame(Playable playable, FrameData info) {
        base.PrepareFrame(playable, info);
        if (null == m_boundGameObject)
            return;

        m_boundGameObject.SetActive(false); //Always hide first, and show it later 
    }

//----------------------------------------------------------------------------------------------------------------------
    public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
        int inputCount = playable.GetInputCount<Playable>();
        if (inputCount == 0 ) {
            return; // it doesn't work as mixer.
        }
        if (m_boundGameObject== null ) {
            return;
        }

        // it is working as mixer.
        double directorTime = m_playableDirector.time;
        foreach (TimelineClip clip in m_clips) {
            FaderPlayableAsset asset = clip.asset as FaderPlayableAsset;
            if (null == asset)
                continue;

            if ( directorTime >= clip.start && directorTime <= clip.end) {
                DoSomething(asset, directorTime, clip);
                break;
            }
        }
    }

//----------------------------------------------------------------------------------------------------------------------
    void DoSomething(FaderPlayableAsset asset, double directorTime, TimelineClip activeClip) {
        Color color = asset.GetColor();
        float maxFade = color.a;

        float fade = (float)( ((directorTime - activeClip.start) / activeClip.duration ) * maxFade);
        if ( asset.GetFadeType() == FadeType.FADE_OUT) {
            fade = maxFade - fade;
        }

        Image image = m_boundGameObject.GetComponent<Image>();
        if ( image == null ) {
            return;
        }

        color.a = fade;
        image.color = color;
        m_boundGameObject.SetActive(true);

    }

//----------------------------------------------------------------------------------------------------------------------

    //[TODO-sin: 2020-3-3] the m_boundGameObject part is the same with FaderTrack. Do something
    public bool BindGameObject(GameObject go) {
        m_boundGameObject = go;
        return true;
    }

//----------------------------------------------------------------------------------------------------------------------
    private GameObject m_boundGameObject;

    internal PlayableDirector m_playableDirector;
    internal IEnumerable<TimelineClip> m_clips;

}

} //end namespace