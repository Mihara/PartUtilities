using System;
using UnityEngine;

namespace JSIPartUtilities
{
	public class JSICrewCapacityManager: PartModule
	{
		[KSPField]
		public int capacityWhenFalse = 0;
		[KSPField]
		public int capacityWhenTrue = 0;

		[KSPField (isPersistant = true)]
		public bool currentState = false;

		[KSPField]
		public bool initialState = false;

		[KSPField (isPersistant = true)]
		public bool spawned;

		public override void OnStart (StartState state)
		{
			if (state == StartState.Editor && !spawned) {
				currentState = initialState;
			}
			spawned = true;
		}

		[KSPEvent (active = true, guiActive = false, guiActiveEditor = false)]
		public void JSISetCrewCapacity (BaseEventData data)
		{
			if (data.GetGameObject ("objectLocal") == part.gameObject) {
				currentState = data.GetBool ("state");
				SwitchState (currentState);
			}
		}

		private void SwitchState (bool state)
		{
			part.CrewCapacity = state ? capacityWhenTrue : capacityWhenFalse;
			AlterCrewCapacity (part.CrewCapacity, part);
		}

		// I might actually want to run this OnUpdate....
		public override void OnFixedUpdate ()
		{
			AlterCrewCapacity (part.CrewCapacity, part);
		}

		private static void AlterCrewCapacity (int value, Part thatPart)
		{
			// Now the fun part.
			// This dirty hack was originally suggested by ozraven, so presented here with special thanks to him.
			// In his implementation, he actually would move the internal seat modules in and out of the list of internal seats.
			// I thought of a much simpler way, however: I can mark them as taken. 
			// All the code that adds kerbals to a seat while in flight (VAB is a very much another story) actually checks for whether the seat is taken, but if it is,
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
							// If our difference value is positive, we need to add seats,
							// so we mark them, in order, as not taken -- since we made sure there's no kerbal in it, we must've been the ones that marked it.
							if (difference > 0 && seat.taken) {
								seat.taken = false;
								difference--;
							}
							// Otherwise we need to take away seats, so we mark them taken.
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

