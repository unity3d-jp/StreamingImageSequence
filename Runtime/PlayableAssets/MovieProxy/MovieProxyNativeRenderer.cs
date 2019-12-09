//#define DEBUG_THREAD

using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityEngine.StreamingImageSequence
{

//[ExecuteInEditMode]
public class MovieProxyNativeRenderer : MonoBehaviour {
#if DEBUG_THREAD
    int m_timer = 0;
#endif
	bool m_ColutinStarted = false;
    private void Awake()
    {
    }
    void Start () {

    }
	
	void Update () {
		// Sometimes the GameObject should be disabled in Activaiton Track.
		// So, we need to restart coroutines in such case.
		if (UpdateManager.useCoroutine && !m_ColutinStarted) {
			StartCoroutine (OnRender ());
			m_ColutinStarted = true;
		}

	}

    void LateUpdate()
    {

    }

	void OnDisable() {
		m_ColutinStarted = false;
	}
#if DEBUG_THREAD
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