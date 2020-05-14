﻿using System.Collections.Generic;
using System;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.StreamingImageSequence
{

    // A PlayableBehaviour that is attached to a Track via CreateTrackMixer() 
    internal class StreamingImageSequencePlayableMixer : BasePlayableMixer<StreamingImageSequencePlayableAsset> {


        public StreamingImageSequencePlayableMixer() {

#if UNITY_EDITOR
            System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
            Type type = assembly.GetType("UnityEditor.GameView");
            m_gameView = EditorWindow.GetWindow(type,false,null,false);

#endif
        }

        
//----------------------------------------------------------------------------------------------------------------------

#region IPlayableBehaviour interfaces
        public override void OnPlayableCreate(Playable playable) {
        }

        public override void OnPlayableDestroy(Playable playable) {
        }

//----------------------------------------------------------------------------------------------------------------------
        public override void OnGraphStart(Playable playable){
            
            
            foreach (TimelineClip clip in GetClips()) {
                StreamingImageSequencePlayableAsset asset = clip.asset as StreamingImageSequencePlayableAsset;
                if (null == asset)
                    continue;
                asset.OnGraphStart(playable);
            }
            

        }
        
//----------------------------------------------------------------------------------------------------------------------
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            int inputCount = playable.GetInputCount<Playable>();
            if (inputCount == 0) {
                return; // it doesn't work as mixer.
            }

            if (m_nextInAdvanceLoadingFrameArray == null) {
                m_nextInAdvanceLoadingFrameArray = new int[inputCount];
                for ( int ii = 0;ii< inputCount;ii++)
                {
                    m_nextInAdvanceLoadingFrameArray[ii] = 0;
                }
            }

            double directorTime = GetPlayableDirector().time;

            bool activeTimelineClipFound = false;
            int i = 0;
            foreach (TimelineClip clip in GetClips()) {

                StreamingImageSequencePlayableAsset asset = clip.asset as StreamingImageSequencePlayableAsset;
                if (null == asset)
                    continue;

                IList<string> imagePaths = asset.GetImagePaths();
                if (null == imagePaths)
                    continue;

                double startTime = clip.start;
                double endTime = clip.end;

                double loadStartOffsetTime = 1.0f + imagePaths.Count * 0.1f;

                //Load clips that might still be inactive, in advance
				if ( directorTime>= startTime - loadStartOffsetTime && directorTime < endTime) {
                    ProcessInAdvanceLoading(directorTime, clip, i );
                }

                if (!activeTimelineClipFound && directorTime >= startTime && directorTime < endTime) {
                    ProcessActiveClipV(asset, directorTime, clip);
                    activeTimelineClipFound = true;
                } 

                ++i;
            }

            //Show game object
            GameObject go = GetBoundGameObject();
            if (activeTimelineClipFound && null != go) {
                go.SetActive(true);
            }
            

        }

#endregion        
//----------------------------------------------------------------------------------------------------------------------

        private void ProcessInAdvanceLoading(double time, TimelineClip clip, int index)
        {
            var asset = clip.asset as StreamingImageSequencePlayableAsset;
            if (null == asset)
                return;

            int count = asset.GetImagePaths().Count;

            if (m_nextInAdvanceLoadingFrameArray[index] < count)
            {
                for (int check = 0; check < 4; check++)
                {

                    if (m_nextInAdvanceLoadingFrameArray[index] >= 0 && m_nextInAdvanceLoadingFrameArray[index] <= count)
                    {
                        if (!asset.IsLoadRequested(m_nextInAdvanceLoadingFrameArray[index]))
                        {
                            asset.LoadRequest(m_nextInAdvanceLoadingFrameArray[index], false, out _);
                        }
                    }
                    m_nextInAdvanceLoadingFrameArray[index]++;
                    if (m_nextInAdvanceLoadingFrameArray[index] >= count)
                    {
                        break;
                    }

                }
            }

        }

//---------------------------------------------------------------------------------------------------------------------
        protected override void ProcessActiveClipV(StreamingImageSequencePlayableAsset asset,
            double directorTime, TimelineClip activeClip) 
        {
            int index = asset.GlobalTimeToImageIndex(directorTime);

            bool texReady = asset.RequestLoadImage(index, false);
            if (texReady) {
                UpdateRendererTexture(asset);

#if UNITY_EDITOR
                if (!EditorApplication.isPlaying) {
                    m_gameView.Repaint();
                }
#endif
            }

        }

//----------------------------------------------------------------------------------------------------------------------

        void Reset() {
            m_spriteRenderer    = null;
            m_image             = null;
            m_meshRenderer      = null;
        }
//---------------------------------------------------------------------------------------------------------------------

        protected override void InitInternalV(GameObject boundGameObject) {
            bool ret = InitRenderers();
            if (!ret) {
                Reset();
            }
        }


//---------------------------------------------------------------------------------------------------------------------
        private bool InitRenderers() {
            GameObject go = GetBoundGameObject();
            if (null == go)
                return false;

            m_spriteRenderer= go.GetComponent<SpriteRenderer>();
            m_meshRenderer  = go.GetComponent<MeshRenderer>();
            m_image         = go.GetComponent<Image>();
            return (null!= m_meshRenderer || null!= m_image || null!=m_spriteRenderer);
        }

//---------------------------------------------------------------------------------------------------------------------

        void UpdateRendererTexture(StreamingImageSequencePlayableAsset asset) {
            Texture2D tex = asset.GetTexture();
            GameObject go = GetBoundGameObject();
            if (null!=m_spriteRenderer ) {
                Sprite sprite = m_spriteRenderer.sprite;
                if (sprite.texture != tex) {
                    m_spriteRenderer.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f, 2, SpriteMeshType.FullRect);
                }
            } else if (null!=m_meshRenderer) {
                Material mat = m_meshRenderer.sharedMaterial;
                mat.mainTexture = tex; 
            } else if (null!= m_image) {
                Sprite sprite = m_image.sprite;
                if (null==sprite || sprite.texture != tex) {
                    m_image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f, 1, SpriteMeshType.FullRect);
                }
            }

        }

//---------------------------------------------------------------------------------------------------------------------
       
        private SpriteRenderer  m_spriteRenderer = null;
        private MeshRenderer    m_meshRenderer = null;
        private Image           m_image = null;

#if UNITY_EDITOR
        readonly EditorWindow m_gameView;
#endif
        private int[] m_nextInAdvanceLoadingFrameArray;

    }



} //end namespace