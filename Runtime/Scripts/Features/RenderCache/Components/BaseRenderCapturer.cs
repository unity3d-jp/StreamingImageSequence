using System.IO;
using UnityEngine;

namespace Unity.StreamingImageSequence {

/// <summary>
/// An abstract MonoBehaviour class for capturing the render result of another component
/// and writing it to disk as a texture
/// </summary>
public abstract class BaseRenderCapturer : MonoBehaviour {


    /// <summary>
    /// Prepare the component for capturing
    /// </summary>
    /// <returns>True if capturing can be executed, false otherwise</returns>
    public abstract bool BeginCapture();


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

        File.WriteAllBytes(outputFilePath, tempTex.EncodeToPNG());
        
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

    protected void SetErrorMessage(string err) { m_errorMessage = err;}

//----------------------------------------------------------------------------------------------------------------------    

    private string m_errorMessage;
    protected  RenderTexture m_rt = null;
    
}

} //end namespace



