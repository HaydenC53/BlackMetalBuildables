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
    private const string PluginVersion = "1.0.0";

    private void Awake()
    {
        PrefabManager.OnVanillaPrefabsAvailable += RegisterPieces;
        Logger.LogInfo($"{PluginName} loaded");
    }

    private static void RegisterPieces()
    {
        PrefabManager.OnVanillaPrefabsAvailable -= RegisterPieces;

        // Black Metal Cage Floor 1x1
        AddPiece(
            new CustomPiece(
                "blackmetal_floor_1x1",
                "iron_floor_1x1_v2",
                CreateBlackMetalPieceConfig(
                    "Black Metal Cage Floor 1x1",
                    "A sturdy cage floor forged from black metal.",
                    1
                )
            )
        );

        // Black Metal Cage Floor 2x2
        AddPiece(
            new CustomPiece(
                "blackmetal_floor_2x2",
                "iron_floor_2x2",
                CreateBlackMetalPieceConfig(
                    "Black Metal Cage Floor",
                    "A sturdy cage floor forged from black metal.",
                    2
                )
            )
        );

        // Black Metal Gate
        AddPiece(
            new CustomPiece(
                "blackmetal_gate",
                "iron_grate",
                CreateBlackMetalPieceConfig(
                    "Black Metal Gate",
                    "A sturdy gate forged from black metal.",
                    2
                )
            )
        );

        // Black Metal Cage Wall 1x1
        AddPiece(
            new CustomPiece(
                "blackmetal_wall_1x1",
                "iron_wall_1x1",
                CreateBlackMetalPieceConfig(
                    "Black Metal Cage Wall 1x1",
                    "A sturdy cage wall forged from black metal.",
                    1
                )
            )
        );

        // Black Metal Cage Wall 2x2
        AddPiece(
            new CustomPiece(
                "blackmetal_wall_2x2",
                "iron_wall_2x2",
                CreateBlackMetalPieceConfig(
                    "Black Metal Cage Wall 2x2",
                    "A sturdy cage wall forged from black metal.",
                    2
                )
            )
        );
    }

    private static PieceConfig CreateBlackMetalPieceConfig(
        string name,
        string description,
        int blackMetalCost)
    {
        var config = new PieceConfig
        {
            Name = name,
            Description = description,
            PieceTable = "Hammer",
            Category = "HeavyBuild",
            CraftingStation = "Forge"
        };

        config.AddRequirement("BlackMetal", blackMetalCost, recover: true);
        return config;
    }

    private static void AddPiece(CustomPiece piece)
    {
        ApplyBlackMetalLook(piece.PiecePrefab);
        PieceManager.Instance.AddPiece(piece);
    }

    private static void ApplyBlackMetalLook(GameObject prefab)
    {
        foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
        {
            var mats = renderer.sharedMaterials;

            for (var i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null)
                {
                    continue;
                }

                var mat = UnityEngine.Object.Instantiate(mats[i]);
                mat.name = $"{mats[i].name}_BMB_BlackMetal";

                TryTintMaterial(mat);

                mats[i] = mat;
            }

            renderer.sharedMaterials = mats;
        }
    }

    private static void TryTintMaterial(Material mat)
    {
        if (mat.HasProperty("_Color"))
        {
            var c = mat.color;
            mat.color = new Color(c.r * 0.40f, c.g * 0.70f, c.b * 0.55f, c.a);
        }
    }
}
