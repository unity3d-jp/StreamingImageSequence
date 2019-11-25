using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Assertions;
using System.Text.RegularExpressions;

namespace Unity.MovieProxy
{

    public class PictureFileImporter
    {

        static string versionString = "MovieProxy version 0.2.1";

        [MenuItem("Edit/Movie Proxy/Create Clip", false, 1)]
        static void RegisterFilesAndCreateMovieProxy()
        {
            importPictureFiles(PictureFileImporterParam.Mode.StereamingAssets);
        }
        static void importPictureFiles(PictureFileImporterParam.Mode mode)
        {
            PictureFileImporterParam param = new PictureFileImporterParam();

            string strPath = EditorUtility.OpenFilePanel("Open File", "", param.strExtensionPng + "," + param.strExtentionTga);
            string strExtension = Path.GetExtension(strPath).ToLower();
            if (strExtension == "." + param.strExtensionPng.ToLower())
            {
                param.strExtension = param.strExtensionPng;
            }
            else if (strExtension == "." + param.strExtentionTga.ToLower())
            {
                param.strExtension = param.strExtentionTga;
            }

            var strFileneWithoutExtention = Path.GetFileNameWithoutExtension(strPath);
            if (!Regex.IsMatch(strFileneWithoutExtention, @"\d+$"))
            {
                Debug.LogError(@"Input doesn't include number.");
                return;
            }



            /// cehck Importing file name
            var regNumbers = new Regex(@"\d+$");
            var matches = regNumbers.Matches(strFileneWithoutExtention);
            Assert.IsTrue(matches.Count > 0);

            param.match = null;
            foreach (Match match in matches)
            {
                param.match = match;
            }

            Assert.IsTrue(param.match != null);

            var parsed = int.Parse(param.match.Value);
            int periodIndex = strFileneWithoutExtention.Length;
            int digits = param.match.Value.Length;
            param.strSrcFolder = Path.GetDirectoryName(strPath);
            var strBaseName = strFileneWithoutExtention.Substring(0, param.match.Index);


            /// create copy destination path


            var strDistFolder = Application.streamingAssetsPath;
            if (mode == PictureFileImporterParam.Mode.SpriteAnimation)
            {
                strDistFolder = Application.dataPath;
            }
            if (!Directory.Exists(strDistFolder))
            {
                Directory.CreateDirectory(strDistFolder);
            }

            param.strAssetName = strBaseName;
            if (param.strAssetName.EndsWith("_") || param.strAssetName.EndsWith("-"))
            {
                param.strAssetName = param.strAssetName.Substring(0, param.strAssetName.Length - 1);
            }

            param.strDstFolder = Path.Combine(strDistFolder, param.strAssetName).Replace("\\", "/");



            /// making list of the files and copy them.
            List<string> strNames = new List<string>();

            for (; ; )
            {
                string strZero = string.Format("{0:D" + digits + "}", parsed++);
                string strFileName = strBaseName + strZero + "." + param.strExtension;
                strFileName = strFileName.Replace("\\", "/");
                string path = Path.Combine(param.strSrcFolder, strFileName).Replace("\\", "/");
                if (!File.Exists(path))
                {
                    break;
                }
                strNames.Add(strFileName);
            }

            param.files = new string[strNames.Count];
            for (int ii = 0; ii < strNames.Count; ii++)
            {
                param.files[ii] = strNames[ii];
            }
            param.mode = mode;
            PictureFileImportWindow.Init(param);

        }

        /*

        [MenuItem("Edit/Movie Proxy/Create MovieProxy/Register files", false, 6)]
        static void ImportAndCreateSpriteAnimation()
        {
            importPictureFiles(PictureFileImporterParam.Mode.SpriteAnimation);

        }
        */

        [MenuItem("Edit/Movie Proxy/Reset",false,50)]
        static void Reset()
        {
            UpdateManager.ResetPlugin();
        }
        [MenuItem("Edit/Movie Proxy/Show version",false,51)]
        static void ShowVersion()
        {
            Debug.Log(versionString);
        }
        public static void import(PictureFileImporterParam param)
        {
            if (param.DoNotCopy)
            {
                param.strDstFolder = param.strSrcFolder.Replace("\\", "/");

            }
            else
            {

                string dstFolder = param.strDstFolder.Replace("\\", "/");
                if (param.mode == PictureFileImporterParam.Mode.StereamingAssets)
                {
                    if (dstFolder.StartsWith(Application.dataPath) && !dstFolder.StartsWith(Path.Combine(Application.dataPath, "StreamingAssets").Replace("\\", "/")))
                    {
                        Debug.LogError("Files must be located under StreamingAssets folder.");
                        return;
                    }
                }
                else
                {
                    if (dstFolder.StartsWith(Application.dataPath) && dstFolder.StartsWith(Path.Combine(Application.dataPath, "StreamingAssets").Replace("\\", "/")))
                    {
                        Debug.LogError("Files must not be located under StreamingAssets folder.");
                        return;
                    }
                }

                if (!Directory.Exists(param.strDstFolder))
                {
                    Directory.CreateDirectory(param.strDstFolder);
                }

                for (int ii = 0; ii < param.files.Length; ii++)
                {
                    string strAbsFilePathDst = Path.Combine(param.strDstFolder, param.files[ii]).Replace("\\", "/");
                    if (File.Exists(strAbsFilePathDst))
                    {
                        File.Delete(strAbsFilePathDst);
                    }
                    string strAbsFilePathSrc = Path.Combine(param.strSrcFolder, param.files[ii]).Replace("\\", "/");
                    FileUtil.CopyFileOrDirectory(strAbsFilePathSrc, strAbsFilePathDst);
                }
            }

            /// ceate assets
            MovieProxyPlayableAssetParam trackMovieContainer = new MovieProxyPlayableAssetParam();
            trackMovieContainer.Pictures = new string[param.files.Length];
            for (int ii = 0; ii < param.files.Length; ii++)
            {
                trackMovieContainer.Pictures[ii] = param.files[ii];
            }

            ///   if possible, convert folder names to relative path.
            string strUnityProjectFolder = null;
            Regex regAssetFolder = new Regex("/Assets$");
            strUnityProjectFolder = Application.dataPath;
            strUnityProjectFolder = regAssetFolder.Replace(strUnityProjectFolder, "");


            if (param.strDstFolder.StartsWith(strUnityProjectFolder))
            {
                int start = strUnityProjectFolder.Length + 1;
                int end = param.strDstFolder.Length - start;
                param.strDstFolder = param.strDstFolder.Substring(start, end);
            }
            trackMovieContainer.Folder = param.strDstFolder;

            if (param.mode == PictureFileImporterParam.Mode.SpriteAnimation)
            {
                Sprite[] sprites = new Sprite[param.files.Length];
                for (int ii = 0; ii < param.files.Length; ii++)
                {
                    string strAssetPath = Path.Combine(param.strDstFolder, param.files[ii]).Replace("\\", "/");

                    AssetDatabase.ImportAsset(strAssetPath);
                    TextureImporter importer = AssetImporter.GetAtPath(strAssetPath) as TextureImporter;
                    importer.textureType = TextureImporterType.Sprite;
                    AssetDatabase.WriteImportSettingsIfDirty(strAssetPath);

                    Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(strAssetPath, typeof(Texture2D));

                    sprites[ii] = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }

                AnimationClip newClip = new AnimationClip();
                newClip.wrapMode = WrapMode.Once;
                SerializedObject serializedClip = new SerializedObject(newClip);
                SerializedProperty settings = serializedClip.FindProperty("m_AnimationClipSettings");
                while (settings.Next(true))
                {
                    if (settings.name == "m_LoopTime")
                    {
                        break;
                    }
                }

                settings.boolValue = true;
                serializedClip.ApplyModifiedProperties();
                ObjectReferenceKeyframe[] Keyframes = new ObjectReferenceKeyframe[param.files.Length];
                EditorCurveBinding curveBinding = new EditorCurveBinding();


                for (int ii = 0; ii < param.files.Length; ii++)
                {
                    Keyframes[ii] = new ObjectReferenceKeyframe();
                    Keyframes[ii].time = 0.25F * ii;
                    Keyframes[ii].value = sprites[ii];
                }
#if false
            curveBinding.type = typeof(SpriteRenderer);
            curveBinding.path = string.Empty;
            curveBinding.propertyName = "m_Sprite";
#else
                curveBinding.type = typeof(Image);
                curveBinding.path = string.Empty;
                curveBinding.propertyName = "m_Sprite";
#endif
                AnimationUtility.SetObjectReferenceCurve(newClip, curveBinding, Keyframes);
                AssetDatabase.CreateAsset(newClip, Path.Combine(param.strDstFolder, "Animation.anim").Replace("\\", "/"));

                //            var proxyAsset = ScriptableObject.CreateInstance<MovieProxyPlayableAsset>(); //new MovieProxyPlayableAsset(trackMovieContainer);
                //            proxyAsset.SetParam(trackMovieContainer);
                //            var strProxyPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine("Assets", param.strAssetName + "_MovieProxy.playable").Replace("\\", "/"));*/
                AssetDatabase.Refresh();
            }
            else
            {
                var proxyAsset = ScriptableObject.CreateInstance<MovieProxyPlayableAsset>(); //new MovieProxyPlayableAsset(trackMovieContainer);
                proxyAsset.SetParam(trackMovieContainer);
                var strProxyPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine("Assets", param.strAssetName + "_MovieProxy.playable").Replace("\\", "/"));

                AssetDatabase.CreateAsset(proxyAsset, strProxyPath);
                if (!param.DoNotCopy)
                {
                    AssetDatabase.Refresh();
                }

            }


        }

    }

    public class PictureFileImporterParam
    {
        public enum Mode
        {
            StereamingAssets,
            SpriteAnimation,
        }

        public readonly string strExtensionPng = "png";
        public readonly string strExtentionTga = "tga";
        public string strExtension;
        public string strAssetName;
        public Match match;
        public string[] files;
        public string strDstFolder;
        public string strSrcFolder;
        public bool IsSelectingFolder;
        public bool DoNotCopy;
        public Mode mode;
    }
}