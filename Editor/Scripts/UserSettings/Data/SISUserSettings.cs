﻿using System;
using Unity.AnimeToolbox;
using UnityEngine;
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

    internal void SetMaxImagesMemoryMB(int value) {
        m_maxImagesMemoryMB = value;
        StreamingImageSequencePlugin.SetMaxImagesMemory(m_maxImagesMemoryMB);        
    }
    internal int GetMaxImagesMemoryMB() {  return m_maxImagesMemoryMB;}

//----------------------------------------------------------------------------------------------------------------------
    static SISUserSettings LoadUserSettings() {
        return FileUtility.DeserializeFromJson<SISUserSettings>(SIS_USER_SETTINGS_PATH);
    }
    
    internal void SaveUserSettings() {
        FileUtility.SerializeToJson<SISUserSettings>(this, SIS_USER_SETTINGS_PATH);
    }    

//----------------------------------------------------------------------------------------------------------------------


    private const string SIS_USER_SETTINGS_PATH = "Library/StreamingImageSequence/SISUserSettings.asset";


    [SerializeField] private int m_maxImagesMemoryMB = 65536;
    
    // ReSharper disable once NotAccessedField.Local
    [SerializeField] private int m_classVersion = 1;


    private static SISUserSettings m_instance;

}

} //end namespace