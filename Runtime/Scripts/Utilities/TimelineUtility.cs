using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {


internal static class TimelineUtility {

//----------------------------------------------------------------------------------------------------------------------
    internal static int CalculateNumFrames(TimelineClip clip) {
        float fps       = clip.parentTrack.timelineAsset.editorSettings.fps;
        int   numFrames = Mathf.RoundToInt((float)(clip.duration * fps));
        return numFrames;
            
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    internal static double CalculateTimePerFrame(TimelineClip clip) {
        float  fps          = clip.parentTrack.timelineAsset.editorSettings.fps;
        double timePerFrame = 1.0f / fps;
        return timePerFrame;
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    public static List<TrackAsset> GetTrackList(TimelineAsset timelineAsset) {
        Assert.IsTrue(timelineAsset == true);
        var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField |
            BindingFlags.GetProperty;
        var type = timelineAsset.GetType();

        var info = type.GetProperty("tracks", bf); // 2017.1 tracks
        if (info == null) {
            //  newer version
            info = type.GetProperty("trackObjects", bf);
        }

        Assert.IsTrue(info.PropertyType.IsGenericType);
        var list           = info.GetValue(timelineAsset, null);
        var trackAssetList = list as List<TrackAsset>;
        if (trackAssetList != null) {
            return trackAssetList;
        }


        var scriptableObjectList = list as List<ScriptableObject>;
        var ret                  = new List<TrackAsset>();
        foreach (var asset in scriptableObjectList) {
            ret.Add(asset as TrackAsset);
        }

        return ret;
    }

    public static List<TrackAsset> GetTrackList(GroupTrack groupTrack) {

        var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;
        var type = groupTrack.GetType().BaseType;
        var fieldInfo = type.GetField("m_Children", bf);
        var clips = fieldInfo.GetValue(groupTrack);
        var trackAssetList = clips as List<TrackAsset>;
        if (trackAssetList != null) {
            return trackAssetList; // 2017.1 tracks
        }

        // later version. 
        var scriptableObjectList = clips as List<ScriptableObject>;
        var ret                  = new List<TrackAsset>();
        foreach (var asset in scriptableObjectList) {
            ret.Add(asset as TrackAsset);
        }

        return ret;
    }

}

} //ena namespace
