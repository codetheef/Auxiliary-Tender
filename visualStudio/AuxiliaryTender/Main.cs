using System;
using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace AuxiliaryTender;

[EnableReloading]
public static class Main
{
	private static Harmony harmony;

	public static ModEntry.ModLogger? Logger { get; private set; }
	private static Harmony? harmony;
	// Unity Mod Manage Wiki: https://wiki.nexusmods.com/index.php/Category:Unity_Mod_Manager
	public static bool Load(ModEntry modEntry)
	{
		Logger = modEntry.Logger;

		try
		{
			Logger?.Log("Auxiliary Tender Mod Loaded");
			harmony = new Harmony(modEntry.Info.Id);
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			modEntry.OnUnload = Unload;
			Harmony.DEBUG = true;
			harmony = new Harmony(modEntry.Info.Id);
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			BehaviorHandler.AttachBehavior();
			WorldStreamingInit.LoadingFinished += Start;
			if (WorldStreamingInit.Instance && WorldStreamingInit.IsLoaded)
			{
				Start();
			}
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

	private static bool Unload(ModEntry modEntry)
	{
		modEntry.Logger.Log("Unloading");
		harmony?.UnpatchAll(modEntry.Info.Id);
		return true;
	}
	private static void Start()
	{
		Main.Logger?.Log("Startup called");
		SpawnerConfig.StartSpawner();
	}
}
