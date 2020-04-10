using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence
{ 
    /// <summary>
    /// A track which clip type is StreamingImageSequencePlayableAsset.
    /// It shows the active image from the images assigned to StreamingImageSequencePlayableAsset,
    /// into a bound GameObject that has StreamingImageSequenceNativeRenderer component.
    /// </summary>
    [TrackClipType(typeof(StreamingImageSequencePlayableAsset))]
    [TrackBindingType(typeof(StreamingImageSequenceNativeRenderer))]
    [TrackColor(0.776f, 0.263f, 0.09f)]
    public class StreamingImageSequenceTrack : TrackAsset
    {
        LoaderPeriodicJob m_LoaderPeriodicJob;
        
        
        /// <inheritdoc/>
        public StreamingImageSequenceTrack()
        {
            LogUtility.LogDebug("StreamingImageSequenceTrack creating ObserverPeriodicJob");
            m_LoaderPeriodicJob = new LoaderPeriodicJob(this);
            m_LoaderPeriodicJob.AddToUpdateManger();
            // above job is removed when finished to load by calling RemoveIfFinished();
        }
#if false
        protected override Playable CreatePlayable(PlayableGraph graph, GameObject go, TimelineClip clip)
        {
            var playable = (ScriptPlayable<StreamingImageSequencePlayableBehaviour>)base.CreatePlayable(graph, go, clip);
            var myBehaviour = playable.GetBehaviour() as StreamingImageSequencePlayableBehaviour;
            myBehaviour.m_clip = clip;
            return playable;
        }
#endif

        /// <inheritdoc/>
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
            var mixer = ScriptPlayable<StreamingImageSequencePlayableMixer>.Create(graph, inputCount);
            var director = go.GetComponent<PlayableDirector>();
            if (director != null) {
                var boundGo = director.GetGenericBinding(this);
                StreamingImageSequenceNativeRenderer nativeRenderer = boundGo as StreamingImageSequenceNativeRenderer;
                StreamingImageSequencePlayableMixer bh = mixer.GetBehaviour();
                bh.m_track = this;
                bh.Init(null == nativeRenderer ? null : nativeRenderer.gameObject, director, GetClips());
            }

            m_trackMixer = mixer.GetBehaviour();
            return mixer;
        }

//---------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Get the currently active PlayableAsset in the track according to the time of PlayableDirector
        /// </summary>
        /// <returns>The TimelineClip's asset as StreamingImageSequencePlayableAsset</returns>
        public StreamingImageSequencePlayableAsset GetActivePlayableAsset() {
            m_trackMixer.GetActiveTimelineClipInto(out TimelineClip clip, out StreamingImageSequencePlayableAsset asset);
            return asset;
        }
        
//---------------------------------------------------------------------------------------------------------------------

        /// <inheritdoc/>
        protected override  void OnAfterTrackDeserialize() {
            //Re-setup the PlayableAsset
            foreach (TimelineClip clip in m_Clips) {
                StreamingImageSequencePlayableAsset playableAsset = clip.asset as StreamingImageSequencePlayableAsset;
                if (null == playableAsset)
                    continue;
                
                playableAsset.OnAfterTrackDeserialize(clip);
            }
        }

//---------------------------------------------------------------------------------------------------------------------

        private StreamingImageSequencePlayableMixer m_trackMixer = null;

    }

}