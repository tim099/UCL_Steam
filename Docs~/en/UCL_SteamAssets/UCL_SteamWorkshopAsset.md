---
title: Steam Workshop Upload (UCL_SteamWorkshopAsset)
description: Settings for uploading a Mod to Steam Workshop — title, description, visibility, changelog, dependencies
last_updated: 2026-05-02
target_audience: [Build_Engineer, Modder, AI_Agent]
---

# Steam Workshop Upload

> Class name: `UCL.SteamLib.UCL_SteamWorkshopAsset`

## Purpose

**Settings for uploading a Mod to Steam Workshop.** Links together:
*   one `UCL_ModuleEntry` (the mod body to upload)
*   the Workshop title / description / changelog / tags / visibility
*   dependent mods (Steam will install them too when the player subscribes to this mod)

Each upload runs through the `SteamUGC` flow (CreateItem / SubmitItemUpdate) and writes the returned `PublishedFileId` back into this Asset.

Inherits from `UCL_Asset<UCL_SteamWorkshopAsset>`.

## What it looks like in the Editor

```
UCL_SteamWorkshopAsset: <ID>
    Module                   ← mod to upload (UCL_ModuleEntry)
    Title                    ← title shown on Workshop
    Description              ← mod description
    SteamAPILangCode  ▾      ← mod language (english / schinese / japanese...)
    EWorkshopFileType  ▾     ← file type (Community is the usual choice)
    Visibility  ▾            ← visibility (Public / FriendsOnly / Private)
    Tags                     ← tag list (for player search)
    ChangeLog                ← change description (shown in update log)
    Dependencies             ← list of dependency PublishedFileIds
    PublishedFileId          ← Workshop ID returned by Steam (written after first upload)
    [upload buttons]         ← multi-stage: CreateItem / StartItemUpdate / DeleteItem
```

## Main fields

| Editor label | Required | Description |
|---|---|---|
| **Module** | Yes | The mod body to upload (`UCL_ModuleEntry`); the corresponding folder is packaged and uploaded |
| **Title** | Yes | Workshop title; the name players see in Workshop |
| **Description** | Yes | Detailed description (partial Markdown support) |
| **SteamAPILangCode** | Yes | Target language (affects Workshop language filtering); default `english` |
| **EWorkshopFileType** | Yes | File type; this game uses `k_EWorkshopFileTypeCommunity` |
| **Visibility** | Yes | `Public` / `FriendsOnly` / `Private`; default `Public` |
| **Tags** | No | Custom tag list for player search and categorization |
| **ChangeLog** | No | Description for this update; shown in the Workshop changelog |
| **Dependencies** | No | List of dependency `PublishedFileId_t`s; Steam auto-downloads these on subscribe |
| **PublishedFileId** | — | Returned by Steam after first `CreateItem`; **must be preserved for subsequent uploads** |

## Behavior

### Upload flow (`UploadState`)

1. **`None`**: idle.
2. **`CreateItem`**: first upload calls `SteamUGC.CreateItem`, gets `PublishedFileId` and writes back to this Asset.
3. **`StartItemUpdate`**: subsequent updates run through this — sets Title / Description / Tags / ChangeLog / Visibility, uploads content folder, then `SteamUGC.SubmitItemUpdate`.
4. **`DeleteItem`**: deletes the published Workshop item (irreversible).

### Module mapping
`m_Module: UCL_ModuleEntry` references a local mod; on upload, that mod's folder is packaged as Steam Workshop content. **The mod must already be registered in `UCL_ModuleService`** before it can be uploaded.

### Dependencies behavior
The `PublishedFileId_t`s in `m_Dependencies` are recorded with the Steam Workshop entry. **When a player subscribes to this mod, the Steam client will auto-download the dependencies** (standard mod-platform mechanism).

## Caveats

*   **First upload must call CreateItem**: jumping straight to StartItemUpdate fails (no PublishedFileId yet).
*   **PublishedFileId must be committed back to the Asset**: after first upload, the new ID is written to JSON; if you forget to commit, the next upload runs `CreateItem` again and creates a second, separate Workshop item.
*   **Visibility can be changed at any time**: ship Private for internal testing, then flip to Public for release.
*   **Editor AppId must be correct**: `steam_appid.txt` must be your game's AppID (1864830 or 2263320), not 480; otherwise SteamUGC hits the SpaceWar Workshop instead.
*   **Tags must be approved on the partner site first**: the visible tag list is configured in the partner site; arbitrary tags in this Asset won't be searchable.
*   **File size limits**: Steam Workshop's per-item cap is typically a few hundred MB; large mods need attention.

---

## Appendix: Programmer Reference

### A.1 Class info
*   **File path**: `CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamWorkshopAsset.cs`
*   **Inherits**: `UCL_Asset<UCL_SteamWorkshopAsset>`
*   **AssetGroup**: `AssetGroup.Steam` / sort = `UCL_SteamWorkshopAsset`
*   **`#if UNITY_STANDALONE_*`** conditional compile

### A.2 Field map

| Code field | Editor label | Type | Notes |
|---|---|---|---|
| `m_Module` | Module | `UCL_ModuleEntry` | mod to upload |
| `m_Title` | Title | `string` | Workshop title |
| `m_Description` | Description | `string` | detailed description |
| `m_SteamAPILangCode` | SteamAPILangCode | `SteamAPILangCode` enum | default `english` |
| `m_EWorkshopFileType` | EWorkshopFileType | `EWorkshopFileType` enum | default `Community` |
| `m_Visibility` | Visibility | `ERemoteStoragePublishedFileVisibility` enum | default `Public` |
| `m_Tags` | Tags | `List<string>` | |
| `m_ChangeLog` | ChangeLog | `string` | |
| `m_Dependencies` | Dependencies | `List<PublishedFileId_t>` | |
| `m_PublishedFileId` | PublishedFileId | `PublishedFileId_t` | written after first upload |

### A.3 Key methods

`UCL_SteamWorkshopAsset` contains the full upload-flow implementation (mostly async / coroutine-based). Main methods:
*   **`CreateItem`** — `SteamUGC.CreateItem(AppId, FileType)` → obtains `m_PublishedFileId`.
*   **`StartItemUpdate`** — `SteamUGC.StartItemUpdate(AppId, m_PublishedFileId)` → set metadata + content folder → `SubmitItemUpdate`.
*   **`DeleteItem`** — `SteamUGC.DeleteItem(m_PublishedFileId)`.
*   **`UploadState` state machine** — drives the UI flow and prevents concurrent-upload conflicts.

### A.4 Interactions

*   **`SteamUGC`** (Steamworks SDK) — main Workshop interface.
*   **`UCL_ModuleEntry` / `UCL_ModuleService`** — mod system.
*   **`UCL_SteamConfigAsset`** — source of the upload target AppID.
*   **`UCL_SteamWorkshopAssetEntry`** (same file, line 490) — Asset Entry wrapper.

### A.5 Known issues

*   After the first upload, `m_PublishedFileId` must be saved back to the Asset manually; the auto-save callback is easy to miss, leading to "a second Workshop item gets created" on the next upload.
*   `m_EWorkshopFileType`'s list includes legacy / deprecated types (e.g. `k_EWorkshopFileTypeFirst`); typical projects only use `Community`.
*   `m_Tags` has no sync mechanism with the partner-site approved tag list — typos pass silently, with no error, and players can't find the mod by that tag.
