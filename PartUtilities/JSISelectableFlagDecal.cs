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
				fb = JUtil.CreateFlagSelectorWindow (this, FlagSelected, FlagDismissed);
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

