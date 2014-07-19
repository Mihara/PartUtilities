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

		public static FlagBrowser CreateFlagSelectorWindow(object caller, FlagBrowser.FlagSelectedCallback selectedCallback, Callback dismissedCallback)
		{
			LogMessage (caller, "Creating flag selector window...");

			// I don't know the actual asset name for the flag prefab. There's probably a way to find it, but it's kind of tricky.
			// But FlagBrowserGUIButton class knows it!
			// So I create a dummy instance of it to get at the actual asset reference, and then replicate 
			// what it's doing to create a flag browser window.
			var sourceButton = new FlagBrowserGUIButton (null, null, null, null);
			FlagBrowser fb = (UnityEngine.Object.Instantiate ((UnityEngine.Object)sourceButton.FlagBrowserPrefab) as GameObject).GetComponent<FlagBrowser> ();
			fb.OnDismiss = dismissedCallback;
			fb.OnFlagSelected = selectedCallback;
			return fb;
		}

	}
}

