#if AT_USE_HDRP

using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Unity.StreamingImageSequence {

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(HDAdditionalCameraData))]

internal class HDRPTextureEndFrameBlitter : BaseTextureBlitter {


    protected override void AwakeInternalV() {
        m_hdData                       = GetComponent<HDAdditionalCameraData>();
        m_hdData.fullscreenPassthrough = true;
        
    }
//----------------------------------------------------------------------------------------------------------------------

    private void OnEnable() {
        UnityEngine.Rendering.RenderPipelineManager.endFrameRendering += OnEndFrameRendering;                
    }

    private void OnDisable() {
        UnityEngine.Rendering.RenderPipelineManager.endFrameRendering -= OnEndFrameRendering; 
        
    }

    
//----------------------------------------------------------------------------------------------------------------------
    void OnEndFrameRendering(UnityEngine.Rendering.ScriptableRenderContext context, Camera[] cams) {
        
        if (null == GetSrcTexture())
            return;
        
        //only blit for specified camera type
        foreach (Camera cam in cams) {
            if (cam.cameraType != m_targetCameraType)
                return;            
        }
        
        BlitToDest(null);
        
    }         

//----------------------------------------------------------------------------------------------------------------------

    internal void SetTargetCameraType(CameraType cameraType) { m_targetCameraType = cameraType; }

//----------------------------------------------------------------------------------------------------------------------    

    [SerializeField] private CameraType m_targetCameraType = CameraType.Game;

    private HDAdditionalCameraData m_hdData;
}

} //end namespace 

#endif // AT_USE_HDRP