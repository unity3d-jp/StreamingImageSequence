using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Assert = NUnit.Framework.Assert;

namespace UnityEditor.StreamingImageSequence.Tests {

internal class StreamingImagePreviewTest {

    [Test]
    public void ShortSceneClipAtBeginning() {

        PreviewClipInfo clipInfo = new PreviewClipInfo() {
            Duration              = 0.1, //6 frames
            TimeScale             = 1,
            ClipIn                = 0,
            FramePerSecond        = 60,
            ImageDimensionRatio   = 1.684f,
            VisibleLocalStartTime =  0,
            VisibleLocalEndTime   = 0.1,
            VisibleRect           = new Rect() {
                x = 0,
                y = 1,
                width = 722,
                height = 25,
            },
            
        }; 
        List<PreviewDrawInfo> drawList = new List<PreviewDrawInfo>();
                
        PreviewUtility.EnumeratePreviewImages(ref clipInfo, (PreviewDrawInfo drawInfo) => {
            drawList.Add(drawInfo);
        });

        Assert.GreaterOrEqual(drawList.Count, 1);
        Assert.AreEqual(drawList[0].LocalTime,0f);
        Assert.AreEqual(drawList[0].DrawRect.x,0f);

        float  xDiff = drawList[1].DrawRect.x - drawList[0].DrawRect.x;
        Assert.AreEqual(xDiff,120.3333f, 0.0001f);

    }
    
}
} //end namespace
