using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence
{
#if UNITY_EDITOR
    [InitializeOnLoad]

    internal class EditorPeriodicJob : PeriodicJob
    {

        static EditorPeriodicJob()
        {
            new EditorPeriodicJob();
        }

        public EditorPeriodicJob() : base(UpdateManager.JobOrder.Normal)
        {
            UpdateManager.AddPeriodicJob(this);
        }

        public override void Initialize()
        {

        }

        public override void Reset()
        {
        }

        public override void Cleanup()
        {

        }
        public override void Execute()
        {


        }

    }
#endif
}
