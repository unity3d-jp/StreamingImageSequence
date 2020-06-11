namespace UnityEngine.StreamingImageSequence {


    internal class ImageLoadBGTask : BackGroundTask {

//----------------------------------------------------------------------------------------------------------------------
        internal static void Queue(string strFileName, int frame) {
            ImageLoadBGTask task = new ImageLoadBGTask(strFileName, frame);
            UpdateManager.QueueBackGroundTask(task);
            
        }

//----------------------------------------------------------------------------------------------------------------------
        private ImageLoadBGTask( string strFileName, int frame) {
            m_strFileName = strFileName;
            m_frame = frame;
        }

//----------------------------------------------------------------------------------------------------------------------

        public override void Execute() {
            const int TEX_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_FULL;
            StreamingImageSequencePlugin.GetImageDataInto(m_strFileName, TEX_TYPE, m_frame, out ImageData tResult);
            switch (tResult.ReadStatus) {
                case StreamingImageSequenceConstants.READ_STATUS_NONE: {
                    //Debug.Log("Loading: " + m_strFileName);
                    StreamingImageSequencePlugin.LoadAndAllocFullImage(m_strFileName,m_frame);
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
        }
        
//----------------------------------------------------------------------------------------------------------------------
        readonly string m_strFileName;
        private readonly int m_frame;

    }
} //end namespace
