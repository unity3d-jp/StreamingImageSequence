namespace Unity.StreamingImageSequence.Editor
{
    
internal interface IEditorTask : ITask {
    void  Reset();
    float GetExecutionFrequency(); //once every xx in seconds
}

}
