using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using System.Reflection;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UTJTimelineUtil
{


    public class Util
    {
        static bool s_isLogEnabled = false;

        static public void Log(object message)
        {
			if (isEnable)
            {
                UnityEngine.Debug.Log(message);
            }
        }

        static bool isEnable
        {
			get {
				// unable to  call this from back thread.
				// return UnityEngine.Debug.isDebugBuild;			
				return s_isLogEnabled;
			}

        }

    }
}
