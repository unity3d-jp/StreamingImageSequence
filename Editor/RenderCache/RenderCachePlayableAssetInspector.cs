using System;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {

/// <summary>
/// The inspector of RenderCachePlayableAsset
/// </summary>
[CustomEditor(typeof(RenderCachePlayableAsset))]
internal class RenderCachePlayableAssetInspector : Editor {

//----------------------------------------------------------------------------------------------------------------------
    void OnEnable() {
        m_asset = target as RenderCachePlayableAsset;
    }

//----------------------------------------------------------------------------------------------------------------------

    public override void OnInspectorGUI() {
        

    }


//----------------------------------------------------------------------------------------------------------------------
    private RenderCachePlayableAsset m_asset = null;

}

}