using System.Collections;
using System.IO;
using NUnit.Framework;
using Unity.EditorCoroutines.Editor;
using UnityEditor.Timeline;
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

            TimelineClip timelineClip = m_asset.GetTimelineClip();
            TrackAsset track = timelineClip.parentTrack;
            TimelineAsset timelineAsset = track.timelineAsset;
            m_director = FindDirectorInScene(timelineAsset);
            if (null == m_director) {
                EditorUtility.DisplayDialog("Streaming Image Sequence",
                    "PlayableAsset is not loaded in scene. Please load the correct scene before doing this operation.",
                    "Ok");
                return;
            }

            m_camera = m_director.GetGenericBinding(track) as Camera;
            if (null == m_camera) {
                EditorUtility.DisplayDialog("Streaming Image Sequence",
                    "Please bind a camera to the playable asset.",
                    "Ok");
                return;                
            }

            
            //Loop time             
            m_timePerFrame = 1.0f / timelineAsset.editorSettings.fps;
            EditorCoroutineUtility.StartCoroutine(UpdateRenderCacheCoroutine(), this);
                        
        }

    }

//----------------------------------------------------------------------------------------------------------------------
    IEnumerator UpdateRenderCacheCoroutine() {
        Assert.IsNotNull(m_camera);
        Assert.IsNotNull(m_director);
        
        //Assign custom render texture to camera
        RenderTexture prevTargetTexture = m_camera.targetTexture;
        RenderTexture rt = new RenderTexture(m_camera.pixelWidth, m_camera.pixelHeight, 24);
        rt.Create();
        m_camera.targetTexture = rt;

        TimelineClip timelineClip = m_asset.GetTimelineClip();
        m_nextDirectorTime = timelineClip.start;
        
        int  fileCounter = 0;
        while (m_nextDirectorTime <= timelineClip.end) {
            SetDirectorTime(m_director, m_nextDirectorTime);
            yield return null;

            Capture(m_camera, fileCounter.ToString("000"));
            m_nextDirectorTime += m_timePerFrame;
            
            Debug.Log("Time: " + m_nextDirectorTime + " " +
                (m_nextDirectorTime < m_director.initialTime + m_director.duration).ToString());
            ++fileCounter;
        }
        
        //Reset camera's target texture
        m_camera.targetTexture = prevTargetTexture;


    }
 
//----------------------------------------------------------------------------------------------------------------------
    
    private static void Capture(Camera cam, string fileCounter) {
        
        
        RenderTexture prevRenderTexture = RenderTexture.active;
  
        RenderTexture camRT = cam.targetTexture;
        RenderTexture.active = camRT;
 
        cam.Render();

        
        Texture2D image = new Texture2D(camRT.width, camRT.height);
        image.ReadPixels(new Rect(0, 0, camRT.width, camRT.height), 0, 0);
        image.Apply();
 
        byte[] bytes = image.EncodeToPNG();
        ObjectUtility.Destroy(image);
 
        File.WriteAllBytes(Application.dataPath + "/StreamingAssets/" + fileCounter + ".png", bytes);
        
        //Set back
        RenderTexture.active = prevRenderTexture;
        
    }

//----------------------------------------------------------------------------------------------------------------------
    private static void SetDirectorTime(PlayableDirector director, double time) {
        director.time = time;
        TimelineEditor.Refresh(RefreshReason.ContentsModified); 
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
    private PlayableDirector m_director = null;
    private Camera m_camera = null;
    private double m_nextDirectorTime = 0;
    private double m_timePerFrame       = 0;

}

}