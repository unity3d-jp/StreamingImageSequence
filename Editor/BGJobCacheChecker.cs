﻿using System;
using UnityEngine;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence  {


    internal class BGJobCacheChecker : BackGroundTask
    {

        BGJobCacheParam m_param;

        public BGJobCacheChecker( BGJobCacheParam param )
        {
            m_param = param;
            UpdateManager.QueueBackGroundTask(this);
            UpdateManager.m_resetDelegate += Reset;
        }
        public override void Execute()
        {
            int loaded = 0;
            for ( int ii = 0; ii < m_param.m_collorArray.Length; ii++ )
            {
                string fileName = m_param.m_asset.GetImagePath(ii);
                //m_collorArray[ii] = notYet;
                if ( StreamingImageSequencePlugin.GetImageData(m_param.m_asset.GetCompleteFilePath(fileName)
                    , StreamingImageSequenceConstants.IMAGE_TYPE_FULL, out ImageData tResult) )
                {
                    if ( tResult.ReadStatus == StreamingImageSequenceConstants.READ_STATUS_SUCCESS )
                    {
                        m_param.m_collorArray[ii] = 0xffffffff;
                        loaded ++;
                    }
                }

            }

            int length = m_param.m_collorArray.Length;
            if (loaded == length)
            {
                m_param.m_allLoaded = true;
            }

        }

        private void Reset()
        {
            m_param.Reinitialize();
        }

    }

    internal class BGJobCacheParam
    {
        public StreamingImageSequencePlayableAsset m_asset;
        public UInt32[] m_collorArray;
        public bool m_allLoaded;

 //       public GUIStyle m_style;
        public BGJobCacheParam(StreamingImageSequencePlayableAsset asset)
        {
           m_asset = asset;
           int imageCount = m_asset.GetImagePaths().Count;
           m_collorArray = new UInt32[imageCount];
           m_allLoaded = false;
 //           m_style = new GUIStyle(GUI.skin.box); 
        }

        public void Reinitialize()
        {
            int imageCount = m_asset.GetImagePaths().Count;
            m_collorArray = new UInt32[imageCount];
            m_allLoaded = false;
        }
    }

}
