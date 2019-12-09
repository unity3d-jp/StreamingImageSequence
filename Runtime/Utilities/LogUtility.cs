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

namespace UnityEngine.StreamingImageSequence
{


    public static class LogUtility
    {
        static bool s_isLogEnabled = false;

        public static void LogDebug(object message)
        {
			if (Enabled)
            {
                UnityEngine.Debug.Log(message);
            }
        }

        static bool Enabled
        {
			get {
				// unable to  call this from back thread.
				// return UnityEngine.Debug.isDebugBuild;			
				return s_isLogEnabled;
			}

        }

    }
}
