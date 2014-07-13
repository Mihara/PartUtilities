using System;
using System.Collections.Generic;

namespace JSIPartUtilities
{
	public class JSIPartComponentGroup: PartModule
	{

		[KSPField]
		public bool areComponentsEnabled;

		[KSPField]
		public string componentToggles = string.Empty;
		[KSPField]
		public string moduleToggles = string.Empty;

		[KSPField]
		public bool activeInEditor = true;
		[KSPField]
		public bool activeInFlight = true;
		[KSPField]
		public bool activeWhenUnfocused = true;
		[KSPField]
		public float unfocusedActivationRange = 10;

		[KSPField]
		public bool showToggleOption = true;
		[KSPField]
		public bool showEnableDisableOption = true;

		[KSPField]
		public bool externalToEVAOnly = false;

		[KSPField]
		public string enableMenuString = string.Empty;
		[KSPField]
		public string disableMenuString = string.Empty;
		[KSPField]
		public string toggleMenuString = string.Empty;


		private List<Actuator> actuators = new List<Actuator> ();
		private bool actuatorState;

		public override void OnStart (PartModule.StartState state)
		{
			try {
				foreach (string actuatorConfig in componentToggles.Split(new [] {'|'},StringSplitOptions.RemoveEmptyEntries)) {
					actuators.Add (new Actuator (actuatorConfig.Trim (), ActuatorType.PartComponent, part));
				}
				foreach (string actuatorConfig in moduleToggles.Split(new [] {'|'},StringSplitOptions.RemoveEmptyEntries)) {
					actuators.Add (new Actuator (actuatorConfig.Trim (), ActuatorType.PartModule, part));
				}
			} catch {
				JUtil.LogErrorMessage (this, "Please check your configuration.");
				Destroy (this);
			}

			Events ["JSIGuiToggleComponents"].guiActive = activeInFlight;
			Events ["JSIGuiToggleComponents"].guiActiveEditor = activeInEditor;
			Events ["JSIGuiToggleComponents"].guiActiveUnfocused = activeWhenUnfocused;

			Events ["JSIGuiToggleComponents"].externalToEVAOnly = externalToEVAOnly;
			Events ["JSIGuiEnableComponents"].externalToEVAOnly = externalToEVAOnly;
			Events ["JSIGuiDisableComponents"].externalToEVAOnly = externalToEVAOnly;

			Events ["JSIGuiToggleComponents"].unfocusedRange = unfocusedActivationRange;
			Events ["JSIGuiEnableComponents"].unfocusedRange = unfocusedActivationRange;
			Events ["JSIGuiDisableComponents"].unfocusedRange = unfocusedActivationRange;

			if (!string.IsNullOrEmpty (enableMenuString)) {
				Events ["JSIGuiEnableComponents"].guiName = enableMenuString;
			}
			if (!string.IsNullOrEmpty (disableMenuString)) {
				Events ["JSIGuiDisableComponents"].guiName = disableMenuString;
			}
			if (!string.IsNullOrEmpty (toggleMenuString)) {
				Events ["JSIGuiToggleComponents"].guiName = toggleMenuString;
			}

			if (areComponentsEnabled) {
				JUtil.ShutdownEvent ("JSIGuiEnableComponents", Events);
			} else {
				JUtil.ShutdownEvent ("JSIGuiDisableComponents", Events);
			}

			if (!showToggleOption) {
				JUtil.ShutdownEvent ("JSIGuiToggleComponents", Events);
			}

			if (!showEnableDisableOption) {
				JUtil.ShutdownEvent ("JSIGuiEnableComponents", Events);
				JUtil.ShutdownEvent ("JSIGuiDisableComponents", Events);
			}

			LoopThroughActuators (areComponentsEnabled);
		}

		[KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Enable component group")]
		public void JSIGuiEnableComponents ()
		{
			areComponentsEnabled = true;
			LoopThroughActuators (areComponentsEnabled);
		}

		[KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Disable component group")]
		public void JSIGuiDisableComponents ()
		{
			areComponentsEnabled = false;
			LoopThroughActuators (areComponentsEnabled);
		}

		[KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Toggle component group")]
		public void JSIGuiToggleComponents ()
		{
			areComponentsEnabled = !areComponentsEnabled;
			LoopThroughActuators (areComponentsEnabled);
		}

		private void LoopThroughActuators (bool state)
		{
			actuatorState = state;
			foreach (Actuator thatActuator in actuators) {
				thatActuator.SetState (part, state);
			}

			if (showEnableDisableOption) {
				if (state) {

					JUtil.ShutdownEvent ("JSIGuiEnableComponents", Events);

					Events ["JSIGuiDisableComponents"].active = true;

					Events ["JSIGuiDisableComponents"].guiActive |= activeInFlight;
					Events ["JSIGuiDisableComponents"].guiActiveEditor |= activeInEditor;
					Events ["JSIGuiDisableComponents"].guiActiveUnfocused |= activeWhenUnfocused;
				} else {

					JUtil.ShutdownEvent ("JSIGuiDisableComponents", Events);

					Events ["JSIGuiEnableComponents"].active = true;
					Events ["JSIGuiEnableComponents"].guiActive |= activeInFlight;
					Events ["JSIGuiEnableComponents"].guiActiveEditor |= activeInEditor;
					Events ["JSIGuiEnableComponents"].guiActiveUnfocused |= activeWhenUnfocused;
				}
			}
		}

	}
}

