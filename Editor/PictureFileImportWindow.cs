using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.Assertions;
using System.IO;

namespace UnityEditor.StreamingImageSequence
{

    public class PictureFileImportWindow : EditorWindow

    {
        static PictureFileImporterParam m_importerParam;
        static Vector2 m_scrollPos;
        public static void Init(PictureFileImporterParam importer)
        {
            Assert.IsTrue(importer != null);

            m_importerParam = importer;
            InitWindow();

        }

        private void OnDisable()
        {
            if (m_importerParam.IsSelectingFolder)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    InitWindow();
                };
            }
        }

        private static void InitWindow()
        {
            Rect rect = new Rect(160, 160, 0, 0);
            PictureFileImportWindow window = ScriptableObject.CreateInstance<PictureFileImportWindow>(); // GetWindow<PictureFileImportWindow>();
                                                                                                         //      PictureFileImportWindow window = GetWindow<PictureFileImportWindow>();
            window.ShowAsDropDown(rect, new Vector2(640, 480));
        }

        void OnGUI()
        {
            if (m_importerParam == null )
            {
                Debug.LogError("m_importerParam is null");
                return;
            }

            m_importerParam.IsSelectingFolder = false;
            Rect rect = new Rect(0, 0, Screen.width, Screen.height);
            Rect rect2 = new Rect(2, 2, Screen.width - 4, Screen.height - 4);
            EditorGUI.DrawRect(rect, Color.gray);
            EditorGUI.DrawRect(rect2, new Color(0.3f, 0.3f, 0.3f));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(8);

            GUI.skin.label.fontSize = 24;
            GUILayout.Label("Following files should be copied.");
            GUI.skin.label.fontSize = 0;
            GUILayout.Space(8);
            m_scrollPos =
             EditorGUILayout.BeginScrollView(m_scrollPos, GUILayout.Width(Screen.width - 4));
            if (m_importerParam.files != null)
            {
                for (int ii = 0; ii < m_importerParam.files.Length; ii++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(16);
                    string str = "" + ii + ":";

                    EditorGUILayout.LabelField(str, GUILayout.Width(40));
                    EditorGUILayout.LabelField(m_importerParam.files[ii], GUILayout.Width(Screen.width - 130));
                    GUILayout.Space(1);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            var space = 170;
            GUILayout.Space(640 - space);

            // C#var options = new GUILayoutOption[] { GUILayout.MaxWidth(Screen.width- space), GUILayout.MinWidth(120.0F) };
            EditorGUI.BeginDisabledGroup(m_importerParam.mode == PictureFileImporterParam.Mode.SpriteAnimation);
            m_importerParam.DoNotCopy = EditorGUILayout.Toggle(@"Don't copy(Use original)", m_importerParam.DoNotCopy, GUILayout.Width(space));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();


            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(8);
            EditorGUI.BeginDisabledGroup(m_importerParam.DoNotCopy);
            EditorGUILayout.LabelField("Copy to:", GUILayout.Width(120));
            EditorGUILayout.TextField(m_importerParam.strDstFolder);
            if (GUILayout.Button("...", GUILayout.Width(40)))
            {

                if (Directory.Exists(m_importerParam.strDstFolder))
                {
                    m_importerParam.IsSelectingFolder = true;
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
                PictureFileImporter.import(m_importerParam);
                this.Close();
            }

            if (GUILayout.Button("Cancel"))
            {
                this.Close();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }


    }
}
