
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

#if AT_USE_PENCILLINE
using Pencil_4;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.StreamingImageSequence {


[ExecuteAlways]
internal class PencilLineRenderCapturer : BaseRenderCapturer {


    public override bool CanCaptureV() {
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
    public override IEnumerator BeginCaptureV() {
#if AT_USE_PENCILLINE

        Assert.IsNotNull(m_pencilLineEffect);

        m_prevPencilLineEffectEnabled  = m_pencilLineEffect.enabled;
        m_pencilLineEffect.enabled = true;
        yield return null; //need one frame for PencilLine to construct its PencilLineRenderer

        PencilLineRenderer lineRenderer = m_pencilLineEffect.PencilRenderer;
        Assert.IsNotNull(lineRenderer);
        
        ReleaseRenderTexture();
        Texture2D pencilTex = lineRenderer.Texture;

        m_rt = new RenderTexture(pencilTex.width, pencilTex.height, 0);
        m_rt.Create();
       
#else
        yield return null;
#endif
    }

    /// <inheritdoc/>
    public override void EndCaptureV() {        
#if AT_USE_PENCILLINE
        Assert.IsNotNull(m_pencilLineEffect);
        m_pencilLineEffect.enabled = m_prevPencilLineEffectEnabled;
        ReleaseRenderTexture();            
#endif
    }
    
    
//----------------------------------------------------------------------------------------------------------------------

    /// <inheritdoc/>
    protected override RenderTexture UpdateRenderTextureV() {
#if AT_USE_PENCILLINE
        PencilLineRenderer lineRenderer = m_pencilLineEffect.PencilRenderer;
        Assert.IsNotNull(lineRenderer);
        Graphics.Blit(lineRenderer.Texture, m_rt);
#endif
        return m_rt;
    }
    
//----------------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
    
    public override Material GetOrCreateBlitToScreenEditorMaterialV() {
        if (null != m_blitToScreenEditorMaterial)
            return m_blitToScreenEditorMaterial;
        
        //Setup blitMaterial
        Shader blitShader = AssetDatabase.LoadAssetAtPath<Shader>(StreamingImageSequenceConstants.TRANSPARENT_BG_COLOR_SHADER_PATH);            
        m_blitToScreenEditorMaterial = new Material(blitShader);
        m_blitToScreenEditorMaterial.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
        return m_blitToScreenEditorMaterial;
    }
    

#endif //UNITY_EDITOR    
    
//----------------------------------------------------------------------------------------------------------------------

#if AT_USE_PENCILLINE
    [SerializeField] private PencilLineEffect m_pencilLineEffect        = null;
    private                  bool             m_prevPencilLineEffectEnabled = false;
#endif

#if UNITY_EDITOR
    private static Material m_blitToScreenEditorMaterial = null;
#endif


}

} //end namespace

