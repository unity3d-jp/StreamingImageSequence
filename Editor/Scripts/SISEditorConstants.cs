using System.IO;

namespace Unity.StreamingImageSequence.Editor {

internal static class SISEditorConstants {

    private const string USER_SETTINGS_PATH = "Packages/com.unity.streaming-image-sequence/Editor/UIElements/UserSettings";
    
    internal static readonly string MAIN_USER_SETTINGS_PATH = Path.Combine(USER_SETTINGS_PATH, "UserSettings_Main");
    internal static readonly string USER_SETTINGS_STYLE_PATH = Path.Combine(USER_SETTINGS_PATH, "UserSettings_Style");

    //width that is too small will affect the placement of preview images
    internal const int MIN_PREVIEW_REGION_WIDTH = 10;


    internal const string SHORTCUT_TOGGLE_FRAME_MARKER = "StreamingImageSequence/Toggle Frame Marker";
    internal const string SHORTCUT_LOCK_AND_EDIT_FRAME = "StreamingImageSequence/Lock and Edit Frame";
    internal const string SHORTCUT_UPDATE_RENDER_CACHE = "StreamingImageSequence/Update Render Cache";
    
    
        

}

} //end namespace

