
using UnityEngine;
#if AT_USE_PENCILLINE
using Pencil_4;
#endif

namespace Unity.StreamingImageSequence {


[ExecuteAlways]
internal class PencilLineRenderCapturer : BaseRenderCapturer {


    /// <inheritdoc/>
    public override bool BeginCapture() {
#if AT_USE_PENCILLINE
        if (null == m_pencilLineEffect) {
            SetErrorMessage("PencilLineEffect is not set on " + gameObject.name);
            return false;
        }

        if (!m_pencilLineEffect.isActiveAndEnabled) {
            SetErrorMessage($"Please enable PencilLineEffect component in {m_pencilLineEffect.gameObject.name}");
            return false;
        }
        
        PencilLineRenderer lineRenderer = m_pencilLineEffect.PencilRenderer;
        if (null == lineRenderer) {
            SetErrorMessage($"PencilLineEffect component in {m_pencilLineEffect.gameObject.name} doesn't have PencilRenderer");
            return false;
        }

        ReleaseRenderTexture();
        m_pencilTex = lineRenderer.Texture;

        m_rt = new RenderTexture(m_pencilTex.width, m_pencilTex.height, 0);
        m_rt.Create();
        return true;
        
#else

        SetErrorMessage("Updating PencilLineRenderCapturer component requires pencil-line package");
        return false;

#endif
    }

    /// <inheritdoc/>
    public override void EndCapture() {        
#if AT_USE_PENCILLINE
        ReleaseRenderTexture();
#endif
    }
    
    
//----------------------------------------------------------------------------------------------------------------------

    /// <inheritdoc/>
    protected override RenderTexture UpdateRenderTexture() {
#if AT_USE_PENCILLINE
        Graphics.Blit(m_pencilTex, m_rt);
#endif
        return m_rt;
    }
    
    
//----------------------------------------------------------------------------------------------------------------------
#if AT_USE_PENCILLINE
    [SerializeField] private PencilLineEffect m_pencilLineEffect = null;
    private Texture2D m_pencilTex = null;
#endif


}

} //end namespace

