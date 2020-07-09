using System.IO;
using UnityEngine;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {

internal class ImageSequenceImportWindow : EditorWindow {

    void OnEnable() {
        m_headerStyle = new GUIStyle(EditorStyles.label) {
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };
        
    }

    private void OnDisable() {
        if (m_isSelectingFolder) {
            UnityEditor.EditorApplication.delayCall += () => {
                InitWindow();
            };
        }
    }
//----------------------------------------------------------------------------------------------------------------------    

    internal static void SetParam(ImageFileImporterParam param) {
        m_importerParam = param;
    }

    internal static void InitWindow() {
        Rect rect = new Rect(160, 160, 0, 0);
        ImageSequenceImportWindow window = ScriptableObject.CreateInstance<ImageSequenceImportWindow>(); // GetWindow<PictureFileImportWindow>();
                                                                                                     //      PictureFileImportWindow window = GetWindow<PictureFileImportWindow>();
        window.ShowAsDropDown(rect, new Vector2(640, 480));
    }
    
//----------------------------------------------------------------------------------------------------------------------    

    void OnGUI() {
        if (m_importerParam == null ) {
            Debug.LogError("m_importerParam is null");
            return;
        }


        m_isSelectingFolder = false;
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        Rect rect2 = new Rect(2, 2, Screen.width - 4, Screen.height - 4);
        EditorGUI.DrawRect(rect, Color.gray);
        EditorGUI.DrawRect(rect2, new Color(0.3f, 0.3f, 0.3f));
        EditorGUILayout.BeginVertical();
        GUILayout.Space(8);

        GUILayout.Label(StreamingImageSequenceConstants.DIALOG_HEADER + " Importer", m_headerStyle);
        int numFiles = m_importerParam.RelativeFilePaths.Count;
        GUILayout.Label(numFiles.ToString() + " external files found in: ");
        GUILayout.Label(m_importerParam.strSrcFolder);
        m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, GUILayout.Width(Screen.width - 4));
        if (m_importerParam.RelativeFilePaths != null)
        {
            for (int ii = 0; ii < numFiles; ii++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                string str = "" + ii + ":";

                EditorGUILayout.LabelField(str, GUILayout.Width(40));
                EditorGUILayout.LabelField(m_importerParam.RelativeFilePaths[ii], GUILayout.Width(Screen.width - 130));
                GUILayout.Space(1);
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
        GUILayout.Space(4);


        //Copy Toggle
        EditorGUILayout.BeginHorizontal();
        // C#var options = new GUILayoutOption[] { GUILayout.MaxWidth(Screen.width- space), GUILayout.MinWidth(120.0F) };
        EditorGUI.BeginDisabledGroup(m_importerParam.mode == ImageFileImporterParam.Mode.SpriteAnimation);
        string copyText = @"Copy to StreamingAssets (Recommended)";
        m_importerParam.CopyToStreamingAssets = GUILayout.Toggle(m_importerParam.CopyToStreamingAssets, copyText);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(8);
        EditorGUI.BeginDisabledGroup(!m_importerParam.CopyToStreamingAssets);
        EditorGUILayout.LabelField("Copy to:", GUILayout.Width(120));
        m_importerParam.strDstFolder = EditorGUILayout.TextField(m_importerParam.strDstFolder);
        if (GUILayout.Button("...", GUILayout.Width(40)))
        {

            if (Directory.Exists(m_importerParam.strDstFolder)) {
                m_isSelectingFolder = true;
                m_importerParam.strDstFolder = EditorUtility.OpenFolderPanel("Choose folder to copy", m_importerParam.strDstFolder, null);
            }
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(4);


        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(320 / 2);
        if (GUILayout.Button("OK"))
        {
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
    
    private bool m_isSelectingFolder;
    static ImageFileImporterParam m_importerParam = new ImageFileImporterParam();

    //Styles
    private GUIStyle m_headerStyle;

    private Vector2 m_scrollPos;

}

} //end namespace
