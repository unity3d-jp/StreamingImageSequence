//#define DEBUG_THREAD

namespace UnityEngine.StreamingImageSequence
{

//[ExecuteInEditMode]
internal class StreamingImageSequenceNativeRenderer : MonoBehaviour {
#if DEBUG_THREAD
    int m_timer = 0;
    private void OnGUI()
    {
        string str = "" + m_timer;
        GUI.TextArea(new Rect(10, 10, Screen.width - 20, 24), str);
        for (int ii = 0; ii < EditorUpdateManager.NUM_THREAD; ii++)
        {
            GUI.TextArea(new Rect(10, 26 + 24 * ii, Screen.width - 20, 24), "" + EditorUpdateManager.GetThreadTickCount(ii));
        }
        m_timer++;
    }
#endif

}

}