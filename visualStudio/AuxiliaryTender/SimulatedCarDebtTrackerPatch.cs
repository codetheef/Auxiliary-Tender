using DV.Damage;
using DV.ServicePenalty;
using DV.Simulation.Cars;
using DV.Simulation.Controllers;
using DV.ThingTypes;
using HarmonyLib;
using LocoSim.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryTender
{
	[HarmonyPatch(typeof(SimulatedCarDebtTracker))]
	internal static class SimulatedCarDebtTrackerPatch
	{
		[HarmonyPatch(MethodType.Constructor)]
		[HarmonyPrefix]
		[HarmonyPatch(new Type[] {typeof(DamageController), typeof(ResourceContainerController), typeof(EnvironmentDamageController), typeof(SimulationFlow), typeof(string), typeof(TrainCarType) })]
		public static void SimulatedCarDebtTrackerConstructorPrefix(DamageController dmgController,
			ResourceContainerController resourceContainerController,
			EnvironmentDamageController envDamageController, SimulationFlow simFlow, string id, ref TrainCarType carType)
		{
			CarTypeConfig? typeConfig = TrainCar.Resolve(dmgController.gameObject).carLivery.prefab.GetComponentInChildren<CarTypeConfig>();
			if (typeConfig != null)
			{
				carType = typeConfig.trainCarType;
			}
		}
	}
}
