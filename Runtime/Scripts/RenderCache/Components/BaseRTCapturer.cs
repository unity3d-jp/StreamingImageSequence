using System.IO;

namespace UnityEngine.StreamingImageSequence {

/// <summary>
/// An abstract class for capturing RenderTexture to disk
/// </summary>
public abstract class BaseRTCapturer : MonoBehaviour {


    /// <summary>
    /// Prepare the component for capturing RenderTexture
    /// </summary>
    /// <returns>True if capturing can be executed, false otherwise</returns>
    public abstract bool BeginCapture();


    /// <summary>
    /// Clean up the component after capturing RenderTexture
    /// </summary>
    public abstract void EndCapture();

    /// <summary>
    /// Gets the internal render texture used for the capturing process
    /// </summary>
    /// <returns>The render texture</returns>
    public abstract RenderTexture GetRenderTexture();
    
//----------------------------------------------------------------------------------------------------------------------
    
    /// <summary>
    /// Capture the contents of RenderTexture into file
    /// </summary>
    /// <param name="outputFilePath">The path of the file</param>
    public void CaptureToFile(string outputFilePath) {
        
        
        RenderTexture prevRenderTexture = RenderTexture.active;

        RenderTexture rt = GetRenderTexture();
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
    /// Get the last error message.
    /// </summary>
    /// <returns>The last error message</returns>
    public string GetLastErrorMessage() { return m_errorMessage;}

    protected void SetErrorMessage(string err) { m_errorMessage = err;}

//----------------------------------------------------------------------------------------------------------------------    

    private string m_errorMessage;
}

} //end namespace



