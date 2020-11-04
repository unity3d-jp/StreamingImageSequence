using System;
using System.Collections;
using System.IO;
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
    public abstract bool CanCapture();

    
    /// <summary>
    /// Prepare the component for capturing. May require several frames
    /// </summary>
    /// <returns>The current position of the begin process</returns>
    public abstract IEnumerator BeginCapture();


    /// <summary>
    /// Clean up the component after capturing
    /// </summary>
    public abstract void EndCapture();

    /// <summary>
    /// Gets the internal texture used for the capturing process
    /// </summary>
    /// <returns>The internal texture</returns>
    public Texture GetInternalTexture() { return m_rt;}

    /// <summary>
    /// Capture the contents of RenderTexture into file
    /// </summary>
    /// <param name="outputFilePath">The path of the file</param>
    public void CaptureToFile(string outputFilePath) {
        
        
        RenderTexture prevRenderTexture = RenderTexture.active;

        RenderTexture rt = UpdateRenderTexture();
        RenderTexture.active = rt;

        Texture2D tempTex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tempTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
        tempTex.Apply();

        try {
            File.WriteAllBytes(outputFilePath, tempTex.EncodeToPNG());
        } catch (Exception e) {
            Debug.LogError($"[SIS] Can't write to file: {outputFilePath}." + Environment.NewLine 
                + $"Error: {e.ToString()}"); 
        }
        
        //Cleanup
        ObjectUtility.Destroy(tempTex);
        RenderTexture.active = prevRenderTexture;
        
    }

//----------------------------------------------------------------------------------------------------------------------
    
    /// <summary>
    /// Updates the render texture used for the capturing process
    /// </summary>
    /// <returns>The updated render texture</returns>
    protected abstract RenderTexture UpdateRenderTexture();
    
    
    /// <summary>
    /// Release Render Texture
    /// </summary>
    protected void ReleaseRenderTexture() {
        if (null == m_rt)
            return;
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

    private string m_errorMessage;
    
    /// <summary>
    /// The internal RenderTexture which is used as a target in the capturing process.
    /// </summary>
    protected RenderTexture m_rt = null;
    
}

} //end namespace



