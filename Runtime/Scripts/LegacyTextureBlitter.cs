using System;

namespace UnityEngine.StreamingImageSequence {

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
internal class LegacyTextureBlitter : MonoBehaviour {  
    
    void Awake() {
        m_camera = GetComponent<Camera>();
        
        //Render nothing
        m_camera.clearFlags  = CameraClearFlags.Depth;
        m_camera.cullingMask = 0;   
    }

//----------------------------------------------------------------------------------------------------------------------    
    
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (null == m_texture) 
            return;

        Graphics.Blit(m_texture, destination);
    }    

//----------------------------------------------------------------------------------------------------------------------    

    internal void SetTexture(Texture tex) { m_texture = tex; }

    internal void SetCameraDepth(int depth) { m_camera.depth = depth; }
    
//----------------------------------------------------------------------------------------------------------------------    

    [SerializeField] private Texture m_texture;
    private Camera m_camera;
}


/*
 [TODO-sin: 2020-5-29]: Handle HDRP 
    private void OnEnable() {
        m_cam = GetComponent<Camera>();
        m_hdData = GetComponent<HDAdditionalCameraData>();

        //Render nothing
        m_cam.clearFlags = CameraClearFlags.Nothing;
        m_cam.cullingMask = 0;
        m_hdData.fullscreenPassthrough = true;
        m_hdData.customRender += BlitRenderStreamingRT;
    }

//---------------------------------------------------------------------------------------------------------------------

    private void OnDisable() {
        m_hdData.customRender -= BlitRenderStreamingRT;
    }

//---------------------------------------------------------------------------------------------------------------------
    public void BlitRenderStreamingRT(UnityEngine.Rendering.ScriptableRenderContext context, HDCamera cam) {
        Graphics.Blit(m_rtCamera.targetTexture, (RenderTexture) null);
    } * 
 */


/*
[TODO-sin: 2020-5-29]: Handle URP 
    private void OnEnable() {
        m_cam = GetComponent<Camera>();

        //Render nothing 
        m_cam.clearFlags = CameraClearFlags.Nothing;
        m_cam.cullingMask = 0;
        UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

//---------------------------------------------------------------------------------------------------------------------

    private void OnDisable() {
        UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

//---------------------------------------------------------------------------------------------------------------------

    void OnEndCameraRendering(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam) {
        if (cam == m_cam && null != m_rtCamera.targetTexture ) {
            //This seems to work only if we have setup PostProcessing Stack V2
            Graphics.Blit(m_rtCamera.targetTexture, (RenderTexture) null);
        }
    } * 
 */

} //end namespace