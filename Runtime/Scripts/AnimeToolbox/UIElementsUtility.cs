using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AnimeToolbox {

//[TODO-sin: 2020-7-21] Move this to com.unity.anime-toolbox
/// <summary>
/// A utility class for performing operations related to UIElements
/// </summary>
internal static class UIElementsUtility  {
    
//----------------------------------------------------------------------------------------------------------------------
	
    
    /// <summary>
    /// Add a VisualElement
    /// </summary>
    /// <param name="parent">The parent that will contain the new VisualElement</param>
    /// <param name="className">The class of the VisualElement. Will be ignored if set to null</param>
    /// <typeparam name="T">The newly created VisualElement</typeparam>
    /// <returns></returns>
    public static T AddElement<T>(VisualElement parent, string className =null) 
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
	
    /// <summary>
    /// Add a UIElements field  
    /// </summary>
    /// <param name="parent">The parent that will contain the new field</param>
    /// <param name="content">The tooltip and label of the field</param>
    /// <param name="initialValue">The initial value of the field</param>
    /// <typeparam name="F">The type of the field</typeparam>
    /// <typeparam name="V">The type of the field's value</typeparam>
    /// <returns></returns>
    public static F AddField<F,V>(VisualElement parent, GUIContent content, V initialValue) 
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
    
}
   
} //end namespace
