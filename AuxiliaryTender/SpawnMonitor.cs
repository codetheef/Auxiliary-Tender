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
		private static readonly String[] validTankNames = new String[] { "S282C" };
		public static SpawnMonitor? Instance { get; private set; }
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
		}

		private void Stop()
		{
			SingletonBehaviour<CarSpawner>.Instance.CarSpawned -= AttachBehavior;
		}

		private void AttachBehavior(TrainCar car)
		{
			Main.Logger?.Log("Loaded a car " + car.name);
			if (validTankNames.Any(name => car.name.Contains(name)))
			{
				AttachWaterModule(car);
			}
		}

		private void AttachWaterModule(TrainCar car)
		{
			Main.Logger?.Log("Adding water module to car " + car.name);
			car.gameObject.AddComponent<WaterModule>();
		}

		private void AddWaterTank(TrainCar car)
		{
			throw new NotImplementedException();
		}
	}
}
