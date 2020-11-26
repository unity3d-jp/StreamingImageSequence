
using UnityEngine;
using UnityEngine.UI;

namespace Unity.StreamingImageSequence  {

/// <summary>
/// A component that stores the output of StreamingImageSequence 
/// </summary>
public sealed class StreamingImageSequenceRenderer : MonoBehaviour {
    
    
    /// <summary>
    /// Get the index of the material to be updated by StreamingImageSequence 
    /// </summary>
    /// <returns></returns>
    public int GetMaterialIndexToUpdate() { return m_materialIndexToUpdate; }
    
    /// <summary>
    /// Set the index of the material to be updated by StreamingImageSequence.
    /// No material will be updated if index is less than 0.
    /// Only used if the gameObject has MeshRenderer or SkinnedMeshRenderer 
    /// </summary>
    /// <param name="index"></param>
    public void SetMaterialIndexToUpdate(int index) { m_materialIndexToUpdate = index; }
    
    /// <summary>
    /// Get the target render texture   
    /// </summary>
    /// <returns>The target RenderTexture</returns>
    public RenderTexture GetTargetTexture() { return m_targetTexture; }

    /// <summary>
    /// Sets StreamingImageSequence to copy its internal texture into a target RenderTexture.     
    /// </summary>
    /// <param name="tex">the target RenderTexture for copying</param>
    public void SetTargetTexture(RenderTexture tex) { m_targetTexture = tex;}
    
//----------------------------------------------------------------------------------------------------------------------
    internal void Init() {

        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_meshRenderer   = GetComponent<MeshRenderer>();
        if (null == m_meshRenderer) {
            m_meshRenderer = GetComponent<SkinnedMeshRenderer>();                
        }
        
        m_image = GetComponent<Image>();
        
    }
    
//----------------------------------------------------------------------------------------------------------------------

    internal void Show(bool show) {
        if (null!=m_spriteRenderer) {
            m_spriteRenderer.enabled = show;
        } else if (null != m_meshRenderer) {
            m_meshRenderer.enabled = show;
        } else if (null!=m_image) {
            m_image.enabled = show;
        } 
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    internal void UpdateTexture(Texture2D tex) {
        const int NO_MATERIAL_OUTPUT = -1;

        RenderTexture rt = m_targetTexture;
        if (null != rt) {
            Graphics.Blit(tex, rt);                
        }
                    
        if (null!=m_spriteRenderer ) {
            Sprite sprite = m_spriteRenderer.sprite;
            if (sprite.texture != tex) {
                m_spriteRenderer.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f, 2, SpriteMeshType.FullRect);
            }
            
        } else if (null!=m_meshRenderer) {
            Material mat;
            int      materialIndex   = m_materialIndexToUpdate;
            int      materialsLength = m_meshRenderer.sharedMaterials.Length;
            if (materialIndex <= NO_MATERIAL_OUTPUT || materialsLength <=0) {
                return;
            }            
            
            //Debug.Log(m_meshRenderer.sharedMaterial + "single material");
            if (materialsLength > 1 && materialIndex < materialsLength) {
                mat = m_meshRenderer.sharedMaterials[materialIndex];
            } else  {                    
                mat = m_meshRenderer.sharedMaterial;
            }

            if (null == mat) {
                return;
            }
            
#if AT_USE_HDRP
            mat.SetTexture(m_hdrpBaseColorMap,tex);
#else
            mat.mainTexture = tex;
#endif            
            
        }else if (null!= m_image) {
            Sprite sprite = m_image.sprite;
            if (null==sprite || sprite.texture != tex) {
                m_image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f, 1, SpriteMeshType.FullRect);
            }
        }

    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    
    [SerializeField] private int m_materialIndexToUpdate;
    [SerializeField] private RenderTexture m_targetTexture;

//----------------------------------------------------------------------------------------------------------------------
    
    private SpriteRenderer m_spriteRenderer = null;
    private Renderer       m_meshRenderer   = null;
    private Image          m_image          = null;
    
    private static readonly int m_hdrpBaseColorMap = Shader.PropertyToID("_BaseColorMap");

}

} //end namespace