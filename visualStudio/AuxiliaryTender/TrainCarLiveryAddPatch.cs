using DV;
using DV.ThingTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryTender
{
	[HarmonyPatch(typeof(List<TrainCarLivery>))]
	internal class TrainCarLiveryAddPatch
	{
		[HarmonyPostfix]
		[HarmonyPatch(nameof(List<TrainCarLivery>.Add))]
		public static void ListAddPostfix(TrainCarLivery item, List<TrainCarLivery> __instance)
		{
			if (__instance.GetType().GetGenericArguments()[0].Equals(typeof(TrainCarLivery)) &&
				Globals.G.Types.Liveries.Equals(__instance) &&
				Globals.G.Types.Liveries.Contains(item))
			{
				Main.Logger?.Log("Train Car added " + item.name);
			}
		}
	}
}
