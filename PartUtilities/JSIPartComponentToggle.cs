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
		public string moduleID = string.Empty;

		[KSPField]
		public bool controlRendering = true;
		[KSPField]
		public bool controlColliders = true;

		[KSPField]
		public bool activeInEditor = true;
		[KSPField]
		public bool activeInFlight = true;
		[KSPField]
		public bool activeWhenUnfocused = true;
		[KSPField]
		public float unfocusedActivationRange = 10;

		[KSPField]
		public bool externalToEVAOnly = false;

		[KSPField]
		public string enableMenuString = string.Empty;
		[KSPField]
		public string disableMenuString = string.Empty;
		[KSPField]
		public string toggleMenuString = string.Empty;

		private string[] componentList;
		private bool startupComplete;

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

			Events ["JSIGuiToggleComponent"].guiActive = activeInFlight;
			Events ["JSIGuiToggleComponent"].guiActiveEditor = activeInEditor;
			Events ["JSIGuiToggleComponent"].guiActiveUnfocused = activeWhenUnfocused;

			Events ["JSIGuiToggleComponent"].externalToEVAOnly = externalToEVAOnly;
			Events ["JSIGuiEnableComponent"].externalToEVAOnly = externalToEVAOnly;
			Events ["JSIGuiDisableComponent"].externalToEVAOnly = externalToEVAOnly;

			Events ["JSIGuiToggleComponent"].unfocusedRange = unfocusedActivationRange;
			Events ["JSIGuiEnableComponent"].unfocusedRange = unfocusedActivationRange;
			Events ["JSIGuiDisableComponent"].unfocusedRange = unfocusedActivationRange;

			if (!string.IsNullOrEmpty (enableMenuString)) {
				Events ["JSIGuiEnableComponent"].guiName = enableMenuString;
			}
			if (!string.IsNullOrEmpty (disableMenuString)) {
				Events ["JSIGuiDisableComponent"].guiName = disableMenuString;
			}
			if (!string.IsNullOrEmpty (toggleMenuString)) {
				Events ["JSIGuiToggleComponent"].guiName = toggleMenuString;
			}

			LoopComponents ();
			startupComplete = true;
		}

		[KSPEvent (guiActive = false, guiActiveEditor = false)]
		public void JSIComponentToggle (BaseEventData data)
		{
			if (data.GetString ("moduleID") == moduleID) {
				componentIsEnabled = data.GetBool ("state");
				LoopComponents ();
			}
		}

		[KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Enable component")]
		public void JSIGuiEnableComponent ()
		{
			componentIsEnabled = true;
			LoopComponents ();
		}

		[KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Disable component")]
		public void JSIGuiDisableComponent ()
		{
			componentIsEnabled = false;
			LoopComponents ();
		}

		[KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Toggle component")]
		public void JSIGuiToggleComponent ()
		{
			componentIsEnabled = !componentIsEnabled;
			LoopComponents ();
		}

		private void LoopComponents ()
		{
			if (!startupComplete)
				return;
			foreach (string componentText in componentList) {
				SetState (part, componentText, componentIsEnabled, controlRendering, controlColliders);
			}
			if (componentIsEnabled) {
				Events ["JSIGuiEnableComponent"].guiActive = false;
				Events ["JSIGuiEnableComponent"].guiActiveEditor = false;
				Events ["JSIGuiEnableComponent"].guiActiveUnfocused = false;

				Events ["JSIGuiDisableComponent"].guiActive |= activeInFlight;
				Events ["JSIGuiDisableComponent"].guiActiveEditor |= activeInEditor;
				Events ["JSIGuiDisableComponent"].guiActiveUnfocused |= activeWhenUnfocused;
			} else {
				Events ["JSIGuiDisableComponent"].guiActive = false;
				Events ["JSIGuiDisableComponent"].guiActiveEditor = false;
				Events ["JSIGuiDisableComponent"].guiActiveUnfocused = false;

				Events ["JSIGuiEnableComponent"].guiActive |= activeInFlight;
				Events ["JSIGuiEnableComponent"].guiActiveEditor |= activeInEditor;
				Events ["JSIGuiEnableComponent"].guiActiveUnfocused |= activeWhenUnfocused;
			}
		}

		private static void SetState (Part thatPart, string targetName, bool state, bool controlRendering, bool controlColliders)
		{
			Component thatComponent = thatPart.FindModelComponent<Component> (targetName);
			if (controlRendering) {
				foreach (Renderer thatRenderer in thatComponent.GetComponentsInChildren<Renderer>()) {
					thatRenderer.enabled = state;
				}
			}
			if (controlColliders) {
				foreach (Collider thatCollider in thatComponent.GetComponentsInChildren<Collider>()) {
					thatCollider.enabled = state;
				}
			}
		}
	}
}
