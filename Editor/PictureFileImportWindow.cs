﻿using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.Assertions;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence
{

    public class PictureFileImportWindow : EditorWindow

    {
        static Vector2 m_scrollPos;

        /// <param name="importerMode"> Importer mode: StreamingAssets or SpriteAnimation</param>
        /// <param name="path"> Can be a directory path or a file path</param>
        public static void Init(PictureFileImporterParam.Mode importerMode, string path) {
            Assert.IsFalse(string.IsNullOrEmpty(path));

            //Convert path to folder here
            string folder = path;
            FileAttributes attr = File.GetAttributes(path);
            if (!attr.HasFlag(FileAttributes.Directory)) {
                folder = Path.GetDirectoryName(folder);
            }

            if (string.IsNullOrEmpty(folder)) {
                Debug.LogError(@"Folder is empty. Path: " + path);
                return;
            }

            //Enumerate all files with the supported extensions and sort
            List<string> fileNames = new List<string>();
            string[] extensions = {
                "*." + PictureFileImporter.PNG_EXTENSION, 
                "*." + PictureFileImporter.TGA_EXTENSION,
            };
            foreach (string ext in extensions) {
                IEnumerable<string> files = Directory.EnumerateFiles(folder, ext, SearchOption.AllDirectories);
                foreach (string filePath in files) {
                    fileNames.Add(Path.GetFileName(filePath));
                }
            }
            if (fileNames.Count <= 0) {
                EditorUtility.DisplayDialog(StreamingImageSequenceConstants.DIALOG_HEADER, @"No files in folder:: " + folder,"OK");
                return;
            }
            fileNames.Sort(FileNameComparer);

            //Estimate the asset name. Use the filename without numbers at the end
            string assetName =  EstimateAssetName(fileNames[0]);

            // set dest folder
            string destFolder = Application.streamingAssetsPath;
            if (importerMode == PictureFileImporterParam.Mode.SpriteAnimation) {
                destFolder = Application.dataPath;
            }
            destFolder = Path.Combine(destFolder, assetName).Replace("\\", "/");

            //Set importer param
            m_importerParam.strAssetName = assetName ;
            m_importerParam.files = fileNames;
            m_importerParam.strSrcFolder = folder;
            m_importerParam.strDstFolder = destFolder;
            m_importerParam.mode = importerMode;

            InitWindow();
        }

        private void OnDisable()
        {
            if (m_isSelectingFolder)
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

            m_isSelectingFolder = false;
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
            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, GUILayout.Width(Screen.width - 4));
            if (m_importerParam.files != null)
            {
                for (int ii = 0; ii < m_importerParam.files.Count; ii++)
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

//---------------------------------------------------------------------------------------------------------------------
        internal static string EstimateAssetName(string fullFilePath) {
            string ret = Path.GetFileNameWithoutExtension(fullFilePath);
            // Find the last number sequence that is not followed by a number sequence
            Regex r = new Regex(@"(\d+)(?!.*\d)", RegexOptions.IgnoreCase);
            Match m = ASSET_NAME_REGEX.Match(ret);
            if (m.Success) {
                ret = ret.Substring(0, m.Index);
            }

            //Fallback: just get the directory name. For example, if the fileName is 00000.png
            if (string.IsNullOrEmpty(ret)) {
                ret = Path.GetFileName(Path.GetDirectoryName(fullFilePath));
            }

            return ret;
        }

//---------------------------------------------------------------------------------------------------------------------

        private static int FileNameComparer(string x, string y) {
            return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);
        }
//---------------------------------------------------------------------------------------------------------------------
        private bool m_isSelectingFolder;
        private static readonly Regex ASSET_NAME_REGEX = new Regex(@"[^a-zA-Z]*(\d+)(?!.*\d)", RegexOptions.IgnoreCase); 
        static PictureFileImporterParam m_importerParam = new PictureFileImporterParam();

    }
} //end namespace
