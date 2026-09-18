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

## AI USAGE

How I used AI:
1. Used to validate C# syntax
2. Used to generate documentation
3. Used to verify Jotunn, BepInEx, and Unity API
4. Generated icon
5. Assisted in using 3D modeling software
  a. Knew almost nothing about 3D modeling, so used it to get answers on how to accomplish tasks in the software (blender, unity)
  b. With it, I used open source materials and primitive items in blender & unity to create the Black Metal Corewood Reinforced assets from scratch
    i. To be ultra clear: I performed the work, with the AI teaching me how to use the software effectively

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
