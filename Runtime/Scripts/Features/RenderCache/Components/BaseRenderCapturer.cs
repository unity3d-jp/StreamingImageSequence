using System;
using System.Collections;
using System.IO;
using JetBrains.Annotations;
using Unity.FilmInternalUtilities;
using UnityEngine;

namespace Unity.StreamingImageSequence {

/// <summary>
/// An abstract MonoBehaviour class for capturing the render result of another component
/// and writing it to disk as a texture
/// </summary>
public abstract class BaseRenderCapturer : MonoBehaviour {

    /// <summary>
    /// Can the capturer perform the capturing process
    /// </summary>
    /// <returns>True if capturing can be executed, false otherwise</returns>
    public abstract bool CanCaptureV();

    
    /// <summary>
    /// Prepare the component for capturing. May require several frames
    /// </summary>
    /// <returns>The current position of the begin process</returns>
    public abstract IEnumerator BeginCaptureV();


    /// <summary>
    /// Clean up the component after capturing
    /// </summary>
    public abstract void EndCaptureV();

    /// <summary>
    /// Gets the internal texture used for the capturing process
    /// </summary>
    /// <returns>The internal texture</returns>
    public Texture GetInternalTexture() { return m_rt;}

    /// <summary>
    /// Capture the contents of RenderTexture to file
    /// </summary>
    /// <param name="outputFilePath">The path of the file</param>
    /// <param name="outputFormat">The output file format</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool CaptureAndSave(string outputFilePath, RenderCacheOutputFormat outputFormat = RenderCacheOutputFormat.PNG) 
    {
        RenderTexture rt = UpdateRenderTextureV();
        if (null == rt)
            return false;
        
        TextureFormat textureFormat = TextureFormat.RGBA32;
        bool isPNG = true;
        if (RenderCacheOutputFormat.EXR == outputFormat) {
            textureFormat = TextureFormat.RGBAFloat;
            isPNG         = false;
        }

        bool writeSuccess = rt.WriteToFile(outputFilePath, textureFormat, isPNG, isLinear: false);
        if (!writeSuccess) {
            Debug.LogError($"[SIS] Can't write to file: {outputFilePath}." + Environment.NewLine);             
        }

        return true;
    }

    /// <summary>
    /// Capture the contents of RenderTexture to file
    /// </summary>
    /// <param name="outputFilePath">The path of the file</param>
    /// <param name="outputFormat">The output file format</param>    
    [Obsolete("Replaced by CaptureAndSave()")]
    public void CaptureToFile(string outputFilePath, RenderCacheOutputFormat outputFormat = RenderCacheOutputFormat.PNG) 
    {
        //[TODO-sin: 2022-9-26] Remove CaptureToFile() function in 0.17.x-preview
        CaptureAndSave(outputFilePath, outputFormat);
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    /// <summary>
    /// Updates the render texture used for the capturing process
    /// </summary>
    /// <returns>The updated render texture if successful, null otherwise</returns>
    [CanBeNull]
    protected abstract RenderTexture UpdateRenderTextureV();
    
    
    /// <summary>
    /// Release Render Texture
    /// </summary>
    protected void ReleaseRenderTexture() {
        if (null == m_rt)
            return;

        if (RenderTexture.active == m_rt)
            RenderTexture.active = null;
        
        m_rt.Release();
        m_rt = null;        
    }

//----------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// Get the last error message.
    /// </summary>
    /// <returns>The last error message</returns>
    public string GetLastErrorMessage() { return m_errorMessage;}

    /// <summary>
    /// Set error message, which will be displayed if there is any error detected during the capturing process.
    /// </summary>
    /// <param name="err">The error message</param>
    protected void SetErrorMessage(string err) { m_errorMessage = err;}


//----------------------------------------------------------------------------------------------------------------------
    
#if UNITY_EDITOR    
    /// <summary>
    /// Get or create the material for blitting camera's target texture to the screen in the editor
    /// Default is null: will blit as is.
    /// </summary>
    public virtual Material GetOrCreateBlitToScreenEditorMaterialV() {
        return null;
    }
    
    
#endif    
    
//----------------------------------------------------------------------------------------------------------------------    

    private string m_errorMessage;
    
    /// <summary>
    /// The internal RenderTexture which is used as a target in the capturing process.
    /// </summary>
    protected RenderTexture m_rt = null;
    
}

} //end namespace



