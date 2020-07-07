using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence
{

    // A PlayableBehaviour that is attached to a Track via CreateTrackMixer() 
    internal class StreamingImageSequencePlayableMixer : BasePlayableMixer<StreamingImageSequencePlayableAsset> {


        public StreamingImageSequencePlayableMixer() {
        }

        
//----------------------------------------------------------------------------------------------------------------------

#region IPlayableBehaviour interfaces

        public override void OnPlayableCreate(Playable playable) {
#if UNITY_EDITOR            
            m_loaderPeriodicJob = new LoaderPeriodicJob(this);
            UpdateManager.AddPeriodicJob( m_loaderPeriodicJob);
#endif //UNITY_EDITOR          
           
        }

        public override void OnPlayableDestroy(Playable playable) {
            base.OnPlayableDestroy(playable);
            
#if UNITY_EDITOR            
            UpdateManager.RemovePeriodicJob( m_loaderPeriodicJob);        
#endif //UNITY_EDITOR          
        }

        
//----------------------------------------------------------------------------------------------------------------------
        public override void OnGraphStart(Playable playable) {
            //Need to bind TimelineClips first
            IEnumerable<TimelineClip> clips = GetClips();           
            foreach (TimelineClip clip in clips) {
                StreamingImageSequencePlayableAsset asset = clip.asset as StreamingImageSequencePlayableAsset;
                if (null == asset)
                    continue;
                asset.BindTimelineClip(clip);
            }
            
            var clipAssets = GetClipAssets();
            foreach (KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset> kv in clipAssets) {
                StreamingImageSequencePlayableAsset sisAsset = kv.Value;                
                sisAsset.OnGraphStart(playable);                
            }
        }

//----------------------------------------------------------------------------------------------------------------------
        public override void OnGraphStop(Playable playable) {
            
            var clipAssets = GetClipAssets();
            foreach (KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset> kv in clipAssets) {
                StreamingImageSequencePlayableAsset sisAsset = kv.Value;                
                sisAsset.OnGraphStop(playable);
                sisAsset.BindTimelineClip(null);
            }
            
        }
        
//----------------------------------------------------------------------------------------------------------------------
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            int inputCount = playable.GetInputCount<Playable>();
            if (inputCount == 0) {
                return; // it doesn't work as mixer.
            }

            double directorTime = GetPlayableDirector().time;

            bool activeTimelineClipFound = false;
            int i = 0;
            var clipAssets = GetClipAssets();
            foreach (KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset> kv in clipAssets) {
                TimelineClip clip = kv.Key;
                StreamingImageSequencePlayableAsset sisAsset = kv.Value;


                IList<string> imagePaths = sisAsset.GetImagePaths();
                if (null == imagePaths || null == clip.parentTrack)
                    continue;

                double startTime = clip.start;
                double endTime = clip.end;

                double loadStartOffsetTime = 1.0f + imagePaths.Count * 0.1f;

                //Start to preload images before the clip is active
                if ( directorTime>= startTime - loadStartOffsetTime && directorTime < endTime) {
                    sisAsset.ContinuePreloadingImages();                    
                }

                if (!activeTimelineClipFound && directorTime >= startTime && directorTime < endTime) {
                    ProcessActiveClipV(sisAsset, directorTime, clip);
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


//---------------------------------------------------------------------------------------------------------------------
        protected override void ProcessActiveClipV(StreamingImageSequencePlayableAsset asset,
            double directorTime, TimelineClip activeClip) 
        {
            int index = asset.GlobalTimeToImageIndex(activeClip, directorTime);

            bool texReady = asset.RequestLoadImage(index);
            if (texReady) {
                UpdateRendererTexture(asset);

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
        LoaderPeriodicJob m_loaderPeriodicJob;        
#endif

    }



} //end namespace