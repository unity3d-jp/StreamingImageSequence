
using System.Collections.ObjectModel;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;


namespace Unity.StreamingImageSequence.Editor {

//[TODO-sin:2020-10-26] move to Anime-Toolbox
internal static class PackageInfoCollectionExtension {

    public  static PackageInfo FindPackage(this ReadOnlyCollection<PackageInfo> packageInfoCollection, string packageName) {
        if (string.IsNullOrEmpty(packageName))
            return null;
        
        foreach (PackageInfo packageInfo in packageInfoCollection) {
            if (packageInfo.name == packageName) {
                return packageInfo;
            }
        }

        return null;

    }

}

} //end namespace

