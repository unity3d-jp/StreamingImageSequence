
using UnityEngine;

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
    
    [SerializeField] private int m_materialIndexToUpdate;
    [SerializeField] private RenderTexture m_targetTexture;


}

} //end namespace