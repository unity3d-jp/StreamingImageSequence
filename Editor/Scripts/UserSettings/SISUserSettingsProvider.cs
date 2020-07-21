using System;
using System.Collections.Generic;
using System.IO;
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


//	private static bool once = false;

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
			VisualElement fieldContainer = AddElement<VisualElement>(content, "field-container");
			m_maxMemoryForImagesSliderInt = AddField<SliderInt, int>(fieldContainer, Contents.MAX_MEMORY_FOR_IMAGES_MB,
				maxImagesMemoryMB);
			
			m_maxMemoryForImagesSliderInt.lowValue = 4096;
			m_maxMemoryForImagesSliderInt.highValue = 65536;
			m_maxMemoryForImagesSliderInt.bindingPath = nameof(IntScriptableObject.Value);
			m_maxMemoryForImagesSliderInt.Bind(maxMemoryForImagesSerializedObject);			

			m_maxMemoryForImagesIntField = AddField<IntegerField, int>(fieldContainer, null,
				maxImagesMemoryMB);
			m_maxMemoryForImagesIntField.bindingPath = nameof(IntScriptableObject.Value);			
			m_maxMemoryForImagesIntField.Bind(maxMemoryForImagesSerializedObject);
			m_maxMemoryForImagesIntField.isReadOnly = true;
			
			Label sliderIntValuePostLabel = AddElement<Label>(fieldContainer);			
			sliderIntValuePostLabel.text = "MB     ";

			m_curMaxMemoryForImagesLabel = AddElement<Label>(fieldContainer);			
			m_curMaxMemoryForImagesLabel.text = $"({maxImagesMemoryMB} MB)";
			

			//Buttons setup
			Button saveButton = root.Query<Button>("SaveButton");
			saveButton.clicked += () => {
				userSettings.SetMaxImagesMemoryMB(m_maxMemoryForImagesSliderInt.value);
				m_curMaxMemoryForImagesLabel.text = $"({userSettings.GetMaxImagesMemoryMB()} MB)";
				userSettings.SaveUserSettings();
			};			

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
	
	//[TODO-sin: 2020-7-20] Move to Anime-toolbox ?
	private static T AddElement<T>(VisualElement parent, string className =null) 
		where T: VisualElement, new()  
	{
        
		T element = new T();
		if (!string.IsNullOrEmpty(className)) {
			element.AddToClassList(className);
		}
        
		parent.Add(element);
		return element;
	}	

//----------------------------------------------------------------------------------------------------------------------
	
	//[TODO-sin: 2020-7-20] Move to Anime-toolbox ?
	private static F AddField<F,V>(VisualElement parent, GUIContent content, V initialValue) 
		where F: BaseField<V>,INotifyValueChanged<V>, new()  
	{
        
		F field = new F();
		field.SetValueWithoutNotify(initialValue);

		if (null != content) {
			field.tooltip = content.tooltip;
			field.label   = content.text;			
		}
        
		parent.Add(field);
		return field;
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
