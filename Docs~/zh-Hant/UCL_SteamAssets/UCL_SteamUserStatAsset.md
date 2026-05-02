---
title: Steam 使用者統計 (UCL_SteamUserStatAsset) 說明
description: 對應 Steam 後台「統計與成就」中的單一統計 ID；提供 Get/Set/Alter 同步
last_updated: 2026-05-02
target_audience: [Build_Engineer, AI_Agent, Designer]
---

# Steam 使用者統計

> 程式類別名稱：`UCL.SteamLib.UCL_SteamUserStatAsset`
>
> 與 EoV 端的成就 / 統計系統 (`RCG_StatsAsset`) 之間的橋樑。

## 用途

**對應 Steam 後台一筆「統計」的設定**。Steam Stats 是可累加的歷史數值（擊殺數、累計時間、總得分⋯）。使用前**必須先在 [Steamworks 後台](https://partner.steamgames.com/apps/) 的「統計與成就」→「統計」中創建對應 ID**，這個 Asset 的 `ID` 欄位必須與後台 ID 完全一致。

繼承自 `UCL_Asset<UCL_SteamUserStatAsset>`。

## 編輯器中的樣貌

```
UCL_SteamUserStatAsset: <ID = 後台統計 ID>
    ValueType  ▾ Int / Float / AVGRate
    m_Int       ← ValueType=Int 時顯示
    m_Float     ← ValueType=Float / AVGRate 時顯示
    [按鈕] SetStat   ← 把目前值上傳 Steam
    [按鈕] Get       ← 從 Steam 取最新值同步到本機
```

## 主要欄位

| 編輯器顯示 | 必填 | 說明 |
|---|---|---|
| **ValueType** | 是 | 數值類型：`Int`（整數）/ `Float`（浮點）/ `AVGRate`（平均速率，例如「每小時得分」） |
| **m_Int** | ValueType=Int | 整數值；Conditional 顯示 |
| **m_Float** | ValueType=Float / AVGRate | 浮點值；Conditional 顯示 |

## 行為說明

### 同步流程
1. **`GetStatInt()`** — 第一次呼叫時走 `GetStat()` 從 Steam 拉值；之後回傳快取的 `m_Int`。需要 Steam 後台已正確建立同名 stat 才會成功。
2. **`SetStat(int)`** — `m_Int = val` → `SteamUserStats.SetStat(ID, m_Int)`，**未呼叫 `StoreStats`**（不會立即推上 Steam server）。
3. **`AlterStat(int)`** — `GetStatInt() → m_Int += val → SteamUserStats.SetStat() → SteamUserStats.StoreStats()`，**有呼叫 `StoreStats`**（推上 server 並觸發成就彈窗）。

### Float 版本
`SetStat(float val)` 與 `Int` 類似，但用 `m_Float`；目前**沒有對應的 `AlterStat(float)` / `GetStatFloat()`**。

### 後台對應失敗
若 Steam 後台沒建這個 ID，`GetStat` 會 `Debug.LogError` 並回傳 false；`SetStat` 也會 fail（這就是先前 Simulation log 內 `Win_EnemyType_Elite` 等錯誤的根因）。

## 注意事項

*   **ID 必須與 Steam 後台一致**：寫錯一個字 stat 就消失，不會自動同步。
*   **Editor 的 AppId 要對**：如果 Editor 的 `steam_appid.txt = 480`（SpaceWar），所有 stat 都會打到 SpaceWar 的後台 stats，不會打到你的遊戲 — 確認 [`UCL_SteamConfigAsset`](UCL_SteamConfigAsset.md) 已切換到正式 AppID（1864830）。
*   **`AlterStat` 自動 StoreStats，但 `SetStat` 不會**：直接 SetStat 後值留在本地，下次 client 重啟可能消失。要永久記錄請用 AlterStat 或自己呼叫 `SteamUserStats.StoreStats`。
*   **Float 與 AVGRate 共用 `m_Float`**：AVGRate 在 Steam 後台是特殊類型（會自動計算速率），但本資料對外只是 float。
*   **`m_Inited` 是 private bool**：runtime cache，第一次成功 GetStat 後設 true；切換 user 或 reset 不會自動重置。

---

## 附錄：程式人員參考 (Programmer Reference)

### A.1 類別資訊
*   **檔案路徑**：`CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamUserStatAsset.cs`
*   **繼承自**：`UCL_Asset<UCL_SteamUserStatAsset>`
*   **AssetGroup**：`AssetGroup.Steam` / sort = `UCL_SteamUserStatAsset`
*   **`#if UNITY_STANDALONE_*`** 條件編譯

### A.2 欄位對照

| 程式欄位 | 編輯器顯示 | 型別 | 備註 |
|---|---|---|---|
| `m_ValueType` | ValueType | `EValueType` enum | `Int` / `Float` / `AVGRate` |
| `m_Int` | m_Int | `int` | `Conditional(Int)` |
| `m_Float` | m_Float | `float` | `Conditional(Float, AVGRate)` |
| `m_Inited` | — | `bool` (private) | runtime 快取旗標 |

### A.3 重要 Method 摘要

*   **`GetStat()` (private)** → `SteamUserStats.GetStat(ID, out val)`，目前**只實作 `Int`** case（`Float` / `AVGRate` 沒做）。
*   **`GetStatInt()`** → 第一次走 `GetStat`；之後回 `m_Int`。
*   **`SetStat()` (no arg)** → 依 ValueType 分流到 `SetStat(int)` 或 `SetStat(float)`。
*   **`SetStat(int val)`** → 寫 `m_Int` → `SteamUserStats.SetStat(ID, m_Int)`，**不呼叫 StoreStats**。
*   **`SetStat(float val)`** → 寫 `m_Float` → `SteamUserStats.SetStat(ID, m_Float)`。
*   **`AlterStat(int val)`** → `GetStatInt() + val` → `SetStat` → `StoreStats`，含 try-catch。

### A.4 與其他系統的互動

*   **`SteamUserStats`** (Steamworks SDK) — `GetStat` / `SetStat` / `StoreStats`。
*   **`RCG_StatsAsset`** (EoV 端) — 透過 `m_SteamUserStat` 欄位引用此 Asset。
*   **`UCL_SteamUserStatAssetEntry`**（同檔）— Asset Entry 包裝；預設 ID = `"Default"`。

### A.5 已知議題

*   `GetStat` 內 `switch (m_ValueType)` 只處理 `Int`，**`Float` / `AVGRate` 完全沒實作**（會直接回 false 但無 LogWarning）。
*   `SetStat(int)` 不呼叫 `StoreStats`，而 `AlterStat` 會 — 容易讓使用者忘記推 server 的差異。
*   `m_Inited` 一旦設為 true 不會自動重置，user logout/login 後快取可能不準。
