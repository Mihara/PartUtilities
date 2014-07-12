using System;
using UnityEngine;

namespace JSIPartUtilities
{
	public class JSIPartComponentToggle: PartModule
	{

		[KSPField]
		public string componentName = string.Empty;
		[KSPField (isPersistant = true)]
		public bool componentIsEnabled = true;
		[KSPField]
		public bool activeInEditor = true;
		[KSPField]
		public bool activeInFlight = true;
		[KSPField]
		public bool activeWhenUnfocused = true;
		[KSPField]
		public string enableMenuString = string.Empty;
		[KSPField]
		public string disableMenuString = string.Empty;
		[KSPField]
		public string toggleMenuString = string.Empty;

		private string[] componentList;

		public override void OnStart (PartModule.StartState state)
		{
			componentList = componentName.Split (new [] { ',', ' ', '|' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string componentText in componentList) {
				Component thatComponent = part.FindModelComponent<Component> (componentText);
				if (thatComponent == null) {
					JUtil.LogErrorMessage (this, "Target part component {0} was not found in part {1}. Selfdestructing the module...", componentText, part.name);
					Destroy (this);
				}
			}
			JUtil.LogMessage (this, "Active in part {0}, handling {1} components", part.name, componentList.Length);
			LoopComponents ();
			Events ["GuiToggleComponent"].guiActive = activeInFlight;
			Events ["GuiToggleComponent"].guiActiveEditor = activeInEditor;
			Events ["GuiToggleComponent"].guiActiveUnfocused = activeWhenUnfocused;
			if (!string.IsNullOrEmpty (enableMenuString)) {
				Events ["GuiEnableComponent"].guiName = enableMenuString;
			}
			if (!string.IsNullOrEmpty (disableMenuString)) {
				Events ["GuiDisableComponent"].guiName = disableMenuString;
			}
			if (!string.IsNullOrEmpty (toggleMenuString)) {
				Events ["GuiToggleComponent"].guiName = toggleMenuString;
			}

		}


		[KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Enable component")]
		public void GuiEnableComponent ()
		{
			componentIsEnabled = true;
			LoopComponents ();
		}

		[KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Disable component")]
		public void GuiDisableComponent ()
		{
			componentIsEnabled = false;
			LoopComponents ();
		}

		[KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Toggle component")]
		public void GuiToggleComponent ()
		{
			componentIsEnabled = !componentIsEnabled;
			LoopComponents ();
		}

		private void LoopComponents ()
		{
			foreach (string componentText in componentList) {
				ToggleState (part, componentText, componentIsEnabled);
			}
			if (componentIsEnabled) {
				Events ["GuiEnableComponent"].guiActive = false;
				Events ["GuiEnableComponent"].guiActiveEditor = false;
				Events ["GuiEnableComponent"].guiActiveUnfocused = false;

				Events ["GuiDisableComponent"].guiActive |= activeInFlight;
				Events ["GuiDisableComponent"].guiActiveEditor |= activeInEditor;
				Events ["GuiDisableComponent"].guiActiveUnfocused |= activeWhenUnfocused;
			} else {
				Events ["GuiDisableComponent"].guiActive = false;
				Events ["GuiDisableComponent"].guiActiveEditor = false;
				Events ["GuiDisableComponent"].guiActiveUnfocused = false;

				Events ["GuiEnableComponent"].guiActive |= activeInFlight;
				Events ["GuiEnableComponent"].guiActiveEditor |= activeInEditor;
				Events ["GuiEnableComponent"].guiActiveUnfocused |= activeWhenUnfocused;
			}
		}

		private static void ToggleState (Part thatPart, string targetName, bool state)
		{
			Component thatComponent = thatPart.FindModelComponent<Component> (targetName);
			foreach (Renderer thatRenderer in thatComponent.GetComponentsInChildren<Renderer>()) {
				thatRenderer.enabled = state;
			}
			foreach (Collider thatCollider in thatComponent.GetComponentsInChildren<Collider>()) {
				thatCollider.enabled = state;
			}
		}
	}
}
