
namespace JSIPartUtilities
{
	public class JSIForbidIVAHere: PartModule
	{
		// This needs to be done in this particular order.
		// Early in the update cycle, we check if we are not the active vessel and return IVAability if we aren't.
		public void Update ()
		{
			if (HighLogic.LoadedSceneIsFlight && !vessel.isActiveVessel) {
				HighLogic.CurrentGame.Parameters.Flight.CanIVA = true;
			}
		}

		// But late in the update cycle, if we are the active vessel, we take it away.
		// This way, multiple pods with this module should not interfere with each other
		// or prevent other pods from IVAing.
		public void LateUpdate ()
		{
			if (HighLogic.LoadedSceneIsFlight && vessel.isActiveVessel) {
				HighLogic.CurrentGame.Parameters.Flight.CanIVA = false;
			}
		}
	}
}

