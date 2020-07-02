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
            m_LoaderPeriodicJob.AddToUpdateManager();
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
            PlayableDirector director = go.GetComponent<PlayableDirector>();
            m_trackMixer = mixer.GetBehaviour();
            
            if (director != null) {
                var boundGo = director.GetGenericBinding(this);
                StreamingImageSequenceNativeRenderer nativeRenderer = boundGo as StreamingImageSequenceNativeRenderer;
                m_trackMixer.Init(null == nativeRenderer ? null : nativeRenderer.gameObject, director, GetClips());
            }
            return mixer;
        }

//----------------------------------------------------------------------------------------------------------------------

        /// <inheritdoc/>
        public override string ToString() { return name; }
        
//----------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Get the currently active PlayableAsset in the track according to the PlayableDirector's time
        /// </summary>
        /// <returns>The TimelineClip's asset as StreamingImageSequencePlayableAsset. Returns null if there is no active
        /// PlayableAsset.
        /// </returns>
        public StreamingImageSequencePlayableAsset GetActivePlayableAsset() {
            double time = (null != m_trackMixer ) ? m_trackMixer.GetDirectorTime() : 0;
            StreamingImageSequencePlayableMixer.GetActiveTimelineClipInto(m_Clips, time,
                out TimelineClip clip, out StreamingImageSequencePlayableAsset asset
            );
            return asset;
        }
        
//---------------------------------------------------------------------------------------------------------------------

        private StreamingImageSequencePlayableMixer m_trackMixer = null;

    }

}