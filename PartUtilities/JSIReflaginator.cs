using System;

// This is a rewrite of the module called Reflaginator, originally presented to the community by Sirkut.
// While I'm not using any code from it, (I stumbled onto a far easier way to get a flag selector)
// I should credit him for the idea.

namespace JSIPartUtilities
{
	public class JSIReflaginator: PartModule
	{
		[KSPField]
		public bool selectable = true;
		[KSPField (isPersistant = true)]
		public int refills = 4;
		[KSPField]
		public string takeFlagMenuString = "Take flag";
		[KSPField]
		public string selectFlagMenuString = "Select flag";
		[KSPField]
		public float unfocusedActivationRange = 5;

		private FlagBrowser fb;

		public override void OnStart (PartModule.StartState state)
		{
			if (state != StartState.Editor) {
				Events ["GetFlag"].unfocusedRange = unfocusedActivationRange;
				Events ["SelectFlag"].unfocusedRange = unfocusedActivationRange;
				Events ["SelectFlag"].guiName = selectFlagMenuString;
				if (!selectable) {
					ShutdownEvent ("SelectFlag");
				}
				CheckRefills ();
			}
		}

		[KSPEvent (active = true, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "Select flag")]
		public void SelectFlag ()
		{
			if (fb == null) {
				fb = JUtil.CreateFlagSelectorWindow (this, FlagSelected, FlagDismissed);
			}
		}

		[KSPEvent (active = true, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "Take new flag")]
		public void GetFlag ()
		{
			if (!FlightGlobals.ActiveVessel.isEVA)
				return;
			if (refills != 0) {
				if (refills > 0) {
					refills--;
				}
				var kerbal = (KerbalEVA)FlightGlobals.ActiveVessel.Parts [0].Modules [0];
				if (kerbal.flagItems == 0) {
					kerbal.flagItems = 1;
					ScreenMessages.PostScreenMessage (string.Format ("{0} took a new flag from storage.", FlightGlobals.ActiveVessel.GetName ()));
				} else {
					ScreenMessages.PostScreenMessage (string.Format ("{0} already has a flag.", FlightGlobals.ActiveVessel.GetName ()));
				}
			}
			CheckRefills ();
		}

		private void CheckRefills ()
		{
			if (refills == 0) {
				ShutdownEvent ("GetFlag");
			} else {
				Events ["GetFlag"].guiName = refills < 0 ? takeFlagMenuString : string.Format ("{0} ({1})", takeFlagMenuString, refills);
			}
		}

		public void FlagSelected (FlagBrowser.FlagEntry selected)
		{
			if (FlightGlobals.ActiveVessel.isEVA && (FlightGlobals.ActiveVessel.Parts [0].Modules [0] as KerbalEVA).flagItems > 0) {
				FlightGlobals.ActiveVessel.Parts [0].flagURL = selected.textureInfo.name;
				ScreenMessages.PostScreenMessage (string.Format ("{0} took a different flag from storage.", FlightGlobals.ActiveVessel.GetName ()));
			}
			Destroy (fb);
		}

		public void FlagDismissed ()
		{
			Destroy (fb);
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

