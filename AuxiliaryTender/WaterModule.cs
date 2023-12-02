using DV.Logic.Job;
using DV.PitStops;
using DV.ThingTypes;
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
		public float Water { get; set; } = 30000f;
		private float waterCapacity = 30000f;
		public float WaterNormalized => Water / waterCapacity;
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
			this.TrainCar = trainCar;
			ProcessConnections(TrainCar.trainset);
			TrainCar.TrainsetChanged += ProcessConnections;
			waterCoro = StartCoroutine(MoveWater());
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
				this.Water -= move;
			}
		}

		private float calcMove(float targetCapacity, float targetAmount)
		{
			var targetNormalized = targetAmount / targetCapacity;
			var normalizedMove = Math.Max(0, WaterNormalized - targetNormalized);
			var move = Math.Min(normalizedMove * waterCapacity, normalizedMove * targetCapacity);
			move = Math.Max(0, move);
			move = Math.Min(move, this.Water);
			move = Math.Min(move, targetCapacity - targetAmount);
			var maxFlow = 800.0f * Math.Max(0, WaterNormalized - targetNormalized); //if we're full and dest is empty - allow max flow, slow down as we equalize
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
