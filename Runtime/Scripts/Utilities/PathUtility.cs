using System.Text.RegularExpressions;
using UnityEditor;

namespace UnityEngine.StreamingImageSequence {


//[TODO-sin: 2020-7-9] Can we reuse or put the code to com.unity.anime-toolbox ?
internal static class PathUtility {

    [InitializeOnLoadMethod]
    static void PathUtilityOnLoad() {
        //Cache variables for access by background thread
        m_appDataPath = Application.dataPath;
       
        Regex regAssetFolder = new Regex("/Assets$");
        m_projectFolder = regAssetFolder.Replace(m_appDataPath, "");
        
    }

//----------------------------------------------------------------------------------------------------------------------
    
    private static string GetApplicationDataPath() {       
        return m_appDataPath;
    }

    public static string GetProjectFolder() {
        return m_projectFolder;
    }

    public static string ToRelativePath( string strAbsPath )
    {
        string newPath = strAbsPath.Remove(0, GetProjectFolder().Length);
        while ( newPath.StartsWith("/"))
        {
            newPath = newPath.Remove(0, 1);
        }
        return newPath;
    }


//----------------------------------------------------------------------------------------------------------------------
    private static string m_appDataPath;
    private static string m_projectFolder;


}

} //ena namespace
