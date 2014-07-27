using System;
using UnityEngine;

namespace JSIPartUtilities
{
	public static class JUtil
	{
		public static bool debugLoggingEnabled = true;

		public static void LogMessage (object caller, string line, params object[] list)
		{
			if (debugLoggingEnabled)
				Debug.Log (String.Format (caller.GetType ().Name + ": " + line, list));
		}

		public static void LogErrorMessage (object caller, string line, params object[] list)
		{
			Debug.LogError (String.Format (caller.GetType ().Name + ": " + line, list));
		}

		public static FlagBrowser CreateFlagSelectorWindow (object caller, FlagBrowser.FlagSelectedCallback selectedCallback, Callback dismissedCallback)
		{
			LogMessage (caller, "Creating flag selector window...");

			// I don't know the actual asset name for the flag prefab. There's probably a way to find it, but it's kind of tricky.
			// But FlagBrowserGUIButton class knows it!
			// So I create a dummy instance of it to get at the actual asset reference, and then replicate 
			// what it's doing to create a flag browser window.
			var sourceButton = new FlagBrowserGUIButton (null, null, null, null);
			FlagBrowser fb = (UnityEngine.Object.Instantiate ((UnityEngine.Object)sourceButton.FlagBrowserPrefab) as GameObject).GetComponent<FlagBrowser> ();
			fb.OnDismiss = dismissedCallback;
			fb.OnFlagSelected = selectedCallback;
			return fb;
		}

		public static void AlterCrewCapacity (int value, Part thatPart)
		{
			thatPart.CrewCapacity = value;
			// Now the fun part.
			// This dirty hack was originally suggested by ozraven, so presented here with special thanks to him.
			// In his implementation, he actually would move the internal seat modules in and out of the list of internal seats.
			// I thought of a much simpler way, however: I can mark them as taken. 
			// All the code that adds kerbals to a seat actually checks for whether the seat is taken, but if it is,
			// the code doesn't concern itself with what's actually in the seat. So it is possible for the seat to be taken up by nothing, which is what
			// we're going to exploit.

			// If the internal model is null, don't do anythying.
			if (thatPart.internalModel != null) {
				// First, let's see what the game thinks about the number of available seats.
				int availableSeats = thatPart.internalModel.GetAvailableSeatCount ();

				// If it didn't match, we alter that.
				int difference = value - availableSeats;
				if (difference != 0) {
					foreach (InternalSeat seat in thatPart.internalModel.seats) {
						// If the seat is taken and actually contains a kerbal, we don't do anything to it, because we can't really handle
						// the case of kicking multiple kerbals out of their seats at once anyway.
						if (!(seat.taken && seat.kerbalRef != null)) {
							// If our difference value is positive, we need to add seats.
							// Otherwise we need to take away seats.
							if (difference > 0 && seat.taken) {
								seat.taken = false;
								difference--;
							}
							if (difference < 0 && !seat.taken) {
								seat.taken = true;
								difference++;
							}
							// If we finished rolling away the difference, we end the loop.
							if (difference == 0)
								break;
						}
					}
				}
			}
		}
	}
}

