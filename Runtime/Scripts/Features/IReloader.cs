namespace Unity.StreamingImageSequence
{
    
internal interface IReloader {

#if UNITY_EDITOR    
    void Reload();
#endif    
}

}
