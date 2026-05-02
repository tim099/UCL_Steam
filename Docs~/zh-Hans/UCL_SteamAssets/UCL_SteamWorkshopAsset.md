---
title: Steam Workshop 上传 (UCL_SteamWorkshopAsset) 说明
description: 模组 (Mod) 上传到 Steam Workshop 的设定：标题、描述、可见度、变更日志、相依模组
last_updated: 2026-05-02
target_audience: [Build_Engineer, Modder, AI_Agent]
---

# Steam Workshop 上传

> 程式类别名称：`UCL.SteamLib.UCL_SteamWorkshopAsset`

## 用途

**把一个 Mod / 模组上传到 Steam Workshop 的设定**。连结：
*   一个 `UCL_ModuleEntry`（要上传的模组本体）
*   Workshop 上的标题 / 描述 / 变更日志 / 标签 / 可见度
*   相依模组（玩家订阅此 Mod 时 Steam 会一起装）

每次上传会走 `SteamUGC` 流程（建立 / 更新 Workshop item），并把回传的 `PublishedFileId` 存回此 Asset。

继承自 `UCL_Asset<UCL_SteamWorkshopAsset>`。

## 编辑器中的样貌

```
UCL_SteamWorkshopAsset: <ID>
    Module                   ← 要上传的模组（UCL_ModuleEntry）
    Title                    ← Workshop 显示的标题
    Description              ← 模组描述
    SteamAPILangCode  ▾      ← 模组语言（english / schinese / japanese...）
    EWorkshopFileType  ▾     ← 档案类型（一般用 Community）
    Visibility  ▾            ← 可见度（Public / FriendsOnly / Private）
    Tags                     ← 标签清单（给玩家搜寻用）
    ChangeLog                ← 变更说明（显示在更新日志）
    Dependencies             ← 相依模组的 PublishedFileId 清单
    PublishedFileId          ← Steam 回传的 Workshop ID（首次上传后写入）
    [上传流程按钮]            ← 多阶段：CreateItem / StartItemUpdate / DeleteItem
```

## 主要栏位

| 编辑器显示 | 必填 | 说明 |
|---|---|---|
| **Module** | 是 | 要上传的模组本体（`UCL_ModuleEntry`），对应的资料夹会被打包上传 |
| **Title** | 是 | Workshop 标题；玩家在 Workshop 看到的名称 |
| **Description** | 是 | 模组详细说明（Markdown 部份支援） |
| **SteamAPILangCode** | 是 | 模组目标语言（影响 Workshop 内语言筛选），预设 `english` |
| **EWorkshopFileType** | 是 | 档案类型；本游戏一般用 `k_EWorkshopFileTypeCommunity` |
| **Visibility** | 是 | `Public`（公开）/ `FriendsOnly`（仅好友）/ `Private`（仅自己），预设 `Public` |
| **Tags** | 否 | 自订标签清单，帮玩家搜寻分类 |
| **ChangeLog** | 否 | 本次更新的变更说明，显示在 Workshop 更新日志中 |
| **Dependencies** | 否 | 相依模组的 `PublishedFileId_t` 清单；玩家订阅时 Steam 会一起装 |
| **PublishedFileId** | — | 首次 `CreateItem` 后 Steam 回传的 ID；**之后再上传要保留这个值** |

## 行为说明

### 上传流程（`UploadState`）

1. **`None`**：闲置状态。
2. **`CreateItem`**：第一次上传时呼叫 `SteamUGC.CreateItem`，取得 `PublishedFileId` 并写回此 Asset。
3. **`StartItemUpdate`**：之后的更新走这条 — 设定 Title / Description / Tags / ChangeLog / Visibility / 上传档案内容到 `SteamUGC.SubmitItemUpdate`。
4. **`DeleteItem`**：删除已发布的 Workshop item（不可逆）。

### 与 Module 的对应
`m_Module: UCL_ModuleEntry` 引用一个本地模组；上传时会把该模组对应的资料夹打成 Steam Workshop 上的 content。**模组需要先在 `UCL_ModuleService` 中注册好**才能上传。

### Dependencies 的作用
`m_Dependencies` 列出的 `PublishedFileId_t` 会被一起记录到 Steam Workshop。**玩家订阅本模组时，Steam client 会自动下载相依模组**（同 mod 平台常见机制）。

## 注意事项

*   **首次上传必填先 CreateItem**：直接 StartItemUpdate 会失败（没 PublishedFileId）。
*   **PublishedFileId 写入 Asset 后必须 commit**：第一次上传完，新 ID 写到 JSON 内；漏 commit 下次就再 `CreateItem` 重新建一次（变两个独立 Workshop item）。
*   **Visibility 可随时改**：先 Private 内测，正式发布时再切 Public。
*   **Editor 的 AppId 要对**：`steam_appid.txt` 必须是你的游戏 AppID（1864830 或 2263320），不能是 480；否则 SteamUGC 会打到 SpaceWar 的 Workshop。
*   **Tag 要在 Steam 后台先批准**：玩家可见的 Tag 清单在 partner site 设定，Asset 内乱填 tag 玩家搜不到。
*   **档案大小限制**：Steam Workshop 单一 item 上限通常数百 MB；大型 mod 要注意。

---

## 附录：程式人员参考 (Programmer Reference)

### A.1 类别资讯
*   **档案路径**：`CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamWorkshopAsset.cs`
*   **继承自**：`UCL_Asset<UCL_SteamWorkshopAsset>`
*   **AssetGroup**：`AssetGroup.Steam` / sort = `UCL_SteamWorkshopAsset`
*   **`#if UNITY_STANDALONE_*`** 条件编译

### A.2 栏位对照

| 程式栏位 | 编辑器显示 | 型别 | 备注 |
|---|---|---|---|
| `m_Module` | Module | `UCL_ModuleEntry` | 要上传的模组 |
| `m_Title` | Title | `string` | Workshop 标题 |
| `m_Description` | Description | `string` | 详细描述 |
| `m_SteamAPILangCode` | SteamAPILangCode | `SteamAPILangCode` enum | 预设 `english` |
| `m_EWorkshopFileType` | EWorkshopFileType | `EWorkshopFileType` enum | 预设 `Community` |
| `m_Visibility` | Visibility | `ERemoteStoragePublishedFileVisibility` enum | 预设 `Public` |
| `m_Tags` | Tags | `List<string>` | |
| `m_ChangeLog` | ChangeLog | `string` | |
| `m_Dependencies` | Dependencies | `List<PublishedFileId_t>` | |
| `m_PublishedFileId` | PublishedFileId | `PublishedFileId_t` | 首次上传后写入 |

### A.3 重要 Method 摘要

`UCL_SteamWorkshopAsset` 内含完整的上传流程实作（async / coroutine 为主），主要方法包括：
*   **`CreateItem`** — `SteamUGC.CreateItem(AppId, FileType)` → 取得 `m_PublishedFileId`。
*   **`StartItemUpdate`** — `SteamUGC.StartItemUpdate(AppId, m_PublishedFileId)` → 设置 metadata + content folder → `SubmitItemUpdate`。
*   **`DeleteItem`** — `SteamUGC.DeleteItem(m_PublishedFileId)`。
*   **`UploadState` 状态机** — UI 流程驱动，避免重复上传冲突。

### A.4 与其他系统的互动

*   **`SteamUGC`** (Steamworks SDK) — Workshop 主介面。
*   **`UCL_ModuleEntry` / `UCL_ModuleService`** — 模组系统。
*   **`UCL_SteamConfigAsset`** — 上传目标 AppID 来源。
*   **`UCL_SteamWorkshopAssetEntry`**（同档，line 490）— Asset Entry 包装。

### A.5 已知议题

*   首次上传后 `m_PublishedFileId` 必须手动 save 回 Asset；自动 save 的 callback 容易遗漏，造成「重新建出第二个 Workshop item」的情况。
*   `m_EWorkshopFileType` 列表中包含旧版 / 已弃用的类型（如 `k_EWorkshopFileTypeFirst` 等）；一般专案只用 `Community`。
*   `m_Tags` 没有与 Steam 后台批准的 tag 同步机制 — 拼错字玩家搜不到也不会报错。
