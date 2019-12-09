using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;

namespace UnityEngine.StreamingImageSequence
{


    public class BGJobPictureLoader : BackGroundTask
    {
		internal static bool m_sUpdated;
        string m_strFileName;
        public BGJobPictureLoader( string strFileName )
        {
            m_strFileName = strFileName;
            UpdateManager.QueueBackGroundTask(this);
        }
        public override void Execute()
        {

            StReadResult tResult;
            StreamingImageSequencePlugin.GetNativTextureInfo(m_strFileName, out tResult);
            if (tResult.readStatus == 0)
            {
                //Debug.Log("Loading: " + m_strFileName);
                StreamingImageSequencePlugin.LoadAndAlloc(m_strFileName);
            }
#if UNITY_EDITOR
            if (tResult.readStatus == 1)
            {
                LogUtility.LogDebug("Already requested:" + m_strFileName);
            }
#endif

            m_sUpdated = true;
        }
    }
}
