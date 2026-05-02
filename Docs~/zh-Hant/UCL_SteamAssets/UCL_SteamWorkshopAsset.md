---
title: Steam Workshop 上傳 (UCL_SteamWorkshopAsset) 說明
description: 模組 (Mod) 上傳到 Steam Workshop 的設定：標題、描述、可見度、變更日誌、相依模組
last_updated: 2026-05-02
target_audience: [Build_Engineer, Modder, AI_Agent]
---

# Steam Workshop 上傳

> 程式類別名稱：`UCL.SteamLib.UCL_SteamWorkshopAsset`

## 用途

**把一個 Mod / 模組上傳到 Steam Workshop 的設定**。連結：
*   一個 `UCL_ModuleEntry`（要上傳的模組本體）
*   Workshop 上的標題 / 描述 / 變更日誌 / 標籤 / 可見度
*   相依模組（玩家訂閱此 Mod 時 Steam 會一起裝）

每次上傳會走 `SteamUGC` 流程（建立 / 更新 Workshop item），並把回傳的 `PublishedFileId` 存回此 Asset。

繼承自 `UCL_Asset<UCL_SteamWorkshopAsset>`。

## 編輯器中的樣貌

```
UCL_SteamWorkshopAsset: <ID>
    Module                   ← 要上傳的模組（UCL_ModuleEntry）
    Title                    ← Workshop 顯示的標題
    Description              ← 模組描述
    SteamAPILangCode  ▾      ← 模組語言（english / schinese / japanese...）
    EWorkshopFileType  ▾     ← 檔案類型（一般用 Community）
    Visibility  ▾            ← 可見度（Public / FriendsOnly / Private）
    Tags                     ← 標籤清單（給玩家搜尋用）
    ChangeLog                ← 變更說明（顯示在更新日誌）
    Dependencies             ← 相依模組的 PublishedFileId 清單
    PublishedFileId          ← Steam 回傳的 Workshop ID（首次上傳後寫入）
    [上傳流程按鈕]            ← 多階段：CreateItem / StartItemUpdate / DeleteItem
```

## 主要欄位

| 編輯器顯示 | 必填 | 說明 |
|---|---|---|
| **Module** | 是 | 要上傳的模組本體（`UCL_ModuleEntry`），對應的資料夾會被打包上傳 |
| **Title** | 是 | Workshop 標題；玩家在 Workshop 看到的名稱 |
| **Description** | 是 | 模組詳細說明（Markdown 部份支援） |
| **SteamAPILangCode** | 是 | 模組目標語言（影響 Workshop 內語言篩選），預設 `english` |
| **EWorkshopFileType** | 是 | 檔案類型；本遊戲一般用 `k_EWorkshopFileTypeCommunity` |
| **Visibility** | 是 | `Public`（公開）/ `FriendsOnly`（僅好友）/ `Private`（僅自己），預設 `Public` |
| **Tags** | 否 | 自訂標籤清單，幫玩家搜尋分類 |
| **ChangeLog** | 否 | 本次更新的變更說明，顯示在 Workshop 更新日誌中 |
| **Dependencies** | 否 | 相依模組的 `PublishedFileId_t` 清單；玩家訂閱時 Steam 會一起裝 |
| **PublishedFileId** | — | 首次 `CreateItem` 後 Steam 回傳的 ID；**之後再上傳要保留這個值** |

## 行為說明

### 上傳流程（`UploadState`）

1. **`None`**：閒置狀態。
2. **`CreateItem`**：第一次上傳時呼叫 `SteamUGC.CreateItem`，取得 `PublishedFileId` 並寫回此 Asset。
3. **`StartItemUpdate`**：之後的更新走這條 — 設定 Title / Description / Tags / ChangeLog / Visibility / 上傳檔案內容到 `SteamUGC.SubmitItemUpdate`。
4. **`DeleteItem`**：刪除已發佈的 Workshop item（不可逆）。

### 與 Module 的對應
`m_Module: UCL_ModuleEntry` 引用一個本地模組；上傳時會把該模組對應的資料夾打成 Steam Workshop 上的 content。**模組需要先在 `UCL_ModuleService` 中註冊好**才能上傳。

### Dependencies 的作用
`m_Dependencies` 列出的 `PublishedFileId_t` 會被一起記錄到 Steam Workshop。**玩家訂閱本模組時，Steam client 會自動下載相依模組**（同 mod 平台常見機制）。

## 注意事項

*   **首次上傳必填先 CreateItem**：直接 StartItemUpdate 會失敗（沒 PublishedFileId）。
*   **PublishedFileId 寫入 Asset 後必須 commit**：第一次上傳完，新 ID 寫到 JSON 內；漏 commit 下次就再 `CreateItem` 重新建一次（變兩個獨立 Workshop item）。
*   **Visibility 可隨時改**：先 Private 內測，正式發佈時再切 Public。
*   **Editor 的 AppId 要對**：`steam_appid.txt` 必須是你的遊戲 AppID（1864830 或 2263320），不能是 480；否則 SteamUGC 會打到 SpaceWar 的 Workshop。
*   **Tag 要在 Steam 後台先批准**：玩家可見的 Tag 清單在 partner site 設定，Asset 內亂填 tag 玩家搜不到。
*   **檔案大小限制**：Steam Workshop 單一 item 上限通常數百 MB；大型 mod 要注意。

---

## 附錄：程式人員參考 (Programmer Reference)

### A.1 類別資訊
*   **檔案路徑**：`CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamWorkshopAsset.cs`
*   **繼承自**：`UCL_Asset<UCL_SteamWorkshopAsset>`
*   **AssetGroup**：`AssetGroup.Steam` / sort = `UCL_SteamWorkshopAsset`
*   **`#if UNITY_STANDALONE_*`** 條件編譯

### A.2 欄位對照

| 程式欄位 | 編輯器顯示 | 型別 | 備註 |
|---|---|---|---|
| `m_Module` | Module | `UCL_ModuleEntry` | 要上傳的模組 |
| `m_Title` | Title | `string` | Workshop 標題 |
| `m_Description` | Description | `string` | 詳細描述 |
| `m_SteamAPILangCode` | SteamAPILangCode | `SteamAPILangCode` enum | 預設 `english` |
| `m_EWorkshopFileType` | EWorkshopFileType | `EWorkshopFileType` enum | 預設 `Community` |
| `m_Visibility` | Visibility | `ERemoteStoragePublishedFileVisibility` enum | 預設 `Public` |
| `m_Tags` | Tags | `List<string>` | |
| `m_ChangeLog` | ChangeLog | `string` | |
| `m_Dependencies` | Dependencies | `List<PublishedFileId_t>` | |
| `m_PublishedFileId` | PublishedFileId | `PublishedFileId_t` | 首次上傳後寫入 |

### A.3 重要 Method 摘要

`UCL_SteamWorkshopAsset` 內含完整的上傳流程實作（async / coroutine 為主），主要方法包括：
*   **`CreateItem`** — `SteamUGC.CreateItem(AppId, FileType)` → 取得 `m_PublishedFileId`。
*   **`StartItemUpdate`** — `SteamUGC.StartItemUpdate(AppId, m_PublishedFileId)` → 設置 metadata + content folder → `SubmitItemUpdate`。
*   **`DeleteItem`** — `SteamUGC.DeleteItem(m_PublishedFileId)`。
*   **`UploadState` 狀態機** — UI 流程驅動，避免重複上傳衝突。

### A.4 與其他系統的互動

*   **`SteamUGC`** (Steamworks SDK) — Workshop 主介面。
*   **`UCL_ModuleEntry` / `UCL_ModuleService`** — 模組系統。
*   **`UCL_SteamConfigAsset`** — 上傳目標 AppID 來源。
*   **`UCL_SteamWorkshopAssetEntry`**（同檔，line 490）— Asset Entry 包裝。

### A.5 已知議題

*   首次上傳後 `m_PublishedFileId` 必須手動 save 回 Asset；自動 save 的 callback 容易遺漏，造成「重新建出第二個 Workshop item」的情況。
*   `m_EWorkshopFileType` 列表中包含舊版 / 已棄用的類型（如 `k_EWorkshopFileTypeFirst` 等）；一般專案只用 `Community`。
*   `m_Tags` 沒有與 Steam 後台批准的 tag 同步機制 — 拼錯字玩家搜不到也不會報錯。
