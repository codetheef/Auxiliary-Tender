using DV.Damage;
using DV.ThingTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryTender
{
	[HarmonyPatch(typeof(CarDamageModel))]
	internal class CarDamageModelPatch
	{
		[HarmonyPatch(nameof(CarDamageModel.OnCreated))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> OnCreatedTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var property = typeof(TrainCarAndCargoDamageProperties).GetProperty(nameof(TrainCarAndCargoDamageProperties.StandardCarDamageProperties));
			var getter = property?.GetGetMethod();
			Main.Logger?.Log("Patching On Created, Standard Cargo Damage Properties " + property + ", Getter " + getter);
			foreach (var instruction in instructions) {
				if (instruction.Calls(getter))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Call, typeof(CarDamageModelPatch).GetMethod("GetCarDamageProperties"));
					continue;
				}
				yield return instruction;
			}
		}
		public static CarDamageProperties GetCarDamageProperties(TrainCar trainCar)
		{
			Main.Logger?.Log("Train Car patch " + trainCar.name);
			var carCost = trainCar.carLivery.prefab.GetComponentInChildren<CarTypeConfig>();
			Main.Logger?.Log("Car Cost carType: " + carCost.trainCarType ?? TrainCarType.NotSet + ", Train Car carType: " + trainCar);
			CarDamageProperties properties;
			var propertiesFound = TrainCarAndCargoDamageProperties.carDamageProperties.TryGetValue(carCost?.trainCarType ?? TrainCarType.NotSet, out properties);
			if (propertiesFound)
			{
				return properties;
			}
			return TrainCarAndCargoDamageProperties.StandardCarDamageProperties;
		}
	}
}
