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
        private int m_textureType;

//----------------------------------------------------------------------------------------------------------------------
        internal static void Queue(string strFileName, int texType) {
            ImageLoadBGTask task = new ImageLoadBGTask(strFileName, texType);
            UpdateManager.QueueBackGroundTask(task);
            
        }

//----------------------------------------------------------------------------------------------------------------------
        private ImageLoadBGTask( string strFileName, int texType ) {
            m_strFileName = strFileName;
            m_textureType = texType;
        }

//----------------------------------------------------------------------------------------------------------------------

        public override void Execute() {
            StreamingImageSequencePlugin.GetNativeTextureInfo(m_strFileName, out ReadResult tResult, m_textureType);
            switch (tResult.ReadStatus) {
                case StreamingImageSequenceConstants.READ_RESULT_NONE: {
                    //Debug.Log("Loading: " + m_strFileName);
                    StreamingImageSequencePlugin.LoadAndAlloc(m_strFileName, m_textureType);
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
