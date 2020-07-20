using System;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.StreamingImageSequence;


namespace UnityEditor.StreamingImageSequence {

[Serializable]
internal class SISUserSettings {

    [InitializeOnLoadMethod]
    static void SISUserSettings_OnLoad() {
        SISUserSettings userSettings = GetInstance();
        StreamingImageSequencePlugin.SetMaxImagesMemory(userSettings.GetMaxImagesMemoryMB());
    } 

    [RuntimeInitializeOnLoadMethod]
    static void SISUserSettings_OnLoadRuntime() {
        SISUserSettings userSettings = GetInstance();
        StreamingImageSequencePlugin.SetMaxImagesMemory(userSettings.GetMaxImagesMemoryMB());
    }    
    

//----------------------------------------------------------------------------------------------------------------------
    

    internal static SISUserSettings GetInstance() {
        if (null != m_instance)
            return m_instance;

        SISUserSettings settings = LoadUserSettings();
        if (null != settings) {
            return settings;
        }

        m_instance = new SISUserSettings();
        m_instance.SaveUserSettings();
        return m_instance;

    }

    internal static string GetSettingsPath() { return SIS_USER_SETTINGS_PATH; }

//----------------------------------------------------------------------------------------------------------------------

    //Constructor
    SISUserSettings() {
    }

//----------------------------------------------------------------------------------------------------------------------


    internal int GetVersion() { return m_classVersion; }

    internal void SetMaxImagesMemoryMB(int value) { m_maxImagesMemoryMB = value; }
    internal int GetMaxImagesMemoryMB() {  return m_maxImagesMemoryMB;}

//----------------------------------------------------------------------------------------------------------------------
    #region File Load/Save for Serialization/deserialization
    static SISUserSettings LoadUserSettings() {
        string path = SIS_USER_SETTINGS_PATH;
        if (!File.Exists(path)) {
            return null;
        }
        
        string json = File.ReadAllText(path);
        SISUserSettings settings = JsonUtility.FromJson<SISUserSettings>(json);
        
        return settings;
    }
    
    void SaveUserSettings() {
        string dir = Path.GetDirectoryName(SIS_USER_SETTINGS_PATH);
        Assert.IsFalse(string.IsNullOrEmpty(dir));
        
        Directory.CreateDirectory(dir);
        string json = JsonUtility.ToJson(this);
        File.WriteAllText(SIS_USER_SETTINGS_PATH, json);
        
    }
    #endregion
    

//----------------------------------------------------------------------------------------------------------------------


    private const string SIS_USER_SETTINGS_PATH = "Library/StreamingImageSequence/SISUserSettings.asset";


    [SerializeField] private int m_maxImagesMemoryMB = 8096;
    
    // ReSharper disable once NotAccessedField.Local
    [SerializeField] private int m_classVersion = 1;


    private static SISUserSettings m_instance;

}

} //end namespace