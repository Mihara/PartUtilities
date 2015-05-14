using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace JSIPartUtilities
{
	public class JSIPartComponentGroup: PartModule, IPartCostModifier, IPartMassModifier
	{

		[KSPField (isPersistant = true)]
		public bool currentState;

		[KSPField (isPersistant = true)]
		public bool spawned;

		[KSPField]
		public string groupID = string.Empty;

		[KSPField]
		public bool areComponentsEnabled = true;

		[KSPField]
		public float costOfBeingEnabled = 0;

		[KSPField]
		public float massOfBeingEnabled = 0;

		[KSPField]
		public bool persistAfterEditor = true;

		[KSPField]
		public bool partLocal = true;

		[KSPField]
		public string componentToggles = string.Empty;
		[KSPField]
		public string groupToggles = string.Empty;
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
		public string managedNodes = string.Empty;

		[KSPField]
		public string controlCrewCapacity = string.Empty;

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

		float IPartCostModifier.GetModuleCost(float defaultCost)
		{
			return currentState ? costOfBeingEnabled : 0;
		}

		float IPartMassModifier.GetModuleMass(float defaultMass)
		{
			return currentState ? massOfBeingEnabled : 0;
		}

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
				ParseSet (groupToggles, ActuatorType.PartComponentGroup);
				ParseSet (moduleToggles, ActuatorType.PartModule);
				ParseSet (textureToggles, ActuatorType.TransformTexture);
				ParseSet (shaderToggles, ActuatorType.TransformShader);
				ParseSet (numericToggles, ActuatorType.StraightParameter);
				ParseSet (managedResources, ActuatorType.Resource);
				ParseSet (controlCrewCapacity, ActuatorType.CrewCapacity);
				ParseSet (managedNodes, ActuatorType.AttachmentNode);
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

			if ((HighLogic.LoadedSceneIsEditor && !spawned) || (HighLogic.LoadedSceneIsFlight && !persistAfterEditor)) {
				currentState = areComponentsEnabled;
			}

			if (currentState) {
				Events ["JSIGuiEnableComponents"].active = false;
				Events ["JSIGuiEnableComponents"].guiActive = false;
				Events ["JSIGuiEnableComponents"].guiActiveEditor = false;
				Events ["JSIGuiEnableComponents"].guiActiveUnfocused = false;
			} else {
				Events ["JSIGuiDisableComponents"].active = false;
				Events ["JSIGuiDisableComponents"].guiActive = false;
				Events ["JSIGuiDisableComponents"].guiActiveEditor = false;
				Events ["JSIGuiDisableComponents"].guiActiveUnfocused = false;
			}

			Events ["JSIGuiToggleComponents"].active &= showToggleOption;

			if (!showEnableDisableOption) {
				Events ["JSIGuiEnableComponents"].active = false;
				Events ["JSIGuiEnableComponents"].guiActive = false;
				Events ["JSIGuiEnableComponents"].guiActiveEditor = false;
				Events ["JSIGuiEnableComponents"].guiActiveUnfocused = false;
				Events ["JSIGuiDisableComponents"].active = false;
				Events ["JSIGuiDisableComponents"].guiActive = false;
				Events ["JSIGuiDisableComponents"].guiActiveEditor = false;
				Events ["JSIGuiDisableComponents"].guiActiveUnfocused = false;
			}
			spawned = true;

			if (HighLogic.LoadedSceneIsEditor) {
				StartCoroutine (DelayedLoop (0.05f));
			} else {
				LoopThroughActuators (currentState);
			}

		}

		// This is needed because in particular, managedNodes do not hide if we don't have that delay for some reason.
		public IEnumerator DelayedLoop (float waitTime)
		{
			yield return new WaitForSeconds (waitTime);
			LoopThroughActuators (currentState);
		}

		[KSPEvent (active = true, guiActive = false, guiActiveEditor = false)]
		public void JSIGroupToggle (BaseEventData data)
		{
			if (data.GetString ("groupID") == groupID && !string.IsNullOrEmpty (groupID)) {
				if (data.GetGameObject ("objectLocal") == null || data.GetGameObject ("objectLocal") == part.gameObject) {
					LoopThroughActuators (data.GetBool ("state"));
				}
			}
		}

		[KSPEvent (active = true, guiActive = true, guiActiveEditor = true, guiName = "Enable component group")]
		public void JSIGuiEnableComponents ()
		{
			LoopThroughActuators (true);
		}

		[KSPEvent (active = true, guiActive = true, guiActiveEditor = true, guiName = "Disable component group")]
		public void JSIGuiDisableComponents ()
		{
			LoopThroughActuators (false);
		}

		[KSPEvent (active = true, guiActive = true, guiActiveEditor = true, guiName = "Toggle component group")]
		public void JSIGuiToggleComponents ()
		{
			LoopThroughActuators (!currentState);
		}

		private void LoopThroughActuators (bool newstate)
		{
			if (!spawned)
				return;

			foreach (Actuator thatActuator in actuators) {
				thatActuator.SetState (part, newstate, partLocal ? part.gameObject : null);
			}

			if (showEnableDisableOption) {
				if (newstate) {
					Events ["JSIGuiEnableComponents"].active = false;
					Events ["JSIGuiDisableComponents"].active = true;
				} else {
					Events ["JSIGuiDisableComponents"].active = false;
					Events ["JSIGuiEnableComponents"].active = true;
				}
			}

			currentState = newstate;

			JUtil.ForceRightclickMenuRefresh ();

			if (HighLogic.LoadedSceneIsEditor) {
				GameEvents.onEditorShipModified.Fire (EditorLogic.fetch.ship);
			}
		}
	}
}

