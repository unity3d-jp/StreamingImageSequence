﻿using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace Unity.StreamingImageSequence.Editor {

    internal static class ImageSequenceImporter {


        
//----------------------------------------------------------------------------------------------------------------------        

        /// Import images in the path to create StreamingImageSequence assets with those images
        /// <param name="path"> Can be a directory path or a file path</param>
        /// <param name="targetAsset"> The target asset where the images are assigned to</param>
        /// <param name="askToCopy"> Ask to copy if path is not under StreamingAssets. Default to true</param>
        internal static void ImportImages(string path,
            StreamingImageSequencePlayableAsset targetAsset, bool askToCopy = true) 
        {
            Assert.IsFalse(string.IsNullOrEmpty(path));

            FindFolderAndImages(path, out string folder, out List<WatchedFileInfo> relFilePaths);
            if (relFilePaths.Count <= 0) {
                EditorUtility.DisplayDialog(StreamingImageSequenceConstants.DIALOG_HEADER, @"No files in folder:: " + folder,"OK");
                return;
            }

            //Estimate the asset name. Use the filename without numbers at the end
            string assetName =  EstimateAssetName(relFilePaths[0].GetName());

            //Set importer param
            ImageFileImporterParam importerParam = new ImageFileImporterParam {
                strSrcFolder = folder,
                ImageFiles = relFilePaths,
                CopyToStreamingAssets = true,
                TargetAsset = targetAsset
            };

            //Import immediately if the assets are already under Assets
            if (folder =="Assets" || folder.StartsWith("Assets/") || !askToCopy) {
                importerParam.strDstFolder = importerParam.strSrcFolder;
                importerParam.CopyToStreamingAssets = false;
                ImageSequenceImporter.Import(importerParam);
            } else {
                string streamingAssetsPath = AssetEditorUtility.NormalizePath( Application.streamingAssetsPath);
                importerParam.strDstFolder = Path.Combine(streamingAssetsPath, assetName).Replace("\\", "/");
                ImageSequenceImportWindow.Show(importerParam);
            }
        }

//----------------------------------------------------------------------------------------------------------------------
        
        internal static void Import(ImageFileImporterParam param) {
            string destFolder = null;
            if (!param.CopyToStreamingAssets) {
                destFolder = param.strSrcFolder.Replace("\\", "/");

            } else {

                destFolder = param.strDstFolder.Replace("\\", "/");
                if (destFolder.StartsWith(Application.dataPath) && !destFolder.StartsWith(Path.Combine(Application.dataPath, "StreamingAssets").Replace("\\", "/")))
                {
                    Debug.LogError("Files must be located under StreamingAssets folder.");
                    return;
                }

                foreach (WatchedFileInfo fileInfo in param.ImageFiles) {
                    string fileName          = fileInfo.GetName();
                    string strAbsFilePathDst = Path.Combine(destFolder,fileName).Replace("\\", "/");
                    if (File.Exists(strAbsFilePathDst))
                    {
                        File.Delete(strAbsFilePathDst);
                    }
                    string strAbsFilePathSrc = Path.Combine(param.strSrcFolder, fileName).Replace("\\", "/");
                    Directory.CreateDirectory(Path.GetDirectoryName(strAbsFilePathDst));//make sure dir exists
                    FileUtil.CopyFileOrDirectory(strAbsFilePathSrc, strAbsFilePathDst);
                }
            }

            //if possible, convert folder names to relative path.
            string strUnityProjectFolder = null;
            Regex regAssetFolder = new Regex("/Assets$");
            strUnityProjectFolder = Application.dataPath;
            strUnityProjectFolder = regAssetFolder.Replace(strUnityProjectFolder, "");

            if (destFolder.StartsWith(strUnityProjectFolder)) {
                int start = strUnityProjectFolder.Length + 1;
                int end = destFolder.Length - start;
                destFolder = destFolder.Substring(start, end);
            }

            //StreamingAsset
            StreamingImageSequencePlayableAsset playableAsset = param.TargetAsset;            
            if (null == playableAsset) {
                string assetName =  EstimateAssetName(param.ImageFiles[0].GetName());
                playableAsset = CreateUniqueSISAsset(
                    Path.Combine("Assets", assetName + "_StreamingImageSequence.playable").Replace("\\", "/")

                );
            }
            
            playableAsset.InitFolderInEditor(destFolder, param.ImageFiles);
            if (param.CopyToStreamingAssets) {
                AssetDatabase.Refresh();
            }
        }
        
//---------------------------------------------------------------------------------------------------------------------
        //Path can point to a file or a folder.
        //If it points to a file, then the folder will be automatically detected
        private static void FindFolderAndImages(string path, out string folder, out List<WatchedFileInfo> imageFiles) {
            Assert.IsFalse(string.IsNullOrEmpty(path));                
            //Convert path to folder here
            folder = path;
            FileAttributes attr = File.GetAttributes(path);
            if (!attr.HasFlag(FileAttributes.Directory)) {
                folder = Path.GetDirectoryName(folder);
            }
            
            imageFiles = WatchedFileInfo.FindFiles(folder, 
                StreamingImageSequencePlayableAsset.GetSupportedImageFilePatterns()
            );  
        }
//---------------------------------------------------------------------------------------------------------------------
        
        private static StreamingImageSequencePlayableAsset CreateUniqueSISAsset(string playableAssetPath) {            
            StreamingImageSequencePlayableAsset playableAsset 
                = ScriptableObject.CreateInstance<StreamingImageSequencePlayableAsset>();
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(playableAssetPath);            
            AssetDatabase.CreateAsset(playableAsset, uniquePath);
            return playableAsset;
        }        

//---------------------------------------------------------------------------------------------------------------------
        //Estimate the asset name. Use the filename without numbers at the end
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

//----------------------------------------------------------------------------------------------------------------------

    internal class ImageFileImporterParam {

        public List<WatchedFileInfo> ImageFiles;
        public string strDstFolder;
        public string strSrcFolder;
        public bool CopyToStreamingAssets;
        public StreamingImageSequencePlayableAsset TargetAsset = null;
    }
}