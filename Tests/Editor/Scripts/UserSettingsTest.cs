using NUnit.Framework;
using Unity.AnimeToolbox.Editor;
using UnityEngine.UIElements;

namespace UnityEditor.StreamingImageSequence.Tests {

internal class UserSettingsTest {

    [Test]
    public void VerifyUIElementsResources() {
        
        VisualTreeAsset main = UIElementsEditorUtility.LoadVisualTreeAsset(SISEditorConstants.MAIN_USER_SETTINGS_PATH);
        Assert.IsNotNull(main);
			
        //Style
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(SISEditorConstants.USER_SETTINGS_STYLE_PATH + ".uss");
        Assert.IsNotNull(styleSheet);
        
    }
}

} //end namespace
