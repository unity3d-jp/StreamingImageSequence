using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityEditor.StreamingImageSequence {


[ScriptedImporter(1, "jstimeline")]
public class JstimelineImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        CreateTimeline(ctx.assetPath,null );
    }

    [MenuItem("Edit/Streaming Image Sequence/Import AE Timeline", false, 10)]
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

        string assetFolder = container.assetFolder;
        if (string.IsNullOrEmpty(assetFolder)) {
            assetFolder = Path.GetDirectoryName(strJsTimelinePath);
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
                strFootagePath = Path.Combine(assetFolder, strFootagePath);
            }
            string strFootageName = Path.GetFileNameWithoutExtension(strFootagePath);
            string strJsonFootage = File.ReadAllText(strFootagePath);
            StreamingImageSequencePlayableAssetParam trackMovieContainer = JsonUtility.FromJson<StreamingImageSequencePlayableAssetParam>(strJsonFootage);

            int numImages = trackMovieContainer.Pictures.Count;
            if (numImages > 0) {

                List<string> originalImagePaths = new List<string>(trackMovieContainer.Pictures);

                for (int xx = 0; xx < numImages; ++xx) {
                    string fileName = trackMovieContainer.Pictures[xx];
                    // replace '~' with the path to home (for Linux environment
                    if (fileName.StartsWith("~")) {
                        fileName = strHome + fileName.Substring(1);
                    }
                    trackMovieContainer.Pictures[xx] = Path.GetFileName(fileName);
                }
                
                string destFolder = Application.streamingAssetsPath;
                Directory.CreateDirectory(destFolder); //make sure the directory exists

                destFolder = Path.Combine(destFolder, strFootageName).Replace("\\", "/");
                trackMovieContainer.Folder = destFolder;

                for (int i=0;i<numImages;++i) {
                    string destFilePath = Path.Combine(destFolder, trackMovieContainer.Pictures[i]);
                    if (File.Exists(destFilePath)) {
                        File.Delete(destFilePath);
                    }
                    
                    string srcFilePath = Path.GetFullPath(Path.Combine(assetFolder, originalImagePaths[i])).Replace("\\", "/");
                    FileUtil.CopyFileOrDirectory(srcFilePath, destFilePath);
                }

            }

            var proxyAsset = ScriptableObject.CreateInstance<StreamingImageSequencePlayableAsset>();
            proxyAsset.SetParam(trackMovieContainer);
            proxyAsset.m_displayOnClipsOnly = true;
            var strProxyPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(strNewFolderPath, strFootageName + "_StreamingImageSequence.playable"));
            AssetDatabase.CreateAsset(proxyAsset, strProxyPath);

            StreamingImageSequenceTrack movieTrack = asset.CreateTrack<StreamingImageSequenceTrack>(null, strFootageName);
            TimelineClip clip = movieTrack.CreateDefaultClip();
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
            if (canvas != null) {
                canvasObj = canvas.gameObject;
            } else {
                canvasObj = new GameObject();
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                canvasObj.name = "Canvas";

            }

            directorGo.transform.SetParent(canvasObj.transform);
            directorGo.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

            GameObject newGo = new GameObject();
            Image image = newGo.AddComponent<Image>();
            StreamingImageSequenceNativeRenderer renderer = newGo.AddComponent<StreamingImageSequenceNativeRenderer>();

            RectTransform rectTransform = newGo.GetComponent<RectTransform>();
            rectTransform.SetParent(directorGo.transform);
            rectTransform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            rectTransform.sizeDelta = new Vector2(trackMovieContainer.Resolution.Width,
                                                  trackMovieContainer.Resolution.Height);

            newGo.name = strFootageName;
            newGo.SetActive(true);
            director.SetGenericBinding(movieTrack, renderer);
        }

        if ( ctx == null ) {
            AssetDatabase.Refresh();
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


} //end namespace

