using System.Collections.Generic;
using System.Reflection;
using BMBLogger = Jotunn.Logger;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

public class BlackMetalCore
{
    private static readonly bool EnableDebugLogging = false;

    private const string BundleName = "blackcore";
    private const string HammerPieceTable = "Hammer";
    private const string HeavyBuildCategory = "HeavyBuild";
    private const string ForgeStation = "Forge";
    private const string SnapPointTag = "snappoint";

    private const string WoodRoleMarker = "Wood_";
    private const string MetalRoleMarker = "Metal_";

    private static readonly string[] EffectsSourcePrefabs =
    {
        "woodiron_pole",
        "woodiron_beam",
        "wood_pole_log"
    };

    private const string MetalSourcePrefab = "iron_wall_1x1";

    private const string BlackCorePole2mPrefab = "bmb_black_core_pole_2m";
    private const string BlackCorePole4mPrefab = "bmb_black_core_pole_4m";
    private const string BlackCoreBeam2mPrefab = "bmb_black_core_beam_2m";
    private const string BlackCoreBeam4mPrefab = "bmb_black_core_beam_4m";

    private const string BlackCorePole2mVisualAsset = "assets/blackcore/prefabs/blackmetalcorepole2m.prefab";
    private const string BlackCorePole4mVisualAsset = "assets/blackcore/prefabs/blackmetalcorepole4m.prefab";
    private const string BlackCoreBeam2mVisualAsset = "assets/blackcore/prefabs/blackmetalcorebeam2m.prefab";
    private const string BlackCoreBeam4mVisualAsset = "assets/blackcore/prefabs/blackmetalcorebeam4m.prefab";

    private const string DarkCoreWoodMaterialAsset = "assets/blackcore/materials/bmb_darkcorewood.mat";

    private static AssetBundle assetBundle;

    public static void RegisterPieces()
    {
        PrefabManager.OnVanillaPrefabsAvailable -= RegisterPieces;

        // Black Core Pole 2m
        RegisterPiece(
            new BlackCorePieceDefinition(
                BlackCorePole2mPrefab,
                BlackCorePole2mVisualAsset,
                "Black Core Pole 2m",
                "Core wood reinforced with black metal bands.",
                coreWoodCost: 1,
                blackMetalCost: 1,
                BlackCorePieceShape.Pole2m
            )
        );

        // Black Core Pole 4m
        RegisterPiece(
            new BlackCorePieceDefinition(
                BlackCorePole4mPrefab,
                BlackCorePole4mVisualAsset,
                "Black Core Pole 4m",
                "Core wood reinforced with black metal bands.",
                coreWoodCost: 2,
                blackMetalCost: 2,
                BlackCorePieceShape.Pole4m
            )
        );

        // Black Core Beam 2m
        RegisterPiece(
            new BlackCorePieceDefinition(
                BlackCoreBeam2mPrefab,
                BlackCoreBeam2mVisualAsset,
                "Black Core Beam 2m",
                "Core wood reinforced with black metal bands.",
                coreWoodCost: 1,
                blackMetalCost: 1,
                BlackCorePieceShape.Beam2m
            )
        );

        // Black Core Beam 4m
        RegisterPiece(
            new BlackCorePieceDefinition(
                BlackCoreBeam4mPrefab,
                BlackCoreBeam4mVisualAsset,
                "Black Core Beam 4m",
                "Core wood reinforced with black metal bands.",
                coreWoodCost: 2,
                blackMetalCost: 2,
                BlackCorePieceShape.Beam4m
            )
        );

    }

    private static void RegisterPiece(BlackCorePieceDefinition definition)
    {
        if (!TryGetDimensions(definition.Shape, out var dimensions))
        {
            BMBLogger.LogError($"Black Core shape '{definition.Shape}' is not configured. Skipping '{definition.PrefabName}'.");
            return;
        }

        try
        {
            var customPiece = new CustomPiece(
                definition.PrefabName,
                addZNetView: true,
                CreateBlackCorePieceConfig(definition)
            );

            ConfigureStandalonePiece(customPiece.PiecePrefab, dimensions);
            TuneSupport(customPiece.PiecePrefab);
            ApplyBuildEffects(customPiece.PiecePrefab);
            LogSnapPointsIfDebug(customPiece.PiecePrefab, "before visual replacement");
            ApplyBundledVisual(customPiece.PiecePrefab, definition.VisualAssetName);
            LogSnapPointsIfDebug(customPiece.PiecePrefab, "after visual replacement");
            ApplyIcon(customPiece);

            if (PieceManager.Instance.AddPiece(customPiece))
            {
                BMBLogger.LogInfo($"Registered Black Core piece '{definition.Name}'.");
            }
            else
            {
                BMBLogger.LogError($"Jotunn rejected Black Core piece '{definition.PrefabName}'.");
            }
        }
        catch (System.Exception ex)
        {
            BMBLogger.LogError($"Failed to register Black Core piece '{definition.PrefabName}': {ex}");
        }
    }

    private static void ApplyBuildEffects(GameObject prefab)
    {
        var sourcePrefab = GetEffectsSourcePrefab();
        if (sourcePrefab == null)
        {
            BMBLogger.LogWarning("Could not find any source prefab for Black Core effects.");
            return;
        }

        CopyPlaceEffect(sourcePrefab, prefab);
        CopyWearNTearEffects(sourcePrefab, prefab);
    }

    private static GameObject GetEffectsSourcePrefab()
    {
        foreach (var sourcePrefabName in EffectsSourcePrefabs)
        {
            var sourcePrefab = PrefabManager.Instance.GetPrefab(sourcePrefabName);
            if (sourcePrefab != null)
            {
                return sourcePrefab;
            }
        }

        return null;
    }

    private static void CopyPlaceEffect(GameObject sourcePrefab, GameObject targetPrefab)
    {
        var sourcePiece = sourcePrefab.GetComponent<Piece>();
        var targetPiece = targetPrefab.GetComponent<Piece>();
        if (sourcePiece == null || targetPiece == null)
        {
            BMBLogger.LogWarning($"Could not copy Black Core place effect from '{sourcePrefab.name}' to '{targetPrefab.name}'. Missing Piece component.");
            return;
        }

        targetPiece.m_placeEffect = sourcePiece.m_placeEffect;
    }

    private static void CopyWearNTearEffects(GameObject sourcePrefab, GameObject targetPrefab)
    {
        var sourceWearNTear = sourcePrefab.GetComponent<WearNTear>();
        var targetWearNTear = targetPrefab.GetComponent<WearNTear>();
        if (sourceWearNTear == null || targetWearNTear == null)
        {
            BMBLogger.LogWarning($"Could not copy Black Core hit/destroy effects from '{sourcePrefab.name}' to '{targetPrefab.name}'. Missing WearNTear component.");
            return;
        }

        targetWearNTear.m_hitEffect = sourceWearNTear.m_hitEffect;
        targetWearNTear.m_destroyedEffect = sourceWearNTear.m_destroyedEffect;
    }

    private static void ApplyIcon(CustomPiece customPiece)
    {
        var icon = RenderManager.Instance.Render(customPiece.PiecePrefab, RenderManager.IsometricRotation);
        if (icon == null)
        {
            BMBLogger.LogWarning($"Failed to render icon for Black Core piece '{customPiece.PiecePrefab.name}'.");
            return;
        }

        customPiece.Piece.m_icon = icon;
    }

    private static PieceConfig CreateBlackCorePieceConfig(BlackCorePieceDefinition definition)
    {
        var config = new PieceConfig
        {
            Name = definition.Name,
            Description = definition.Description,
            PieceTable = HammerPieceTable,
            Category = HeavyBuildCategory,
            CraftingStation = ForgeStation
        };

        config.AddRequirement("RoundLog", definition.CoreWoodCost, recover: true);
        config.AddRequirement("BlackMetal", definition.BlackMetalCost, recover: true);
        return config;
    }

    private static void TuneSupport(GameObject prefab)
    {
        var wearNTear = prefab.GetComponent<WearNTear>() ?? prefab.AddComponent<WearNTear>();
        if (wearNTear == null)
        {
            BMBLogger.LogWarning($"Black Core prefab '{prefab.name}' has no WearNTear component. Support tuning was skipped.");
            return;
        }

        wearNTear.m_materialType = WearNTear.MaterialType.Iron;
        wearNTear.m_supports = true;
        if (wearNTear.m_health <= 0f)
        {
            wearNTear.m_health = 1000f;
        }
    }

    private static void ConfigureStandalonePiece(GameObject prefab, BlackCorePieceDimensions dimensions)
    {
        AddSnapPoint(prefab.transform, "$hud_snappoint_top", dimensions.TopSnap);
        AddSnapPoint(prefab.transform, "$hud_snappoint_bottom", dimensions.BottomSnap);
        EnsureStandaloneCollider(prefab, dimensions);
        DisablePlaceholderRenderers(prefab);
    }

    private static void AddSnapPoint(Transform parent, string name, Vector3 localPosition)
    {
        var existing = parent.Find(name);
        var snapPoint = existing != null
            ? existing
            : new GameObject(name).transform;

        snapPoint.SetParent(parent, worldPositionStays: false);
        snapPoint.localPosition = localPosition;
        snapPoint.localRotation = Quaternion.identity;
        snapPoint.localScale = Vector3.one;

        try
        {
            snapPoint.gameObject.tag = SnapPointTag;
        }
        catch (UnityException ex)
        {
            BMBLogger.LogWarning($"Could not tag Black Core snap point '{name}' as '{SnapPointTag}': {ex.Message}");
        }
    }

    private static void EnsureStandaloneCollider(GameObject prefab, BlackCorePieceDimensions dimensions)
    {
        foreach (var existingCollider in prefab.GetComponents<Collider>())
        {
            existingCollider.enabled = false;
        }

        var collider = prefab.GetComponent<CapsuleCollider>() ?? prefab.AddComponent<CapsuleCollider>();
        collider.radius = dimensions.ColliderRadius;
        collider.height = dimensions.ColliderHeight;
        collider.direction = dimensions.ColliderDirection;
        collider.center = dimensions.ColliderCenter;
        collider.enabled = true;
    }

    private static bool TryGetDimensions(BlackCorePieceShape shape, out BlackCorePieceDimensions dimensions)
    {
        switch (shape)
        {
            case BlackCorePieceShape.Pole2m:
                dimensions = new BlackCorePieceDimensions(
                    topSnap: new Vector3(0f, 1f, 0f),
                    bottomSnap: new Vector3(0f, -1f, 0f),
                    colliderCenter: Vector3.zero,
                    colliderRadius: 0.3f,
                    colliderHeight: 2f,
                    colliderDirection: 1
                );
                return true;

            case BlackCorePieceShape.Pole4m:
                dimensions = new BlackCorePieceDimensions(
                    topSnap: new Vector3(0f, 2f, 0f),
                    bottomSnap: new Vector3(0f, -2f, 0f),
                    colliderCenter: Vector3.zero,
                    colliderRadius: 0.3f,
                    colliderHeight: 4f,
                    colliderDirection: 1
                );
                return true;

            case BlackCorePieceShape.Beam2m:
                dimensions = new BlackCorePieceDimensions(
                    topSnap: new Vector3(1f, 0f, 0f),
                    bottomSnap: new Vector3(-1f, 0f, 0f),
                    colliderCenter: Vector3.zero,
                    colliderRadius: 0.3f,
                    colliderHeight: 2f,
                    colliderDirection: 0
                );
                return true;

            case BlackCorePieceShape.Beam4m:
                dimensions = new BlackCorePieceDimensions(
                    topSnap: new Vector3(2f, 0f, 0f),
                    bottomSnap: new Vector3(-2f, 0f, 0f),
                    colliderCenter: Vector3.zero,
                    colliderRadius: 0.3f,
                    colliderHeight: 4f,
                    colliderDirection: 0
                );
                return true;

            default:
                dimensions = default;
                return false;
        }
    }

    private static void DisablePlaceholderRenderers(GameObject prefab)
    {
        foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = false;
        }
    }

    private static void LogSnapPointsIfDebug(GameObject prefab, string phase)
    {
        if (!EnableDebugLogging)
        {
            return;
        }

        var piece = prefab.GetComponent<Piece>();
        if (piece == null)
        {
            BMBLogger.LogWarning($"Black Core prefab '{prefab.name}' has no Piece component while logging snap points ({phase}).");
            return;
        }

        var snapPoints = new List<Transform>();
        piece.GetSnapPoints(snapPoints);
        BMBLogger.LogInfo($"Black Core prefab '{prefab.name}' has {snapPoints.Count} snap points {phase}.");

        for (var i = 0; i < snapPoints.Count; i++)
        {
            var snapPoint = snapPoints[i];
            if (snapPoint == null)
            {
                BMBLogger.LogInfo($"Black Core snap point {i} is null {phase}.");
                continue;
            }

            BMBLogger.LogInfo(
                $"Black Core snap point {i} {phase}: path '{GetTransformPath(snapPoint)}', " +
                $"localPosition {FormatVector(snapPoint.localPosition)}, " +
                $"localRotation {FormatVector(snapPoint.localEulerAngles)}, " +
                $"localScale {FormatVector(snapPoint.localScale)}."
            );
        }
    }

    private static void ApplyBundledVisual(GameObject prefab, string assetName)
    {
        var bundle = GetAssetBundle();
        if (bundle == null)
        {
            BMBLogger.LogError($"Failed to load Black Core AssetBundle '{BundleName}' from embedded resources. Leaving placeholder visuals for '{prefab.name}'.");
            return;
        }

        LogBundleAssetsIfDebug(bundle);

        var visualPrefab = bundle.LoadAsset<GameObject>(assetName);
        if (visualPrefab == null)
        {
            BMBLogger.LogError($"Failed to load Black Core visual prefab '{assetName}' from AssetBundle '{BundleName}'. Leaving placeholder visuals for '{prefab.name}'.");
            return;
        }

        BMBLogger.LogInfo($"Loaded Black Core visual prefab '{assetName}' from AssetBundle '{BundleName}'.");

        DisablePlaceholderRenderers(prefab);

        var visual = UnityEngine.Object.Instantiate(visualPrefab, prefab.transform, worldPositionStays: false);
        visual.name = assetName;
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        LogVisualHierarchyIfDebug(visual);
        DisableVisualColliders(visual);

        ApplyBundledVisualMaterials(visual, bundle);

        BMBLogger.LogInfo($"Applied bundled Black Core visual '{assetName}' to '{prefab.name}'.");
    }

    private static void DisableVisualColliders(GameObject visual)
    {
        foreach (var collider in visual.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }
    }

    private static AssetBundle GetAssetBundle()
    {
        if (assetBundle != null)
        {
            return assetBundle;
        }

        assetBundle = AssetUtils.LoadAssetBundleFromResources(BundleName, Assembly.GetExecutingAssembly());
        if (assetBundle != null)
        {
            BMBLogger.LogInfo($"Loaded Black Core AssetBundle '{BundleName}'.");
        }

        return assetBundle;
    }

    private static void LogBundleAssetsIfDebug(AssetBundle bundle)
    {
        if (!EnableDebugLogging)
        {
            return;
        }

        foreach (var assetName in bundle.GetAllAssetNames())
        {
            BMBLogger.LogInfo($"Black Core AssetBundle contains asset '{assetName}'.");
        }
    }

    private static void LogVisualHierarchyIfDebug(GameObject visual)
    {
        if (!EnableDebugLogging)
        {
            return;
        }

        foreach (var transform in visual.GetComponentsInChildren<Transform>(true))
        {
            BMBLogger.LogInfo($"Black Core visual hierarchy node '{GetTransformPath(transform)}'.");
        }
    }

    private static void ApplyBundledVisualMaterials(GameObject visual, AssetBundle bundle)
    {
        var woodMaterial = CreateTexturedWoodMaterial(
            "BMB_BlackCore_Wood",
            bundle,
            DarkCoreWoodMaterialAsset,
            new Color(0.20f, 0.10f, 0.03f, 1f)
        );

        var metalMaterial = TryCreateClonedMaterialFromPrefab(
            MetalSourcePrefab,
            "BMB_BlackCore_Metal",
            new Color(0.35f, 0.65f, 0.50f, 1f)
        );

        if (woodMaterial == null)
        {
            woodMaterial = CreateDiffuseMaterial(
                "BMB_Fallback_BlackCore_Wood",
                new Color(0.20f, 0.10f, 0.03f, 1f)
            );
        }

        if (metalMaterial == null)
        {
            metalMaterial = CreateDiffuseMaterial(
                "BMB_Fallback_BlackCore_Metal",
                new Color(0.08f, 0.16f, 0.13f, 1f)
            );
        }

        foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
        {
            var path = GetTransformPath(renderer.transform);
            var classification = ClassifyRenderer(path);
            LogRendererIfDebug(renderer, path, classification);

            if (classification == "Wood")
            {
                AssignMaterial(renderer, woodMaterial, path);
            }
            else if (classification == "Metal")
            {
                AssignMaterial(renderer, metalMaterial, path);
            }
            else
            {
                BMBLogger.LogWarning($"Black Core renderer '{path}' has no Wood_/Metal_ role marker. Leaving bundled material unchanged.");
            }
        }
    }

    private static string ClassifyRenderer(string path)
    {
        if (path.Contains(WoodRoleMarker))
        {
            return "Wood";
        }

        if (path.Contains(MetalRoleMarker))
        {
            return "Metal";
        }

        return "Unknown";
    }

    private static void AssignMaterial(Renderer renderer, Material material, string path)
    {
        if (material == null)
        {
            BMBLogger.LogWarning($"Could not assign null Black Core material to renderer '{path}'. Leaving bundled material unchanged.");
            return;
        }

        renderer.sharedMaterial = material;
    }

    private static void LogRendererIfDebug(Renderer renderer, string path, string classification)
    {
        if (!EnableDebugLogging)
        {
            return;
        }

        BMBLogger.LogInfo($"Black Core renderer '{path}' type '{renderer.GetType().Name}' materials [{DescribeMaterials(renderer.sharedMaterials)}] classified as '{classification}'.");
    }

    private static string GetTransformPath(Transform transform)
    {
        var path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = $"{transform.name}/{path}";
        }

        return path;
    }

    private static string DescribeMaterials(Material[] materials)
    {
        if (materials == null || materials.Length == 0)
        {
            return "none";
        }

        var descriptions = new string[materials.Length];
        for (var i = 0; i < materials.Length; i++)
        {
            var material = materials[i];
            descriptions[i] = material == null
                ? "null"
                : $"{material.name} shader '{material.shader?.name}'";
        }

        return string.Join(", ", descriptions);
    }

    private static Material CreateTexturedWoodMaterial(string name, AssetBundle bundle, string materialAssetName, Color fallbackColor)
    {
        var material = CreateDiffuseMaterial(name, fallbackColor);
        if (material == null)
        {
            return null;
        }

        ApplyBundledMaterialTexture(material, bundle, materialAssetName);
        return material;
    }

    private static void ApplyBundledMaterialTexture(Material targetMaterial, AssetBundle bundle, string materialAssetName)
    {
        var bundledMaterial = bundle.LoadAsset<Material>(materialAssetName);
        if (bundledMaterial == null)
        {
            BMBLogger.LogWarning($"Could not load bundled material '{materialAssetName}' for '{targetMaterial.name}'. Keeping cloned Valheim texture.");
            return;
        }

        var texture = GetMaterialTexture(bundledMaterial);
        if (texture == null)
        {
            BMBLogger.LogWarning($"Bundled material '{bundledMaterial.name}' has no _MainTex or _BaseMap texture. Keeping cloned Valheim texture for '{targetMaterial.name}'.");
            return;
        }

        if (targetMaterial.HasProperty("_MainTex"))
        {
            targetMaterial.SetTexture("_MainTex", texture);
        }

        if (targetMaterial.HasProperty("_BaseMap"))
        {
            targetMaterial.SetTexture("_BaseMap", texture);
        }

        if (targetMaterial.HasProperty("_Color"))
        {
            targetMaterial.color = GetMaterialColor(bundledMaterial);
        }

        BMBLogger.LogInfo($"Applied bundled texture '{texture.name}' from '{bundledMaterial.name}' to '{targetMaterial.name}' while preserving shader '{targetMaterial.shader?.name}'.");
    }

    private static Texture GetMaterialTexture(Material material)
    {
        if (material.HasProperty("_MainTex") && material.GetTexture("_MainTex") != null)
        {
            return material.GetTexture("_MainTex");
        }

        if (material.HasProperty("_BaseMap") && material.GetTexture("_BaseMap") != null)
        {
            return material.GetTexture("_BaseMap");
        }

        return null;
    }

    private static Color GetMaterialColor(Material material)
    {
        if (material.HasProperty("_BaseColor"))
        {
            return material.GetColor("_BaseColor");
        }

        if (material.HasProperty("_Color"))
        {
            return material.color;
        }

        return Color.white;
    }

    private static string FormatVector(Vector3 vector)
    {
        return $"({vector.x:0.###}, {vector.y:0.###}, {vector.z:0.###})";
    }

    private static Material TryCreateClonedMaterialFromPrefab(string sourcePrefabName, string materialName, Color tint)
    {
        var sourcePrefab = PrefabManager.Instance.GetPrefab(sourcePrefabName);
        if (sourcePrefab == null)
        {
            BMBLogger.LogWarning($"Could not find source prefab '{sourcePrefabName}' for Black Core material '{materialName}'.");
            return null;
        }

        foreach (var renderer in sourcePrefab.GetComponentsInChildren<Renderer>(true))
        {
            foreach (var sourceMaterial in renderer.sharedMaterials)
            {
                if (sourceMaterial == null)
                {
                    continue;
                }

                var material = UnityEngine.Object.Instantiate(sourceMaterial);
                material.name = materialName;
                TintMaterial(material, tint);

                BMBLogger.LogInfo($"Created Black Core material '{material.name}' from '{sourcePrefabName}' material '{sourceMaterial.name}' using shader '{sourceMaterial.shader?.name}'.");
                return material;
            }
        }

        BMBLogger.LogWarning($"Could not find renderer materials on source prefab '{sourcePrefabName}' for Black Core material '{materialName}'.");
        return null;
    }

    private static void TintMaterial(Material material, Color tint)
    {
        if (material.HasProperty("_Color"))
        {
            var c = material.color;
            material.color = new Color(c.r * tint.r, c.g * tint.g, c.b * tint.b, c.a * tint.a);
        }
    }

    private static Material CreateDiffuseMaterial(string name, Color color)
    {
        var shader = Shader.Find("Diffuse");
        if (shader == null)
        {
            BMBLogger.LogError($"Could not find Diffuse fallback shader for Black Core material '{name}'.");
            return null;
        }

        BMBLogger.LogWarning($"Using Diffuse material '{name}' for Black Core visuals.");
        var material = new Material(shader)
        {
            name = name,
            color = color
        };
        return material;
    }

    private enum BlackCorePieceShape
    {
        Pole2m,
        Pole4m,
        Beam2m,
        Beam4m
    }

    private readonly struct BlackCorePieceDimensions
    {
        public BlackCorePieceDimensions(
            Vector3 topSnap,
            Vector3 bottomSnap,
            Vector3 colliderCenter,
            float colliderRadius,
            float colliderHeight,
            int colliderDirection
        )
        {
            TopSnap = topSnap;
            BottomSnap = bottomSnap;
            ColliderCenter = colliderCenter;
            ColliderRadius = colliderRadius;
            ColliderHeight = colliderHeight;
            ColliderDirection = colliderDirection;
        }

        public Vector3 TopSnap { get; }

        public Vector3 BottomSnap { get; }

        public Vector3 ColliderCenter { get; }

        public float ColliderRadius { get; }

        public float ColliderHeight { get; }

        public int ColliderDirection { get; }
    }

    private readonly struct BlackCorePieceDefinition
    {
        public BlackCorePieceDefinition(
            string prefabName,
            string visualAssetName,
            string name,
            string description,
            int coreWoodCost,
            int blackMetalCost,
            BlackCorePieceShape shape
        )
        {
            PrefabName = prefabName;
            VisualAssetName = visualAssetName;
            Name = name;
            Description = description;
            CoreWoodCost = coreWoodCost;
            BlackMetalCost = blackMetalCost;
            Shape = shape;
        }

        public string PrefabName { get; }

        public string VisualAssetName { get; }

        public string Name { get; }

        public string Description { get; }

        public int CoreWoodCost { get; }

        public int BlackMetalCost { get; }

        public BlackCorePieceShape Shape { get; }
    }
}
