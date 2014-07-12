using System;

namespace JSIPartUtilities
{
	public class Actuator
	{
		private readonly string moduleID;
		private readonly bool inverted;

		public Actuator (ConfigNode node)
		{
			moduleID = node.GetValue ("moduleID");
			inverted = node.HasValue ("inverted");
			JUtil.LogMessage (this, "Controlling moduleID {0}, {1}", moduleID, inverted ? "inverted" : "regular");
		}

		public void SetState (Part thatPart, bool newstate)
		{
			if (inverted)
				newstate = !newstate;
			// Note to other people who want to use this:
			// If you want to control a JSIPartComponentToggle, this is how you do it!
			var eventData = new BaseEventData (BaseEventData.Sender.USER);
			eventData.Set ("moduleID", moduleID);
			eventData.Set ("state", newstate);
			thatPart.SendEvent ("JSIComponentToggle", eventData);
		}
	}
}

