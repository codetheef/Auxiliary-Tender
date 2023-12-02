using DV.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryTender
{
	internal class SpawnMonitor
	{
		public static SpawnMonitor? Instance { get; private set; }
		private static List<TrainCar> pendingCars = new List<TrainCar>();
		public static void Destroy()
		{
			if (null != Instance)
			{
				Instance.Stop();
				Instance = null;
			}
		}
		public static void Create()
		{
			if (null == Instance)
			{
				Instance = new SpawnMonitor();
				Instance.Start();
			}
		}

		private void Start()
		{
			var spawner = SingletonBehaviour<CarSpawner>.Instance;
			spawner.CarSpawned += AttachBehavior;
			pendingCars.ForEach(car => AttachWaterModule(car));
			pendingCars.Clear();
		}

		private void Stop()
		{
			SingletonBehaviour<CarSpawner>.Instance.CarSpawned -= AttachBehavior;
		}

		private void AttachBehavior(TrainCar car)
		{
			Main.Logger?.Log("Loaded a car " + car.name);
			if (Constants.validTankNames.Any(name => car.name.Contains(name)))
			{
				AttachWaterModule(car);
			}
		}

		private void AttachWaterModule(TrainCar car)
		{
			Main.Logger?.Log("Adding water module to car " + car.name);
			car.gameObject.AddComponent<WaterModule>();
		}

		internal static void Monitor(TrainCar result)
		{
			if (Instance != null)
			{
				Instance.AttachWaterModule(result);
			} else
			{
				pendingCars.Add(result);
			}
		}
	}
}
