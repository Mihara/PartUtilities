using System;
using System.Collections.Generic;

namespace JSIPartUtilities
{

	public enum ActuatorType
	{
		PartComponent,
		PartModule,
	}

	public class Actuator
	{
		private readonly string moduleID;
		private readonly bool inverted;
		private readonly ActuatorType type;
		private readonly PartModule controlledModule;

		public Actuator (string configData, ActuatorType creatingType, Part thatPart)
		{
			type = creatingType;
			string remainder;
			if (configData.StartsWith ("!", StringComparison.Ordinal)) {
				inverted = true;
				remainder = configData.Substring (1).Trim();
			} else {
				inverted = false;
				remainder = configData;
			}

			switch (type) {
			case ActuatorType.PartComponent:
				moduleID = remainder;
				JUtil.LogMessage (this, "Controlling PartComponent with moduleID {0}, {1}", moduleID, inverted ? "inverted" : "regular");
				break;
			case ActuatorType.PartModule:
				string moduleName = remainder.Split (',') [0].Trim ();
				int moduleIndex = int.Parse (remainder.Split (',') [1]);
				List<PartModule> thoseModules = new List<PartModule> ();
				foreach (PartModule thatModule in thatPart.Modules) {
					if (thatModule.ClassName == moduleName) {
						thoseModules.Add (thatModule);
					}
					if (moduleIndex < thoseModules.Count) {
						controlledModule = thoseModules [moduleIndex];
					} else {
						JUtil.LogErrorMessage (this, "Could not find PartModule named {2} number {0} in part {1}", moduleIndex, thatPart.name, moduleName);
					}
				}
				JUtil.LogMessage (this, "Controlling PartModule named {0}, {1}", controlledModule.ClassName, inverted ? "inverted" : "regular");
				break;
			}
		}

		public void SetState (Part thatPart, bool newstate)
		{
			if (inverted)
				newstate = !newstate;
			switch (type) {
			case ActuatorType.PartComponent:
			// Note to other people who want to use this:
			// If you want to control a JSIPartComponentToggle, this is how you do it!
				var eventData = new BaseEventData (BaseEventData.Sender.USER);
				eventData.Set ("moduleID", moduleID);
				eventData.Set ("state", newstate);
				thatPart.SendEvent ("JSIComponentToggle", eventData);
				break;
			case ActuatorType.PartModule:
				controlledModule.enabled = newstate;
				controlledModule.isEnabled = newstate;
				break;
			}
		}
	}
}

