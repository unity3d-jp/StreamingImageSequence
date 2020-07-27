using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.StreamingImageSequence {


internal static class PathUtility {

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void PathUtility_OnLoad() {
        Init();
    }
#endif
    
    [RuntimeInitializeOnLoadMethod]
    static void PathUtility_OnLoadRuntime() {
        Init();

    }
    
    static void Init() {
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
        string newPath = strAbsPath.Remove(0, m_projectFolder.Length);
        while ( newPath.StartsWith("/"))
        {
            newPath = newPath.Remove(0, 1);
        }
        return newPath;
    }


//----------------------------------------------------------------------------------------------------------------------
    
    //Have "/" as the folder separator
    private static string m_appDataPath;
    private static string m_projectFolder;


}

} //ena namespace
