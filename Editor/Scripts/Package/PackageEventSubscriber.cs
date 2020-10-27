#if UNITY_2020_2

using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;


namespace Unity.StreamingImageSequence.Editor {

internal static class PackageEventSubscriber {

    [InitializeOnLoadMethod]
    private static void PackageEventSubscriber_OnEditorLoad() {
        Debug.Log("PackageEventSubscriber_OnEditorLoad()");
        Events.registeredPackages += OnPackagesRegistered;
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    static void OnPackagesRegistered(PackageRegistrationEventArgs packageRegistrationEventArgs) {
        Debug.Log("Removed packages:");
        Debug.Log(packageRegistrationEventArgs.removed.JoinToString());
        Debug.Log("ChangedTo packages:");
        Debug.Log(packageRegistrationEventArgs.changedTo.JoinToString());
        
        PackageInfo curPackage = packageRegistrationEventArgs.removed.FindPackage(SISEditorConstants.PACKAGE_NAME);

        if (null == curPackage) {
            curPackage = packageRegistrationEventArgs.changedTo.FindPackage(SISEditorConstants.PACKAGE_NAME);
        }

        if (null == curPackage) {
            return;
        }        
        EditorRestartMessageNotifier.RequestOnLoadNotification(curPackage);                    
    }
}

} //end namespace

#endif //UNITY_2020_2