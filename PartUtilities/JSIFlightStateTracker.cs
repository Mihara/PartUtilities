using System;
using System.Collections.Generic;

namespace JSIPartUtilities
{
	public class JSIFlightStateTracker: PartModule
	{
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
		public string controlCrewCapacity = string.Empty;

		[KSPField]
		public bool partLocal = true;
		[KSPField]
		public string trueFlightStates = string.Empty;

		private readonly List<Vessel.Situations> trueSituations = new List<Vessel.Situations> ();
		private readonly List<Actuator> actuators = new List<Actuator> ();
		private bool actuatorState;

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
				ParseSet (controlCrewCapacity, ActuatorType.CrewCapacity);
			} catch {
				JUtil.LogErrorMessage (this, "Please check your configuration.");
				Destroy (this);
			}
			foreach (string statestring in trueFlightStates.Split (new [] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries)) {
				var situation = (Vessel.Situations)Enum.Parse (typeof(Vessel.Situations), statestring);
				if (Enum.IsDefined (typeof(Vessel.Situations), situation)) {
					trueSituations.Add (situation);
				} else {
					throw new ArgumentException ("Unknown vessel situation type: " + statestring);
				}
			}

			if (state != StartState.Editor) {
				actuatorState = trueSituations.Contains (vessel.situation);
				LoopThroughActuators (actuatorState);
			}
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
		}

		public override void OnUpdate ()
		{
			if (HighLogic.LoadedSceneIsFlight) {
				bool newstate = trueSituations.Contains (vessel.situation);
				if (newstate != actuatorState) {
					actuatorState = newstate;
					LoopThroughActuators (actuatorState);
				}
			}
		}
	}
}

