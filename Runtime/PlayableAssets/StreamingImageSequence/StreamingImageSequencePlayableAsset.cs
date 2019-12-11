using System;
using System.IO;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.StreamingImageSequence {
    [System.Serializable]
    public class StreamingImageSequencePlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        [System.Serializable]
        public struct StPicResolution
        {
            public int Width;
            public int Height;
        };

        [System.Serializable]
        public struct StQuadSize
        {
            public float sizX;
            public int sizY;
        }

        public int Version;
        public StreamingImageSequencePlayableAssetParam.StPicResolution Resolution;
        public StreamingImageSequencePlayableAssetParam.StQuadSize QuadSize;
        public bool m_displayOnClipsOnly;
        private bool[] LoadRequested;
        public int m_loadingIndex = -1;
		private int m_lastIndex = -1;
        private bool m_verified;

        public IList<string> GetImagePaths() { return m_imagePaths; }
        public System.Collections.IList GetImagePathsNonGeneric() { return m_imagePaths; }

        public string GetImagePath(int index) {
            if (null == m_imagePaths || index >= m_imagePaths.Count)
                return null;

            return m_imagePaths[index];
        }

        public string GetFolder() { return m_folder; }
        public UnityEditor.DefaultAsset GetTimelineDefaultAsset() { return m_timelineDefaultAsset; }

        internal void Reset()
        {
            m_loadingIndex = -1;
            m_lastIndex = -1;
            LoadRequested = null;
        }

        public ClipCaps clipCaps
        {
            get
            {
                return ClipCaps.None;
            }
        }

        public bool Verified
        {
            get
            {
                if (!m_verified)
                {
                    m_verified = !string.IsNullOrEmpty(m_folder) && 
                                 m_folder.StartsWith("Assets/StreamingAssets") &&
                                 Directory.Exists(m_folder) && 
                                 m_imagePaths != null && 
                                 m_imagePaths.Count > 0;
                }
                
                return m_verified;
            }
        }


        public StreamingImageSequencePlayableAsset()
        {
            m_loadingIndex = -1;
        }
         
        public void SetParam(StreamingImageSequencePlayableAssetParam param)
        {
            Version = param.Version;
            Resolution = param.Resolution;
            QuadSize = param.QuadSize;
            m_imagePaths = param.Pictures;
            m_folder = param.Folder;
            if (m_folder.StartsWith("Assets")) {
                m_timelineDefaultAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.DefaultAsset>(m_folder);
            } else {
                m_timelineDefaultAsset = null;
            }
            EditorUtility.SetDirty(this);
        }



        // Factory method that generates a playable based on this asset
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go) {


            var bh = new StreamingImageSequencePlayableBehaviour();
            return ScriptPlayable<StreamingImageSequencePlayableBehaviour>.Create(graph,bh);
        }

        internal void LoadRequest(bool isDirectorIdle) {
            if (null == m_imagePaths)
                return;

            int numPictures = m_imagePaths.Count;
            if (LoadRequested == null && numPictures > 0) {
                LoadRequested = new bool[numPictures];
            }

            // request loading while editor is idle.
            if (isDirectorIdle)
            {
                LoadStep(4);
            }
            else
            {
                LoadStep(2);
            }
        }

        private void LoadStep(int step)
        {
            if (UpdateManager.IsPluginResetting())
            {
                return;
            }
            StReadResult tResult = new StReadResult();
            int loadRequestMax = m_loadingIndex + step;
            if (loadRequestMax > m_imagePaths.Count)
            {
                loadRequestMax = m_imagePaths.Count;
            }
            for (int ii = m_loadingIndex; ii <= loadRequestMax - 1; ii++)
            {
                if (ii == -1)
                {
                    continue;
                }

                LoadRequest(ii, false, out tResult);

            }
            m_loadingIndex = loadRequestMax;

            
        }

        internal bool IsLoadRequested(int index)
        {
            string filename = m_imagePaths[index];
            filename = GetCompleteFilePath(filename);
            StReadResult tResult = new StReadResult();
            StreamingImageSequencePlugin.GetNativTextureInfo(filename, out tResult);
            return (tResult.readStatus != 0);

        }
        internal string LoadRequest(int index, bool isBlocking, out StReadResult tResult)
        {
            string filename = m_imagePaths[index];
            filename = GetCompleteFilePath(filename);
            if (LoadRequested == null)
            {
                LoadRequested = new bool[m_imagePaths.Count];
            }

            StreamingImageSequencePlugin.GetNativTextureInfo(filename, out tResult);
            //Debug.Log("tResult.readStatus " + tResult.readStatus + "Loading " + filename);
            if (tResult.readStatus == 0)
            {
                new BGJobPictureLoader(filename);
            }
            if ( isBlocking )
            {
                while (tResult.readStatus != 2)
                {
                    StreamingImageSequencePlugin.GetNativTextureInfo(filename, out tResult);
                    
                }
            }
#if false //UNITY_EDITOR
            if ( tResult.readStatus == 1 )
            {
                Util.Log("Already requestd:" + filename);
            }
#endif
            return filename;
        }
        internal bool SetTexture(GameObject go, int index, bool isBlocking, bool isAlreadySet)
        {
            if (null == m_imagePaths || index < 0 || index >= m_imagePaths.Count || string.IsNullOrEmpty(m_imagePaths[index])) {
                return false;
            }


            var sID = go.GetInstanceID();
            StReadResult tResult = new StReadResult();
            
            string filename = LoadRequest(index,isBlocking, out tResult);

            if (!isAlreadySet &&  tResult.readStatus == 2)
            {
                Texture2D tex = null;
#if UNITY_STANDALONE_OSX
				var textureFormat = TextureFormat.RGBA32;
#elif UNITY_STANDALONE_WIN
                var textureFormat = TextureFormat.BGRA32;
#endif

				tex = new Texture2D(tResult.width, tResult.height, textureFormat, false, false);
                tex.LoadRawTextureData(tResult.buffer, tResult.width * tResult.height * 4);
                tex.filterMode = FilterMode.Bilinear;
                tex.Apply();

                var renderer = go.GetComponent<Renderer>();
                Image image = null;
                SpriteRenderer spriteRenderer = null;
                IntPtr ptr = IntPtr.Zero;

                if ((spriteRenderer = go.GetComponent<SpriteRenderer>()) != null)
                {
                    spriteRenderer.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f, 2, SpriteMeshType.FullRect);
                    ptr = spriteRenderer.sprite.texture.GetNativeTexturePtr();
                    Assert.IsTrue(ptr != IntPtr.Zero);
                }
                else if (renderer != null)
                {
                    var mat = go.GetComponent<Renderer>().sharedMaterial;
                    mat.mainTexture = tex; //
                    ptr = mat.mainTexture.GetNativeTexturePtr();
                    Assert.IsTrue(ptr != IntPtr.Zero);
                }
                else if ((image = go.GetComponent<Image>()) != null)
                {
                    image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f, 1, SpriteMeshType.FullRect);
                    ptr = image.mainTexture.GetNativeTexturePtr();
                    Assert.IsTrue(ptr != IntPtr.Zero);
					var material = image.material;
#if UNITY_STANDALONE_WIN
#if UNITY_2017_2_OR_NEWER
#else
                    if (material != null) {
						var id = Shader.PropertyToID("_GammaCorrection");
						if (id > 0) {
							material.SetInt (id, 1);
						}
                    }
#endif

#endif
                }

                StreamingImageSequencePlugin.SetNativeTexturePtr(ptr, (uint)tResult.width, (uint)tResult.height, sID);
                isAlreadySet = true;
 

            }
			bool textureIsSet = false;
			if (tResult.readStatus == 2 && m_lastIndex != index) {
				StreamingImageSequencePlugin.SetLoadedTexture (filename, sID);
				textureIsSet = true;
			}
			if (textureIsSet)
            {
                GL.IssuePluginEvent(StreamingImageSequencePlugin.GetRenderEventFunc(), sID);
            }
			m_lastIndex = index;
            return isAlreadySet;
        }

        public  string GetCompleteFilePath(string filePath)
        {
            string strOverridePath = m_folder;

            if (!string.IsNullOrEmpty(strOverridePath))
            {
                filePath = Path.Combine(strOverridePath, filePath).Replace("\\", "/");

            }

            if (Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(UpdateManager.GetProjectFolder(), filePath).Replace("\\", "/");
            }
            return filePath;
        }

//---------------------------------------------------------------------------------------------------------------------
        [SerializeField] private string m_folder;
        [SerializeField] List<string> m_imagePaths;

#if UNITY_EDITOR
        [SerializeField] private UnityEditor.DefaultAsset m_timelineDefaultAsset = null; //Enabling Folder D&D to Timeline
#endif

    }


}