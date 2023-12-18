using DV.ThingTypes;
using DV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DV.Utils;
using DV.Logic.Job;
using DV.PointSet;
using System.Reflection;

namespace AuxiliaryTender
{
	internal class SpawnerConfig : MonoBehaviour
	{
		static GameObject? _object;
		bool spawnersInitialized = false;
		internal static void StartSpawner()
		{
			if (_object == null)
			{
				_object = new GameObject("loader");
				_object.AddComponent<SpawnerConfig>();
				_object.SetActive(true);
			}
		}
		internal static void StopSpawner()
		{
			if (null != _object)
			{
				GameObject.Destroy(_object);
				_object = null;
			}
		}
		private void SetupSpawners()
		{
			Main.Logger?.Log("Setting up spawners");
			var spawners = FindObjectsOfType<StationLocoSpawner>();
			Main.Logger?.Log("Spawners size " + spawners.Length);
			if (spawners.Length == 0)
			{
				return;
			}
			StationLocoSpawner referenceSpawner = spawners[0];
			foreach (var spawner in spawners)
			{
				referenceSpawner = spawner;
				Main.Logger?.Log("Spawner found on track " + spawner.locoSpawnTrackName);
				if (spawner.locoSpawnTrackName.Contains("HB") && spawner.locoTypeGroupsToSpawn.Count > 1)
				{
					var liveries = (from l in Globals.G.Types.Liveries
									where l.prefab != null && Constants.validTankNames.Any(name => l.prefab.name.Contains(name))
									select new ListTrainCarTypeWrapper(new List<TrainCarLivery> { l })).ToList();
					spawner.locoTypeGroupsToSpawn.AddRange(liveries);
					Main.Logger?.Log("Added aux tenders as potential spawns to " + spawner.locoSpawnTrackName);
				}
			}
			var track = SingletonBehaviour<RailTrackRegistry>.Instance.GetTrackWithName("[Y]_[HB]_[A-02-P]");
			var newSpawner = new GameObject("LocoSpawnerHB-A2");
			newSpawner.SetActive(false);
			newSpawner.transform.parent = referenceSpawner.transform.parent;
			newSpawner.transform.position = track.transform.position;
			var locoSpawner = newSpawner.AddComponent<StationLocoSpawner>();
			Main.Logger?.Log("Spawner attached");
			Main.Logger?.Log("Calculated position " + track.transform.position);
			locoSpawner.locoSpawnTrackName = "[Y]_[HB]_[A-02-P]";
			locoSpawner.locoTypeGroupsToSpawn = (from l in Globals.G.Types.Liveries
												 where l.prefab != null && Constants.validTankNames.Any(name => l.prefab.name.Contains(name))
												 select new ListTrainCarTypeWrapper(new List<TrainCarLivery> { l })).ToList();
			locoSpawner.spawnRotationFlipped = false;
			Main.Logger?.Log("Spawner activated");
			newSpawner.SetActive(true);
			Main.Logger?.Log("Spawner maybe created?");
			spawnersInitialized = true;
		}
		private void CenterSpawner()
		{
			var locoSpawner = (from spawner in FindObjectsOfType<StationLocoSpawner>()
							   where spawner.locoSpawnTrackName.Equals("[Y]_[HB]_[A-02-P]")
							   select spawner).FirstOrDefault();
			if (locoSpawner == null) { return; }
			var anchor = spawnTrackMiddleAnchor.GetValue(locoSpawner) as GameObject;
			if (anchor != null)
			{
				Main.Logger?.Log("Anchor is not null, setting position");
				anchor.transform.position = RecalculateMidPoint(locoSpawner.locoSpawnTrack);
				Main.Logger?.Log("New Position is " + anchor.transform.position);
				ValidateDistance.Invoke(locoSpawner, new object[] { });
				StopSpawner();
			}

		}
		public void Update()
		{
			if (!spawnersInitialized)
			{
				SetupSpawners();
			} else
			{
				CenterSpawner();
			}
		}
		Vector3 RecalculateMidPoint(RailTrack track)
		{
			var pointSet = EquiPointSet.FromBezierEquidistant(track.curve, track.curve.resolution, 0f, false, true);
			var position = (Vector3)pointSet.points[pointSet.points.Length / 2].position;
			return position;
		}
		private static readonly FieldInfo spawnTrackMiddleAnchor = typeof(StationLocoSpawner).GetField("spawnTrackMiddleAnchor", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo ValidateDistance = typeof(StationLocoSpawner).GetMethod("ValidateDistances", BindingFlags.NonPublic | BindingFlags.Instance);
	}
}
