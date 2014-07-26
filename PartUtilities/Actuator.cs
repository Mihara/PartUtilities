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
		TransformShader,
		StraightParameter,
		Resource,
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
		private readonly float originalParameterValue;
		private readonly float addToParameterWhenEnabled;
		private readonly string nameOfParameter = string.Empty;
		private readonly string resourceName = string.Empty;
		private readonly float maxAmount = 0;
		private readonly bool resourceAlreadyExists;

		private PartResource resourcePointer;

		private string[] knownStraightParameters = {
			"mass",
			"maxTemp",
			"crashTolerance",
			"maximum_drag",
			"minimum_drag",
			"breakingForce",
			"breakingTorque"
		};

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
			case ActuatorType.StraightParameter:
				if (tokens.Length == 2) {
					if (float.TryParse (tokens [1], out addToParameterWhenEnabled)) {
						if (Array.IndexOf (knownStraightParameters, tokens [0].Trim ()) >= 0) {
							nameOfParameter = tokens [0].Trim ();
							originalParameterValue = GetParameter (nameOfParameter, thatPart);
						} else {
							throw new ArgumentException ("Bad arguments, unknown straight parameter " + tokens [0]);
						}
					} else {
						throw new ArgumentException ("Bad argument, maxTemp must be a float.");
					}
				} else {
					throw new ArgumentException ("Bad arguments.");
				}
				break;
			case ActuatorType.Resource:
				if (tokens.Length == 2) {
					if (float.TryParse (tokens [1], out maxAmount)) {
						bool found = false;
						resourceName = tokens [0].Trim ();
						foreach (PartResourceDefinition thatResource in PartResourceLibrary.Instance.resourceDefinitions) {
							found |= thatResource.name == resourceName;
						}
						if (!found)
							throw new ArgumentException ("Bad resource name.");
						resourceAlreadyExists |= thatPart.Resources [resourceName] != null;
					} else {
						throw new ArgumentException ("Bad resource amount.");
					}
				} else {
					throw new ArgumentException ("Bad arguments.");
				}
				break;
			}
		}

		private static float GetParameter (string name, Part thatPart)
		{
			switch (name) {
			case "mass":
				return thatPart.mass;
			case "maxTemp":
				return thatPart.maxTemp;
			case "crashTolerance":
				return thatPart.crashTolerance;
			case "maximum_drag":
				return thatPart.maximum_drag;
			case "minimum_drag":
				return thatPart.minimum_drag;
			case "breakingForce":
				return thatPart.breakingForce;
			case "breakingTorque":
				return thatPart.breakingTorque;
			}
			return 0;
		}

		private static void SetParameter (string name, Part thatPart, float value)
		{
			switch (name) {
			case "mass":
				thatPart.mass = value;
				break;
			case "maxTemp":
				thatPart.maxTemp = value;
				break;
			case "crashTolerance":
				thatPart.crashTolerance = value;
				break;
			case "maximum_drag":
				thatPart.maximum_drag = value;
				break;
			case "minimum_drag":
				thatPart.minimum_drag = value;
				break;
			case "breakingForce":
				thatPart.breakingForce = value;
				break;
			case "breakingTorque":
				thatPart.breakingTorque = value;
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
			case ActuatorType.StraightParameter:
				if (newstate) {
					SetParameter (nameOfParameter, thatPart, originalParameterValue + addToParameterWhenEnabled);
				} else {
					SetParameter (nameOfParameter, thatPart, originalParameterValue);
				}
				break;
			case ActuatorType.Resource:
				// We do not manipulate resource records out of the editor because fsckit.
				if (HighLogic.LoadedSceneIsEditor) {
					if (resourceAlreadyExists) {
						resourcePointer = thatPart.Resources [resourceName];
						if (resourcePointer != null) {
							if (newstate) {
								resourcePointer.maxAmount += maxAmount;
								resourcePointer.amount = resourcePointer.maxAmount;
							} else {
								resourcePointer.maxAmount -= maxAmount;
								resourcePointer.amount = resourcePointer.maxAmount;
							}
						}
					} else {
						if (newstate && resourcePointer == null) {
							var node = new ConfigNode ("RESOURCE");
							node.AddValue ("name", resourceName);
							node.AddValue ("amount", maxAmount);
							node.AddValue ("maxAmount", maxAmount);
							resourcePointer = thatPart.AddResource (node);
							resourcePointer.enabled = true;
						} 
						if (!newstate && resourcePointer != null) {
							thatPart.Resources.list.Remove (resourcePointer);
							UnityEngine.Object.Destroy (resourcePointer);
							resourcePointer = null;
						}
					}
					GameEvents.onEditorShipModified.Fire (EditorLogic.fetch.ship);
				}
				break;
			}
		}
	}
}

