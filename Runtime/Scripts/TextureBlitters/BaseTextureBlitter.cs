using UnityEngine;
using UnityEngine.Serialization;

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

    protected static void BlitTexture(Texture source, RenderTexture destination, Material blitMat) {
        if (null == source) 
            return;

        if (null == blitMat) {
            Graphics.Blit(source, destination);
            return;
        }
        
        Graphics.Blit(source, destination, blitMat);
        
    }

//----------------------------------------------------------------------------------------------------------------------    

    internal void SetSrcTexture(Texture tex) { m_srcTexture = tex; }
    protected Texture GetSrcTexture() { return m_srcTexture; }
    
    internal void SetBlitMaterial(Material blitMat) { m_blitMaterial = blitMat; }
    internal void SetCameraDepth(int depth) { m_camera.depth = depth; }

    protected Camera GetCamera() { return m_camera; }
    
//----------------------------------------------------------------------------------------------------------------------    

    [FormerlySerializedAs("m_texture")] [SerializeField] private Texture  m_srcTexture;    
    [SerializeField] Material m_blitMaterial = null;
    
    private Camera m_camera;
}

} //end namespace