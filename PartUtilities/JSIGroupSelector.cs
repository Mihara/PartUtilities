using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace JSIPartUtilities
{
	public class JSIGroupSelector: PartModule
	{

		[KSPField]
		public bool activeInEditor = true;
		[KSPField]
		public bool activeInFlight = true;
		[KSPField]
		public bool activeWhenUnfocused = true;
		[KSPField]
		public bool externalToEVAOnly = false;
		[KSPField]
		public float unfocusedActivationRange = 10;
		[KSPField]
		public bool persistAfterEditor = true;

		[KSPField]
		public string nextMenuButton = "Next";
		[KSPField]
		public string previousMenuButton = "Previous";
		[KSPField]
		public string stateGuiName = "State";


		[KSPField]
		public bool partLocal = true;

		[KSPField (isPersistant = true)]
		public bool spawned;

		[KSPField]
		public string groupStateList = string.Empty;
		[KSPField]
		public string initialState = string.Empty;

		[KSPField (guiActive = true, guiActiveEditor = true, guiName = "State", isPersistant = true)]
		public string currentState = string.Empty;

		private List<string> groupStates = new List<string> ();

		public override void OnStart (StartState state)
		{
			foreach (string eventName in new [] {"JSIGuiNextGroupState","JSIGuiPreviousGroupState"}) {
				Events [eventName].guiActive = activeInFlight;
				Events [eventName].guiActiveEditor = activeInEditor;
				Events [eventName].guiActiveUnfocused = activeWhenUnfocused;
				Events [eventName].externalToEVAOnly = externalToEVAOnly;
				Events [eventName].unfocusedRange = unfocusedActivationRange;
			}
			Fields ["currentState"].guiName = stateGuiName;
			if (!string.IsNullOrEmpty (nextMenuButton)) {
				Events ["JSIGuiNextGroupState"].guiName = nextMenuButton;
			} else {
				Events ["JSIGuiNextGroupState"].active = false;
			}
			if (!string.IsNullOrEmpty (previousMenuButton)) {
				Events ["JSIGuiPreviousGroupState"].guiName = previousMenuButton;
			} else {
				Events ["JSIGuiPreviousGroupState"].active = false;
			}

			groupStates = groupStateList.Split (new [] { ',', ' ', '|' }, StringSplitOptions.RemoveEmptyEntries).ToList ();

			if (groupStates.Count == 0) {
				throw new ArgumentException ("List of group states is empty!");
			}

			if ((state == StartState.Editor && !spawned) || (!persistAfterEditor && state != StartState.Editor)) {
				currentState = groupStates.Contains (initialState) ? initialState : groupStates [0];
			}
			spawned = true;
		}

		private void SwitchToState (string state)
		{
			// To be absolutely sure all the mess with resource tanks works correctly, first we disable everything, then we enable one group.
			foreach (string stateName in groupStates) {
				ToggleGroup (stateName, false, partLocal ? part.gameObject : null);
			}
			ToggleGroup (state, true, partLocal ? part.gameObject : null);
		}


		[KSPEvent (active = true, guiActive = true, guiActiveEditor = true, guiName = "Previous")]
		public void JSIGuiNextGroupState ()
		{
			int newitemindex = (groupStates.IndexOf (currentState) + 1) % (groupStates.Count - 1);
			currentState = groupStates [newitemindex == -1 ? 0 : newitemindex];
			SwitchToState (currentState);
		}

		[KSPEvent (active = true, guiActive = true, guiActiveEditor = true, guiName = "Next")]
		public void JSIGuiPreviousGroupState ()
		{
			int newitemindex = (groupStates.IndexOf (currentState) - 1) % (groupStates.Count - 1);
			currentState = groupStates [newitemindex == -1 ? 0 : newitemindex];
			SwitchToState (currentState);
		}

		private void ToggleGroup (string groupID, bool newstate, GameObject objectLocal)
		{
			var eventData = new BaseEventData (BaseEventData.Sender.USER);
			eventData.Set ("groupID", groupID);
			eventData.Set ("state", newstate);
			eventData.Set ("objectLocal", objectLocal);
			part.SendEvent ("JSIGroupToggle", eventData);
		}
	}
}

