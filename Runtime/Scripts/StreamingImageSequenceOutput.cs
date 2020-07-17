﻿
namespace UnityEngine.StreamingImageSequence  {

/// <summary>
/// A component that stores the output of StreamingImageSequence 
/// </summary>
public sealed class StreamingImageSequenceOutput : MonoBehaviour {
    
    
    /// <summary>
    /// Get the index of the material to be updated by StreamingImageSequence 
    /// </summary>
    /// <returns></returns>
    public int GetMaterialIndexToUpdate() { return m_materialIndexToUpdate; }
    
    /// <summary>
    /// Set the index of the material to be updated by StreamingImageSequence 
    /// </summary>
    /// <param name="index"></param>
    public void SetMaterialIndexToUpdate(int index) { m_materialIndexToUpdate = index; }
    
    /// <summary>
    /// Get the texture output by StreamingImageSequence  
    /// </summary>
    /// <returns></returns>
    public Texture GetOutputTexture() { return m_outputTexture; }

    internal void SetOutputTexture(Texture tex) { m_outputTexture = tex;}
    
//----------------------------------------------------------------------------------------------------------------------    
    
    [SerializeField] private int m_materialIndexToUpdate;
    [SerializeField] private Texture m_outputTexture;


}

} //end namespace