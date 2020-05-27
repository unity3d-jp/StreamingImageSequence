using System;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.StreamingImageSequence;
using UnityEngine.UIElements;

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
        //[TODO-sin: 2020-5-27] Check the MD5 hash of the folder before overwriting
        if (GUILayout.Button("Refresh")) {
            Debug.Log("Clicked the image");
        }

    }


//----------------------------------------------------------------------------------------------------------------------
    private RenderCachePlayableAsset m_asset = null;

}

}