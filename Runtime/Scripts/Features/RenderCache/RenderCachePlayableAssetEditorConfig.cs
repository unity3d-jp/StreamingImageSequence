using UnityEngine;

namespace Unity.StreamingImageSequence {

[System.Serializable]
internal class RenderCachePlayableAssetEditorConfig {
    
//----------------------------------------------------------------------------------------------------------------------

    internal void  SetUpdateBGColor(Color color) { m_updateBGColor = color; }
    internal Color GetUpdateBGColor()            { return m_updateBGColor; }

    internal void SetCaptureStartFrame(int frame) { m_captureStartFrame = frame;}
    internal int  GetCaptureStartFrame()          { return m_captureStartFrame;}
    internal void SetCaptureEndFrame(int frame)   { m_captureEndFrame = frame;}
    internal int  GetCaptureEndFrame()            { return m_captureEndFrame;}

    internal void SetCaptureAllFrames(bool capture){ m_captureAllFrames = capture;}
    internal bool GetCaptureAllFrames()            { return m_captureAllFrames;}
         
//----------------------------------------------------------------------------------------------------------------------
    
    [HideInInspector][SerializeField] private Color m_updateBGColor = Color.black;
    [HideInInspector][SerializeField] private int m_captureStartFrame = 0;
    [HideInInspector][SerializeField] private int m_captureEndFrame = 0;
    [HideInInspector][SerializeField] private bool m_captureAllFrames;
}


} //end namespace

