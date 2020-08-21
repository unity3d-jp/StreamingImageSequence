using System;
using System.IO;
using UnityEngine;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {

internal class ImageSequenceImportWindow : EditorWindow {

    
//----------------------------------------------------------------------------------------------------------------------    

    internal static ImageSequenceImportWindow  Show(ImageFileImporterParam param) {
        Rect rect = new Rect(160, 160, 0, 0);
        ImageSequenceImportWindow window = EditorWindow.GetWindow<ImageSequenceImportWindow>();
                                                                                                     
        window.ShowAsDropDown(rect, new Vector2(640, 480));
        window.m_importerParam = param;
        return window;
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    private void OnLostFocus() {
        Close();
    }
//----------------------------------------------------------------------------------------------------------------------    

    void OnGUI() {
        if (m_importerParam == null ) {
            Debug.LogError("m_importerParam is null");
            return;
        }
        
        InitStyles();


        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        Rect rect2 = new Rect(2, 2, Screen.width - 4, Screen.height - 4);
        EditorGUI.DrawRect(rect, Color.gray);
        EditorGUI.DrawRect(rect2, new Color(0.3f, 0.3f, 0.3f));
        EditorGUILayout.BeginVertical();
        GUILayout.Space(8);

        GUILayout.Label(StreamingImageSequenceConstants.DIALOG_HEADER + " Importer", m_headerStyle);
        if (m_importerParam.ImageFiles != null) {
            int numFiles = m_importerParam.ImageFiles.Count;
            GUILayout.Label(numFiles.ToString() + " external files found in: ");
            GUILayout.Label(m_importerParam.strSrcFolder);
            
            const int SCROLL_VIEW_COUNT  = 16;
            const int SCROLL_ITEM_HEIGHT = 12;
            const int TOP_MARGIN = 12;
            
            int numDigits = (int) Math.Floor(Math.Log10(numFiles) + 1);

            m_scrollPos = DrawScrollView(m_scrollPos, TOP_MARGIN, numFiles, SCROLL_VIEW_COUNT, SCROLL_ITEM_HEIGHT, (int index) => {
                GUILayout.Space(30);

                string indexStr = index.ToString();
                indexStr = indexStr.PadLeft(numDigits - indexStr.Length);
                
                string str = indexStr + ":";

                EditorGUILayout.LabelField(str, GUILayout.Width(40));
                EditorGUILayout.LabelField(m_importerParam.ImageFiles[index]);
            });
            
        }
        
        GUILayout.Space(4);
        

        //Copy Toggle
        EditorGUILayout.BeginHorizontal();
        // C#var options = new GUILayoutOption[] { GUILayout.MaxWidth(Screen.width- space), GUILayout.MinWidth(120.0F) };
        string copyText = @"Copy to StreamingAssets (Recommended)";
        m_importerParam.CopyToStreamingAssets = GUILayout.Toggle(m_importerParam.CopyToStreamingAssets, copyText);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(8);
        EditorGUI.BeginDisabledGroup(!m_importerParam.CopyToStreamingAssets);
        EditorGUILayout.LabelField("Copy to:", GUILayout.Width(120));
        m_importerParam.strDstFolder = EditorGUILayout.TextField(m_importerParam.strDstFolder);
        if (GUILayout.Button("...", GUILayout.Width(40))) {

            if (Directory.Exists(m_importerParam.strDstFolder)) {
                m_importerParam.strDstFolder = EditorUtility.OpenFolderPanel("Choose folder to copy", m_importerParam.strDstFolder, null);
            }
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(4);


        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(320 / 2);
        if (GUILayout.Button("OK")) {
            
            ImageSequenceImporter.Import(m_importerParam);
            
            this.Close();
        }

        if (GUILayout.Button("Cancel"))
        {
            this.Close();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

//---------------------------------------------------------------------------------------------------------------------

    static Vector2 DrawScrollView(Vector2 scrollPos, int topMargin, int numItems, int viewCount, int itemHeight, Action<int> drawGUIItem) 
    {
        bool showVertical = (numItems > viewCount);

        //Calculate where we should start drawing the visible items
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, showVertical);
        int firstIndex = (int)( scrollPos.y / itemHeight);
        firstIndex = Mathf.Clamp(firstIndex, 0, numItems - viewCount);
        GUILayout.Space(firstIndex * itemHeight + topMargin);
        
        int lastIndex = Mathf.Min(numItems, firstIndex + viewCount);
            
        for (int ii = firstIndex; ii < lastIndex; ++ii) {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(itemHeight));
            drawGUIItem(ii);
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.Space(Mathf.Max(0,(numItems - firstIndex - viewCount) * itemHeight));
        EditorGUILayout.EndScrollView();
        return scrollPos;
    }

//----------------------------------------------------------------------------------------------------------------------
    
    void InitStyles() {
        if (null != m_headerStyle)
            return;
        
        m_headerStyle = new GUIStyle(EditorStyles.label) {
            fontSize  = 18,
            fontStyle = FontStyle.Bold
        };
        
    }    
    
//----------------------------------------------------------------------------------------------------------------------
    
    ImageFileImporterParam m_importerParam = new ImageFileImporterParam();

    //Styles
    private GUIStyle m_headerStyle = null;

    private Vector2 m_scrollPos;


}

} //end namespace
