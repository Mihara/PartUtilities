using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

namespace JSIPartUtilities
{
	public class JSIListAllAnimations: PartModule
	{

		// With special thanks to ozraven for his idea.
		public override void OnStart (StartState state)
		{
			if (state == StartState.Editor) {
				var sb = new StringBuilder ();
				sb.Append (string.Format ("Animation names in part '{0}':\n", part.partName));

				var names = new List<string> ();
				foreach (Animation thatAnim in part.FindModelAnimators ()) {
					names.Add (thatAnim.name);
				}
				sb.Append (string.Join (", ", names.ToArray ()));
				JUtil.LogMessage (this, sb.ToString ());
			}
		}
	}
}

