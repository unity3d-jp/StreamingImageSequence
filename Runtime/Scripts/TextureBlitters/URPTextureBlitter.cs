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
        Texture tex = GetTexture();
        if (cam == GetCamera() && null != tex ) {
            BlitTexture(tex,(RenderTexture) null);
        }
    } 
        
//---------------------------------------------------------------------------------------------------------------------- 
}


} //end namespace