#if UNITY_2020_2

using Unity.FilmInternalUtilities.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;


namespace Unity.StreamingImageSequence.Editor {

internal static class PackageEventSubscriber {

    [InitializeOnLoadMethod]
    private static void PackageEventSubscriber_OnEditorLoad() {
        Events.registeredPackages += OnPackagesRegistered;
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    static void OnPackagesRegistered(PackageRegistrationEventArgs packageRegistrationEventArgs) {
       
        PackageInfo curPackage = packageRegistrationEventArgs.removed.FindPackage(SISEditorConstants.PACKAGE_NAME);

        if (null == curPackage) {
            curPackage = packageRegistrationEventArgs.changedTo.FindPackage(SISEditorConstants.PACKAGE_NAME);
        }

        if (null == curPackage) {
            return;
        }        
        EditorRestartMessageNotifier.RequestNotificationOnLoad(curPackage);                    
    }
}

} //end namespace

#endif //UNITY_2020_2