using System;
using Unity.FilmInternalUtilities;
using UnityEngine;
using UnityEngine.Timeline;


namespace Unity.StreamingImageSequence {

[Serializable]
internal class SISClipData : PlayableFrameClipData, IAnimationCurveOwner {

    public SISClipData() : base() { }

    internal SISClipData(TimelineClip clipOwner) : base (clipOwner) {}

    internal SISClipData(TimelineClip owner, SISClipData other) : base(owner, other) {}

//----------------------------------------------------------------------------------------------------------------------    
    public  void           SetAnimationCurve(AnimationCurve curve) { m_animationCurve = curve; }
    public  AnimationCurve GetAnimationCurve()                     {  return m_animationCurve; }
    

//----------------------------------------------------------------------------------------------------------------------    
    
#pragma warning disable 414    
    [HideInInspector][SerializeField] private int m_sisClipDataVersion = CUR_SIS_CLIP_DATA_VERSION;        
#pragma warning restore 414    
    
    [SerializeField] private AnimationCurve m_animationCurve;
    
    
    private const int CUR_SIS_CLIP_DATA_VERSION = 1;
    
}

} //end namespace


