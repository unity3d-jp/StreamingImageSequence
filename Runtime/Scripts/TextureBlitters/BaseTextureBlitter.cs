using UnityEngine;

namespace Unity.StreamingImageSequence {

[RequireComponent(typeof(Camera))]
internal abstract class BaseTextureBlitter : MonoBehaviour {    
    
    void Awake() {
        m_camera = GetComponent<Camera>();
        
        //Render nothing
        m_camera.clearFlags  = CameraClearFlags.Depth;
        m_camera.cullingMask = 0;

        AwakeInternalV();
    }

    protected abstract void AwakeInternalV();
    
//----------------------------------------------------------------------------------------------------------------------    

    protected void BlitTexture(Texture source, RenderTexture destination) {
        if (null == m_texture) 
            return;

        if (null == m_blitMaterial) {
            Graphics.Blit(m_texture, destination);
            return;
        }
        
        Graphics.Blit(m_texture, destination, m_blitMaterial);
        
    }

//----------------------------------------------------------------------------------------------------------------------    

    internal void SetTexture(Texture tex) { m_texture = tex; }
    protected Texture GetTexture() { return m_texture; }
    
    internal void SetBlitMaterial(Material blitMat) { m_blitMaterial = blitMat; }
    internal void SetCameraDepth(int depth) { m_camera.depth = depth; }

    protected Camera GetCamera() { return m_camera; }
    
//----------------------------------------------------------------------------------------------------------------------    

    [SerializeField] private Texture  m_texture;    
    [SerializeField] Material m_blitMaterial = null;
    
    private Camera m_camera;
}

} //end namespace