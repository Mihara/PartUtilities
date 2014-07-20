using System;
using System.Collections.Generic;

namespace JSIPartUtilities
{
	public class JSIPartComponentGroup: PartModule
	{

		[KSPField (isPersistant = true)]
		public bool currentState;

		[KSPField]
		public bool areComponentsEnabled = true;

		[KSPField]
		public bool persistAfterEditor = true;

		[KSPField]
		public bool partLocal = true;

		[KSPField]
		public string componentToggles = string.Empty;
		[KSPField]
		public string moduleToggles = string.Empty;
		[KSPField]
		public string textureToggles = string.Empty;
		[KSPField]
		public string shaderToggles = string.Empty;

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
				foreach (string actuatorConfig in textureToggles.Split(new [] {'|'},StringSplitOptions.RemoveEmptyEntries)) {
					actuators.Add (new Actuator (actuatorConfig.Trim (), ActuatorType.TransformTexture, part));
				}
				foreach (string actuatorConfig in shaderToggles.Split(new [] {'|'},StringSplitOptions.RemoveEmptyEntries)) {
					actuators.Add (new Actuator (actuatorConfig.Trim (), ActuatorType.TransformShader, part));
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

			if (state == StartState.Editor || (!persistAfterEditor && state != StartState.Editor)) {
				currentState = areComponentsEnabled;
			}

			if (currentState) {
				ShutdownEvent ("JSIGuiEnableComponents");
			} else {
				ShutdownEvent ("JSIGuiDisableComponents");
			}

			if (!showToggleOption) {
				ShutdownEvent ("JSIGuiToggleComponents");
			}

			if (!showEnableDisableOption) {
				ShutdownEvent ("JSIGuiEnableComponents");
				ShutdownEvent ("JSIGuiDisableComponents");
			}

			LoopThroughActuators (currentState);
		}

		[KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Enable component group")]
		public void JSIGuiEnableComponents ()
		{
			currentState = true;
			LoopThroughActuators (currentState);
		}

		[KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Disable component group")]
		public void JSIGuiDisableComponents ()
		{
			currentState = false;
			LoopThroughActuators (currentState);
		}

		[KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Toggle component group")]
		public void JSIGuiToggleComponents ()
		{
			currentState = !currentState;
			LoopThroughActuators (currentState);
		}

		private void LoopThroughActuators (bool state)
		{
			actuatorState = state;
			foreach (Actuator thatActuator in actuators) {
				if (partLocal) {
					thatActuator.SetState (part, state, part.gameObject);
				} else {
					thatActuator.SetState (part, state, null);
				}
			}

			if (showEnableDisableOption) {
				if (state) {

					ShutdownEvent ("JSIGuiEnableComponents");

					Events ["JSIGuiDisableComponents"].active = true;

					Events ["JSIGuiDisableComponents"].guiActive |= activeInFlight;
					Events ["JSIGuiDisableComponents"].guiActiveEditor |= activeInEditor;
					Events ["JSIGuiDisableComponents"].guiActiveUnfocused |= activeWhenUnfocused;
				} else {

					ShutdownEvent ("JSIGuiDisableComponents");

					Events ["JSIGuiEnableComponents"].active = true;
					Events ["JSIGuiEnableComponents"].guiActive |= activeInFlight;
					Events ["JSIGuiEnableComponents"].guiActiveEditor |= activeInEditor;
					Events ["JSIGuiEnableComponents"].guiActiveUnfocused |= activeWhenUnfocused;
				}
			}
		}

		private void ShutdownEvent (string eventName)
		{
			Events [eventName].active = false;
			Events [eventName].guiActive = false;
			Events [eventName].guiActiveEditor = false;
			Events [eventName].guiActiveUnfocused = false;
		}

	}
}

