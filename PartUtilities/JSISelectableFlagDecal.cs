using UnityEngine;
using System;
using Contracts;
using System.Collections.Generic;

namespace JSIPartUtilities
{
	public class JSISelectableFlagDecal: PartModule
	{
		[KSPField (isPersistant = true)]
		public string selectedFlag = string.Empty;
		[KSPField (isPersistant = true)]
		public bool flagWasSelected = false;

		[KSPField]
		public string defaultFlag = string.Empty;
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
					Destroy (this);
					return;
				}

				if (!flagWasSelected && !string.IsNullOrEmpty (defaultFlag)) {
					switch (defaultFlag) {
					case "$RANDOM$":
						var allFlags = GameDatabase.Instance.GetAllTexturesInFolderType ("Flags");
						if (allFlags.Count > 0) {
							defaultFlag = allFlags [UnityEngine.Random.Range (0, allFlags.Count - 1)].name;
						} else {
							JUtil.LogMessage (this, "Could not find any flags to pick a random one, wtf?");
						}
						break;
					case "$SPONSOR$":
						if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER) {
							// Other modes can't have contracts.
							var agentURLs = new List<string> ();
							foreach (Contract thatContract in ContractSystem.Instance.Contracts) {
								if (!agentURLs.Contains (thatContract.Agent.LogoURL) && thatContract.ContractState == Contract.State.Active)
									agentURLs.Add (thatContract.Agent.LogoURL);
							}
							if (agentURLs.Count > 0) {
								defaultFlag = agentURLs [UnityEngine.Random.Range (0, agentURLs.Count - 1)];
							}
						} else {
							var Agencies = Contracts.Agents.AgentList.Instance.Agencies;
							if (Agencies.Count > 0) {
								defaultFlag = Agencies[UnityEngine.Random.Range (0, Agencies.Count - 1)].LogoURL;
							}
						}
						break;
					}
					if (GameDatabase.Instance.ExistsTexture (defaultFlag)) {
						selectedFlag = defaultFlag;
					}
				}

				Events ["SelectFlag"].guiName = menuString;
				ChangeFlag ();
			} else {
				JUtil.LogMessage (this, "Nothing to do, flag transform name is empty.");
				Destroy (this);
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
			JUtil.LogMessage (this, "Selected flag {0}", selectedFlag);
			flagWasSelected = true;
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

