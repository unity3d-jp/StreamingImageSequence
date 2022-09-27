#if AT_USE_PENCILLINE

using System.Collections;
using System.Collections.Generic;
using Pencil_4;
using Unity.EditorCoroutines.Editor;
using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using Unity.StreamingImageSequence;
using Unity.StreamingImageSequence.Editor;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[CustomEditor(typeof(PencilLineCacheUpdater))]
[CanEditMultipleObjects]
internal class PencilLineCacheUpdaterInspector : Editor {
    void OnEnable() {
        m_lineCacheUpdater = this.target as PencilLineCacheUpdater;
        Assert.IsNotNull(m_lineCacheUpdater);
    }

    public override void OnInspectorGUI() {
        EditorGUIDrawerUtility.DrawUndoableGUI(m_lineCacheUpdater, "PencilLineCacheUpdater",
            guiFunc: () => {
                return EditorGUILayout.ObjectField("Director", m_lineCacheUpdater.GetPlayableDirector(),
                    typeof(PlayableDirector), allowSceneObjects: true) as PlayableDirector;
            },
            updateFunc: (PlayableDirector director) => { m_lineCacheUpdater.SetPlayableDirector(director); }
        );

        PlayableDirector director = m_lineCacheUpdater.GetPlayableDirector();
        if (null == director) {
            EditorGUILayout.HelpBox("Please assign a PlayableDirector to browse RenderCache assets.", MessageType.Warning);
        }

        DrawRenderCachePlayableAssetGUI(director);

        RenderCachePlayableAsset renderCachePlayableAsset = m_lineCacheUpdater.GetRenderCachePlayableAsset();
        if (null == renderCachePlayableAsset) {
            EditorGUILayout.HelpBox("Click the browse button and assign a RenderCachePlayableAsset.", MessageType.Warning);
        }


        DrawDefaultInspector();
        GUILayout.Space(30);

        bool canUpdate = null != director && null != renderCachePlayableAsset;
        EditorGUI.BeginDisabledGroup(!canUpdate);
        if (GUILayout.Button("Update Pencil Line Cache")) {
            UpdatePencilLineCache(director, renderCachePlayableAsset);
        }

        EditorGUI.EndDisabledGroup();
    }

    void DrawRenderCachePlayableAssetGUI(PlayableDirector director) {
        const int BUTTON_WIDTH = 30;

        Rect rect = EditorGUILayout.GetControlRect();
        rect.width -= BUTTON_WIDTH;
        EditorGUI.ObjectField(rect, "Render Cache Playable Asset", m_lineCacheUpdater.GetRenderCachePlayableAsset(),
            typeof(RenderCachePlayableAsset), allowSceneObjects: false);


        Rect buttonRect = rect;
        buttonRect.x     = rect.x + rect.width;
        buttonRect.width = BUTTON_WIDTH;

        if (null == director) {
            return;
        }

        if (!GUI.Button(buttonRect, ".."))
            return;

        // Show the dropdown under the field only, but not the label
        Rect fieldRect = rect;
        fieldRect.xMin += EditorGUIUtility.labelWidth;

        if (null == director.playableAsset) {
            EditorUtility.DisplayDialog(SISEditorConstants.DIALOG_TITLE, ASSIGN_TIMELINE_ASSET_MSG, "OK");
            return;
        }

        TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;
        RenderCachePlayableAssetPopup.Show(fieldRect, new Vector2(fieldRect.width, 80), timelineAsset, OnClipSelected);
    }

    void OnClipSelected(TimelineClip clip) {
        m_lineCacheUpdater.SetRenderCachePlayableAsset(null == clip ? null : clip.asset as RenderCachePlayableAsset);
    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------

    void UpdatePencilLineCache(PlayableDirector director, RenderCachePlayableAsset renderCachePlayableAsset) {
        Assert.IsNotNull(director);
        Assert.IsNotNull(renderCachePlayableAsset);

        TimelineAsset timelineAsset = renderCachePlayableAsset.GetBoundClipData()?.GetOwner().GetParentTrack().timelineAsset;
        if (null == timelineAsset || timelineAsset != director.playableAsset) {
            EditorUtility.DisplayDialog(SISEditorConstants.DIALOG_TITLE, "The RenderCachePlayableAsset does not exist in the specified Director object", "OK");
            return;
        }

        EditorCoroutineUtility.StartCoroutineOwnerless(UpdatePencilLineCacheCoroutine(director, renderCachePlayableAsset));
    }

    private IEnumerator UpdatePencilLineCacheCoroutine(PlayableDirector director, RenderCachePlayableAsset renderCachePlayableAsset) {
        TimelineEditorUtility.SelectDirectorInTimelineWindow(director); //Need to show in TimelineWindow
        yield return null;

        if (TimelineEditor.inspectedAsset != director.playableAsset) {
            EditorUtility.DisplayDialog(SISEditorConstants.DIALOG_TITLE, "Can't show the specified Director in the Timeline Window. Please unlock it if it's locked.", "OK");
            yield break;
        }

        if (m_lineCacheUpdater.IsUpdateSeparately()) {
            yield return UpdateSeparatePencilLineCacheCoroutine(director, renderCachePlayableAsset);
        }
        else {
            yield return RenderCachePlayableAssetInspector.UpdateRenderCacheCoroutine(director, renderCachePlayableAsset);
        }
    }


    private IEnumerator UpdateSeparatePencilLineCacheCoroutine(PlayableDirector director,
        RenderCachePlayableAsset playableAsset) {
        Dictionary<LineNode, bool> origLineNodeEnabledDic = new Dictionary<LineNode, bool>();
        string                     origFolder             = playableAsset.GetFolder();


        foreach (PencilLineCacheInfo info in m_lineCacheUpdater.EnumerateSeparateCacheInfo()) {
            origLineNodeEnabledDic[info.lineNode] = info.lineNode.enabled;
            info.lineNode.enabled                 = false;
        }

        //disable all lineNodes        
        foreach (PencilLineCacheInfo info in m_lineCacheUpdater.EnumerateSeparateCacheInfo()) {
            info.lineNode.enabled = false;
        }


        foreach (PencilLineCacheInfo info in m_lineCacheUpdater.EnumerateSeparateCacheInfo()) {
            info.lineNode.enabled = true;
            playableAsset.SetFolder(info.cacheFolder);
            IEnumerator it = RenderCachePlayableAssetInspector.UpdateRenderCacheCoroutine(director, playableAsset);
            while (it.MoveNext()) {
                yield return null;
            }

            info.lineNode.enabled = false;
            yield return null;
        }

        //return original values        
        foreach (PencilLineCacheInfo info in m_lineCacheUpdater.EnumerateSeparateCacheInfo()) {
            info.lineNode.enabled = origLineNodeEnabledDic[info.lineNode];
        }

        playableAsset.SetFolder(origFolder);
    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    private PencilLineCacheUpdater m_lineCacheUpdater = null;

    private const string ASSIGN_TIMELINE_ASSET_MSG = "Please assign a TimelineAsset to the PlayableDirector.";
}


#endif //AT_USE_PENCILLINE
