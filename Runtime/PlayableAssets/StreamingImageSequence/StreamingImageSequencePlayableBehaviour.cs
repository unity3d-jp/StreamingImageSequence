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

namespace UnityEngine.StreamingImageSequence
{

    // A behaviour that is attached to a playable

    //[TODO-sin: 2020-3-2] Never used ?
    internal class StreamingImageSequencePlayableBehaviour : PlayableBehaviour
    {
        static string strPorjectFolder = null;

		internal TimelineClip m_clip = null;
        public StreamingImageSequencePlayableBehaviour()
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
            var asset = m_clip.asset as StreamingImageSequencePlayableAsset;

            string strOverridePath = asset.GetFolder();

            if (!string.IsNullOrEmpty(strOverridePath))
            {
                filePath = Path.Combine(strOverridePath, filePath).Replace("\\", "/");

            }

            if (Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(strPorjectFolder, filePath).Replace("\\", "/");
            }
            return filePath;
        }

    }
}