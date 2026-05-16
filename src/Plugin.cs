using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace BlackMetalBuildables;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("com.jotunn.jotunn")]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "haydenc52.BlackMetalBuildables";
    private const string PluginName = "Black Metal Buildables";
    private const string PluginVersion = "1.0.1";

    private void Awake()
    {
        PrefabManager.OnVanillaPrefabsAvailable += BlackMetalCages.RegisterPieces;
        PrefabManager.OnVanillaPrefabsAvailable += BlackMetalCore.RegisterPieces;
        Logger.LogInfo($"{PluginName} loaded");
    }
}
