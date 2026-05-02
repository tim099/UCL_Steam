---
title: Steam 成就 (UCL_SteamAchievementAsset) 说明
description: 对应 Steam 后台「成就」单笔设定；达成时推送 server 并触发内建弹窗
last_updated: 2026-05-02
target_audience: [Build_Engineer, AI_Agent, Designer]
---

# Steam 成就

> 程式类别名称：`UCL.SteamLib.UCL_SteamAchievementAsset`
>
> 与 EoV 端的 `RCG_AchievementAsset` 配合 — RCG 那层管「条件达成判断」，这层管「推送 Steam」。

## 用途

**对应 Steam 后台一笔「成就」的设定**。使用前**必须先在 [Steamworks 后台](https://partner.steamgames.com/apps/) 的「统计与成就」→「成就」中创建对应 ID**，本 Asset 的 `ID` 必须与后台 ID 完全一致。达成后系统呼叫 `SetAchievement` + `StoreStats`，Steam client 会跳出内建的成就弹窗。

继承自 `UCL_Asset<UCL_SteamAchievementAsset>`。

## 编辑器中的样貌

```
UCL_SteamAchievementAsset: <ID = 后台成就 ID>
    Flag        ← 是否已获得（runtime 快取）
    [按钮] SetStat   ← 推送目前 Flag 到 Steam
    [按钮] Get       ← 从 Steam 取最新状态
```

## 主要栏位

| 编辑器显示 | 必填 | 说明 |
|---|---|---|
| **Flag** | — | 是否已获得此成就（runtime 快取，等同 `SteamUserStats.GetAchievement` 的回传） |

## 行为说明

### 取得状态 (`GetStat`)
*   `m_Inited = false` → `SteamUserStats.GetAchievement(ID, out m_Flag)` 从 Steam 拉值。
*   返回 `m_Flag`。

### 设定成就 (`SetStat(bool flag)`)
*   `flag = true`：
    *   `SteamUserStats.SetAchievement(ID)` → 标记已获得。
    *   成功 → `SteamUserStats.StoreStats()` 推送 server，触发 Steam 弹窗。
*   `flag = false`（少用）：
    *   `SteamUserStats.ClearAchievement(ID)` → 清除（测试用，正式应该不会用到）。
*   全程 try-catch 保护。

### 与 RCG_AchievementAsset 的关系
EoV 端的 `RCG_AchievementAsset` 透过 `m_SteamAchievement` 栏位引用本 Asset；当 RCG 条件满足，呼叫 `steamAchievement.GetData().SetStat(true)` 推 Steam。

## 注意事项

*   **ID 必须与 Steam 后台一致**：拼错字成就完全不会触发。
*   **Editor 的 AppId 要对**：`steam_appid.txt = 480` 时所有成就会打到 SpaceWar — 完全不会推到你的游戏后台。
*   **`SetAchievement` 是幂等的**：重复呼叫同一个成就 ID 不会跳出多次弹窗（Steam 已内建去重）。
*   **后台尚未发布的成就**：在 partner site 进行中（draft）的成就，runtime 呼叫 `SetAchievement` 不会跳弹窗也不会记录；要先在后台「Publish」。
*   **`GetStat` 会 LogError 而非 LogInfo**：每次呼叫都会印 `GetStat ID:... m_Flag:..., success:...`，这在程式内被当作 `LogError` 处理 — log 中即使 success=true 也会显示为 error，是程式设计上的小瑕疵。

---

## 附录：程式人员参考 (Programmer Reference)

### A.1 类别资讯
*   **档案路径**：`CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamAchievementAsset.cs`
*   **继承自**：`UCL_Asset<UCL_SteamAchievementAsset>`
*   **AssetGroup**：`AssetGroup.Steam` / sort = `UCL_SteamAchievementAsset`
*   **`#if UNITY_STANDALONE_*`** 条件编译

### A.2 栏位对照

| 程式栏位 | 编辑器显示 | 型别 | 备注 |
|---|---|---|---|
| `m_Flag` | Flag | `bool` | runtime 快取 |
| `m_Inited` | — | `bool` (private) | 已从 Steam 同步过？ |

### A.3 重要 Method 摘要

*   **`GetStat()` → bool** — 第一次呼叫从 Steam 取，之后回快取；含 try-catch。
*   **`SetStat()` (no arg)** → 呼叫 `SetStat(m_Flag)`。
*   **`SetStat(bool flag)`** — `m_Flag = flag`：
    *   `flag=true` → `SteamUserStats.SetAchievement(ID)` → 成功时 `SteamUserStats.StoreStats()`。
    *   `flag=false` → `SteamUserStats.ClearAchievement(ID)`（测试）。
*   **OnGUI** → 提供 `SetStat` / `Get` 按钮（手动测试）。

### A.4 与其他系统的互动

*   **`SteamUserStats`** (Steamworks SDK) — `GetAchievement` / `SetAchievement` / `ClearAchievement` / `StoreStats`。
*   **`RCG_AchievementAsset`** (EoV) — `m_SteamAchievement: UCL_SteamAchievementEntry` 引用此 Asset。
*   **`UCL_SteamAchievementEntry`**（同档）— Asset Entry 包装；预设 ID = `"Default"`。

### A.5 已知议题

*   `GetStat` 内成功路径也用 `Debug.LogError` 印 log（档案行 56 与 96），讯息看起来像错误但实际不是。
*   `m_Inited` 设定点不一致：line 53 的 `if (!m_Inited)` 包住整个取值，但 `m_Inited = true` 没写在里面 — **每次呼叫都会重打一次 Steam API**（疑似 bug，留意）。
*   `SetStat` 与 `UCL_SteamUserStatAsset` 共用 OnGUI 按钮命名（`SetStat` / `Get`），UI 一致但对 achievement 而言「Get」按钮仅返回 bool，没回写资料。
