using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;

namespace Unity.StreamingImageSequence
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
            PluginUtil.GetNativTextureInfo(m_strFileName, out tResult);
            if (tResult.readStatus == 0)
            {
                //Debug.Log("Loading: " + m_strFileName);
                PluginUtil.LoadAndAlloc(m_strFileName);
            }
#if UNITY_EDITOR
            if (tResult.readStatus == 1)
            {
                Util.Log("Already requestd:" + m_strFileName);
            }
#endif

            m_sUpdated = true;
        }
    }
}
