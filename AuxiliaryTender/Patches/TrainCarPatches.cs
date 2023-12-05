using DV.Damage;
using DV.Simulation.Cars;
using HarmonyLib;
using LocoSim.Attributes;
using LocoSim.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryTender.Patches
{
	[HarmonyPatch(typeof(TrainCar))]
	internal static class TrainCarPatches
	{
		[HarmonyPatch("Awake")]
		[HarmonyPrefix]
		public static void TrainCarAwakPrefix(TrainCar __instance)
		{
			if (Constants.validTankNames.Any(name => __instance.name.Contains(name)))
			{
				InitializeSimControllerForTank(__instance);
			}
		}
		[HarmonyPatch("Awake")]
		[HarmonyPostfix]
		public static void TrainCarAwakePostfix(TrainCar __instance)
		{
			if (Constants.validTankNames.Any(name => __instance.name.Contains(name)))
			{
				__instance.GetComponentInChildren<DamageController>().IgnoreDamage(true);
			}
		}
		private static void InitializeSimControllerForTank(TrainCar car)
		{
			//var damageController = car.gameObject.AddComponent<DamageController>();
			//foreach (var item in damageController.GetType().GetFields())
			//{
			//	if (item.CustomAttributes.Any(attr => attr.AttributeType == typeof(PortIdAttribute)))
			//	{
			//		item.SetValue(damageController, new string[] { });
			//	}
			//}
			//var waterContainer = car.gameObject.AddComponent<WaterContainerDefinition>();
			//waterContainer.ID = "auxWater";
			//waterContainer.capacity = 45000f;
			//waterContainer.defaultValue = waterContainer.capacity;
			//var simConnections = car.gameObject.AddComponent<SimConnectionDefinition>();
			//simConnections.executionOrder = new SimComponentDefinition[]
			//{
			//		waterContainer
			//};
			//simConnections.connections = new Connection[0];
			//simConnections.portReferenceConnections = new PortReferenceConnection[0];
			//var simController = car.gameObject.AddComponent<SimController>();
			//simController.connectionsDefinition = simConnections;
			//simController.otherSimControllers = new DV.Simulation.Controllers.ASimInitializedController[0];
			//Main.Logger?.Log("Sim Controller created and initialized for car " + car.name);
		}
	}
}
