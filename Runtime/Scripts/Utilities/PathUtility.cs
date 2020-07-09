using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {


//[TODO-sin: 2020-7-9] Can we reuse or put the code to com.unity.anime-toolbox ?
internal static class PathUtility {

    private static string GetApplicationDataPath() {
        
        // Application.dataPath cant be used in back thread, so we cache it hire.
        if (s_AppDataPath == null)
        {
            s_AppDataPath = Application.dataPath;
        }
        return s_AppDataPath;
    }


    public static string GetProjectFolder()
    {
        Regex regAssetFolder = new Regex("/Assets$");
        var strPorjectFolder = PathUtility.GetApplicationDataPath(); //  Application.dataPath; cant use this in back thread;
        strPorjectFolder = regAssetFolder.Replace(strPorjectFolder, "");

        return strPorjectFolder;
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
    private static string s_AppDataPath;


}

} //ena namespace
