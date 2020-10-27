
using System.Collections.ObjectModel;
using System.Text;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;


namespace Unity.StreamingImageSequence.Editor {

//[TODO-sin:2020-10-26] move to Anime-Toolbox
internal static class PackageInfoCollectionExtension {

    public static PackageInfo FindPackage(this ReadOnlyCollection<PackageInfo> packageInfoCollection, string packageName) {
        if (string.IsNullOrEmpty(packageName))
            return null;
        
        foreach (PackageInfo packageInfo in packageInfoCollection) {
            if (packageInfo.name == packageName) {
                return packageInfo;
            }
        }

        return null;

    }

//----------------------------------------------------------------------------------------------------------------------    
    public static string JoinToString(this ReadOnlyCollection<PackageInfo> packageInfoCollection) {        
        StringBuilder sb = new StringBuilder();            
        foreach (PackageInfo packageInfo in packageInfoCollection) {
            sb.AppendLine($"{packageInfo.name}@{packageInfo.version}");
        }
        return sb.ToString();
    }
    
}

} //end namespace

