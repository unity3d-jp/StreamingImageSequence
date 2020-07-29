using System;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {

//[TODO-sin: 2020-7-16] Replaced by SISPlayableFrame.
//This is a dummy structure so that opening assets/scenes created using 
//StreamingImageSequence up to 0.3.x don't produce any errors.

[Serializable]
internal class PlayableFrame : ScriptableObject {

}

} //end namespace


