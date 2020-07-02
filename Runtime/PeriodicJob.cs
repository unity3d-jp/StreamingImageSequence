namespace UnityEngine.StreamingImageSequence  { 

    internal abstract class PeriodicJob  {
        public UpdateManager.JobOrder m_order;
        internal bool m_RemoveRequestFlag;
        internal bool m_InitializedFlag;
        public abstract void Execute();
        public abstract void Initialize();
        public abstract void Cleanup(); // Uninitialize
        public abstract void Reset();   // called while resetting.

        public  PeriodicJob(UpdateManager.JobOrder order) {
            m_order = order;
        }
        public void AddToUpdateManager() {
            UpdateManager.AddPeriodicJob( this);
        }

        public void RemoveFromUpdateManager() {
            UpdateManager.RemovePeriodicJob(this);
        }

        public void RemoveIfFinished()
        {
            m_RemoveRequestFlag = true;
        }
    }

} //end namespace

