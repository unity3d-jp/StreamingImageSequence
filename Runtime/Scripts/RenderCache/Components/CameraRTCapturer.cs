namespace UnityEngine.StreamingImageSequence {


[ExecuteAlways]
internal class CameraRTCapturer : BaseRTCapturer {


    /// <inheritdoc/>
    public override bool BeginCapture() {

        if (null == m_camera) {
            SetErrorMessage("Camera has not been assigned to " + this.name);
            return false;
        }
        
        if (!m_camera.enabled || !m_camera.gameObject.activeInHierarchy) {
            SetErrorMessage($"Camera {m_camera.gameObject.name} is not active." +
                "Please activate it in the scene before capturing.");
            return false;           
        }
        
        
        m_origCameraTargetTexture = m_camera.targetTexture;

        //Assign local render texture to camera
        ReleaseRenderTexture();
        m_rt = new RenderTexture(m_camera.pixelWidth, m_camera.pixelHeight, 24);
        m_rt.Create();
        m_camera.targetTexture = m_rt;
        return true;

    }

    /// <inheritdoc/>
    public override void EndCapture() {        
        m_camera.targetTexture = m_origCameraTargetTexture;
        ReleaseRenderTexture();
    }
    
    /// <inheritdoc/>
    protected override RenderTexture UpdateRenderTexture() {
        return m_rt;
    }

//----------------------------------------------------------------------------------------------------------------------
    [SerializeField] private Camera m_camera = null;

    private RenderTexture m_origCameraTargetTexture = null;

}


} //end namepsace