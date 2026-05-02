---
title: Steam User Stat (UCL_SteamUserStatAsset)
description: Maps to a single Stat ID in Steam backend's "Stats and Achievements"; provides Get/Set/Alter sync
last_updated: 2026-05-02
target_audience: [Build_Engineer, AI_Agent, Designer]
---

# Steam User Stat

> Class name: `UCL.SteamLib.UCL_SteamUserStatAsset`
>
> Bridges between the EoV achievement / stats system (`RCG_StatsAsset`) and Steam.

## Purpose

**Maps to a single "Stat" entry in the Steam backend.** Steam Stats are accumulating historical numbers (kills, total play time, total score, ...). Before use, **you must first create the matching ID under "Stats and Achievements" → "Stats" in the [Steamworks partner site](https://partner.steamgames.com/apps/)**; the Asset's `ID` field must match the backend ID exactly.

Inherits from `UCL_Asset<UCL_SteamUserStatAsset>`.

## What it looks like in the Editor

```
UCL_SteamUserStatAsset: <ID = backend Stat ID>
    ValueType  ▾ Int / Float / AVGRate
    m_Int       ← shown when ValueType=Int
    m_Float     ← shown when ValueType=Float / AVGRate
    [Button] SetStat   ← upload current value to Steam
    [Button] Get       ← fetch latest value from Steam to local
```

## Main fields

| Editor label | Required | Description |
|---|---|---|
| **ValueType** | Yes | Numeric type: `Int` / `Float` / `AVGRate` (average rate, e.g. "score per hour") |
| **m_Int** | When ValueType=Int | Integer value; conditional display |
| **m_Float** | When ValueType=Float / AVGRate | Float value; conditional display |

## Behavior

### Sync flow
1. **`GetStatInt()`** — first call goes to `GetStat()` to fetch from Steam; subsequent calls return cached `m_Int`. Requires the matching Stat ID to exist in the Steam backend, otherwise it fails.
2. **`SetStat(int)`** — `m_Int = val` → `SteamUserStats.SetStat(ID, m_Int)`, **does NOT call `StoreStats`** (so it isn't pushed to the Steam server immediately).
3. **`AlterStat(int)`** — `GetStatInt() → m_Int += val → SteamUserStats.SetStat() → SteamUserStats.StoreStats()`, **calls `StoreStats`** (pushes to server and triggers achievement popups).

### Float variant
`SetStat(float val)` works like the Int version but on `m_Float`; there is currently **no matching `AlterStat(float)` / `GetStatFloat()`**.

### Backend mismatch
If the Steam backend doesn't have this ID, `GetStat` does `Debug.LogError` and returns false; `SetStat` also fails. (This was the root cause of the `Win_EnemyType_Elite` errors in Simulation log.)

## Caveats

*   **ID must match the Steam backend exactly**: one typo and the stat silently disappears.
*   **Editor's AppId must be correct**: if `steam_appid.txt = 480` (SpaceWar), every stat call hits SpaceWar's backend instead of your game — confirm [`UCL_SteamConfigAsset`](UCL_SteamConfigAsset.md) is switched to the production AppID (1864830).
*   **`AlterStat` calls StoreStats automatically; `SetStat` doesn't**: a raw `SetStat` keeps the value local and may be lost on next client restart. For permanent records use `AlterStat` or call `SteamUserStats.StoreStats` yourself.
*   **Float and AVGRate share `m_Float`**: AVGRate is a special backend type (Steam computes the rate), but to this Asset it's just a float.
*   **`m_Inited` is a private bool**: runtime cache, set true after first successful GetStat; switching user or resetting won't clear it automatically.

---

## Appendix: Programmer Reference

### A.1 Class info
*   **File path**: `CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamUserStatAsset.cs`
*   **Inherits**: `UCL_Asset<UCL_SteamUserStatAsset>`
*   **AssetGroup**: `AssetGroup.Steam` / sort = `UCL_SteamUserStatAsset`
*   **`#if UNITY_STANDALONE_*`** conditional compile

### A.2 Field map

| Code field | Editor label | Type | Notes |
|---|---|---|---|
| `m_ValueType` | ValueType | `EValueType` enum | `Int` / `Float` / `AVGRate` |
| `m_Int` | m_Int | `int` | `Conditional(Int)` |
| `m_Float` | m_Float | `float` | `Conditional(Float, AVGRate)` |
| `m_Inited` | — | `bool` (private) | runtime cache flag |

### A.3 Key methods

*   **`GetStat()` (private)** → `SteamUserStats.GetStat(ID, out val)`; currently **only the `Int` case is implemented** (Float / AVGRate are stubbed out).
*   **`GetStatInt()`** → first call goes to `GetStat`; subsequent calls return `m_Int`.
*   **`SetStat()` (no arg)** → branches by ValueType to `SetStat(int)` or `SetStat(float)`.
*   **`SetStat(int val)`** → writes `m_Int` → `SteamUserStats.SetStat(ID, m_Int)`, **does NOT call StoreStats**.
*   **`SetStat(float val)`** → writes `m_Float` → `SteamUserStats.SetStat(ID, m_Float)`.
*   **`AlterStat(int val)`** → `GetStatInt() + val` → `SetStat` → `StoreStats`, with try-catch.

### A.4 Interactions

*   **`SteamUserStats`** (Steamworks SDK) — `GetStat` / `SetStat` / `StoreStats`.
*   **`RCG_StatsAsset`** (EoV-side) — references this Asset via the `m_SteamUserStat` field.
*   **`UCL_SteamUserStatAssetEntry`** (same file) — Asset Entry wrapper; default ID = `"Default"`.

### A.5 Known issues

*   `GetStat`'s `switch (m_ValueType)` only handles `Int`; **`Float` / `AVGRate` are completely unimplemented** (silently returns false, no LogWarning).
*   `SetStat(int)` does not call `StoreStats` while `AlterStat` does — easy to forget the difference and skip server push.
*   `m_Inited`, once set true, is never reset automatically; cache may become stale after user logout/login.
