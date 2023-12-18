using DV.Logic.Job;
using DV.ThingTypes;
using DV.Utils;
using DV.Wheels;
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
	[HarmonyPatch(typeof(StationLocoSpawner))]
	internal class StationLocoSpawnerPatch
	{
		[HarmonyPatch("Start")]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> LocoSpawnStartTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (CodeInstruction instruction in instructions)
			{
				if (!instruction.Calls(isAnyLocomotiveOrTender))
				{
					Main.Logger?.Log(instruction.ToString());
					yield return instruction;
				} else
				{
					Main.Logger?.Log("Locomotive check found, intercepting");
					yield return new CodeInstruction(OpCodes.Pop);
					yield return new CodeInstruction(OpCodes.Ldc_I4_1);
				}
			}
		}
		[HarmonyPatch("Update")]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> LocoSpawnUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var iterator = instructions.GetEnumerator();
			while (iterator.MoveNext()) {
				var instruction = iterator.Current;
				Main.Logger?.Log(instruction.ToString());
				if (instruction.opcode.Equals(OpCodes.Call) && ((MethodInfo)instruction.operand).Name.Equals("Any"))
				{
					yield return instruction;
					iterator.MoveNext();
					var brtrue = iterator.Current;
					Main.Logger?.Log(brtrue.operand.ToString());
					yield return brtrue;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, locoTypeGroupsToSpawn);
					yield return new CodeInstruction(OpCodes.Ldloc_3);
					yield return CodeInstruction.Call((List<ListTrainCarTypeWrapper> wrappers, List<Car> cars) => IsAlreadySpawned(wrappers, cars));
					yield return new CodeInstruction(OpCodes.Brtrue, brtrue.operand);
				} else
				{
					yield return instruction;
				}
			}
		}
		public static bool IsAlreadySpawned(List<ListTrainCarTypeWrapper> wrappers, List<Car> cars)
		{
			foreach (var w in wrappers.SelectMany(wrapper => wrapper.liveries))
			{
				Main.Logger?.Log("\tCar in wrappers " + w.name);
			}
			foreach (Car car in cars)
			{
				Main.Logger?.Log("\tCar on track " + car.carType.name);
			}
			bool matchFound = false;
			foreach (Car car in cars)
			{
				TrainCar trainCar;
				if (!SingletonBehaviour<IdGenerator>.Instance.logicCarToTrainCar.TryGetValue(car, out trainCar))
				{
					Main.Logger?.Log("Failed to load TrainCar for " + car.carType.name);
					continue;
				}
				matchFound = (from wrapper in wrappers
							where IsFullyPresentInTrainset(wrapper, trainCar.trainset)
							select wrapper).Any();
				if (matchFound)
				{
					break;
				}
			}
			Main.Logger?.Log("Checking cars to see if they are in provided wrappers ");
			Main.Logger?.Log("Found: " + matchFound);
			return matchFound;
		}
		private static readonly FieldInfo locoTypeGroupsToSpawn = typeof(StationLocoSpawner).GetField("locoTypeGroupsToSpawn");
		private static readonly MethodInfo isAnyLocomotiveOrTender = typeof(CarTypes).GetMethod("IsAnyLocomotiveOrTender");

		public static bool IsFullyPresentInTrainset(ListTrainCarTypeWrapper wrapper, Trainset trainset)
		{
			Main.Logger?.Log("Checking wrapper " + wrapper + " against trainset " + trainset);
			var liveriesPresent = trainset.cars.Select(trainCar => trainCar.carLivery.name);
			Main.Logger?.Log("liveries present count " + liveriesPresent.Count());
			bool found = wrapper.liveries.All(trainCar => liveriesPresent.Contains(trainCar.name));
			Main.Logger?.Log("Wrapper is all present: " + found);
			return found;
		}
	}
}
