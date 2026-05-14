# Black Metal Buildables

Black Metal Buildables is a small Valheim mod that adds black metal variants of vanilla iron building pieces.

The current implementation clones existing Valheim iron build-piece prefabs, changes their gameplay metadata, registers them in the Hammer build menu, and applies a lightweight runtime material tint so they read as black metal without custom meshes, textures, icons, or Unity asset bundles.

## Status

This mod is in early development.

Working foundation:

- BepInEx plugin loads successfully.
- Jotunn dependency is wired and loading.
- Custom pieces are registered through Jotunn.
- Pieces appear in the Hammer menu under the vanilla heavy building category.
- Pieces use BlackMetal as their build requirement.
- Placed pieces can be removed and refund materials.
- Runtime material tinting works on cloned prefabs.

Current version:

```text
0.1.0
```

## Current Pieces

| Piece | New prefab | Source prefab | Cost | Station |
|---|---|---|---:|---|
| Black Metal Cage Floor 1x1 | `blackmetal_floor_1x1` | `iron_floor_1x1_v2` | 1 BlackMetal | Forge |
| Black Metal Cage Floor | `blackmetal_floor_2x2` | `iron_floor_2x2` | 2 BlackMetal | Forge |
| Black Metal Gate | `blackmetal_gate` | `iron_grate` | 2 BlackMetal | Forge |
| Black Metal Cage Wall 1x1 | `blackmetal_wall_1x1` | `iron_wall_1x1` | 1 BlackMetal | Forge |
| Black Metal Cage Wall 2x2 | `blackmetal_wall_2x2` | `iron_wall_2x2` | 2 BlackMetal | Forge |

All pieces are added to:

```text
Piece table: Hammer
Category: HeavyBuild
```

## Design Goals

Keep the first version boring and functional:

- Reuse vanilla Valheim prefabs.
- Avoid custom Unity asset bundles.
- Avoid custom meshes.
- Avoid custom textures and icons for now.
- Avoid Harmony unless a later feature genuinely needs it.
- Prefer simple explicit piece registration over a larger content framework.

The goal is to build confidence in the Valheim modding loop before adding more polish.

## Requirements

Development and runtime assumptions:

- Valheim
- BepInExPack Valheim `v5.4.2333`
- Jotunn `v2.29.0`
- .NET SDK capable of building a `net472` project
- .NET Framework 4.7.2 reference assemblies

The project includes:

```xml
Microsoft.NETFramework.ReferenceAssemblies.net472
```

so IDE language services and command-line builds can resolve framework types consistently.

## Local Paths

The project currently references local assemblies from a Valheim/r2modman setup.

Expected BepInEx reference:

```text
C:\Users\hayden\AppData\Roaming\r2modmanPlus-local\Valheim\profiles\updated\BepInEx\core\BepInEx.dll
```

Expected Jotunn reference:

```text
C:\Users\hayden\AppData\Roaming\r2modmanPlus-local\Valheim\profiles\updated\BepInEx\plugins\ValheimModding-Jotunn\Jotunn.dll
```

Expected Valheim managed assemblies:

```text
C:\Program Files (x86)\Steam\steamapps\common\Valheim\valheim_Data\Managed
```

If your local paths differ, update the `<HintPath>` values in:

```text
BlackMetalBuildables/BlackMetalBuildables.csproj
```

## Build

From the repo root:

```powershell
dotnet build .\BlackMetalBuildables\BlackMetalBuildables.csproj
```

Build output:

```text
BlackMetalBuildables\bin\Debug\net472\BlackMetalBuildables.dll
```

## Manual Install For Testing

This repo intentionally does not auto-deploy into a mod profile.

After building, copy:

```text
BlackMetalBuildables\bin\Debug\net472\BlackMetalBuildables.dll
```

into a BepInEx plugins folder, for example:

```text
C:\Users\hayden\AppData\Roaming\r2modmanPlus-local\Valheim\profiles\updated\BepInEx\plugins\BlackMetalBuildables\
```

Then launch Valheim with that profile.

Expected log line:

```text
Black Metal Buildables loaded
```

## Implementation Notes

Custom pieces are registered after vanilla prefabs are available:

```csharp
PrefabManager.OnVanillaPrefabsAvailable += RegisterPieces;
```

Registering directly in plugin `Awake()` failed because Jotunn could not clone the vanilla prefab yet.

Each piece is registered explicitly:

```csharp
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
```

`AddPiece` applies the runtime material tint and then registers the piece with Jotunn.

The tinting approach clones renderer materials before changing color values, so vanilla shared materials are not modified globally.

## Reference Files

Development notes and scratchpad:

```text
black-metal-buildables-scratchpad.md
```

Known iron building prefab names:

```text
iron_buildings_prefab_names.md
```

## Repository Description

Suggested short repository description:

```text
A Valheim mod that adds black metal variants of vanilla iron building pieces using BepInEx and Jotunn.
```
