﻿using UnityEngine;

namespace UnityEditor.StreamingImageSequence {

internal struct PreviewClipInfo {
    //Taken from TimelineClip
    public double Duration;
    public double TimeScale;
    public double ClipIn;
    public float FramePerSecond;
    public float ImageDimensionRatio;

    //What is visible on the TimelineWindow
    public double VisibleLocalStartTime;
    public double VisibleLocalEndTime;
    public Rect  VisibleRect;
    
    
}
} //end namespace
