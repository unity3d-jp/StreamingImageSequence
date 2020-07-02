namespace UnityEngine.StreamingImageSequence  { 

internal abstract class PeriodicJob  {
    public abstract void Execute();
    public abstract void Cleanup();
    protected PeriodicJob() { }

}

} //end namespace

