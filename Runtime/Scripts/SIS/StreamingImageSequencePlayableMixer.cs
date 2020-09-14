using System.Collections.Generic;
using UnityEngine.Assertions;
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
            m_editorUpdateTask = new SISPlayableMixerEditorUpdateTask(this);
            EditorUpdateManager.AddEditorUpdateTask( m_editorUpdateTask);
#endif //UNITY_EDITOR          
           
        }

        public override void OnPlayableDestroy(Playable playable) {
            
            var clipAssets = GetClipAssets();
            foreach (KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset> kv in clipAssets) {
                StreamingImageSequencePlayableAsset sisAsset = kv.Value;
                sisAsset.OnPlayableDestroy(playable);
            }
            
            base.OnPlayableDestroy(playable);
            
#if UNITY_EDITOR            
            EditorUpdateManager.RemoveEditorUpdateTask( m_editorUpdateTask);        
#endif //UNITY_EDITOR          
        }

        
//----------------------------------------------------------------------------------------------------------------------
        public override void OnGraphStart(Playable playable) {
           
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
            }
            
        }
        
//----------------------------------------------------------------------------------------------------------------------
        public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
            
            base.ProcessFrame(playable, info, playerData); // Calls ProcessActiveClipV()

#if UNITY_EDITOR            
            if (!Application.isPlaying) {
                return;
            }
#endif            
            
            //Preload images here only in play mode
            double directorTime = GetPlayableDirector().time;            
            var clipAssets = GetClipAssets();
            foreach (KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset> kv in clipAssets) {
                TimelineClip clip = kv.Key;
                StreamingImageSequencePlayableAsset sisAsset = kv.Value;

                int numImages  = sisAsset.GetNumImages();
                if (numImages <= 0|| null == clip.parentTrack)
                    continue;

                double startTime = clip.start;
                double endTime = clip.end;
                double loadStartOffsetTime = 1.0f + numImages * 0.1f;

                //Start to preload images before the clip is active
                if ( directorTime>= startTime - loadStartOffsetTime && directorTime < endTime) {
                    sisAsset.ContinuePreloadingImages();                    
                }

            }

        }

#endregion        


//---------------------------------------------------------------------------------------------------------------------
        protected override void ProcessActiveClipV(StreamingImageSequencePlayableAsset asset,
            double directorTime, TimelineClip activeClip) 
        {
            int numImages  = asset.GetNumImages();
            if (numImages <=0 || null == activeClip.parentTrack)
                return;
            
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

        protected override void InitInternalV(GameObject gameObject) {
            bool ret = (null!=gameObject && InitRenderers(gameObject));
            if (!ret) {
                Reset();
            }
        }


//---------------------------------------------------------------------------------------------------------------------
        private bool InitRenderers(GameObject go) {
            Assert.IsNotNull(go);

            m_spriteRenderer= go.GetComponent<SpriteRenderer>();
            m_meshRenderer  = go.GetComponent<MeshRenderer>();
            if (null == m_meshRenderer) {
                m_meshRenderer = go.GetComponent<SkinnedMeshRenderer>();                
            }
            
            m_image         = go.GetComponent<Image>();
            m_sisRenderer = go.GetComponent<StreamingImageSequenceRenderer>();
            return (null!= m_sisRenderer);
        }


//---------------------------------------------------------------------------------------------------------------------
        protected override void ShowObjectV(bool show) {
            if (null!=m_spriteRenderer) {
                m_spriteRenderer.enabled = show;
            } else if (null != m_meshRenderer) {
                m_meshRenderer.enabled = show;
            } else if (null!=m_image) {
                m_image.enabled = show;
            } 
        }
        
//---------------------------------------------------------------------------------------------------------------------

        //[TODO-sin: 2020-7-22] This should be moved to StreamingImageSequenceRenderer
        void UpdateRendererTexture(StreamingImageSequencePlayableAsset asset) {
            Texture2D tex = asset.GetTexture();

            const int NO_MATERIAL_OUTPUT = -1;

            RenderTexture rt = m_sisRenderer.GetTargetTexture();
            if (null != rt) {
                Graphics.Blit(tex, rt);                
            }
                        
            if (null!=m_spriteRenderer ) {
                Sprite sprite = m_spriteRenderer.sprite;
                if (sprite.texture != tex) {
                    m_spriteRenderer.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f, 2, SpriteMeshType.FullRect);
                }
                
            } else if (null!=m_meshRenderer) {
                Material mat;
                int materialIndex = m_sisRenderer.GetMaterialIndexToUpdate();
                if (materialIndex <= NO_MATERIAL_OUTPUT) {
                    return;
                }
                
                int materialsLength = m_meshRenderer.sharedMaterials.Length;
                
                // Debug.Log(m_meshRenderer.sharedMaterial + "single material");
                if (materialsLength > 1 && materialIndex < materialsLength) {
                    mat = m_meshRenderer.sharedMaterials[materialIndex];
                } else  {                    
                   mat = m_meshRenderer.sharedMaterial;
                }
                mat.mainTexture = tex;
                
            }else if (null!= m_image) {
                Sprite sprite = m_image.sprite;
                if (null==sprite || sprite.texture != tex) {
                    m_image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f, 1, SpriteMeshType.FullRect);
                }
            }

        }

//---------------------------------------------------------------------------------------------------------------------
       
        private SpriteRenderer  m_spriteRenderer = null;
        private Renderer        m_meshRenderer = null;
        private Image           m_image = null;
        private StreamingImageSequenceRenderer m_sisRenderer = null;

#if UNITY_EDITOR
        SISPlayableMixerEditorUpdateTask m_editorUpdateTask;        
#endif

    }



} //end namespace