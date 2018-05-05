using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor;
using UnityEngine.Playables;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UTJTimelineUtil;

[ScriptedImporter(1, "jstimeline")]
public class JstimelineImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        CreateTimeline(ctx.assetPath,null );
    }

    [MenuItem("Movie Proxy/Samples/Import AE Timeline")]
    static void CreateTimeline()
    {
        string strPath = EditorUtility.OpenFilePanel("Open File", "", "jstimeline");
        JstimelineImporter.CreateTimeline(strPath);
    }

    public static void CreateTimeline(string strJsTimelinePath, AssetImportContext ctx = null )
    {

        string strUnityPorjectFolder = null;
        Regex regAssetFolder = new Regex("/Assets$");
        strUnityPorjectFolder = Application.dataPath;
        strUnityPorjectFolder = regAssetFolder.Replace(strUnityPorjectFolder, "");

        // prepare paths

        var strFolderName = Path.GetDirectoryName(strJsTimelinePath);
        var strAssetName = Path.GetFileNameWithoutExtension(strJsTimelinePath);

        var strGuid = AssetDatabase.CreateFolder("Assets", strAssetName);
        var strNewFolderPath = AssetDatabase.GUIDToAssetPath(strGuid);

        /*
        var strMaterialFolder = Path.Combine(strNewFolderPath, "Material");
        if (!Directory.Exists(strMaterialFolder))
        {
            Directory.CreateDirectory(strMaterialFolder);
        }*/

        // create working folder
        var strJson = File.ReadAllText(strJsTimelinePath);
        var container = JsonUtility.FromJson<TimelineParam>(strJson);

        string strAssetFolder = null;
        if (container.assetFolder == "" || container.assetFolder == null)
        {
            strAssetFolder = strFolderName;
        }

        var strAssetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(strNewFolderPath, strAssetName + "_Timeline.playable"));
        TimelineAsset asset = ScriptableObject.CreateInstance<TimelineAsset>();
        AssetDatabase.CreateAsset(asset, strAssetPath);

        var directorGo = new GameObject(strAssetName);
        var director = directorGo.AddComponent<PlayableDirector>();
        director.playableAsset = asset;

        var strHome = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

        int numTracks = container.Tracks.Length;
 
        for (int index = numTracks - 1; index >= 0; index--)
        {
            var track = container.Tracks[index];
            string strFootagePath = track.Footage;
            // remove '~' if necessary
            if (strFootagePath.StartsWith("~"))
            {
                strFootagePath = strHome + strFootagePath.Substring(1);
            }
            if (!Path.IsPathRooted(strFootagePath))
            {
                strFootagePath = Path.Combine(strAssetFolder, strFootagePath);
            }
            string strFootageName = Path.GetFileNameWithoutExtension(strFootagePath);
            string strJsonFootage = File.ReadAllText(strFootagePath);
            MovieProxyPlayableAssetParam trackMovieContainer = JsonUtility.FromJson<MovieProxyPlayableAssetParam>(strJsonFootage);




            if (trackMovieContainer.Pictures.Length != 0)
            {
                // remove '~' if necessary
                for (int xx = 0; xx < trackMovieContainer.Pictures.Length; xx++)
                {
                    string filename = trackMovieContainer.Pictures[xx];
                    if (!filename.StartsWith("~"))
                    {
                        continue;
                    }
                    string newFileName = strHome + filename.Substring(1);
                    trackMovieContainer.Pictures[xx] = newFileName;

                }

                var strDir = trackMovieContainer.Pictures[0];
                strDir = Path.GetDirectoryName(strDir);
                for (int xx = 0; xx < trackMovieContainer.Pictures.Length; xx++)
                {
                    var strFileName = Path.GetFileName(trackMovieContainer.Pictures[xx]);
                    trackMovieContainer.Pictures[xx] = strFileName;
                }

                
                string strStreamingAssets = "Assets/StreamingAssets";
                string strDstFolder = Application.streamingAssetsPath;
 
                if (!Directory.Exists(strDstFolder))
                {
                    Directory.CreateDirectory(strDstFolder);
                }

                strDstFolder = Path.Combine(strDstFolder, strFootageName).Replace("\\", "/");
                if (!Directory.Exists(strDstFolder))
                {
                    Directory.CreateDirectory(strDstFolder);
                }

                for (int ii = 0; ii < trackMovieContainer.Pictures.Length; ii++)
                {
                    string strAbsFilePathDst = Path.Combine(strDstFolder, trackMovieContainer.Pictures[ii]).Replace("\\", "/");
                    if (File.Exists(strAbsFilePathDst))
                    {
                        File.Delete(strAbsFilePathDst);
                    }
                    string strAbsFilePathSrc = Path.Combine(strDir, trackMovieContainer.Pictures[ii]).Replace("\\", "/");
                    FileUtil.CopyFileOrDirectory(strAbsFilePathSrc, strAbsFilePathDst);
                }

                trackMovieContainer.Folder = strDstFolder;

            }




            var proxyAsset = ScriptableObject.CreateInstance<MovieProxyPlayableAsset>();
            proxyAsset.SetParam(trackMovieContainer);
            proxyAsset.m_displayOnClipsOnly = true;
            var strProxyPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(strNewFolderPath, strFootageName + "_MovieProxy.playable"));
            AssetDatabase.CreateAsset(proxyAsset, strProxyPath);

            var movieTrack = asset.CreateTrack<MovieProxyTrack>(null, strFootageName);
            var clip = movieTrack.CreateDefaultClip();
            clip.asset = proxyAsset;
            clip.start = track.Start;
            clip.duration = track.Duration;


            if (Object.FindObjectOfType(typeof(UnityEngine.EventSystems.EventSystem)) == null)
            {
                var es = new GameObject();
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                es.name = "EventSystem";
            }
            GameObject canvasObj = null;
            Canvas canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
            if (canvas != null)
            {
                canvasObj = canvas.gameObject;
            }
            else
            {
                canvasObj = new GameObject();
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                canvasObj.name = "Canvas";

            }

            directorGo.transform.SetParent(canvasObj.transform);
            directorGo.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

            var newGo = new GameObject();
            Image image = newGo.AddComponent<Image>();

            var rectTransform =
            newGo.GetComponent<RectTransform>();
            rectTransform.SetParent(directorGo.transform);
            rectTransform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            rectTransform.sizeDelta =
                new Vector2(trackMovieContainer.Resolution.Width,
                            trackMovieContainer.Resolution.Height);


            newGo.name = strFootageName;
            newGo.SetActive(true);
            director.SetGenericBinding(movieTrack, newGo);


        }

        if ( ctx == null )
        {
            // cause crash if this is called inside of OnImportAsset()
            UnityEditor.EditorApplication.delayCall += () => {
                Selection.activeGameObject = directorGo;
            };
        }
    }

    [System.Serializable]
    public class TimelineParam
    {
        public float Version;
        public string assetFolder;
        public TrackParam[] Tracks;
    }

    [System.Serializable]
    public class TrackParam
    {
        public float[] Position;
        public float Start;
        public float Duration;
        public string Footage;
    }
}



