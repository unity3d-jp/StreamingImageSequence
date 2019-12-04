using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;
using System.Text.RegularExpressions;
using UnityEngine.Assertions;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.StreamingImageSequence
{

    // A behaviour that is attached to a playable

    public class MovieProxyPlayableBehaviour : PlayableBehaviour
    {
        static string strPorjectFolder = null;

		internal TimelineClip m_clip;
        public MovieProxyPlayableBehaviour()
        {
            if (strPorjectFolder == null)
            {
                Regex regAssetFolder = new Regex("/Assets$");
                strPorjectFolder = Application.dataPath;
                strPorjectFolder = regAssetFolder.Replace(strPorjectFolder, "");
            }
        }



        private string GetCompleteFilePath(string filePath)
        {
            var asset = m_clip.asset as MovieProxyPlayableAsset;

            string strOverridePath = asset.Folder;

            if (strOverridePath != null && strOverridePath != "")
            {
                filePath = Path.Combine(strOverridePath, Path.GetFileName(filePath)).Replace("\\", "/");

            }

            if (Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(strPorjectFolder, filePath).Replace("\\", "/");
            }
            else
            {
                string strStreamingAssets = "Assets/StreamingAssets";
                if (strOverridePath != null && strOverridePath.StartsWith(strStreamingAssets))
                {
                    string rest = strOverridePath.Substring(strStreamingAssets.Length + 1, strOverridePath.Length - strStreamingAssets.Length - 1);
                    string dir = UpdateManager.GetStreamingAssetPath();
                    string dir2 = Path.Combine(dir, rest);
                    filePath = Path.Combine(dir2, Path.GetFileName(filePath)).Replace("\\", "/");
                }
            }
            return filePath;
        }

    }
}