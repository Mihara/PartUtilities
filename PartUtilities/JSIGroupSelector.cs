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

			foreach (string item in groupStateList.Split (new [] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries)) {
				groupStates.Add (item.Trim ());
			}

			if (groupStates.Count == 0) {
				throw new ArgumentException ("List of group states is empty!");
			}

			if ((state == StartState.Editor && !spawned) || (!persistAfterEditor && state != StartState.Editor)) {
				currentState = groupStates.Contains (initialState) ? initialState : groupStates [0];
			}
			SwitchToState (currentState);
			spawned = true;
		}

		private void SwitchToState (string newState)
		{
			ToggleGroup (currentState, false, partLocal ? part.gameObject : null);
			ToggleGroup (newState, true, partLocal ? part.gameObject : null);

			// Arrrgh.
			foreach (UIPartActionWindow thatWindow in FindObjectsOfType<UIPartActionWindow>()) {
				thatWindow.displayDirty = true;
			}

			if (HighLogic.LoadedSceneIsEditor) {
				GameEvents.onEditorShipModified.Fire (EditorLogic.fetch.ship);
			}
			currentState = newState;
		}


		[KSPEvent (active = true, guiActive = true, guiActiveEditor = true, guiName = "Next")]
		public void JSIGuiNextGroupState ()
		{
			int newitemindex = (groupStates.IndexOf (currentState) + 1) % groupStates.Count;
			SwitchToState (groupStates [newitemindex == -1 ? 0 : newitemindex]);
		}

		[KSPEvent (active = true, guiActive = true, guiActiveEditor = true, guiName = "Previous")]
		public void JSIGuiPreviousGroupState ()
		{
			int newitemindex = groupStates.IndexOf (currentState) - 1;
			SwitchToState (groupStates [newitemindex < 0 ? groupStates.Count - 1 : newitemindex]);
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

