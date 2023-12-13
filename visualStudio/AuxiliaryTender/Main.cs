using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CCLCareerSpawnerTypes;
using DV;
using DV.ThingTypes;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace AuxiliaryTender;

[EnableReloading]
public static class Main
{
	private static Harmony harmony;

	public static ModEntry.ModLogger? Logger { get; private set; }
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
			BehaviorHandler.AttachBehavior();
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
}
