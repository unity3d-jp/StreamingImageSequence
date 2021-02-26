using System.Collections.Generic;


namespace Unity.StreamingImageSequence.Editor
{
internal class ImageLoadEditorTask : IEditorTask {

    public void Reset() {
        m_requestedImageLoadBGTasks.Clear();
        m_taskHashSet.Clear();
        m_latestFrame = 0;        
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    public void Execute() {

        if (m_requestedImageLoadBGTasks.Count <= 0)
            return;
        
        //Don't push everything to ThreadManager
        const int MAX_BACKGROUND_TASKS  = 16;
        int       numBackGroundTasks              = ThreadManager.GetNumBackGroundTasks();
        if (numBackGroundTasks >= MAX_BACKGROUND_TASKS) {
            return;
        }

        BaseImageLoadBGTask task = m_requestedImageLoadBGTasks.Dequeue();
        m_taskHashSet.Remove(task.GetImagePath());

        ThreadManager.QueueBackGroundTask(task);
    }

    
//----------------------------------------------------------------------------------------------------------------------

    internal bool RequestLoadImage(BaseImageLoadBGTask task) {
        //Clear old tasks
        int taskRequestFrame = task.GetRequestFrame();
        if (taskRequestFrame > m_latestFrame) {
            m_latestFrame = taskRequestFrame;
            m_requestedImageLoadBGTasks.Clear();
            m_taskHashSet.Clear();
        }
        
        
        const int MAX_PENDING_REQUESTS  = 32;
        if (m_requestedImageLoadBGTasks.Count >= MAX_PENDING_REQUESTS) {
            return false;
        }
    
        string imagePath = task.GetImagePath();
        if (m_taskHashSet.Contains(imagePath)) {
            return true;            
        }
                          
        m_requestedImageLoadBGTasks.Enqueue(task);
        m_taskHashSet.Add(imagePath);

        return true;
    }


    
//----------------------------------------------------------------------------------------------------------------------
    
    readonly Queue<BaseImageLoadBGTask> m_requestedImageLoadBGTasks = new Queue<BaseImageLoadBGTask>();
    readonly HashSet<string> m_taskHashSet = new HashSet<string>();
    private int m_latestFrame = 0;

}

} //end namespace

