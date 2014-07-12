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

			LoopThroughActuators (actuatorState);
		}

		public override void OnLoad (ConfigNode node)
		{
			JUtil.LogMessage (this, "HasNode returns {0}, nodes count is {1}, values count is {2}", node.HasNode ("ACTUATOR"), node.CountNodes, node.CountValues);
			foreach (ConfigNode thatActuator in node.GetNodes ("ACTUATOR")) {
				actuators.Add (new Actuator (thatActuator));
			}
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

