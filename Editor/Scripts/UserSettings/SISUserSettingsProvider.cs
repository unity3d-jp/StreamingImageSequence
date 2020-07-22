using System.Collections.Generic;
using Unity.AnimeToolbox;
using Unity.AnimeToolbox.Editor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace UnityEditor.StreamingImageSequence {
internal class SISUserSettingsProvider : SettingsProvider {
	
	// ReSharper disable once ClassNeverInstantiated.Local
	private class Contents {
 		internal static readonly GUIContent MAX_MEMORY_FOR_IMAGES_MB = EditorGUIUtility.TrTextContent("Max Memory for Images ");
	}
		
//----------------------------------------------------------------------------------------------------------------------	

	private SISUserSettingsProvider() : base(USER_SETTINGS_MENU_PATH,SettingsScope.User) {
		
	
		//activateHandler is called when the user clicks on the Settings item in the Settings window.
		
		activateHandler = (string searchContext, VisualElement root) => {

			
			//Main Tree
			VisualTreeAsset main = UIElementsEditorUtility.LoadVisualTreeAsset(SISEditorConstants.MAIN_USER_SETTINGS_PATH);
			Assert.IsNotNull(main);
			main.CloneTree(root);			
			
			//Style
			UIElementsEditorUtility.LoadAndAddStyle( root.styleSheets, SISEditorConstants.USER_SETTINGS_STYLE_PATH);			
			
			VisualElement content = root.Query<VisualElement>("Content");
			Assert.IsNotNull(content);				
			SISUserSettings userSettings = SISUserSettings.GetInstance();
			int maxImagesMemoryMB = userSettings.GetMaxImagesMemoryMB();

			//Prepare objects for binding
			m_maxMemoryForImagesScriptableObject       = ScriptableObject.CreateInstance<IntScriptableObject>();
			m_maxMemoryForImagesScriptableObject.Value = userSettings.GetMaxImagesMemoryMB();
			SerializedObject maxMemoryForImagesSerializedObject = new SerializedObject(m_maxMemoryForImagesScriptableObject);
			
			//Slider
			VisualElement fieldContainer = UIElementsUtility.AddElement<VisualElement>(content, "field-container");
			m_maxMemoryForImagesSliderInt = UIElementsUtility.AddField<SliderInt, int>(fieldContainer, Contents.MAX_MEMORY_FOR_IMAGES_MB,
				maxImagesMemoryMB);
			
			m_maxMemoryForImagesSliderInt.lowValue = 4096;
			m_maxMemoryForImagesSliderInt.highValue = 65536;
			m_maxMemoryForImagesSliderInt.bindingPath = nameof(IntScriptableObject.Value);
			m_maxMemoryForImagesSliderInt.Bind(maxMemoryForImagesSerializedObject);			

			m_maxMemoryForImagesIntField = UIElementsUtility.AddField<IntegerField, int>(fieldContainer, null,
				maxImagesMemoryMB);
			m_maxMemoryForImagesIntField.bindingPath = nameof(IntScriptableObject.Value);			
			m_maxMemoryForImagesIntField.Bind(maxMemoryForImagesSerializedObject);
			
			Label sliderIntValuePostLabel = UIElementsUtility.AddElement<Label>(fieldContainer);			
			sliderIntValuePostLabel.text = "MB     ";

			m_curMaxMemoryForImagesLabel = UIElementsUtility.AddElement<Label>(fieldContainer);			
			m_curMaxMemoryForImagesLabel.text = $"({maxImagesMemoryMB} MB)";
			

			//Buttons setup
			Button saveButton = root.Query<Button>("SaveButton");
			saveButton.clicked += () => {
				userSettings.SetMaxImagesMemoryMB(m_maxMemoryForImagesSliderInt.value);
				m_curMaxMemoryForImagesLabel.text = $"({userSettings.GetMaxImagesMemoryMB()} MB)";
				userSettings.SaveUserSettings();
			};

			m_activated = true;

		};
				
		deactivateHandler = () => {
			if (m_activated) {
				m_maxMemoryForImagesSliderInt.Unbind();
				m_maxMemoryForImagesIntField.Unbind();
			
				Object.DestroyImmediate(m_maxMemoryForImagesScriptableObject);
				m_maxMemoryForImagesScriptableObject = null;
				m_activated = false;
			}
		};

		//keywords
		HashSet<string> sisKeywords = new HashSet<string>(new[] { "StreamingImageSequence",});
		sisKeywords.UnionWith(GetSearchKeywordsFromGUIContentProperties<SISUserSettingsProvider.Contents>());

		keywords = sisKeywords;
		
	}


	

//----------------------------------------------------------------------------------------------------------------------

    [SettingsProvider]
    internal static SettingsProvider CreateSISUserSettingsProvider() {
	    m_settingsProvider = new SISUserSettingsProvider();
	    return m_settingsProvider;
    }
    
	
//----------------------------------------------------------------------------------------------------------------------

	private static SISUserSettingsProvider m_settingsProvider = null;
	private const string USER_SETTINGS_MENU_PATH = "Preferences/StreamingImageSequence";
	
	
	private IntScriptableObject m_maxMemoryForImagesScriptableObject = null;
	private SliderInt m_maxMemoryForImagesSliderInt = null;
	private IntegerField m_maxMemoryForImagesIntField = null;
	private Label  m_curMaxMemoryForImagesLabel = null;
	private bool m_activated = false;

//----------------------------------------------------------------------------------------------------------------------

}

	
}
