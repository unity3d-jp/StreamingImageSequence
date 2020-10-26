
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;


namespace Unity.StreamingImageSequence.Editor {

internal static class EditorRestartMessageNotifier {

    //[TODO-sin: 2020-10-26] Move to anime-toolbox
    [InitializeOnLoadMethod]
    private static void EditorRestartMessageNotifier_OnEditorLoad() {
        m_notifyTime             = EditorApplication.timeSinceStartup + WAIT_THRESHOLD;
        EditorApplication.update += WaitUntilNotify;
    }
    
//----------------------------------------------------------------------------------------------------------------------    

    static void WaitUntilNotify() {
        if (EditorApplication.timeSinceStartup < m_notifyTime) {
            return;
        }

        if (m_onLoadPackageRequesters.Count <= 0) {
            EditorApplication.update -= WaitUntilNotify;            
            return;            
        }
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Please restart editor because the following packages have been updated: ");
        foreach (PackageInfo packageInfo in m_onLoadPackageRequesters) {
            sb.AppendLine($"- {packageInfo.name}@{packageInfo.version}");
        }

        if (EditorUtility.DisplayDialog("Warning", sb.ToString(), "Exit Unity now", "Later")) {
            EditorApplication.Exit(0);
        }
                    
        m_onLoadPackageRequesters.Clear();
        EditorApplication.update -= WaitUntilNotify;            
    }


    internal static void RequestOnLoadNotification(PackageInfo packageInfo) {
        m_onLoadPackageRequesters.Add(packageInfo);
        m_notifyTime = EditorApplication.timeSinceStartup + WAIT_THRESHOLD;
        
    }
    
//----------------------------------------------------------------------------------------------------------------------  

    private static readonly List<PackageInfo> m_onLoadPackageRequesters = new List<PackageInfo>();
    private static double m_notifyTime = 0;
    private const double WAIT_THRESHOLD = 1.0f;
}

} //end namespace

