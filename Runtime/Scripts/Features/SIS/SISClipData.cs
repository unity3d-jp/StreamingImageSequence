using System;
using JetBrains.Annotations;
using Unity.FilmInternalUtilities;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.StreamingImageSequence {

[Serializable]
internal class SISClipData : PlayableFrameClipData, IAnimationCurveOwner {

    public SISClipData() : base() { }

    internal SISClipData(TimelineClip clipOwner) : base(clipOwner) { }

    internal SISClipData(TimelineClip owner, SISClipData other) : base(owner, other) { }

//----------------------------------------------------------------------------------------------------------------------    
    
    //[Note-sin: 2021-3-17] Cache TimelineClip.curve. If the value differs, then it will be overwritten
    public  void           SetAnimationCurve(AnimationCurve curve) { m_animationCurve = curve; }
    
    [CanBeNull]
    public  AnimationCurve GetAnimationCurve()                     {  return m_animationCurve; }

    public float CalculateCurveDuration() {
        if (null == m_animationCurve || m_animationCurve.length <= 0)
            return 0;

        return Mathf.Abs(m_animationCurve.keys[m_animationCurve.length - 1].time - m_animationCurve.keys[0].time);

    }

    internal void SetCurveDurationInEditor(float newDuration) {

        TimelineClip clip = GetOwner();
        Assert.IsNotNull(clip);

        float prevCurveDuration = CalculateCurveDuration();
        if (Mathf.Approximately(prevCurveDuration, 0) || Mathf.Approximately(prevCurveDuration, newDuration))
            return;
        
        float timeScale = newDuration / prevCurveDuration;        
        Keyframe[] keys    = m_animationCurve.keys;
        int        numKeys = keys.Length;
        for (int i = 0; i < numKeys; ++i) {
            keys[i].time       *= timeScale;
            keys[i].inTangent  /= timeScale;
            keys[i].outTangent /= timeScale;
        }

        m_animationCurve.keys = keys;

        //Set to clip
#if UNITY_EDITOR        
        EditorCurveBinding curveBinding = StreamingImageSequencePlayableAsset.GetTimeCurveBinding();                 
        AnimationUtility.SetEditorCurve(clip.curves, curveBinding, m_animationCurve);
#endif        
        



    }


//----------------------------------------------------------------------------------------------------------------------    
    
#pragma warning disable 414    
    [HideInInspector][SerializeField] private int m_sisClipDataVersion = CUR_SIS_CLIP_DATA_VERSION;        
#pragma warning restore 414    
    
    [SerializeField] private AnimationCurve m_animationCurve;
    
    
    private const int CUR_SIS_CLIP_DATA_VERSION = 1;
    
}

} //end namespace


