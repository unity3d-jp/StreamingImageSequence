using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.StreamingImageSequence;
using UnityEngine.UI;

namespace UnityEditor.StreamingImageSequence {

    internal static class ImageSequenceImporter {
        private const string PNG_EXTENSION = "png";
        private const string TGA_EXTENSION = "tga";


        /// Import images in the path to create StreamingImageSequence assets with those images
        /// <param name="path"> Can be a directory path or a file path</param>
        /// <param name="targetAsset"> The target asset where the images are assigned to</param>
        /// <param name="askToCopy"> Ask to copy if path is not under StreamingAssets. Default to true</param>
        internal static void ImportPictureFiles(string path,
            StreamingImageSequencePlayableAsset targetAsset, bool askToCopy = true) 
        {
            Assert.IsFalse(string.IsNullOrEmpty(path));

            //Convert path to folder here
            string folder = path;
            FileAttributes attr = File.GetAttributes(path);
            if (!attr.HasFlag(FileAttributes.Directory)) {
                folder = Path.GetDirectoryName(folder);
            }
            string fullSrcPath = Path.GetFullPath(folder).Replace("\\", "/");
            Uri fullSrcPathUri = new Uri(fullSrcPath + "/");


            if (string.IsNullOrEmpty(folder)) {
                Debug.LogError(@"Folder is empty. Path: " + path);
                return;
            }

            //Enumerate all files with the supported extensions and sort
            List<string> relFilePaths = new List<string>();
            string[] extensions = {
                "*." + ImageSequenceImporter.PNG_EXTENSION, 
                "*." + ImageSequenceImporter.TGA_EXTENSION,
            };
            foreach (string ext in extensions) {
                IEnumerable<string> files = Directory.EnumerateFiles(fullSrcPath, ext, SearchOption.AllDirectories);
                foreach (string filePath in files) {
                    Uri curPathUri = new Uri(filePath.Replace("\\", "/"));
                    Uri diff = fullSrcPathUri.MakeRelativeUri(curPathUri);
                    relFilePaths.Add(diff.OriginalString);
                }
            }
            if (relFilePaths.Count <= 0) {
                EditorUtility.DisplayDialog(StreamingImageSequenceConstants.DIALOG_HEADER, @"No files in folder:: " + folder,"OK");
                return;
            }
            relFilePaths.Sort(FileNameComparer);

            //Estimate the asset name. Use the filename without numbers at the end
            string assetName =  EstimateAssetName(relFilePaths[0]);

            // set dest folder
            string streamingAssetsPath = Application.streamingAssetsPath;

            //Set importer param
            ImageFileImporterParam importerParam = new ImageFileImporterParam {
                strAssetName = assetName,
                strSrcFolder = folder,
                RelativeFilePaths = relFilePaths,
                CopyToStreamingAssets = true,
                TargetAsset = targetAsset
            };


            //Import immediately if the assets are already under StreamingAssets
            if (fullSrcPath.StartsWith(streamingAssetsPath) || !askToCopy) {
                importerParam.strDstFolder = importerParam.strSrcFolder;
                importerParam.CopyToStreamingAssets = false;
                ImageSequenceImporter.Import(importerParam);
            } else {
                importerParam.strDstFolder = Path.Combine(streamingAssetsPath, assetName).Replace("\\", "/");
                ImageSequenceImportWindow.Show(importerParam);
            }
        }

//----------------------------------------------------------------------------------------------------------------------
        
        internal static void Import(ImageFileImporterParam param)
        {
            if (!param.CopyToStreamingAssets) {
                param.strDstFolder = param.strSrcFolder.Replace("\\", "/");

            } else {

                string dstFolder = param.strDstFolder.Replace("\\", "/");
                if (dstFolder.StartsWith(Application.dataPath) && !dstFolder.StartsWith(Path.Combine(Application.dataPath, "StreamingAssets").Replace("\\", "/")))
                {
                    Debug.LogError("Files must be located under StreamingAssets folder.");
                    return;
                }

                foreach (string relPath in param.RelativeFilePaths) {
                    string strAbsFilePathDst = Path.Combine(param.strDstFolder, relPath).Replace("\\", "/");
                    if (File.Exists(strAbsFilePathDst))
                    {
                        File.Delete(strAbsFilePathDst);
                    }
                    string strAbsFilePathSrc = Path.Combine(param.strSrcFolder, relPath).Replace("\\", "/");
                    Directory.CreateDirectory(Path.GetDirectoryName(strAbsFilePathDst));//make sure dir exists
                    FileUtil.CopyFileOrDirectory(strAbsFilePathSrc, strAbsFilePathDst);
                }
            }

            // create assets
            StreamingImageSequencePlayableAssetParam trackMovieContainer = new StreamingImageSequencePlayableAssetParam();
            trackMovieContainer.Pictures = new List<string>();
            foreach (string relPath in param.RelativeFilePaths) {
                trackMovieContainer.Pictures.Add(relPath);
            }

            //if possible, convert folder names to relative path.
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

            //StreamingAsset
            StreamingImageSequencePlayableAsset proxyAsset = param.TargetAsset;
            if (null == proxyAsset) {
                proxyAsset = ScriptableObject.CreateInstance<StreamingImageSequencePlayableAsset>(); 
                string strProxyPath = AssetDatabase.GenerateUniqueAssetPath(
                    Path.Combine("Assets", param.strAssetName + "_StreamingImageSequence.playable").Replace("\\", "/")
                );
                AssetDatabase.CreateAsset(proxyAsset, strProxyPath);
            }

            proxyAsset.SetParam(trackMovieContainer);
            if (param.CopyToStreamingAssets)
            {
                AssetDatabase.Refresh();
            }


        }

//---------------------------------------------------------------------------------------------------------------------

        private static int FileNameComparer(string x, string y) {
            return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);
        }

//---------------------------------------------------------------------------------------------------------------------
        internal static string EstimateAssetName(string fullFilePath) {
            string ret = Path.GetFileNameWithoutExtension(fullFilePath.Replace("\\","/"));
            //From the filename, find the last number sequence that is not followed by a number sequence
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

        private static readonly Regex ASSET_NAME_REGEX = new Regex(@"[^a-zA-Z]*(\d+)(?!.*\d)", RegexOptions.IgnoreCase); 


    }


    internal class ImageFileImporterParam {

        public string strAssetName;
        public List<string> RelativeFilePaths;
        public string strDstFolder;
        public string strSrcFolder;
        public bool CopyToStreamingAssets;
        public StreamingImageSequencePlayableAsset TargetAsset = null;
    }
}