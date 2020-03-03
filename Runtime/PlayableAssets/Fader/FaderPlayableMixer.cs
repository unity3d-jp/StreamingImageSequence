using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence
{

// A behaviour that is attached to a playable
internal class FaderPlayableMixer : PlayableBehaviour
{
    internal PlayableDirector m_PlayableDirector;
    internal Color m_initialColor;
    internal IEnumerable<TimelineClip> m_clips;

  

    // Called when the owning graph starts playing
    public override void OnGraphStart(Playable playable)
    {
        SetInitialColor(playable);
    }

    // Called when the owning graph stops playing
    public override void OnGraphStop(Playable playable)
    {
        SetInitialColor(playable);
    }

#if false
    // Called when the state of the playable is set to Play
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {

    }

    // Called when the state of the playable is set to Paused
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {

    }

#endif
//----------------------------------------------------------------------------------------------------------------------

    // Called each frame while the state is set to Play
    public override void PrepareFrame(Playable playable, FrameData info)
    {
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

        Image image = m_boundGameObject.GetComponent<Image>();
        if ( image == null ) {
            return;
        }

        TimelineClip activeClip = null;

        float fade = m_initialColor.a;
        Color color = m_initialColor;

        // it is working as mixer.
        double directorTime = m_PlayableDirector.time;
        var enumulator = m_clips.GetEnumerator();
        enumulator.MoveNext();
        for (int ii = 0; ii < inputCount; ii++, enumulator.MoveNext()) {

            var clip = enumulator.Current; 
            var asset = clip.asset as FaderPlayableAsset;

            color = asset.m_color;
            if ( directorTime >= clip.start && directorTime <= clip.end) {
                fade = (float)((directorTime - clip.start) / clip.duration);
                if ( asset.m_type == FadeType.FadeOut)
                {
                    fade = 1.0f - fade;
                }

                activeClip = clip;
                break;
            }
        }
        color.a = fade;
        image.color = color;

        //Hide/Show
        if (null != activeClip) {
            m_boundGameObject.SetActive(true);
        }
    }

//----------------------------------------------------------------------------------------------------------------------

    //[TODO-sin: 2020-3-3] the m_boundGameObject part is the same with FaderTrack. Do something
    public bool BindGameObject(GameObject go) {
        m_boundGameObject = go;
        return true;
    }

//----------------------------------------------------------------------------------------------------------------------

    private void SetInitialColor(Playable playable)
    {
        if (m_boundGameObject == null) {
            return;
        }

        Image image = m_boundGameObject.GetComponent<Image>();
        IEnumerator<TimelineClip> enumlator = m_clips.GetEnumerator();
        enumlator.MoveNext();
        if (playable.GetInputCount<Playable>() > 0)
        {
            var myScriptPlayable = (ScriptPlayable<FaderPlayableBehaviour>)playable.GetInput(0);
            var myBehaviour = myScriptPlayable.GetBehaviour();
            if (myBehaviour == null )
            {
                return;
            }
            TimelineClip clip = enumlator.Current;
            FaderPlayableAsset asset = clip.asset as FaderPlayableAsset;

            m_initialColor = asset.m_color;

            if (image != null ) {
                image.color = m_initialColor;
            }
        }

    }

//----------------------------------------------------------------------------------------------------------------------
    private GameObject m_boundGameObject;

}

} //end namespace