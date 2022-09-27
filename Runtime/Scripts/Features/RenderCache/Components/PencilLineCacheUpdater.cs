#if AT_USE_PENCILLINE

using System.Collections.Generic;
using Unity.StreamingImageSequence;
using UnityEngine;
using UnityEngine.Playables;

public class PencilLineCacheUpdater : MonoBehaviour {
    internal bool IsUpdateSeparately() => m_updateSeparately;

    internal PlayableDirector GetPlayableDirector() => m_playableDirector;

    internal void SetPlayableDirector(PlayableDirector director) {
        m_playableDirector = director;
    }

    internal RenderCachePlayableAsset GetRenderCachePlayableAsset() => m_renderCachePlayableAsset;

    internal void SetRenderCachePlayableAsset(RenderCachePlayableAsset playableAsset) {
        m_renderCachePlayableAsset = playableAsset;
    }
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------


    internal IEnumerable<PencilLineCacheInfo> EnumerateSeparateCacheInfo() {
        foreach (PencilLineCacheInfo cacheInfo in m_separateCacheInfoList)
            yield return cacheInfo;
    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------

    [SerializeField] [HideInInspector] private PlayableDirector m_playableDirector;

    [SerializeField] [HideInInspector] private RenderCachePlayableAsset m_renderCachePlayableAsset;

    [SerializeField] private bool                      m_updateSeparately;
    [SerializeField] private List<PencilLineCacheInfo> m_separateCacheInfoList;
}

#endif //AT_USE_PENCILLINE
