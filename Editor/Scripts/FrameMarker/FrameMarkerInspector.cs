using System.IO;
using NUnit.Framework;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.StreamingImageSequence;
using UnityObject = UnityEngine.Object;


namespace UnityEditor.StreamingImageSequence {

[CustomEditor(typeof(FrameMarker), true)]
[CanEditMultipleObjects]
internal class FrameMarkerInspector: Editor {

    void OnEnable() {
        int numTargets = targets.Length;
        m_assets = new FrameMarker[numTargets];
        for (int i = 0; i < numTargets; i++) {
            m_assets[i] = targets[i] as FrameMarker;
        }
    }

//----------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI() {
        ShortcutBinding useFrameShortcut 
            = ShortcutManager.instance.GetShortcutBinding(SISEditorConstants.SHORTCUT_TOGGLE_FRAME_MARKER);            
        bool prevUseFrame= m_assets[0].IsFrameUsed();
        bool useFrame = EditorGUILayout.Toggle($"Use Frame ({useFrameShortcut})", prevUseFrame);
        if (useFrame != prevUseFrame) {
            //Set all selected objects
            foreach (FrameMarker m in m_assets) {
                SetMarkerValueByContext(m,useFrame);
            }            
        }
               
        //Only show lock and edit for RenderCachePlayableAsset
        foreach (FrameMarker frameMarker in m_assets) {
            SISPlayableFrame playableFrame       = frameMarker.GetOwner();
            RenderCachePlayableAsset playableAsset = playableFrame.GetTimelineClipAsset<RenderCachePlayableAsset>();            
            if (null == playableAsset)
                return;        
        }        
        
        //m_assets only contain RenderCachePlayableAsset at this point
        ShortcutBinding lockAndEditShortcut 
            = ShortcutManager.instance.GetShortcutBinding(SISEditorConstants.SHORTCUT_LOCK_AND_EDIT_FRAME);            
        if (GUILayout.Button($"Lock and Edit ({lockAndEditShortcut})")) {
            foreach (FrameMarker frameMarker in m_assets) {
                SISPlayableFrame playableFrame       = frameMarker.GetOwner();
                RenderCachePlayableAsset playableAsset = playableFrame.GetTimelineClipAsset<RenderCachePlayableAsset>();
                Assert.IsNotNull(playableAsset);
                LockAndEditPlayableFrame(playableFrame, playableAsset);
            }        
            
        }
    }
    
//----------------------------------------------------------------------------------------------------------------------

    internal static void LockAndEditPlayableFrame(SISPlayableFrame playableFrame, 
        RenderCachePlayableAsset renderCachePlayableAsset) 
    {
        int    index    = playableFrame.GetIndex();
        string filePath = renderCachePlayableAsset.GetImageFilePath(index);
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) {
            EditorUtility.DisplayDialog(StreamingImageSequenceConstants.DIALOG_HEADER,
                "Please update RenderCachePlayableAsset.",
                "Ok");
            return;
        }        
                    
        string fullPath = Path.GetFullPath(filePath);
        playableFrame.SetLocked(true);
        string imageAppPath = EditorPrefs.GetString("kImagesDefaultApp");
        if (string.IsNullOrEmpty(imageAppPath) || !File.Exists(imageAppPath)) {
            System.Diagnostics.Process.Start(fullPath);
            return;
        }
        
        System.Diagnostics.Process.Start(imageAppPath, fullPath);
      
    } 
    

//----------------------------------------------------------------------------------------------------------------------
    private static void SetMarkerValueByContext(FrameMarker frameMarker, bool value) {
        SISPlayableFrame    playableFrame       = frameMarker.GetOwner();
        TimelineClipSISData timelineClipSISData = playableFrame.GetOwner();
        PlayableFramePropertyID inspectedPropertyID = timelineClipSISData.GetInspectedProperty();
        switch (inspectedPropertyID) {
            case PlayableFramePropertyID.USED: {
                playableFrame.SetUsed(value);
                break;
            }
            case PlayableFramePropertyID.LOCKED: {
                playableFrame.SetLocked(value);
                break;
            }
            
        }
               
    }

    internal static void ToggleMarkerValueByContext(FrameMarker frameMarker) {
        SISPlayableFrame    playableFrame         = frameMarker.GetOwner();
        TimelineClipSISData timelineClipSISData   = playableFrame.GetOwner();
        PlayableFramePropertyID inspectedPropertyID = timelineClipSISData.GetInspectedProperty();
        switch (inspectedPropertyID) {
            case PlayableFramePropertyID.USED: {
                playableFrame.SetUsed(!playableFrame.IsUsed());
                break;
            }
            case PlayableFramePropertyID.LOCKED: {
                playableFrame.SetLocked(!playableFrame.IsLocked());
                break;
            }

        }
    }
//----------------------------------------------------------------------------------------------------------------------

    private FrameMarker[] m_assets = null;
}

} //end namespace

