---
title: Steam 使用者统计 (UCL_SteamUserStatAsset) 说明
description: 对应 Steam 后台「统计与成就」中的单一统计 ID；提供 Get/Set/Alter 同步
last_updated: 2026-05-02
target_audience: [Build_Engineer, AI_Agent, Designer]
---

# Steam 使用者统计

> 程式类别名称：`UCL.SteamLib.UCL_SteamUserStatAsset`
>
> 与 EoV 端的成就 / 统计系统 (`RCG_StatsAsset`) 之间的桥梁。

## 用途

**对应 Steam 后台一笔「统计」的设定**。Steam Stats 是可累加的历史数值（击杀数、累计时间、总得分⋯）。使用前**必须先在 [Steamworks 后台](https://partner.steamgames.com/apps/) 的「统计与成就」→「统计」中创建对应 ID**，这个 Asset 的 `ID` 栏位必须与后台 ID 完全一致。

继承自 `UCL_Asset<UCL_SteamUserStatAsset>`。

## 编辑器中的样貌

```
UCL_SteamUserStatAsset: <ID = 后台统计 ID>
    ValueType  ▾ Int / Float / AVGRate
    m_Int       ← ValueType=Int 时显示
    m_Float     ← ValueType=Float / AVGRate 时显示
    [按钮] SetStat   ← 把目前值上传 Steam
    [按钮] Get       ← 从 Steam 取最新值同步到本机
```

## 主要栏位

| 编辑器显示 | 必填 | 说明 |
|---|---|---|
| **ValueType** | 是 | 数值类型：`Int`（整数）/ `Float`（浮点）/ `AVGRate`（平均速率，例如「每小时得分」） |
| **m_Int** | ValueType=Int | 整数值；Conditional 显示 |
| **m_Float** | ValueType=Float / AVGRate | 浮点值；Conditional 显示 |

## 行为说明

### 同步流程
1. **`GetStatInt()`** — 第一次呼叫时走 `GetStat()` 从 Steam 拉值；之后回传快取的 `m_Int`。需要 Steam 后台已正确建立同名 stat 才会成功。
2. **`SetStat(int)`** — `m_Int = val` → `SteamUserStats.SetStat(ID, m_Int)`，**未呼叫 `StoreStats`**（不会立即推上 Steam server）。
3. **`AlterStat(int)`** — `GetStatInt() → m_Int += val → SteamUserStats.SetStat() → SteamUserStats.StoreStats()`，**有呼叫 `StoreStats`**（推上 server 并触发成就弹窗）。

### Float 版本
`SetStat(float val)` 与 `Int` 类似，但用 `m_Float`；目前**没有对应的 `AlterStat(float)` / `GetStatFloat()`**。

### 后台对应失败
若 Steam 后台没建这个 ID，`GetStat` 会 `Debug.LogError` 并回传 false；`SetStat` 也会 fail（这就是先前 Simulation log 内 `Win_EnemyType_Elite` 等错误的根因）。

## 注意事项

*   **ID 必须与 Steam 后台一致**：写错一个字 stat 就消失，不会自动同步。
*   **Editor 的 AppId 要对**：如果 Editor 的 `steam_appid.txt = 480`（SpaceWar），所有 stat 都会打到 SpaceWar 的后台 stats，不会打到你的游戏 — 确认 [`UCL_SteamConfigAsset`](UCL_SteamConfigAsset.md) 已切换到正式 AppID（1864830）。
*   **`AlterStat` 自动 StoreStats，但 `SetStat` 不会**：直接 SetStat 后值留在本地，下次 client 重启可能消失。要永久记录请用 AlterStat 或自己呼叫 `SteamUserStats.StoreStats`。
*   **Float 与 AVGRate 共用 `m_Float`**：AVGRate 在 Steam 后台是特殊类型（会自动计算速率），但本资料对外只是 float。
*   **`m_Inited` 是 private bool**：runtime cache，第一次成功 GetStat 后设 true；切换 user 或 reset 不会自动重置。

---

## 附录：程式人员参考 (Programmer Reference)

### A.1 类别资讯
*   **档案路径**：`CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamUserStatAsset.cs`
*   **继承自**：`UCL_Asset<UCL_SteamUserStatAsset>`
*   **AssetGroup**：`AssetGroup.Steam` / sort = `UCL_SteamUserStatAsset`
*   **`#if UNITY_STANDALONE_*`** 条件编译

### A.2 栏位对照

| 程式栏位 | 编辑器显示 | 型别 | 备注 |
|---|---|---|---|
| `m_ValueType` | ValueType | `EValueType` enum | `Int` / `Float` / `AVGRate` |
| `m_Int` | m_Int | `int` | `Conditional(Int)` |
| `m_Float` | m_Float | `float` | `Conditional(Float, AVGRate)` |
| `m_Inited` | — | `bool` (private) | runtime 快取旗标 |

### A.3 重要 Method 摘要

*   **`GetStat()` (private)** → `SteamUserStats.GetStat(ID, out val)`，目前**只实作 `Int`** case（`Float` / `AVGRate` 没做）。
*   **`GetStatInt()`** → 第一次走 `GetStat`；之后回 `m_Int`。
*   **`SetStat()` (no arg)** → 依 ValueType 分流到 `SetStat(int)` 或 `SetStat(float)`。
*   **`SetStat(int val)`** → 写 `m_Int` → `SteamUserStats.SetStat(ID, m_Int)`，**不呼叫 StoreStats**。
*   **`SetStat(float val)`** → 写 `m_Float` → `SteamUserStats.SetStat(ID, m_Float)`。
*   **`AlterStat(int val)`** → `GetStatInt() + val` → `SetStat` → `StoreStats`，含 try-catch。

### A.4 与其他系统的互动

*   **`SteamUserStats`** (Steamworks SDK) — `GetStat` / `SetStat` / `StoreStats`。
*   **`RCG_StatsAsset`** (EoV 端) — 透过 `m_SteamUserStat` 栏位引用此 Asset。
*   **`UCL_SteamUserStatAssetEntry`**（同档）— Asset Entry 包装；预设 ID = `"Default"`。

### A.5 已知议题

*   `GetStat` 内 `switch (m_ValueType)` 只处理 `Int`，**`Float` / `AVGRate` 完全没实作**（会直接回 false 但无 LogWarning）。
*   `SetStat(int)` 不呼叫 `StoreStats`，而 `AlterStat` 会 — 容易让使用者忘记推 server 的差异。
*   `m_Inited` 一旦设为 true 不会自动重置，user logout/login 后快取可能不准。
