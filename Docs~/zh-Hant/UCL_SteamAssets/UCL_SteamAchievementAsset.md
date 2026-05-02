---
title: Steam 成就 (UCL_SteamAchievementAsset) 說明
description: 對應 Steam 後台「成就」單筆設定；達成時推送 server 並觸發內建彈窗
last_updated: 2026-05-02
target_audience: [Build_Engineer, AI_Agent, Designer]
---

# Steam 成就

> 程式類別名稱：`UCL.SteamLib.UCL_SteamAchievementAsset`
>
> 與 EoV 端的 `RCG_AchievementAsset` 配合 — RCG 那層管「條件達成判斷」，這層管「推送 Steam」。

## 用途

**對應 Steam 後台一筆「成就」的設定**。使用前**必須先在 [Steamworks 後台](https://partner.steamgames.com/apps/) 的「統計與成就」→「成就」中創建對應 ID**，本 Asset 的 `ID` 必須與後台 ID 完全一致。達成後系統呼叫 `SetAchievement` + `StoreStats`，Steam client 會跳出內建的成就彈窗。

繼承自 `UCL_Asset<UCL_SteamAchievementAsset>`。

## 編輯器中的樣貌

```
UCL_SteamAchievementAsset: <ID = 後台成就 ID>
    Flag        ← 是否已獲得（runtime 快取）
    [按鈕] SetStat   ← 推送目前 Flag 到 Steam
    [按鈕] Get       ← 從 Steam 取最新狀態
```

## 主要欄位

| 編輯器顯示 | 必填 | 說明 |
|---|---|---|
| **Flag** | — | 是否已獲得此成就（runtime 快取，等同 `SteamUserStats.GetAchievement` 的回傳） |

## 行為說明

### 取得狀態 (`GetStat`)
*   `m_Inited = false` → `SteamUserStats.GetAchievement(ID, out m_Flag)` 從 Steam 拉值。
*   返回 `m_Flag`。

### 設定成就 (`SetStat(bool flag)`)
*   `flag = true`：
    *   `SteamUserStats.SetAchievement(ID)` → 標記已獲得。
    *   成功 → `SteamUserStats.StoreStats()` 推送 server，觸發 Steam 彈窗。
*   `flag = false`（少用）：
    *   `SteamUserStats.ClearAchievement(ID)` → 清除（測試用，正式應該不會用到）。
*   全程 try-catch 保護。

### 與 RCG_AchievementAsset 的關係
EoV 端的 `RCG_AchievementAsset` 透過 `m_SteamAchievement` 欄位引用本 Asset；當 RCG 條件滿足，呼叫 `steamAchievement.GetData().SetStat(true)` 推 Steam。

## 注意事項

*   **ID 必須與 Steam 後台一致**：拼錯字成就完全不會觸發。
*   **Editor 的 AppId 要對**：`steam_appid.txt = 480` 時所有成就會打到 SpaceWar — 完全不會推到你的遊戲後台。
*   **`SetAchievement` 是冪等的**：重複呼叫同一個成就 ID 不會跳出多次彈窗（Steam 已內建去重）。
*   **後台尚未發佈的成就**：在 partner site 進行中（draft）的成就，runtime 呼叫 `SetAchievement` 不會跳彈窗也不會記錄；要先在後台「Publish」。
*   **`GetStat` 會 LogError 而非 LogInfo**：每次呼叫都會印 `GetStat ID:... m_Flag:..., success:...`，這在程式內被當作 `LogError` 處理 — log 中即使 success=true 也會顯示為 error，是程式設計上的小瑕疵。

---

## 附錄：程式人員參考 (Programmer Reference)

### A.1 類別資訊
*   **檔案路徑**：`CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamAchievementAsset.cs`
*   **繼承自**：`UCL_Asset<UCL_SteamAchievementAsset>`
*   **AssetGroup**：`AssetGroup.Steam` / sort = `UCL_SteamAchievementAsset`
*   **`#if UNITY_STANDALONE_*`** 條件編譯

### A.2 欄位對照

| 程式欄位 | 編輯器顯示 | 型別 | 備註 |
|---|---|---|---|
| `m_Flag` | Flag | `bool` | runtime 快取 |
| `m_Inited` | — | `bool` (private) | 已從 Steam 同步過？ |

### A.3 重要 Method 摘要

*   **`GetStat()` → bool** — 第一次呼叫從 Steam 取，之後回快取；含 try-catch。
*   **`SetStat()` (no arg)** → 呼叫 `SetStat(m_Flag)`。
*   **`SetStat(bool flag)`** — `m_Flag = flag`：
    *   `flag=true` → `SteamUserStats.SetAchievement(ID)` → 成功時 `SteamUserStats.StoreStats()`。
    *   `flag=false` → `SteamUserStats.ClearAchievement(ID)`（測試）。
*   **OnGUI** → 提供 `SetStat` / `Get` 按鈕（手動測試）。

### A.4 與其他系統的互動

*   **`SteamUserStats`** (Steamworks SDK) — `GetAchievement` / `SetAchievement` / `ClearAchievement` / `StoreStats`。
*   **`RCG_AchievementAsset`** (EoV) — `m_SteamAchievement: UCL_SteamAchievementEntry` 引用此 Asset。
*   **`UCL_SteamAchievementEntry`**（同檔）— Asset Entry 包裝；預設 ID = `"Default"`。

### A.5 已知議題

*   `GetStat` 內成功路徑也用 `Debug.LogError` 印 log（檔案行 56 與 96），訊息看起來像錯誤但實際不是。
*   `m_Inited` 設定點不一致：line 53 的 `if (!m_Inited)` 包住整個取值，但 `m_Inited = true` 沒寫在裡面 — **每次呼叫都會重打一次 Steam API**（疑似 bug，留意）。
*   `SetStat` 與 `UCL_SteamUserStatAsset` 共用 OnGUI 按鈕命名（`SetStat` / `Get`），UI 一致但對 achievement 而言「Get」按鈕僅返回 bool，沒回寫資料。
