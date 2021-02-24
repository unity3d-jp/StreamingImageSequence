using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FilmInternalUtilities {


public interface IAnimationCurveOwner {
    void SetAnimationCurve(AnimationCurve curve);
    AnimationCurve  GetAnimationCurve();
}

} //end namespace
