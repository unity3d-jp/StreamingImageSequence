using UnityEngine;

namespace Unity.StreamingImageSequence {

[ExecuteAlways]
internal class URPTextureBlitter : BaseTextureBlitter {    
    
    protected override void AwakeInternalV() { }
    
//---------------------------------------------------------------------------------------------------------------------
    
    private void OnEnable() {
        UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }


    private void OnDisable() {
        UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }
    

//----------------------------------------------------------------------------------------------------------------------    
    
    void OnEndCameraRendering(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam) {
        if (cam == GetCamera() && null != GetSrcTexture()) {
            BlitToDest(null);
        }
    } 
        
//---------------------------------------------------------------------------------------------------------------------- 
}


} //end namespace