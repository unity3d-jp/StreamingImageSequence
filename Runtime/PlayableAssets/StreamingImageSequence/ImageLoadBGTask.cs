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


    public class ImageLoadBGTask : BackGroundTask
    {
		internal static bool m_sUpdated;
        string m_strFileName;

//----------------------------------------------------------------------------------------------------------------------
        internal static void Queue(string strFileName) {
            ImageLoadBGTask task = new ImageLoadBGTask(strFileName);
            UpdateManager.QueueBackGroundTask(task);
            
        }

//----------------------------------------------------------------------------------------------------------------------
        private ImageLoadBGTask( string strFileName ) {
            m_strFileName = strFileName;
        }

//----------------------------------------------------------------------------------------------------------------------

        public override void Execute() {
            StreamingImageSequencePlugin.GetNativTextureInfo(m_strFileName, out ReadResult tResult);
            switch (tResult.ReadStatus) {
                case StreamingImageSequenceConstants.READ_RESULT_NONE: {
                    //Debug.Log("Loading: " + m_strFileName);
                    StreamingImageSequencePlugin.LoadAndAlloc(m_strFileName);
                    break;
                }
                case StreamingImageSequenceConstants.READ_RESULT_REQUESTED: {
#if UNITY_EDITOR
                    LogUtility.LogDebug("Already requested:" + m_strFileName);
#endif
                    break;
                }
                default: break;
            }

            m_sUpdated = true;
        }
    }
}
