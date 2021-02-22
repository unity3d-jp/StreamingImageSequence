﻿using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using Unity.FilmInternalUtilities;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.StreamingImageSequence { 
/// <summary>
/// A track which requires its TimelineClip to store SISClipData as an extension
/// </summary>
internal abstract class BaseTimelineClipSISDataTrack: BaseExtendedClipTrack<BaseExtendedClipPlayableAsset<SISClipData>, SISClipData>   
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

