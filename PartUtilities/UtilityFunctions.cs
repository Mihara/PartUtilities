using System;
using UnityEngine;

namespace JSIPartUtilities
{
	public static class JUtil
	{
		public static bool debugLoggingEnabled = true;

		public static void LogMessage (object caller, string line, params object[] list)
		{
			if (debugLoggingEnabled)
				Debug.Log (String.Format (caller.GetType ().Name + ": " + line, list));
		}

		public static void LogErrorMessage (object caller, string line, params object[] list)
		{
			Debug.LogError (String.Format (caller.GetType ().Name + ": " + line, list));
		}
	}
}

