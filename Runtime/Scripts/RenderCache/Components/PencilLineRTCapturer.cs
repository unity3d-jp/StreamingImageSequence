using Pencil_4;

#if AT_USE_PENCILLINE

namespace UnityEngine.StreamingImageSequence {


[ExecuteAlways]
internal class PencilLineRTCapturer : BaseRTCapturer {


    /// <inheritdoc/>
    public override bool BeginCapture() {
        if (null == m_pencilLineEffect) {
            SetErrorMessage("PencilLineEffect is not set on " + gameObject.name);
            return false;
        }

        PencilLineRenderer lineRenderer = m_pencilLineEffect.PencilRenderer;
        if (null == lineRenderer) {
            SetErrorMessage("PencilLineEffect doesn't have PencilRenderer on " + gameObject.name);
            return false;
        }

        ReleaseRenderTexture();
        m_pencilTex = lineRenderer.Texture;

        m_rt = new RenderTexture(m_pencilTex.width, m_pencilTex.height, 0);
        m_rt.Create();
        
        return true;
    }

    /// <inheritdoc/>
    public override void EndCapture() {        
        ReleaseRenderTexture();
    }
    
    
//----------------------------------------------------------------------------------------------------------------------

    /// <inheritdoc/>
    protected override RenderTexture UpdateRenderTexture() {
        Graphics.Blit(m_pencilTex, m_rt);
        return m_rt;
    }
    
    
//----------------------------------------------------------------------------------------------------------------------
    [SerializeField] private PencilLineEffect m_pencilLineEffect = null;

    private Texture2D m_pencilTex = null;

}

} //end namespace

#endif //AT_USE_PENCILLINE