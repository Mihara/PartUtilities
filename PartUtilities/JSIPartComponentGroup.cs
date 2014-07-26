using System;
using System.Collections.Generic;

namespace JSIPartUtilities
{
	public class JSIPartComponentGroup: PartModule, IPartCostModifier
	{

		[KSPField (isPersistant = true)]
		public bool currentState;

		[KSPField (isPersistant = true)]
		public bool spawned;

		[KSPField]
		public bool areComponentsEnabled = true;

		[KSPField]
		public float costOfBeingEnabled = 0;

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
		public string numericToggles = string.Empty;
		[KSPField]
		public string managedResources = string.Empty;

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


		private readonly List<Actuator> actuators = new List<Actuator> ();
		private bool actuatorState;

		#region IPartCostModifier implementation

		public float GetModuleCost ()
		{
			return currentState ? costOfBeingEnabled : 0;
		}

		#endregion

		private void ParseSet (string input, ActuatorType type)
		{
			foreach (string actuatorConfig in input.Split(new [] {'|'},StringSplitOptions.RemoveEmptyEntries)) {
				actuators.Add (new Actuator (actuatorConfig.Trim (), type, part));
			}
		}

		public override void OnStart (PartModule.StartState state)
		{
			try {
				ParseSet (componentToggles, ActuatorType.PartComponent);
				ParseSet (moduleToggles, ActuatorType.PartModule);
				ParseSet (textureToggles, ActuatorType.TransformTexture);
				ParseSet (shaderToggles, ActuatorType.TransformShader);
				ParseSet (numericToggles, ActuatorType.StraightParameter);
				ParseSet (managedResources, ActuatorType.Resource);
			} catch {
				JUtil.LogErrorMessage (this, "Please check your configuration.");
				Destroy (this);
			}

			foreach (string eventName in new [] {"JSIGuiToggleComponents","JSIGuiEnableComponents","JSIGuiDisableComponents"}) {
				Events [eventName].guiActive = activeInFlight;
				Events [eventName].guiActiveEditor = activeInEditor;
				Events [eventName].guiActiveUnfocused = activeWhenUnfocused;
				Events [eventName].externalToEVAOnly = externalToEVAOnly;
				Events [eventName].unfocusedRange = unfocusedActivationRange;
			}

			if (!string.IsNullOrEmpty (enableMenuString)) {
				Events ["JSIGuiEnableComponents"].guiName = enableMenuString;
			}
			if (!string.IsNullOrEmpty (disableMenuString)) {
				Events ["JSIGuiDisableComponents"].guiName = disableMenuString;
			}
			if (!string.IsNullOrEmpty (toggleMenuString)) {
				Events ["JSIGuiToggleComponents"].guiName = toggleMenuString;
			}

			if ((state == StartState.Editor && !spawned) || (!persistAfterEditor && state != StartState.Editor)) {
				currentState = areComponentsEnabled;
			}

			if (currentState) {
				Events ["JSIGuiEnableComponents"].active = false;
			} else {
				Events ["JSIGuiDisableComponents"].active = false;
			}

			Events ["JSIGuiToggleComponents"].active &= showToggleOption;

			if (!showEnableDisableOption) {
				Events ["JSIGuiEnableComponents"].active = false;
				Events ["JSIGuiDisableComponents"].active = false;
			}
			spawned = true;
			LoopThroughActuators (currentState);
		}

		[KSPEvent (active = true, guiActive = true, guiActiveEditor = true, guiName = "Enable component group")]
		public void JSIGuiEnableComponents ()
		{
			currentState = true;
			LoopThroughActuators (currentState);
		}

		[KSPEvent (active = true, guiActive = true, guiActiveEditor = true, guiName = "Disable component group")]
		public void JSIGuiDisableComponents ()
		{
			currentState = false;
			LoopThroughActuators (currentState);
		}

		[KSPEvent (active = true, guiActive = true, guiActiveEditor = true, guiName = "Toggle component group")]
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
					Events ["JSIGuiEnableComponents"].active = false;
					Events ["JSIGuiDisableComponents"].active = true;
				} else {
					Events ["JSIGuiDisableComponents"].active = false;
					Events ["JSIGuiEnableComponents"].active = true;
				}
			}

			if (HighLogic.LoadedSceneIsEditor) {
				GameEvents.onEditorShipModified.Fire (EditorLogic.fetch.ship);
			}

		}
	}
}

