using System;
using System.Collections.Generic;
using UnityEngine;

namespace JSIPartUtilities
{

	public enum ActuatorType
	{
		PartComponent,
		PartModule,
		TransformTexture,
		TransformShader
	}

	public class Actuator
	{
		private readonly string moduleID;
		private readonly bool inverted;
		private readonly ActuatorType type;
		private readonly PartModule controlledModule;
		private readonly Transform targetTransform;
		private readonly string textureLayer = "_MainTex";
		private readonly string falseString, trueString;

		public Actuator (string configData, ActuatorType creatingType, Part thatPart)
		{
			type = creatingType;
			string remainder;
			if (configData.StartsWith ("!", StringComparison.Ordinal)) {
				inverted = true;
				remainder = configData.Substring (1).Trim ();
			} else {
				inverted = false;
				remainder = configData;
			}

			string[] tokens = remainder.Split (',');
			switch (type) {
			case ActuatorType.PartComponent:
				moduleID = remainder;
				JUtil.LogMessage (this, "Controlling PartComponent with moduleID {0}, {1}", moduleID, inverted ? "inverted" : "regular");
				break;
			case ActuatorType.PartModule:
				int moduleIndex = int.Parse (remainder.Split (',') [1]);
				List<PartModule> thoseModules = new List<PartModule> ();
				foreach (PartModule thatModule in thatPart.Modules) {
					if (thatModule.ClassName == tokens [0].Trim ()) {
						thoseModules.Add (thatModule);
					}
					if (moduleIndex < thoseModules.Count) {
						controlledModule = thoseModules [moduleIndex];
					} else {
						JUtil.LogErrorMessage (this, "Could not find PartModule named {2} number {0} in part {1}", moduleIndex, thatPart.name, tokens [0].Trim ());
					}
				}
				JUtil.LogMessage (this, "Controlling PartModule named {0}, {1}", controlledModule.ClassName, inverted ? "inverted" : "regular");
				break;
			case ActuatorType.TransformTexture:

				if (tokens.Length == 3 || tokens.Length == 4) {
					targetTransform = thatPart.FindModelTransform (tokens [0].Trim ());
					if (targetTransform == null) {
						throw new ArgumentException ("Could not find model transform.");
					}
					if (GameDatabase.Instance.ExistsTexture (tokens [1].Trim ()) && GameDatabase.Instance.ExistsTexture (tokens [2].Trim ())) {
						falseString = tokens [1].Trim ();
						trueString = tokens [2].Trim ();
						JUtil.LogMessage (this, "Controlling texture on transfomrm '{0}', {1}", tokens [0].Trim (), inverted ? "inverted" : "regular");
					} else {
						throw new ArgumentException ("Textures not found.");
					}
					if (tokens.Length == 4) {
						textureLayer = tokens [3].Trim ();
					}
				} else {
					throw new ArgumentException ("Bad arguments.");
				}
				break;
			case ActuatorType.TransformShader:
				if (tokens.Length == 3) {
					targetTransform = thatPart.FindModelTransform (tokens [0].Trim ());
					if (targetTransform == null) {
						throw new ArgumentException ("Could not find model transform.");
					}
					if (Shader.Find (tokens [1].Trim ()) && Shader.Find (tokens [2].Trim ())) {
						falseString = tokens [1].Trim ();
						trueString = tokens [2].Trim ();
					} else {
						throw new ArgumentException ("Shaders not found.");
					}
				} else {
					throw new ArgumentException ("Bad arguments.");
				}
				break;
			}
		}

		public void SetState (Part thatPart, bool newstate, GameObject objectLocal)
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
				eventData.Set ("objectLocal", objectLocal);
				thatPart.SendEvent ("JSIComponentToggle", eventData);
				break;
			case ActuatorType.PartModule:
				controlledModule.enabled = newstate;
				controlledModule.isEnabled = newstate;
				break;
			case ActuatorType.TransformTexture:
				Renderer mat = targetTransform.GetComponent<Renderer> ();
				mat.material.SetTexture (textureLayer, newstate ? GameDatabase.Instance.GetTexture (trueString, false) : GameDatabase.Instance.GetTexture (falseString, false));
				break;
			case ActuatorType.TransformShader:
				Renderer shm = targetTransform.GetComponent<Renderer> ();
				shm.material.shader = Shader.Find (newstate ? trueString : falseString);
				break;
			}
		}
	}
}

