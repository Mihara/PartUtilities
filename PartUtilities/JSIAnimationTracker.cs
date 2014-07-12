using System;
using System.Collections.Generic;
using UnityEngine;

namespace JSIPartUtilities
{
	public class JSIAnimationTracker: PartModule
	{
		[KSPField]
		public int moduleIndexToTrack = 1000;

		[KSPField (isPersistant = true)]
		public bool actuatorState = false;

		[KSPField]
		public string componentToggles = string.Empty;
        
		[KSPField]
		public string moduleToggles = string.Empty;

		private ModuleAnimateGeneric thatAnimator = null;
		private List<Actuator> actuators = new List<Actuator> ();

		public override void OnStart (PartModule.StartState state)
		{
			var animators = new List<ModuleAnimateGeneric> ();
			foreach (PartModule thatModule in part.Modules) {
				var thisAnimator = thatModule as ModuleAnimateGeneric;
				if (thisAnimator != null) {
					animators.Add (thisAnimator);
				}
			}
			if (moduleIndexToTrack < animators.Count) {
				thatAnimator = animators [moduleIndexToTrack];
			} else {
				Debug.LogError (string.Format ("Could not find ModuleAnimateGeneric number {0} in part {1}", moduleIndexToTrack, part.name));
				Destroy (this);
			}

			// Bloody Squad and their ConfigNodes that never work properly!
			foreach (string actuatorConfig in componentToggles.Split(new [] {'|'},StringSplitOptions.RemoveEmptyEntries)) {
				try {
					actuators.Add (new Actuator (actuatorConfig.Trim (), ActuatorType.PartComponent, part));
				} catch {
					JUtil.LogErrorMessage (this, "Please check your configuration.");
				}
			}
			foreach (string actuatorConfig in moduleToggles.Split(new [] {'|'},StringSplitOptions.RemoveEmptyEntries)) {
				try {
					actuators.Add (new Actuator (actuatorConfig.Trim (), ActuatorType.PartModule, part));
				} catch {
					JUtil.LogErrorMessage (this, "Please check your configuration.");
				}
			}

			LoopThroughActuators (actuatorState);
		}

		private void LoopThroughActuators (bool state)
		{
			actuatorState = state;
			foreach (Actuator thatActuator in actuators) {
				thatActuator.SetState (part, state);
			}
		}

		public override void OnUpdate ()
		{
			if (HighLogic.LoadedSceneIsFlight && thatAnimator != null) {
				bool newstate = thatAnimator.animSwitch && thatAnimator.status == "Locked";
				if (newstate != actuatorState) {
					LoopThroughActuators (newstate);
				}
			}
		}
	}
}

