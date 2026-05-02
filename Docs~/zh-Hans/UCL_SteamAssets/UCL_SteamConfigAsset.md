---
title: Steam 设定 (UCL_SteamConfigAsset) 说明
description: 设定 Steamworks 连接的 AppID；Editor 内按 Apply 即覆盖 CardGame/steam_appid.txt
last_updated: 2026-05-02
target_audience: [Build_Engineer, AI_Agent, Designer]
---

# Steam 设定

> 程式类别名称：`UCL.SteamLib.UCL_SteamConfigAsset`
>
> 整体 Steam AppID 架构分析请参考 專案層級的 Steam AppID 架構文件

## 用途

**为 Steamworks SDK 指定要连哪个 Steam app**。每个 Asset 包一个 AppID 字串；按下 `Apply AppId` 按钮会把该 ID 写入专案根 `CardGame/steam_appid.txt`，Editor 重启 / Domain Reload 后 SteamAPI 会用新 ID 初始化。

继承自 `UCL_Asset<UCL_SteamConfigAsset>`。

本专案目前的 entry：

| Asset ID | AppID | 用途 |
|---|---|---|
| `Default` | 1864830 | 正式版 Emblem of Valor |
| `Demo` | 2263320 | Demo 版本 |

> [!IMPORTANT]
> Editor 预设读的 `steam_appid.txt` 目前是 **`480`**（Steamworks SDK 范例 SpaceWar），不是你的游戏 — 要在 Editor 内验证成就 / 统计请依 §「切换 SOP」操作。

## 编辑器中的样貌

```
UCL_SteamConfigAsset: <ID>
    AppId    ← 字串型别的 AppID（如 "1864830"）
    [按钮] Apply AppId         ← 写入 CardGame/steam_appid.txt
    ConfigPath: ...            ← 显示目标档的绝对路径
    [按钮] SteamAPI Shutdown   ← 关闭现有 SteamAPI 连线
```

## 主要栏位

| 编辑器显示 | 必填 | 说明 |
|---|---|---|
| **AppId** | 是 | Steam AppID 字串（会被 `uint.TryParse` 解析为 `AppId_t`，无法解析时回 `Invalid`） |

## 行为说明

### 写入时机 (`ApplyAppId`)
按 `Apply AppId` 按钮呼叫 `ApplyAppId(UCL.Core.FileLib.Lib.GameFolder)` →
*   `GameFolder` = `Path.GetDirectoryName(Application.dataPath)`，Editor 内 = `<project>/CardGame/`。
*   写入路径 = `<GameFolder>/steam_appid.txt`。
*   Build 时走 `UCL_SteamPostBuildSetting` 把 AppID 写入 build 输出目录（前提：该 step IsEnable=True；本专案目前为 False）。

### Editor 切换 AppID SOP

1. Developer Page → `Steam` 群组 → `UCL_SteamConfigAsset`
2. 选择要切的 entry（`Default` / `Demo`）
3. 按 `Apply AppId` — 覆盖 `CardGame/steam_appid.txt`
4. 按 `SteamAPI Shutdown` — 关闭现连线
5. 触发 Domain Reload（改任意 .cs 存档，或 Editor 内 Ctrl+R）
6. `SteamManager.Awake` 用新 AppID 重新 `SteamAPI.Init()`

## 注意事项

*   **`steam_appid.txt` 是 git 追踪档**：Apply 后的变动会被 git 侦测到，commit 前确认你想推哪个 ID。
*   **没对应授权无法切**：你的 Steam 帐号必须拥有对应 AppID 的 license（partner site / 开发者授权）才能在 Editor 跑。
*   **Demo (2263320) 与正式 (1864830) 是独立 app**：成就 / 统计各自独立，不互通。
*   **若要常切回 480 测试**：建议补一个 `Dev.json` (AppId=480) entry，避免手动编 steam_appid.txt 打字错。

---

## 附录：程式人员参考 (Programmer Reference)

### A.1 类别资讯
*   **档案路径**：`CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamConfigAsset.cs`
*   **继承自**：`UCL_Asset<UCL_SteamConfigAsset>`
*   **AssetGroup**：`AssetGroup.Steam` / sort = `UCL_SteamConfigAsset`
*   **`#if UNITY_STANDALONE_*`** 条件编译，仅 PC 平台启用

### A.2 栏位对照

| 程式栏位 | 编辑器显示 | 型别 | 备注 |
|---|---|---|---|
| `m_AppId` | AppId | `string` | uint.TryParse 失败 → `AppId_t.Invalid` |

### A.3 重要 Method 摘要

*   **`AppId` (property)** → `m_AppId` 字串转 `AppId_t`；解析失败回预设值（`Invalid`）。
*   **`ApplyAppId(string path)`** → `File.WriteAllText(GetConfigPath(path), m_AppId)`，把 AppID 写到 `path/steam_appid.txt`。
*   **`GetConfigPath(string path)` (static)** → `Path.Combine(path, "steam_appid.txt")`。
*   **`ConfigPath` (static property)** → 预设目标 = `GameFolder/steam_appid.txt`。
*   **OnGUI** → 提供 `Apply AppId` 与 `SteamAPI Shutdown` 两个按钮。

### A.4 与其他系统的互动

*   **`SteamManager.Awake`** ([SteamManager.cs:99,122](../../../UCL_SteamworksScript/UCL_SteamworkScripts/Steamworks.NET/SteamManager.cs)) — `SteamAPI.RestartAppIfNecessary(AppId_t.Invalid)` + `SteamAPI.Init()`，依赖 `steam_appid.txt`。
*   **`UCL_SteamPostBuildSetting`** — Build 后把对应 SteamConfig 的 AppId 写入 build 输出目录（目前各 BuildAsset 都 disabled）。
*   **`UCL_SteamConfigAssetEntry`**（同档）— Asset Entry 包装；预设 ID = `"Default"`。

### A.5 已知议题

*   `m_AppId` 设计成 string 而非 uint 是为了支援空值与渐进初始化（Asset 初次建立时 ID 是 `"Asset ID"`）。
*   `ApplyAppId` 没有 try-catch，目标路径无权限时会直接抛例外（极少见）。
