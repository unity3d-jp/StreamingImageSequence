using System.Collections.Generic;

#if UNITY_EDITOR        

namespace UnityEngine.StreamingImageSequence
{
internal class ImageLoadEditorUpdateTask : ITask {
    
    public void Execute() {

        if (m_requestedImageLoadBGTasks.Count <= 0)
            return;
        
        //Don't push everything to ThreadManager
        const int BACKGROUND_TASKS_WAIT_THRESHOLD = 32;
        int       numBackGroundTasks              = ThreadManager.GetNumBackGroundTasks();
        if (numBackGroundTasks >= BACKGROUND_TASKS_WAIT_THRESHOLD) {
            return;
        }

        BaseImageLoadBGTask task = m_requestedImageLoadBGTasks.Dequeue();
        m_taskHashSet.Remove(task.GetImagePath());

        ThreadManager.QueueBackGroundTask(task);
    }

    
//----------------------------------------------------------------------------------------------------------------------

    internal void RequestLoadImage(BaseImageLoadBGTask task) {
        //Clear old tasks
        int taskRequestFrame = task.GetRequestFrame();
        if (taskRequestFrame > m_latestFrame) {
            m_latestFrame = taskRequestFrame;
            m_requestedImageLoadBGTasks.Clear();
            m_taskHashSet.Clear();
        }

    
        string imagePath = task.GetImagePath();
        if (m_taskHashSet.Contains(imagePath)) {
            return;            
        }
                          
        m_requestedImageLoadBGTasks.Enqueue(task);
        m_taskHashSet.Add(imagePath);
            

    }


    
//----------------------------------------------------------------------------------------------------------------------
    
    readonly Queue<BaseImageLoadBGTask> m_requestedImageLoadBGTasks = new Queue<BaseImageLoadBGTask>();
    readonly HashSet<string> m_taskHashSet = new HashSet<string>();
    
    
    private int m_latestFrame = 0;

}

} //end namespace

#endif //UNITY_EDITOR