using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.StreamingImageSequence {


[ExecuteAlways]
internal class CameraRenderCapturer : BaseRenderCapturer {


    /// <inheritdoc/>
    public override bool CanCaptureV() {
        if (null == m_camera) {
            SetErrorMessage("Camera has not been assigned to " + this.name);
            return false;
        }
        
        if (!m_camera.enabled || !m_camera.gameObject.activeInHierarchy) {
            SetErrorMessage($"Camera {m_camera.gameObject.name} is not active." +
                "Please activate it in the scene before capturing.");
            return false;           
        }

        return true;
    }

    /// <inheritdoc/>
    public override IEnumerator BeginCaptureV() {

        Assert.IsNotNull(m_camera);
        m_origCameraTargetTexture = m_camera.targetTexture;

        //Assign local render texture to camera
        ReleaseRenderTexture();
        m_rt = new RenderTexture(m_camera.pixelWidth, m_camera.pixelHeight, 24);
        m_rt.Create();
        m_camera.targetTexture = m_rt;
        yield return null;
    }

    /// <inheritdoc/>
    public override void EndCaptureV() {        
        m_camera.targetTexture = m_origCameraTargetTexture;
        ReleaseRenderTexture();
    }
    
    /// <inheritdoc/>
    [CanBeNull]
    protected override RenderTexture UpdateRenderTextureV() {
        return m_rt;
    }

//----------------------------------------------------------------------------------------------------------------------

    internal void SetCamera(Camera cam) { m_camera = cam; }

//----------------------------------------------------------------------------------------------------------------------
    [SerializeField] private Camera m_camera = null;

    private RenderTexture m_origCameraTargetTexture = null;

}


} //end namepsace