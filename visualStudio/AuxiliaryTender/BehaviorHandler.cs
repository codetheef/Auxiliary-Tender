using DV;
using DV.CabControls.Spec;
using DV.Damage;
using DV.Logic.Job;
using DV.Simulation.Cars;
using DV.Simulation.Controllers;
using DV.Simulation.Ports;
using DV.ThingTypes;
using DV.UI;
using LocoSim.Attributes;
using LocoSim.Definitions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AuxiliaryTender
{
	internal class BehaviorHandler : MonoBehaviour
	{
		static GameObject? _object = null;
		public static void AttachBehavior()
		{
			if (_object == null)
			{
				_object = new GameObject("loader");
				_object.AddComponent<BehaviorHandler>();
				_object.SetActive(true);
			}
		}
		private static void TerminateAttach()
		{
			if ( _object != null )
			{
				GameObject.Destroy(_object);
				_object = null;
			}
		}
		public void Awake()
		{
		}
		public void Update()
		{
			Main.Logger?.Log("Coroutine active");
			while (!Globals.G.Types.Liveries.Any(c => c.prefab != null && c.prefab.name.Contains("S282C")))
			{
				Main.Logger?.Log("not found, waiting");
				return;
			}
			Main.Logger?.Log("Aux tender found, patching");
			(from l in Globals.G.Types.Liveries
			 where l.prefab != null && Constants.validTankNames.Any(name => l.prefab.name.Contains(name))
			 select l).ToList<TrainCarLivery>().ForEach(livery =>
			 {
				 //bool hasHatch = AttachHatch(livery);
				 //AttachWaterResource(livery, hasHatch);
				 //AttachWaterIndicator(livery);
				 //ConfigureDamage(livery);
				 livery.prefab.gameObject.AddComponent<WaterModule>();
				 var carTypeConfig = livery.prefab.AddComponent<CarTypeConfig>();
				 carTypeConfig.trainCarType = TrainCarType.Tender;
				 Main.Logger?.Log("Sim Controller created and initialized for prefab " + livery.prefab.name);
			 });
			TerminateAttach();
		}
		private void ConfigureDamage(TrainCarLivery livery)
		{
			Main.Logger?.Log("Attempting to configure damage for " + livery.parentType + ", TrainType: " + livery.prefab.GetComponentInChildren<TrainCar>().carType);
			livery.parentType.damage.wheelsHP = 1000;
			livery.parentType.damage.bodyPrice = 22000;
			livery.parentType.damage.wheelsPrice = 7000;
		}

		private static void AttachWaterResource(TrainCarLivery livery, bool hasHatch)
		{
			var prefab = livery.prefab;
			Main.Logger?.Log("Patching prefab " + prefab.name);
			var waterBox = FindRecursive(prefab.transform, "[WaterCollider]");
			Main.Logger?.Log("waterBox found " + waterBox);
			var receiver = waterBox?.gameObject.AddComponent<LocoResourceReceiver>();
			if (receiver != null && waterBox != null)
			{
				receiver.resourceType = ResourceType.Water;
				waterBox.gameObject.tag = "LocoResourceReceiver";
			}
			prefab.gameObject.AddComponent<WaterModule>();
			Main.Logger?.Log("Added water resource to " + livery.name);
		}

		private static Transform? FindRecursive(Transform source, string name)
		{
			Transform? child = null;
			List<Transform> children = new() { source };
			while (children.Count > 0 && child == null)
			{
				child = (from found in (from c in children
										select c.Find(name))
						 where found != null
						 select found).FirstOrDefault();
				if (child == null)
				{
					children = children.SelectMany<Transform, Transform>(i => i.Cast<Transform>().ToList()).ToList();
				}
			}
			return child;
		}
	}
}
