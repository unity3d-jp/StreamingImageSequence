using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;

namespace UnityEngine.StreamingImageSequence {


    internal class ImageLoadBGTask : BackGroundTask
    {
		internal static bool m_sUpdated;
        
        string m_strFileName;

//----------------------------------------------------------------------------------------------------------------------
        internal static void Queue(string strFileName) {
            ImageLoadBGTask task = new ImageLoadBGTask(strFileName);
            UpdateManager.QueueBackGroundTask(task);
            
        }

//----------------------------------------------------------------------------------------------------------------------
        private ImageLoadBGTask( string strFileName) {
            m_strFileName = strFileName;
        }

//----------------------------------------------------------------------------------------------------------------------

        public override void Execute() {
            const int TEX_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_FULL;
            StreamingImageSequencePlugin.GetImageData(m_strFileName, TEX_TYPE, out ImageData tResult);
            switch (tResult.ReadStatus) {
                case StreamingImageSequenceConstants.READ_STATUS_NONE: {
                    //Debug.Log("Loading: " + m_strFileName);
                    StreamingImageSequencePlugin.LoadAndAllocFullImage(m_strFileName);
                    break;
                }
                case StreamingImageSequenceConstants.READ_STATUS_LOADING: {
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
