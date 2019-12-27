using System;
using System.IO;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.StreamingImageSequence {
    
    //ITimelineClipAsset interface is used to define the clip capabilities (ClipCaps) 
    [System.Serializable]
    public class StreamingImageSequencePlayableAsset : PlayableAsset, ITimelineClipAsset  {
//----------------------------------------------------------------------------------------------------------------------        
        public StreamingImageSequencePlayableAsset() {
            m_loadingIndex = -1;
        }
//----------------------------------------------------------------------------------------------------------------------        
        
        ~StreamingImageSequencePlayableAsset() {
            Reset();
        }
//----------------------------------------------------------------------------------------------------------------------        

        public int GetVersion() { return m_version; }
        public IList<string> GetImagePaths() { return m_imagePaths; }
        public ImageDimensionInt GetResolution() { return m_resolution; }
        public System.Collections.IList GetImagePathsNonGeneric() { return m_imagePaths; }
        public Texture2D GetTexture() { return m_texture; }

        public string GetImagePath(int index) {
            if (null == m_imagePaths || index >= m_imagePaths.Count)
                return null;

            return m_imagePaths[index];
        }

        public string GetFolder() { return m_folder; }
        public UnityEditor.DefaultAsset GetTimelineDefaultAsset() { return m_timelineDefaultAsset; }

//----------------------------------------------------------------------------------------------------------------------        
        internal void Reset() {
            m_loadingIndex = -1;
            m_lastIndex = -1;
            LoadRequested = null;
            ResetTexture();
            m_resolution = new ImageDimensionInt();
        }
//----------------------------------------------------------------------------------------------------------------------        

        public ClipCaps clipCaps {
            get { return ClipCaps.None; }
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

//----------------------------------------------------------------------------------------------------------------------        

         
        public void SetParam(StreamingImageSequencePlayableAssetParam param) {
            m_resolution = param.Resolution;
            m_imagePaths = param.Pictures;
            m_folder = param.Folder;
            if (m_folder.StartsWith("Assets")) {
                m_timelineDefaultAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.DefaultAsset>(m_folder);
            } else {
                m_timelineDefaultAsset = null;
            }
            m_texture = null;
            EditorUtility.SetDirty(this);
        }

//---------------------------------------------------------------------------------------------------------------------

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go) {
            //Dummy. We just need to implement this from PlayableAsset because folder D&D support. See notes below
            return Playable.Null;
        }
//---------------------------------------------------------------------------------------------------------------------

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

                LoadRequest(ii, false, out ReadResult readResult);

            }
            m_loadingIndex = loadRequestMax;

            
        }

        internal bool IsLoadRequested(int index)
        {
            string filename = m_imagePaths[index];
            filename = GetCompleteFilePath(filename);
            StreamingImageSequencePlugin.GetNativTextureInfo(filename, out ReadResult readResult);
            return (readResult.ReadStatus != 0);

        }

//----------------------------------------------------------------------------------------------------------------------        
        internal string LoadRequest(int index, bool isBlocking, out ReadResult readResult) {
            string filename = m_imagePaths[index];
            filename = GetCompleteFilePath(filename);
            if (LoadRequested == null) {
                LoadRequested = new bool[m_imagePaths.Count];
            }

            StreamingImageSequencePlugin.GetNativTextureInfo(filename, out readResult);
            //Debug.Log("readResult.readStatus " + readResult.readStatus + "Loading " + filename);
            if (readResult.ReadStatus == 0) {
                new BGJobPictureLoader(filename);
            }
            if ( isBlocking ) {
                while (readResult.ReadStatus != 2) {
                    StreamingImageSequencePlugin.GetNativTextureInfo(filename, out readResult);
                }
            }
#if false //UNITY_EDITOR
            if ( readResult.readStatus == 1 )
            {
                Util.Log("Already requestd:" + filename);
            }
#endif
            return filename;
        }
//----------------------------------------------------------------------------------------------------------------------        
        internal bool RequestLoadImage(int index, bool isBlocking)
        {
            if (null == m_imagePaths || index < 0 || index >= m_imagePaths.Count || string.IsNullOrEmpty(m_imagePaths[index])) {
                return false;
            }
           
            string filename = LoadRequest(index,isBlocking, out ReadResult readResult);

            if (null == m_texture &&  readResult.ReadStatus == (int)LoadStatus.Loaded)
            {
#if UNITY_STANDALONE_OSX
				const TextureFormat textureFormat = TextureFormat.RGBA32;
#elif UNITY_STANDALONE_WIN
                const TextureFormat textureFormat = TextureFormat.BGRA32;
#endif
                m_texture = new Texture2D(readResult.Width, readResult.Height, textureFormat, false, false);
                m_texture.LoadRawTextureData(readResult.Buffer, readResult.Width * readResult.Height * 4);
                m_texture.filterMode = FilterMode.Bilinear;
                m_texture.Apply();

                IntPtr ptr =  m_texture.GetNativeTexturePtr();
                int texInstanceID = m_texture.GetInstanceID();
                
                UpdateResolution(readResult);
                StreamingImageSequencePlugin.SetNativeTexturePtr(ptr, (uint)readResult.Width, (uint)readResult.Height, texInstanceID);
            }

            //Update the texture
			if (readResult.ReadStatus == (int)LoadStatus.Loaded && m_lastIndex != index) {
                int texInstanceID = m_texture.GetInstanceID();
                StreamingImageSequencePlugin.SetLoadedTexture (filename, texInstanceID);
                GL.IssuePluginEvent(StreamingImageSequencePlugin.GetRenderEventFunc(), texInstanceID);
			}

			m_lastIndex = index;
            return null!=m_texture;
        }
//----------------------------------------------------------------------------------------------------------------------        

        public  string GetCompleteFilePath(string filePath)
        {
            string strOverridePath = m_folder;

            if (!string.IsNullOrEmpty(strOverridePath)) {
                filePath = Path.Combine(strOverridePath, filePath).Replace("\\", "/");
            }

            if (Path.IsPathRooted(filePath)) {
                filePath = Path.Combine(UpdateManager.GetProjectFolder(), filePath).Replace("\\", "/");
            }
            return filePath;
        }

//---------------------------------------------------------------------------------------------------------------------
        void ResetTexture() {
            if (null != m_texture) {
                StreamingImageSequencePlugin.ResetLoadedTexture(m_texture.GetInstanceID());
                m_texture = null;
            }
        }

//---------------------------------------------------------------------------------------------------------------------
        void UpdateResolution(ReadResult readResult) {
            m_resolution.Width  = readResult.Width;
            m_resolution.Height = readResult.Height;
        }
        
//---------------------------------------------------------------------------------------------------------------------

        [SerializeField] private string m_folder;
        [SerializeField] List<string> m_imagePaths;
        [SerializeField] private int m_version = STREAMING_IMAGE_SEQUENCE_PLAYABLE_ASSET_VERSION;        
        [SerializeField] private ImageDimensionInt  m_resolution;

#if UNITY_EDITOR
        [SerializeField] private UnityEditor.DefaultAsset m_timelineDefaultAsset = null; //Folder D&D. See notes below
#endif
        private bool[] LoadRequested;
        public int m_loadingIndex = -1;
		private int m_lastIndex = -1;
        private bool m_verified;

        Texture2D m_texture = null;

        private const int STREAMING_IMAGE_SEQUENCE_PLAYABLE_ASSET_VERSION = 1;

    }
}

//---------------------------------------------------------------------------------------------------------------------
//[Note-Sin: 2019-12-23] We need two things, in order to enable folder drag/drop to the timeline Window
//1. Derive this class from PlayableAsset
//2. Declare UnityEditor.DefaultAsset variable 



