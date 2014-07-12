using System;
using System.Collections.Generic;
using UnityEngine;

namespace JSIPartUtilities
{
	public class JSIAnimationTracker: PartModule
	{
		[KSPField]
		public int moduleIndexToTrack = 1000;
		[KSPField]
		public string moduleID = string.Empty;

		[KSPField (isPersistant = true)]
		public bool actuatorState = false;

		private ModuleAnimateGeneric thatAnimator = null;

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

			Debug.Log (string.Format ("Animation in part {0} will control the activity of PartComponentToggle with moduleID '{1}'", part.name, moduleID));
			SetActuator (actuatorState);
		}

		private void SetActuator (bool newstate)
		{
			actuatorState = newstate;
			var eventData = new BaseEventData (BaseEventData.Sender.USER);
			eventData.Set ("moduleID", moduleID);
			eventData.Set ("state", newstate);
			part.SendEvent ("JSIComponentToggle", eventData);
		}

		public override void OnUpdate ()
		{
			if (HighLogic.LoadedSceneIsFlight && thatAnimator != null) {
				bool newstate = thatAnimator.animSwitch && thatAnimator.status == "Locked";
				if (newstate != actuatorState) {
					SetActuator (newstate);
				}
			}
		}
	}
}

