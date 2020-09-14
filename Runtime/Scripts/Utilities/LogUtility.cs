
namespace UnityEngine.StreamingImageSequence
{


    internal static class LogUtility
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
