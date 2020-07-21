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


			//Prepare objects for binding
			m_maxMemoryForImagesScriptableObject       = ScriptableObject.CreateInstance<IntScriptableObject>();
			m_maxMemoryForImagesScriptableObject.Value = userSettings.GetMaxImagesMemoryMB();
			m_maxMemoryForImagesSerializedObject       = new SerializedObject(m_maxMemoryForImagesScriptableObject);
			
			//Slider
			VisualTreeAsset sliderIntTemplate = UIElementsEditorUtility.LoadVisualTreeAsset(
				Path.Combine(SISEditorConstants.USER_SETTINGS_PATH,"SliderIntTemplate")				
			);
			m_maxMemoryForImagesMBContainer = AddFieldFromTemplate<int>(sliderIntTemplate, content, 
				Contents.MAX_MEMORY_FOR_IMAGES_MB,
				userSettings.GetMaxImagesMemoryMB(), (int newValue) => {
				}
			);
			m_maxMemoryForImagesSliderInt = m_maxMemoryForImagesMBContainer.Query<SliderInt>();
			m_maxMemoryForImagesSliderInt.lowValue = 4096;
			m_maxMemoryForImagesSliderInt.highValue = 65536;
			m_maxMemoryForImagesSliderInt.bindingPath = nameof(IntScriptableObject.Value);
			m_maxMemoryForImagesSliderInt.Bind(m_maxMemoryForImagesSerializedObject);			


			m_maxMemoryForImagesTextField = m_maxMemoryForImagesMBContainer.Query<TextField>("SliderIntValueTextField");
			m_maxMemoryForImagesTextField.bindingPath = nameof(IntScriptableObject.Value);			
			m_maxMemoryForImagesTextField.Bind(m_maxMemoryForImagesSerializedObject);	
			
			Label sliderIntValuePostLabel = m_maxMemoryForImagesMBContainer.Query<Label>("SliderIntValuePostLabel");
			sliderIntValuePostLabel.text = "MB";
			m_activated = true;

		};
				
		deactivateHandler = () => {
			if (m_activated) {
				m_maxMemoryForImagesSliderInt.Unbind();
				m_maxMemoryForImagesTextField.Unbind();
			
				Object.DestroyImmediate(m_maxMemoryForImagesScriptableObject);
				m_maxMemoryForImagesScriptableObject = null;
				m_maxMemoryForImagesSerializedObject = null;
				m_activated = false;
			 	
			}
		};

		//keywords
		HashSet<string> sisKeywords = new HashSet<string>(new[] { "StreamingImageSequence",});
		sisKeywords.UnionWith(GetSearchKeywordsFromGUIContentProperties<SISUserSettingsProvider.Contents>());

		keywords = sisKeywords;
		
	}

	~SISUserSettingsProvider() {
		
	}
	

//----------------------------------------------------------------------------------------------------------------------	

	//[TODO-sin: 2020-7-20] Move to Anime-toolbox ?
	private static F AddField<F,V>(VisualElement parent, GUIContent content, V initialValue) 
		where F: BaseField<V>,INotifyValueChanged<V>, new()  
	{
		
		F field = new F();
		field.SetValueWithoutNotify(initialValue);
		field.tooltip = content.tooltip;
		field.label = content.text;
		
		parent.Add(field);
		return field;
	}

//----------------------------------------------------------------------------------------------------------------------
	

	//[TODO-sin: 2020-7-20] Move to Anime-toolbox ?
	private TemplateContainer AddFieldFromTemplate<V>(VisualTreeAsset template, VisualElement parent, GUIContent content,
		V initialValue, Action<V> onValueChanged)   where V : IComparable<V>
	{
		TemplateContainer templateInstance = template.CloneTree();
		BaseField<V> field = templateInstance.Query<BaseField<V>>();
		Assert.IsNotNull(field);
		field.label = content.text;
		field.tooltip = content.tooltip;	
		
		field.SetValueWithoutNotify(initialValue);
		field.RegisterValueChangedCallback((ChangeEvent<V> changeEvent) => {		
			onValueChanged(changeEvent.newValue);
		});				
		parent.Add(templateInstance);
		return templateInstance;
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
	
	
	private TemplateContainer m_maxMemoryForImagesMBContainer = null;
	private IntScriptableObject m_maxMemoryForImagesScriptableObject = null;
	private SerializedObject m_maxMemoryForImagesSerializedObject = null;
	private SliderInt m_maxMemoryForImagesSliderInt = null;
	private TextField m_maxMemoryForImagesTextField = null;
	private bool m_activated = false;

//----------------------------------------------------------------------------------------------------------------------

}

	
}
