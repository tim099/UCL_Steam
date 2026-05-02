---
title: Steam 設定 (UCL_SteamConfigAsset) 說明
description: 設定 Steamworks 連接的 AppID；Editor 內按 Apply 即覆蓋 CardGame/steam_appid.txt
last_updated: 2026-05-02
target_audience: [Build_Engineer, AI_Agent, Designer]
---

# Steam 設定

> 程式類別名稱：`UCL.SteamLib.UCL_SteamConfigAsset`
>
> 整體 Steam AppID 架構分析請參考 專案層級的 Steam AppID 架構文件

## 用途

**為 Steamworks SDK 指定要連哪個 Steam app**。每個 Asset 包一個 AppID 字串；按下 `Apply AppId` 按鈕會把該 ID 寫入專案根 `CardGame/steam_appid.txt`，Editor 重啟 / Domain Reload 後 SteamAPI 會用新 ID 初始化。

繼承自 `UCL_Asset<UCL_SteamConfigAsset>`。

本專案目前的 entry：

| Asset ID | AppID | 用途 |
|---|---|---|
| `Default` | 1864830 | 正式版 Emblem of Valor |
| `Demo` | 2263320 | Demo 版本 |

> [!IMPORTANT]
> Editor 預設讀的 `steam_appid.txt` 目前是 **`480`**（Steamworks SDK 範例 SpaceWar），不是你的遊戲 — 要在 Editor 內驗證成就 / 統計請依 §「切換 SOP」操作。

## 編輯器中的樣貌

```
UCL_SteamConfigAsset: <ID>
    AppId    ← 字串型別的 AppID（如 "1864830"）
    [按鈕] Apply AppId         ← 寫入 CardGame/steam_appid.txt
    ConfigPath: ...            ← 顯示目標檔的絕對路徑
    [按鈕] SteamAPI Shutdown   ← 關閉現有 SteamAPI 連線
```

## 主要欄位

| 編輯器顯示 | 必填 | 說明 |
|---|---|---|
| **AppId** | 是 | Steam AppID 字串（會被 `uint.TryParse` 解析為 `AppId_t`，無法解析時回 `Invalid`） |

## 行為說明

### 寫入時機 (`ApplyAppId`)
按 `Apply AppId` 按鈕呼叫 `ApplyAppId(UCL.Core.FileLib.Lib.GameFolder)` →
*   `GameFolder` = `Path.GetDirectoryName(Application.dataPath)`，Editor 內 = `<project>/CardGame/`。
*   寫入路徑 = `<GameFolder>/steam_appid.txt`。
*   Build 時走 `UCL_SteamPostBuildSetting` 把 AppID 寫入 build 輸出目錄（前提：該 step IsEnable=True；本專案目前為 False）。

### Editor 切換 AppID SOP

1. Developer Page → `Steam` 群組 → `UCL_SteamConfigAsset`
2. 選擇要切的 entry（`Default` / `Demo`）
3. 按 `Apply AppId` — 覆蓋 `CardGame/steam_appid.txt`
4. 按 `SteamAPI Shutdown` — 關閉現連線
5. 觸發 Domain Reload（改任意 .cs 存檔，或 Editor 內 Ctrl+R）
6. `SteamManager.Awake` 用新 AppID 重新 `SteamAPI.Init()`

## 注意事項

*   **`steam_appid.txt` 是 git 追蹤檔**：Apply 後的變動會被 git 偵測到，commit 前確認你想推哪個 ID。
*   **沒對應授權無法切**：你的 Steam 帳號必須擁有對應 AppID 的 license（partner site / 開發者授權）才能在 Editor 跑。
*   **Demo (2263320) 與正式 (1864830) 是獨立 app**：成就 / 統計各自獨立，不互通。
*   **若要常切回 480 測試**：建議補一個 `Dev.json` (AppId=480) entry，避免手動編 steam_appid.txt 打字錯。

---

## 附錄：程式人員參考 (Programmer Reference)

### A.1 類別資訊
*   **檔案路徑**：`CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamConfigAsset.cs`
*   **繼承自**：`UCL_Asset<UCL_SteamConfigAsset>`
*   **AssetGroup**：`AssetGroup.Steam` / sort = `UCL_SteamConfigAsset`
*   **`#if UNITY_STANDALONE_*`** 條件編譯，僅 PC 平台啟用

### A.2 欄位對照

| 程式欄位 | 編輯器顯示 | 型別 | 備註 |
|---|---|---|---|
| `m_AppId` | AppId | `string` | uint.TryParse 失敗 → `AppId_t.Invalid` |

### A.3 重要 Method 摘要

*   **`AppId` (property)** → `m_AppId` 字串轉 `AppId_t`；解析失敗回預設值（`Invalid`）。
*   **`ApplyAppId(string path)`** → `File.WriteAllText(GetConfigPath(path), m_AppId)`，把 AppID 寫到 `path/steam_appid.txt`。
*   **`GetConfigPath(string path)` (static)** → `Path.Combine(path, "steam_appid.txt")`。
*   **`ConfigPath` (static property)** → 預設目標 = `GameFolder/steam_appid.txt`。
*   **OnGUI** → 提供 `Apply AppId` 與 `SteamAPI Shutdown` 兩個按鈕。

### A.4 與其他系統的互動

*   **`SteamManager.Awake`** ([SteamManager.cs:99,122](../../../UCL_SteamworksScript/UCL_SteamworkScripts/Steamworks.NET/SteamManager.cs)) — `SteamAPI.RestartAppIfNecessary(AppId_t.Invalid)` + `SteamAPI.Init()`，依賴 `steam_appid.txt`。
*   **`UCL_SteamPostBuildSetting`** — Build 後把對應 SteamConfig 的 AppId 寫入 build 輸出目錄（目前各 BuildAsset 都 disabled）。
*   **`UCL_SteamConfigAssetEntry`**（同檔）— Asset Entry 包裝；預設 ID = `"Default"`。

### A.5 已知議題

*   `m_AppId` 設計成 string 而非 uint 是為了支援空值與漸進初始化（Asset 初次建立時 ID 是 `"Asset ID"`）。
*   `ApplyAppId` 沒有 try-catch，目標路徑無權限時會直接拋例外（極少見）。
