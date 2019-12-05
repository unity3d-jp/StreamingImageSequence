using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.StreamingImageSequence;
using UnityEngine.UI;

namespace UnityEditor.StreamingImageSequence {

    public class PictureFileImporter
    {
        public const string PNG_EXTENSION = "png";
        public const string TGA_EXTENSION = "tga";

        static string versionString = "MovieProxy version 0.2.1";

        [MenuItem("Edit/Movie Proxy/Create Clip", false, 1)]
        static void RegisterFilesAndCreateMovieProxy()
        {
            ImportPictureFiles(PictureFileImporterParam.Mode.StreamingAssets);
        }

        static void ImportPictureFiles(PictureFileImporterParam.Mode importerMode) {
            string path = EditorUtility.OpenFilePanel("Open File", "", PNG_EXTENSION + "," + TGA_EXTENSION);
            if (string.IsNullOrEmpty(path)) {
                return;
            }

            PictureFileImportWindow.Init(importerMode, path);
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
                if (param.mode == PictureFileImporterParam.Mode.StreamingAssets)
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

                for (int ii = 0; ii < param.files.Count; ii++)
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
            StreamingImageSequencePlayableAssetParam trackMovieContainer = new StreamingImageSequencePlayableAssetParam();
            trackMovieContainer.Pictures = new string[param.files.Count];
            for (int ii = 0; ii < param.files.Count; ii++)
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
                Sprite[] sprites = new Sprite[param.files.Count];
                for (int ii = 0; ii < param.files.Count; ii++)
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
                ObjectReferenceKeyframe[] Keyframes = new ObjectReferenceKeyframe[param.files.Count];
                EditorCurveBinding curveBinding = new EditorCurveBinding();


                for (int ii = 0; ii < param.files.Count; ii++)
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

                //            var proxyAsset = ScriptableObject.CreateInstance<StreamingImageSequencePlayableAsset>(); //new StreamingImageSequencePlayableAsset(trackMovieContainer);
                //            proxyAsset.SetParam(trackMovieContainer);
                //            var strProxyPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine("Assets", param.strAssetName + "_MovieProxy.playable").Replace("\\", "/"));*/
                AssetDatabase.Refresh();
            }
            else
            {
                var proxyAsset = ScriptableObject.CreateInstance<StreamingImageSequencePlayableAsset>(); //new StreamingImageSequencePlayableAsset(trackMovieContainer);
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
            StreamingAssets,
            SpriteAnimation,
        }

        public string strAssetName;
        public List<string> files;
        public string strDstFolder;
        public string strSrcFolder;
        public bool DoNotCopy;
        public Mode mode;
    }
}