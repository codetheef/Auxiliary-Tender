using DV;
using DV.CabControls.Spec;
using DV.Damage;
using DV.Logic.Job;
using DV.Simulation.Cars;
using DV.ThingTypes;
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
			 select l).ToList().ForEach(livery =>
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
				 var damageController = prefab.gameObject.AddComponent<DamageController>();
				 foreach (var item in damageController.GetType().GetFields())
				 {
					 if (item.CustomAttributes.Any(attr => attr.AttributeType == typeof(PortIdAttribute)))
					 {
						 item.SetValue(damageController, new string[] { });
					 }
				 }
				 var waterContainer = prefab.gameObject.AddComponent<WaterContainerDefinition>();
				 waterContainer.ID = "auxWater";
				 waterContainer.capacity = 45000f;
				 waterContainer.defaultValue = waterContainer.capacity;
				 var simConnections = prefab.gameObject.AddComponent<SimConnectionDefinition>();
				 simConnections.executionOrder = new SimComponentDefinition[]
				 {
					waterContainer
				 };
				 simConnections.connections = new Connection[0];
				 simConnections.portReferenceConnections = new PortReferenceConnection[0];
				 var simController = prefab.gameObject.AddComponent<SimController>();
				 simController.connectionsDefinition = simConnections;
				 simController.otherSimControllers = new DV.Simulation.Controllers.ASimInitializedController[0];
				 prefab.gameObject.AddComponent<WaterModule>();
				 var externalInteractions = livery.externalInteractablesPrefab;
				 var hatch = FindRecursive(externalInteractions.transform, "AxTenderHatch")?.gameObject;
				 if (hatch != null)
				 {
					 var lever = hatch.AddComponent<Lever>();
					 lever.rigidbodyMass = 30;
					 lever.rigidbodyDrag = 4;
					 lever.rigidbodyAngularDrag = 0;
					 lever.blockAngularDrag = 0;
					 lever.blockDrag = 0;
					 lever.maxForceAppliedMagnitude = float.PositiveInfinity;
					 lever.scrollWheelSpring = 0;
					 lever.notches = 2;
					 lever.jointAxis = Vector3.right;
					 lever.useSpring = true;
					 lever.jointSpring = 175;
					 lever.jointDamper = 17.5f;
					 lever.useLimits = true;
					 lever.jointLimitMin = 0;
					 lever.jointLimitMax = 160;
					 lever.colliderGameObjects = new GameObject[] { hatch.transform.Find("[colliders]").gameObject };
					 hatch.transform.Find("water fill blocker").gameObject.layer = 15; // force this one to layer 15 so it can prevent filling.
				 }
				 Main.Logger?.Log("Sim Controller created and initialized for prefab " + prefab.name);
			 });
			TerminateAttach();
		}
		private static Transform? FindRecursive(Transform source, string name)
		{
			Transform? child = null;
			List<Transform> children = new List<Transform> { source };
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
