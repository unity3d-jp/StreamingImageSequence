using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

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
        if (GUILayout.Button("Update Render Cache")) {

            TimelineAsset timelineAsset = m_asset.GetTimelineClip().parentTrack.timelineAsset;
            PlayableDirector director = FindDirectorInScene(timelineAsset);
            if (null == director) {
                EditorUtility.DisplayDialog("Streaming Image Sequence",
                    "PlayableAsset is not loaded in scene. Please load the correct scene before doing this operation",
                    "Ok");
                return;
            }
            

            
            
            Debug.Log("Director found");
        }

    }
//----------------------------------------------------------------------------------------------------------------------

    PlayableDirector FindDirectorInScene(TimelineAsset timelineAsset) {
        PlayableDirector[] directors = UnityEngine.Object.FindObjectsOfType<PlayableDirector>();
        foreach (PlayableDirector director in directors) {
            if (timelineAsset == director.playableAsset) {
                return director;
            }            
        }

        return null;
    }


//----------------------------------------------------------------------------------------------------------------------
    private RenderCachePlayableAsset m_asset = null;

}

}