# Black Metal Buildables

Black Metal Buildables is a small Valheim mod that adds black metal variants of vanilla iron building pieces.

The current implementation clones existing Valheim iron build-piece prefabs, changes their gameplay metadata, registers them in the Hammer build menu, and applies a lightweight runtime material tint so they read as black metal without custom meshes, textures, icons, or Unity asset bundles.

## Current Pieces

| Piece | New prefab | Source prefab | Cost |
|---|---|---|---:|
| Black Metal Cage Floor 1x1 | `blackmetal_floor_1x1` | `iron_floor_1x1_v2` | 1x Black Metal |
| Black Metal Cage Floor | `blackmetal_floor_2x2` | `iron_floor_2x2` | 2x Black Metal |
| Black Metal Gate | `blackmetal_gate` | `iron_grate` | 2x Black Metal |
| Black Metal Cage Wall 1x1 | `blackmetal_wall_1x1` | `iron_wall_1x1` | 1x Black Metal |
| Black Metal Cage Wall 2x2 | `blackmetal_wall_2x2` | `iron_wall_2x2` | 2x Black Metal |

All pieces are added to:

```text
Piece table: Hammer
Category: HeavyBuild
```

## Build

From the repo root:

```powershell
dotnet build .\src\BlackMetalBuildables.csproj
```

Build output:

```text
src\bin\Debug\net472\BlackMetalBuildables.dll
```

## Manual Install For Testing

This repo intentionally does not auto-deploy into a mod profile.

After building, copy:

```text
src\bin\Debug\net472\BlackMetalBuildables.dll
```

into a BepInEx plugins folder. Then launch Valheim with that profile.
