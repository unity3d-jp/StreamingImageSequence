using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using Unity.FilmInternalUtilities;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.StreamingImageSequence { 
/// <summary>
/// A track which requires its TimelineClip to store PlayableFrameClipData as an extension
/// </summary>
internal abstract class FrameMarkerTrack<T>: BaseExtendedClipTrack<BaseExtendedClipPlayableAsset<T>, T>
where T: PlayableFrameClipData, new()
{

    protected void DeleteInvalidMarkers() {
        foreach(IMarker m in GetMarkers()) {
            FrameMarker marker = m as FrameMarker;
            if (null == marker)
                continue;

            if (!marker.Refresh()) {
                m_markersToDelete.Add(marker);                
            }      
        }

        foreach (FrameMarker marker in m_markersToDelete) {
            DeleteMarker(marker);
        }
    }
   
//----------------------------------------------------------------------------------------------------------------------

        
    private readonly List<FrameMarker> m_markersToDelete = new List<FrameMarker>();
    
}

} //end namespace

//----------------------------------------------------------------------------------------------------------------------

