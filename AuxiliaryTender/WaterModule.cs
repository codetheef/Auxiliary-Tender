using DV.Damage;
using DV.Logic.Job;
using DV.PitStops;
using DV.Simulation.Cars;
using DV.ThingTypes;
using LocoSim.Implementations;
using LocoSim.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace AuxiliaryTender
{
	internal class WaterModule : MonoBehaviour
	{
		private ResourceContainerController resourceController;

		public float Level => resourceController.GetResourceContainer(ResourceContainerType.WATER).amountReadOut.Value;
		public float Capacity => resourceController.GetResourceContainer(ResourceContainerType.WATER).capacity;
		public float WaterNormalized => Level / Capacity;
		public ResourceContainer Container => resourceController.GetResourceContainer(ResourceContainerType.WATER);
		private TrainCar TrainCar { get; set; }
		public CarPitStopParametersBase? FrontCar { get; private set; }
		public CarPitStopParametersBase? RearCar { get; private set; }
		private Coroutine waterCoro;

		public void Start()
		{
			var trainCar = this.GetComponentInParent<TrainCar>();
			if (trainCar == null)
			{
				Destroy(this);
				Main.Logger?.Log("Failed to load water module");
				return;
			}
			var simController = trainCar.GetComponent<SimController>();
			if (simController == null)
			{
				Destroy(this);
				Main.Logger?.Log("Failed to load water module, no sim controller");
				return;
			}
			this.resourceController = simController.resourceContainerController;
			this.TrainCar = trainCar;
			ProcessConnections(TrainCar.trainset);
			TrainCar.TrainsetChanged += ProcessConnections;
			waterCoro = StartCoroutine(MoveWater());
			trainCar.GetComponentInChildren<DamageController>().IgnoreDamage(true);
		}

		private IEnumerator MoveWater()
		{
			do
			{
				if (null != FrontCar)
				{
					moveWater(FrontCar);
				}
				yield return new WaitForSeconds(1);
				if (null != RearCar)
				{
					moveWater(RearCar);
				}
				yield return new WaitForSeconds(1);
			} while (true);
		}

		private void moveWater(CarPitStopParametersBase car) {
			var tank = car.GetCarPitStopParameters()[ResourceType.Water];
			if (tank != null)
			{
				var move = calcMove(tank.maxValue, tank.value);
				car.UpdateCarPitStopParameter(ResourceType.Water, move);
				this.Container.consumeExtIn.Value = move;
			}
		}

		private float calcMove(float targetCapacity, float targetAmount)
		{
			var targetNormalized = targetAmount / targetCapacity;
			var normalizedMove = Math.Max(0, WaterNormalized - targetNormalized);
			var move = Math.Min(normalizedMove * Capacity, normalizedMove * targetCapacity);
			move = Math.Max(0, move);
			move = Math.Min(move, Level);
			move = Math.Min(move, targetCapacity - targetAmount);
			var maxFlow = Mathf.Lerp(10.0f, 800.0f, WaterNormalized - targetNormalized);
			move = Math.Min(move, maxFlow); // can't move more than maxFlow
			Main.Logger?.Log("Moving " + move + " Water");
			return move;
		}

		private CarPitStopParametersBase? getWaterPitStopCar(Coupler coupler)
		{
			if (coupler.IsCoupled())
			{
				var car = coupler.coupledTo.train;
				var pitstop = car.GetComponentInChildren<CarPitStopParametersBase>();
				if (pitstop != null && pitstop.GetCarPitStopParameters().ContainsKey(ResourceType.Water)) {
					return pitstop;
				}
			}
			return null;
		}

		private void ProcessConnections(Trainset trainset)
		{
			FrontCar = getWaterPitStopCar(TrainCar.frontCoupler);
			RearCar = getWaterPitStopCar(TrainCar.rearCoupler);
		}
	}
}
