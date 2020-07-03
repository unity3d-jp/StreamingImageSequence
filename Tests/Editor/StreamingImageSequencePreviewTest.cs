﻿using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Assert = NUnit.Framework.Assert;

namespace UnityEditor.StreamingImageSequence.Tests {

internal class StreamingImagePreviewTest {

    [Test]
    public void ViewNearShortClipAtBeginning() {

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
        Assert.AreEqual(0f, drawList[0].DrawRect.x);
        Assert.AreEqual(0f, drawList[0].LocalTime);

        float  xDiff = drawList[1].DrawRect.x - drawList[0].DrawRect.x;
        Assert.AreEqual(120.3333f, xDiff, EPSILON);

    }

//----------------------------------------------------------------------------------------------------------------------
    
    [Test]
    public void ViewNearShortClipAtEnd() {
        
        PreviewClipInfo clipInfo = new PreviewClipInfo() {
            Duration              = 0.1, //6 frames
            TimeScale             = 1,
            ClipIn                = 0,
            FramePerSecond        = 60,
            ImageDimensionRatio   = 1.684f,
            VisibleLocalStartTime =  0.0751406848430634,
            VisibleLocalEndTime   = 0.1,
            VisibleRect = new Rect() {
                x      = 1435,
                y      = 1,
                width  = 475,
                height = 25,
            },
            
        }; 
        List<PreviewDrawInfo> drawList = new List<PreviewDrawInfo>();
                
        PreviewUtility.EnumeratePreviewImages(ref clipInfo, (PreviewDrawInfo drawInfo) => {
            drawList.Add(drawInfo);
        });

        Assert.GreaterOrEqual(drawList.Count, 1);
        Assert.AreEqual(1591.562f, drawList[0].DrawRect.x,EPSILON);
        Assert.AreEqual(0.08333, drawList[0].LocalTime, EPSILON);
    }

    
//----------------------------------------------------------------------------------------------------------------------

    [Test]
    public void ViewNearScaledClipAtBeginning() {
        
        PreviewClipInfo clipInfo = new PreviewClipInfo() {
            Duration              = 30, //1800 frames
            TimeScale             = 1,
            ClipIn                = 0,
            FramePerSecond        = 60,
            ImageDimensionRatio   = 1.684f,
            VisibleLocalStartTime =  29.9543800354004,
            VisibleLocalEndTime   = 30,
            VisibleRect = new Rect() {
                x      = 276922,
                y      = 1,
                width  = 422,
                height = 25,
            },
            
        }; 
        List<PreviewDrawInfo> drawList = new List<PreviewDrawInfo>();
                
        PreviewUtility.EnumeratePreviewImages(ref clipInfo, (PreviewDrawInfo drawInfo) => {
            drawList.Add(drawInfo);
        });

        Assert.GreaterOrEqual(drawList.Count, 2);
        Assert.AreEqual(276881.4687f, drawList[0].DrawRect.x,EPSILON);
        Assert.AreEqual(29.95f, drawList[0].LocalTime, EPSILON);
        
        float xDiff = drawList[1].DrawRect.x - drawList[0].DrawRect.x;
        Assert.AreEqual(154.187f, xDiff, EPSILON);
        
    }

    
//----------------------------------------------------------------------------------------------------------------------
    [Test]
    public void ViewNearShortClipWithClipInAtBeginning() {
               
        PreviewClipInfo clipInfo = new PreviewClipInfo() {
            Duration              = 0.05, //3  frames
            TimeScale             = 1,
            ClipIn                = 0.05,
            FramePerSecond        = 60,
            ImageDimensionRatio   = 1.684f,
            VisibleLocalStartTime =  0.05,
            VisibleLocalEndTime   = 0.0834165304899216,
            VisibleRect = new Rect() {
                x      = 0,
                y      = 1,
                width  = 263,
                height = 25,
            },
            
        }; 
        List<PreviewDrawInfo> drawList = new List<PreviewDrawInfo>();
                
        PreviewUtility.EnumeratePreviewImages(ref clipInfo, (PreviewDrawInfo drawInfo) => {
            drawList.Add(drawInfo);
        });

        Assert.GreaterOrEqual(drawList.Count, 2);
        Assert.AreEqual(0, drawList[0].DrawRect.x,EPSILON);
        Assert.AreEqual(0.05, drawList[0].LocalTime, EPSILON);
        
        float xDiff = drawList[1].DrawRect.x - drawList[0].DrawRect.x;
        Assert.AreEqual(131.3333, xDiff, EPSILON);
        
    }

//----------------------------------------------------------------------------------------------------------------------
    
    [Test]
    public void ViewFarScaledClipWithClipIn() {
            
        PreviewClipInfo clipInfo = new PreviewClipInfo() {
            Duration              = 0.25, //15  frames
            TimeScale             = 1,
            ClipIn                = 0.25,
            FramePerSecond        = 60,
            ImageDimensionRatio   = 1.684f,
            VisibleLocalStartTime =  0.25,
            VisibleLocalEndTime   = 0.50,
            VisibleRect = new Rect() {
                x      = 0,
                y      = 1,
                width  = 652,
                height = 25,
            },
            
        }; 
        List<PreviewDrawInfo> drawList = new List<PreviewDrawInfo>();                
        PreviewUtility.EnumeratePreviewImages(ref clipInfo, (PreviewDrawInfo drawInfo) => {
            drawList.Add(drawInfo);
        });

        Assert.GreaterOrEqual(drawList.Count, 2);
        Assert.AreEqual(0f, drawList[0].DrawRect.x,EPSILON);
        Assert.AreEqual(0.25, drawList[0].LocalTime, EPSILON);
        
        float xDiff = drawList[1].DrawRect.x - drawList[0].DrawRect.x;
        Assert.AreEqual(43.46667, xDiff, EPSILON);
        
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    const float EPSILON = 0.001f;
    
    
}
} //end namespace
