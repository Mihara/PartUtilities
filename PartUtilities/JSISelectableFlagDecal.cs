using System;
using UnityEngine;

namespace JSIPartUtilities
{
	public class JSISelectableFlagDecal: PartModule
	{
		[KSPField (isPersistant = true)]
		public string selectedFlag = string.Empty;
		[KSPField]
		public string flagTransform = string.Empty;
		[KSPField]
		public string menuString = "Select flag";
		[KSPField]
		public string textureLayer = "_MainTex";

		private FlagBrowser fb;
		private Texture initialTexture;

		public override void OnStart (PartModule.StartState state)
		{
			if (!string.IsNullOrEmpty (flagTransform)) {
				Transform target = part.FindModelTransform (flagTransform);
				if (target != null) {
					Renderer mat = target.GetComponent<Renderer> ();
					initialTexture = mat.material.GetTexture (textureLayer);
				} else {
					JUtil.LogMessage (this, "Flag transform '{0}' not found.", flagTransform);
					return;
				}
				Events ["SelectFlag"].guiName = menuString;
				ChangeFlag ();
			} else {
				JUtil.LogMessage (this, "Nothing to do, flag transform name is empty.");
				Events ["SelectFlag"].guiActiveEditor = false;
			}
		}

		[KSPEvent (guiActive = false, guiActiveEditor = true, guiName = "Select flag")]
		public void SelectFlag ()
		{
			if (fb == null) {
				JUtil.LogMessage (this, "Creating flag selector window...");

				// I don't know the actual asset name for the flag prefab. There's probably a way to find it, but it's kind of tricky.
				// But FlagBrowserGUIButton class knows it!
				// So I create a dummy instance of it to get at the actual asset reference, and then replicate 
				// what it's doing to create a flag browser window.
				var sourceButton = new FlagBrowserGUIButton (initialTexture, FlagDismissed, FlagSelected, FlagDismissed);
				fb = (UnityEngine.Object.Instantiate ((UnityEngine.Object)sourceButton.FlagBrowserPrefab) as GameObject).GetComponent<FlagBrowser> ();

				fb.OnDismiss = FlagDismissed;
				fb.OnFlagSelected = FlagSelected;
				JUtil.LogMessage (this, "All done, waiting for input.");
			}
		}

		public void FlagSelected (FlagBrowser.FlagEntry selected)
		{
			selectedFlag = selected.textureInfo.name;
			ChangeFlag ();
			Destroy (fb);
		}

		public void FlagDismissed ()
		{
			selectedFlag = string.Empty;
			ChangeFlag ();
			Destroy (fb);
		}

		private void ChangeFlag ()
		{
			Transform target = part.FindModelTransform (flagTransform);
			if (target != null) {
				Renderer mat = target.GetComponent<Renderer> ();
				if (!string.IsNullOrEmpty (selectedFlag)) {
					mat.material.SetTexture (textureLayer, GameDatabase.Instance.GetTexture (selectedFlag, false));
				} else {
					mat.material.SetTexture (textureLayer, initialTexture);
				}
			}
		}
	}
}

