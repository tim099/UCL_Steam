---
title: Steam Config (UCL_SteamConfigAsset)
description: Sets the AppID Steamworks connects to; clicking Apply in Editor overwrites CardGame/steam_appid.txt
last_updated: 2026-05-02
target_audience: [Build_Engineer, AI_Agent, Designer]
---

# Steam Config

> Class name: `UCL.SteamLib.UCL_SteamConfigAsset`
>
> For the full Steam AppID architecture analysis, see 專案層級的 Steam AppID 架構文件

## Purpose

**Specifies which Steam app the Steamworks SDK should connect to.** Each Asset wraps one AppID string; pressing the `Apply AppId` button writes that ID into `<project>/CardGame/steam_appid.txt`. After an Editor restart / Domain Reload, SteamAPI will initialize against the new ID.

Inherits from `UCL_Asset<UCL_SteamConfigAsset>`.

Current entries in this project:

| Asset ID | AppID | Purpose |
|---|---|---|
| `Default` | 1864830 | Production Emblem of Valor |
| `Demo` | 2263320 | Demo build |

> [!IMPORTANT]
> The Editor currently reads `steam_appid.txt` as **`480`** (Steamworks SDK sample SpaceWar), not your game. To verify achievements / stats inside the Editor, follow the SOP in §"Switch SOP" below.

## What it looks like in the Editor

```
UCL_SteamConfigAsset: <ID>
    AppId    ← AppID as a string (e.g. "1864830")
    [Button] Apply AppId           ← writes to CardGame/steam_appid.txt
    ConfigPath: ...                ← shows the absolute target path
    [Button] SteamAPI Shutdown     ← closes the current SteamAPI connection
```

## Main fields

| Editor label | Required | Description |
|---|---|---|
| **AppId** | Yes | Steam AppID string; parsed via `uint.TryParse` to `AppId_t` (returns `Invalid` on parse failure) |

## Behavior

### Write timing (`ApplyAppId`)
Pressing `Apply AppId` calls `ApplyAppId(UCL.Core.FileLib.Lib.GameFolder)` →
*   `GameFolder` = `Path.GetDirectoryName(Application.dataPath)`, which inside the Editor = `<project>/CardGame/`.
*   Target path = `<GameFolder>/steam_appid.txt`.
*   At build time, `UCL_SteamPostBuildSetting` writes the AppID into the build output folder (only when that step's `IsEnable=True`; currently `False` in this project).

### Editor switch SOP

1. Developer Page → `Steam` group → `UCL_SteamConfigAsset`
2. Pick the entry you want (`Default` / `Demo`)
3. Press `Apply AppId` — overwrites `CardGame/steam_appid.txt`
4. Press `SteamAPI Shutdown` — closes the current connection
5. Trigger a Domain Reload (save any .cs file, or Ctrl+R in the Editor)
6. `SteamManager.Awake` re-runs `SteamAPI.Init()` with the new AppID

## Caveats

*   **`steam_appid.txt` is git-tracked**: any change after Apply will show up in `git status`. Confirm which ID you want to commit.
*   **No license, no switch**: your Steam account must own a license for the target AppID (partner site / dev grant) before the Editor can run with it.
*   **Demo (2263320) and Production (1864830) are separate apps**: achievements / stats are independent and do not sync between them.
*   **If you frequently switch back to 480 for testing**: consider adding a `Dev.json` (AppId=480) entry to avoid hand-editing steam_appid.txt and making typos.

---

## Appendix: Programmer Reference

### A.1 Class info
*   **File path**: `CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamConfigAsset.cs`
*   **Inherits**: `UCL_Asset<UCL_SteamConfigAsset>`
*   **AssetGroup**: `AssetGroup.Steam` / sort = `UCL_SteamConfigAsset`
*   **`#if UNITY_STANDALONE_*`** conditional compile, PC only

### A.2 Field map

| Code field | Editor label | Type | Notes |
|---|---|---|---|
| `m_AppId` | AppId | `string` | uint.TryParse failure → `AppId_t.Invalid` |

### A.3 Key methods

*   **`AppId` (property)** → parses `m_AppId` to `AppId_t`; returns `Invalid` on parse failure.
*   **`ApplyAppId(string path)`** → `File.WriteAllText(GetConfigPath(path), m_AppId)`, writes the AppID to `path/steam_appid.txt`.
*   **`GetConfigPath(string path)` (static)** → `Path.Combine(path, "steam_appid.txt")`.
*   **`ConfigPath` (static property)** → default target = `GameFolder/steam_appid.txt`.
*   **OnGUI** → exposes `Apply AppId` and `SteamAPI Shutdown` buttons.

### A.4 Interactions

*   **`SteamManager.Awake`** ([SteamManager.cs:99,122](../../../UCL_SteamworksScript/UCL_SteamworkScripts/Steamworks.NET/SteamManager.cs)) — `SteamAPI.RestartAppIfNecessary(AppId_t.Invalid)` + `SteamAPI.Init()`, depends on `steam_appid.txt`.
*   **`UCL_SteamPostBuildSetting`** — Post-build step that writes the configured AppID into the build output folder (currently disabled across all BuildAssets).
*   **`UCL_SteamConfigAssetEntry`** (same file) — Asset Entry wrapper; default ID = `"Default"`.

### A.5 Known issues

*   `m_AppId` is intentionally a string (not uint) to support empty values and progressive initialization (Asset starts with ID `"Asset ID"`).
*   `ApplyAppId` has no try-catch; throws if the target path is unwritable (rare).
