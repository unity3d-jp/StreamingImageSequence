using System.IO;

namespace UnityEditor.StreamingImageSequence {

internal static class SISEditorConstants {
    internal const string USER_SETTINGS_PATH = "Packages/com.unity.streaming-image-sequence/Editor/UIElements/UserSettings";
    
    internal static readonly string MAIN_USER_SETTINGS_PATH = Path.Combine(USER_SETTINGS_PATH, "UserSettings_Main");
    internal static readonly string USER_SETTINGS_STYLE_PATH = Path.Combine(USER_SETTINGS_PATH, "UserSettings_Style");

    //width that is too small will affect the placement of preview images
    internal const int MIN_PREVIEW_REGION_WIDTH = 10;


}

} //end namespace