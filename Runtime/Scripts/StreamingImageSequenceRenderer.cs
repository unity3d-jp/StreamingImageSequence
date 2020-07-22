
namespace UnityEngine.StreamingImageSequence  {

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
    /// Get the assigned RenderTexture  
    /// </summary>
    /// <returns>The assigned RenderTexture</returns>
    public RenderTexture GetRenderTexture() { return m_renderTexture; }

    /// <summary>
    /// Sets a RenderTexture to which StreamingImageSequence will copy its internal texture     
    /// </summary>
    public void SetRenderTexture(RenderTexture tex) { m_renderTexture = tex;}
    
//----------------------------------------------------------------------------------------------------------------------    
    
    [SerializeField] private int m_materialIndexToUpdate;
    [SerializeField] private RenderTexture m_renderTexture;


}

} //end namespace