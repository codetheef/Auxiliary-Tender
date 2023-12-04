using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryTender.Patches
{
	[HarmonyPatch(typeof(CarsSaveManager))]
	internal static class CarSaveManagerPatches
	{
		[HarmonyPatch("InstantiateCar")]
		[HarmonyPostfix]
		private static void InstantiateCarPostFix(TrainCar __result)
		{
			if (Constants.validTankNames.Any(name => __result.name.Contains(name)))
			{
				SpawnMonitor.Monitor(__result);
			}
		}
	}
}
