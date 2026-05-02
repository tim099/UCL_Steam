---
title: Steam Achievement (UCL_SteamAchievementAsset)
description: Maps to a single Steam backend achievement; pushes to server on unlock and triggers the built-in popup
last_updated: 2026-05-02
target_audience: [Build_Engineer, AI_Agent, Designer]
---

# Steam Achievement

> Class name: `UCL.SteamLib.UCL_SteamAchievementAsset`
>
> Pairs with EoV's `RCG_AchievementAsset` — RCG owns "did the condition trigger", this Asset owns "push to Steam".

## Purpose

**Maps to a single "Achievement" entry in the Steam backend.** Before use, **you must first create the matching ID under "Stats and Achievements" → "Achievements" in the [Steamworks partner site](https://partner.steamgames.com/apps/)**; this Asset's `ID` must match exactly. On unlock, the system calls `SetAchievement` + `StoreStats` and the Steam client pops up the built-in achievement toast.

Inherits from `UCL_Asset<UCL_SteamAchievementAsset>`.

## What it looks like in the Editor

```
UCL_SteamAchievementAsset: <ID = backend Achievement ID>
    Flag        ← whether unlocked (runtime cache)
    [Button] SetStat   ← push current Flag to Steam
    [Button] Get       ← fetch latest state from Steam
```

## Main fields

| Editor label | Required | Description |
|---|---|---|
| **Flag** | — | Whether this achievement is unlocked (runtime cache; equivalent to `SteamUserStats.GetAchievement` return) |

## Behavior

### Get state (`GetStat`)
*   `m_Inited = false` → `SteamUserStats.GetAchievement(ID, out m_Flag)` reads from Steam.
*   Returns `m_Flag`.

### Set achievement (`SetStat(bool flag)`)
*   `flag = true`:
    *   `SteamUserStats.SetAchievement(ID)` → marks unlocked.
    *   On success → `SteamUserStats.StoreStats()` pushes to server, triggers Steam popup.
*   `flag = false` (rarely used):
    *   `SteamUserStats.ClearAchievement(ID)` → clears (test only; should never happen in production).
*   Wrapped in try-catch throughout.

### Relationship with RCG_AchievementAsset
EoV's `RCG_AchievementAsset` references this Asset via the `m_SteamAchievement` field; when the RCG condition is met, it calls `steamAchievement.GetData().SetStat(true)` to push to Steam.

## Caveats

*   **ID must match the Steam backend exactly**: one typo and the achievement never fires.
*   **Editor's AppId must be correct**: if `steam_appid.txt = 480`, every achievement call hits SpaceWar — completely bypassing your game's backend.
*   **`SetAchievement` is idempotent**: calling it repeatedly for the same ID does not pop the toast multiple times (Steam dedups internally).
*   **Unpublished achievements**: achievements still in "draft" on the partner site won't pop or persist when `SetAchievement` is called at runtime; you must "Publish" them in the backend first.
*   **`GetStat` logs as LogError, not LogInfo**: every call prints `GetStat ID:... m_Flag:..., success:...` via `LogError` — successful reads still appear as errors in the log. Minor implementation quirk.

---

## Appendix: Programmer Reference

### A.1 Class info
*   **File path**: `CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamAchievementAsset.cs`
*   **Inherits**: `UCL_Asset<UCL_SteamAchievementAsset>`
*   **AssetGroup**: `AssetGroup.Steam` / sort = `UCL_SteamAchievementAsset`
*   **`#if UNITY_STANDALONE_*`** conditional compile

### A.2 Field map

| Code field | Editor label | Type | Notes |
|---|---|---|---|
| `m_Flag` | Flag | `bool` | runtime cache |
| `m_Inited` | — | `bool` (private) | Whether already synced from Steam |

### A.3 Key methods

*   **`GetStat()` → bool** — first call reads from Steam, subsequent calls return cache; wrapped in try-catch.
*   **`SetStat()` (no arg)** → calls `SetStat(m_Flag)`.
*   **`SetStat(bool flag)`** — `m_Flag = flag`:
    *   `flag=true` → `SteamUserStats.SetAchievement(ID)` → on success `SteamUserStats.StoreStats()`.
    *   `flag=false` → `SteamUserStats.ClearAchievement(ID)` (test only).
*   **OnGUI** → exposes `SetStat` / `Get` buttons (manual testing).

### A.4 Interactions

*   **`SteamUserStats`** (Steamworks SDK) — `GetAchievement` / `SetAchievement` / `ClearAchievement` / `StoreStats`.
*   **`RCG_AchievementAsset`** (EoV) — `m_SteamAchievement: UCL_SteamAchievementEntry` references this Asset.
*   **`UCL_SteamAchievementEntry`** (same file) — Asset Entry wrapper; default ID = `"Default"`.

### A.5 Known issues

*   `GetStat` uses `Debug.LogError` even on the success path (file lines 56 and 96); messages look like errors but aren't.
*   `m_Inited` is set inconsistently: line 53's `if (!m_Inited)` wraps the read, but `m_Inited = true` is never assigned inside — **every call hits the Steam API again** (suspected bug; watch out).
*   `SetStat` and `UCL_SteamUserStatAsset` share OnGUI button names (`SetStat` / `Get`); the UI is consistent but for an achievement the `Get` button only returns a bool — it doesn't write back anywhere.
