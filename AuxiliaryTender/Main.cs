using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace AuxiliaryTender;

[EnableReloading]
public static class Main
{
	public static ModEntry.ModLogger? Logger { get; private set; }
	private static Harmony? harmony { get; set; }

	// Unity Mod Manage Wiki: https://wiki.nexusmods.com/index.php/Category:Unity_Mod_Manager
	public static bool Load(ModEntry modEntry)
	{
		Logger = modEntry.Logger;

		try
		{
			harmony = new Harmony(modEntry.Info.Id);
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			Logger?.Log("Auxiliary Tender Mod Loaded");
			Directory.EnumerateFiles(modEntry.Path, "car.json", SearchOption.AllDirectories);
			WorldStreamingInit.LoadingFinished += Start;
			if (WorldStreamingInit.Instance && WorldStreamingInit.IsLoaded)
			{
				Start();
			}
			modEntry.OnUnload = Unload;
			// Other plugin startup logic
		}
		catch (Exception ex)
		{
			modEntry.Logger.LogException($"Failed to load {modEntry.Info.DisplayName}:", ex);
			harmony?.UnpatchAll(modEntry.Info.Id);
			return false;
		}

		return true;
	}

	private static bool Unload(ModEntry entry)
	{
		entry.Logger.Log("Unloading");
		harmony?.UnpatchAll(entry.Info.Id);
		WorldStreamingInit.LoadingFinished -= Start;
		Stop();
		return true;
	}
	private static void Start()
	{
		SpawnMonitor.Create();
	}
	private static void Stop()
	{
		SpawnMonitor.Destroy();
	}
}
