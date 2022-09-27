using System;
using System.Collections.Generic;
using Unity.FilmInternalUtilities; //Required when using Timeline 1.4.x or below
using Unity.StreamingImageSequence;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

internal class RenderCachePlayableAssetPopup : EditorWindow {
    public static void Show(Rect popupRect, Vector2 size, TimelineAsset timelineAsset, Action<TimelineClip> onClipSelected) {
        RenderCachePlayableAssetPopup popup = CreateInstance<RenderCachePlayableAssetPopup>();
        popup.Init(timelineAsset, onClipSelected);
        popup.ShowAsDropDown(GUIUtility.GUIToScreenRect(popupRect), size);
    }

    private void Init(TimelineAsset timelineAsset, Action<TimelineClip> onClipSelected) {
        Assert.IsNotNull(timelineAsset);
        m_onClipSelected = onClipSelected;

        m_trackClips.Clear();
        m_trackClips.Add(null); //for "none option"

        foreach (TrackAsset t in timelineAsset.GetOutputTracks()) {
            RenderCacheTrack rcTrack = t as RenderCacheTrack;
            if (null == rcTrack)
                continue;

            foreach (TimelineClip clip in rcTrack.GetClips()) {
                m_trackClips.Add(clip);
            }
        }
    }
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void CreateGUI() {
        ListView list  = new ListView(m_trackClips, itemHeight: 21, MakeListItem, BindListItem);
        IStyle   style = list.style;
        style.flexGrow        = 1;
        style.borderLeftColor = style.borderRightColor = style.borderTopColor = style.borderBottomColor = Color.black;
        style.borderLeftWidth = style.borderRightWidth = style.borderTopWidth = style.borderBottomWidth = 1;
        rootVisualElement.Add(list);
    }

    private VisualElement MakeListItem() {
        return new Label();
    }

    private void BindListItem(VisualElement element, int index) {
        TimelineClip clip = m_trackClips[index];

        Label label = element as Label;
        Assert.IsNotNull(label);
        if (null == clip) {
            label.text = "None";
        } else {
            TrackAsset track = clip.GetParentTrack();
            label.text = $"{track.name}-{clip.displayName}";
        }


        element.userData = clip;
        element.RegisterCallback<MouseDownEvent>(OnMouseDown);
    }

    private void OnMouseDown(MouseDownEvent evt) {
        TimelineClip clip = ((VisualElement)evt.currentTarget).userData as TimelineClip;
        m_onClipSelected?.Invoke(clip);
        Close();
    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------

    private readonly List<TimelineClip>   m_trackClips     = new List<TimelineClip>();
    private          Action<TimelineClip> m_onClipSelected = null;
}