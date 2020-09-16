
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

#if AT_USE_PENCILLINE
using Pencil_4;

#endif

namespace Unity.StreamingImageSequence {


[ExecuteAlways]
internal class PencilLineRenderCapturer : BaseRenderCapturer {


    public override bool CanCapture() {
#if AT_USE_PENCILLINE
        if (null == m_pencilLineEffect) {
            SetErrorMessage("PencilLineEffect is not set on " + gameObject.name);
            return false;
        }

        if (!m_pencilLineEffect.gameObject.activeInHierarchy) {
            SetErrorMessage($"Please enable gameObject in the hierarchy: {m_pencilLineEffect.gameObject.name}");
            return false;
        }
                
        return true;
        
#else

        SetErrorMessage("Updating PencilLineRenderCapturer component requires pencil-line package");
        return false;

#endif
        
    }
    
    /// <inheritdoc/>
    public override IEnumerator BeginCapture() {
#if AT_USE_PENCILLINE

        Assert.IsNotNull(m_pencilLineEffect);

        m_prevPencilLineEffectEnabled  = m_pencilLineEffect.enabled;
        m_pencilLineEffect.enabled = true;
        yield return null; //need one frame for PencilLine to construct its PencilLineRenderer

        PencilLineRenderer lineRenderer = m_pencilLineEffect.PencilRenderer;
        Assert.IsNotNull(lineRenderer);
        
        ReleaseRenderTexture();
        m_pencilTex = lineRenderer.Texture;

        m_rt = new RenderTexture(m_pencilTex.width, m_pencilTex.height, 0);
        m_rt.Create();
       
#else
        yield return null;
#endif
    }

    /// <inheritdoc/>
    public override void EndCapture() {        
#if AT_USE_PENCILLINE
        Assert.IsNotNull(m_pencilLineEffect);
        m_pencilLineEffect.enabled = m_prevPencilLineEffectEnabled;
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
    [SerializeField] private PencilLineEffect m_pencilLineEffect        = null;
    private                  Texture2D        m_pencilTex               = null;
    private                  bool             m_prevPencilLineEffectEnabled = false;
#endif


}

} //end namespace

