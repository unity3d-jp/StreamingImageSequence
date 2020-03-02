//#define DEBUG_THREAD

using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

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
        for (int ii = 0; ii < UpdateManager.NUM_THREAD; ii++)
        {
            GUI.TextArea(new Rect(10, 26 + 24 * ii, Screen.width - 20, 24), "" + UpdateManager.GetThreadTickCount(ii));
        }
        m_timer++;
    }
#endif
    IEnumerator OnRender()
    {
        int instanceId = this.gameObject.GetInstanceID();
        for (;;)
        {
            yield return new WaitForEndOfFrame();
            GL.IssuePluginEvent(StreamingImageSequencePlugin.GetRenderEventFunc(), instanceId);

        }
    }

}

}