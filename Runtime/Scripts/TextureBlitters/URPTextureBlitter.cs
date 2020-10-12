using UnityEngine;

namespace Unity.StreamingImageSequence {

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
internal class URPTextureBlitter : MonoBehaviour {    
    
    void Awake() {
        m_camera = GetComponent<Camera>();        
       
        //Render nothing 
        m_camera.clearFlags       = CameraClearFlags.Nothing;
        m_camera.cullingMask = 0;        
    }
//---------------------------------------------------------------------------------------------------------------------
    
    private void OnEnable() {
        UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }


    private void OnDisable() {
        UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }
    

//----------------------------------------------------------------------------------------------------------------------    
    
    void OnEndCameraRendering(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam) {
        if (cam == m_camera && null != m_texture ) {
            if (null == m_texture) 
                return;

            if (null == m_blitMaterial) {
                Graphics.Blit(m_texture, (RenderTexture) null);
                return;
            }
        
            Graphics.Blit(m_texture, (RenderTexture) null, m_blitMaterial);
        }
    } 
    
//----------------------------------------------------------------------------------------------------------------------    

    internal void SetTexture(Texture tex) { m_texture = tex; }
    internal void SetBlitMaterial(Material blitMat) { m_blitMaterial = blitMat; }
    internal void SetCameraDepth(int depth) { m_camera.depth = depth; }
    
//----------------------------------------------------------------------------------------------------------------------    

    [SerializeField] private Texture  m_texture;    
    [SerializeField] Material m_blitMaterial = null;
    
    private Camera m_camera;
}


} //end namespace