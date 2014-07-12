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

		public override void OnStart (PartModule.StartState state)
		{
			Component thatComponent = part.FindModelComponent<Component> (componentName);
			if (thatComponent == null) {
				JUtil.LogErrorMessage (this, "Target part component {0} was not found. Selfdestructing the module...");
				Destroy (this);
			} else {
				ToggleState (part, componentName, isEnabled);
			}
		}

		[KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Toggle component")]
		public void GuiToggleComponent ()
		{
			componentIsEnabled = !componentIsEnabled;
			ToggleState (part, componentName, componentIsEnabled);
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
