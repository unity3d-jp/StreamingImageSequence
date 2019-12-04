using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.StreamingImageSequence
{ 
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
        public string[] Pictures;
        private bool[] LoadRequested;
        public int m_loadingIndex = -1;
		private int m_sLastIndex = -1;

        public string GetFolder() { return m_folder; }

        internal void Reset()
        {
            m_loadingIndex = -1;
            m_sLastIndex = -1;
            LoadRequested = null;
        }

        public ClipCaps clipCaps
        {
            get
            {
                return ClipCaps.None;
            }
        }



        public StreamingImageSequencePlayableAsset()
        {
            m_loadingIndex = -1;
            Util.Log("StreamingImageSequencePlayableAsset");
        }
         
        public void SetParam(StreamingImageSequencePlayableAssetParam param)
        {
            Version = param.Version;
            Resolution = param.Resolution;
            QuadSize = param.QuadSize;
            Pictures = param.Pictures;
            m_folder = param.Folder;
        }



        // Factory method that generates a playable based on this asset
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go) {


            var bh = new MovieProxyPlayableBehaviour();
            return ScriptPlayable<MovieProxyPlayableBehaviour>.Create(graph,bh);
        }

        internal void LoadRequest(bool isDirectorIdle) {
            if (null == Pictures)
                return;

            int numPictures = Pictures.Length;
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
            if (loadRequestMax > Pictures.Length)
            {
                loadRequestMax = Pictures.Length;
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
            string filename = Pictures[index];
            filename = GetCompleteFilePath(filename);
            StReadResult tResult = new StReadResult();
            PluginUtil.GetNativTextureInfo(filename, out tResult);
            return (tResult.readStatus != 0);

        }
        internal string LoadRequest(int index, bool isBlocking, out StReadResult tResult)
        {
            string filename = Pictures[index];
            filename = GetCompleteFilePath(filename);
            if (LoadRequested == null)
            {
                LoadRequested = new bool[Pictures.Length];
            }

            PluginUtil.GetNativTextureInfo(filename, out tResult);
            //Debug.Log("tResult.readStatus " + tResult.readStatus + "Loading " + filename);
            if (tResult.readStatus == 0)
            {
                new BGJobPictureLoader(filename);
            }
            if ( isBlocking )
            {
                while (tResult.readStatus != 2)
                {
                    PluginUtil.GetNativTextureInfo(filename, out tResult);
                    
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

                PluginUtil.SetNativeTexturePtr(ptr, (uint)tResult.width, (uint)tResult.height, sID);
                isAlreadySet = true;
 

            }
			bool textureIsSet = false;
			if (tResult.readStatus == 2 && m_sLastIndex != index) {
				PluginUtil.SetLoadedTexture (filename, sID);
				textureIsSet = true;
			}
			if (textureIsSet && !UpdateManager.useCoroutine)
            {
                GL.IssuePluginEvent(PluginUtil.GetRenderEventFunc(), sID);
            }
			m_sLastIndex = index;
            return isAlreadySet;
        }

        public  string GetCompleteFilePath(string filePath)
        {
            string strOverridePath = m_folder;

            if (strOverridePath != null && strOverridePath != "")
            {
                filePath = Path.Combine(strOverridePath, Path.GetFileName(filePath)).Replace("\\", "/");

            }

            if (Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(UpdateManager.GetProjectFolder(), filePath).Replace("\\", "/");
            }
            else
            {
                string strStreamingAssets = "Assets/StreamingAssets";
                if (strOverridePath != null && strOverridePath.StartsWith(strStreamingAssets))
                {
                    string rest = strOverridePath.Substring(strStreamingAssets.Length + 1, strOverridePath.Length - strStreamingAssets.Length - 1);
                    string dir = UpdateManager.GetStreamingAssetPath();
                    string dir2 = Path.Combine(dir, rest);
                    filePath = Path.Combine(dir2, Path.GetFileName(filePath)).Replace("\\", "/");
                }
            }
            return filePath;
        }

//---------------------------------------------------------------------------------------------------------------------
        private string m_folder;

    }


}