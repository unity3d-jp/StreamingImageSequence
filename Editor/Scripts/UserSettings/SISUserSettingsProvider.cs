using System;
using System.Collections.Generic;
using System.IO;
using Unity.AnimeToolbox.Editor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace UnityEditor.StreamingImageSequence {
internal class SISUserSettingsProvider : SettingsProvider {
	
	// ReSharper disable once ClassNeverInstantiated.Local
	private class Contents {
 		internal static readonly GUIContent MAX_MEMORY_FOR_IMAGES_MB = EditorGUIUtility.TrTextContent("Max Memory for Images (MB)");
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
			
			//Template
			VisualTreeAsset sliderIntTemplate = UIElementsEditorUtility.LoadVisualTreeAsset(
				Path.Combine(SISEditorConstants.USER_SETTINGS_PATH,"SliderIntTemplate")				
			);
			
			SISUserSettings userSettings = SISUserSettings.GetInstance();

			
			m_maxMemoryForImagesMBField = AddFieldFromTemplate<int>(sliderIntTemplate, content, Contents.MAX_MEMORY_FOR_IMAGES_MB,
				userSettings.GetMaxImagesMemoryMB(), (int newValue) => {
				}
			);



		};
				
		deactivateHandler = () => {
		};

		//keywords
		HashSet<string> sisKeywords = new HashSet<string>(new[] { "StreamingImageSequence",});
		sisKeywords.UnionWith(GetSearchKeywordsFromGUIContentProperties<SISUserSettingsProvider.Contents>());

		keywords = sisKeywords;
		
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
	private TemplateContainer m_maxMemoryForImagesMBField = null;

	
//----------------------------------------------------------------------------------------------------------------------
	
}

	
}
