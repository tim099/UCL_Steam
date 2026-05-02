---
title: Steam ユーザー統計 (UCL_SteamUserStatAsset) 説明
description: Steam バックエンド「統計と実績」内の単一統計 ID に対応。Get/Set/Alter 同期を提供
last_updated: 2026-05-02
target_audience: [Build_Engineer, AI_Agent, Designer]
---

# Steam ユーザー統計

> プログラムクラス名：`UCL.SteamLib.UCL_SteamUserStatAsset`
>
> EoV 側の実績 / 統計システム (`RCG_StatsAsset`) との橋渡し。

## 用途

**Steam バックエンドの「統計」1 件分の設定に対応する。** Steam Stats は累積可能な履歴値（撃破数、累計時間、累計スコア…）。使用前に**[Steamworks 後台](https://partner.steamgames.com/apps/) の「統計と実績」→「統計」で対応する ID を作成しておく必要がある**。本 Asset の `ID` フィールドは後台 ID と完全一致させること。

`UCL_Asset<UCL_SteamUserStatAsset>` を継承。

## エディタ上の見え方

```
UCL_SteamUserStatAsset: <ID = 後台統計 ID>
    ValueType  ▾ Int / Float / AVGRate
    m_Int       ← ValueType=Int のとき表示
    m_Float     ← ValueType=Float / AVGRate のとき表示
    [ボタン] SetStat   ← 現在値を Steam にアップロード
    [ボタン] Get       ← Steam から最新値を取得してローカルに同期
```

## 主なフィールド

| エディタ表示 | 必須 | 説明 |
|---|---|---|
| **ValueType** | はい | 数値型：`Int`（整数）/ `Float`（浮動小数）/ `AVGRate`（平均レート、例「1 時間あたりのスコア」） |
| **m_Int** | ValueType=Int | 整数値、Conditional 表示 |
| **m_Float** | ValueType=Float / AVGRate | 浮動小数値、Conditional 表示 |

## 動作

### 同期フロー
1. **`GetStatInt()`** — 初回呼び出しで `GetStat()` 経由で Steam から取得、以降はキャッシュした `m_Int` を返す。Steam 後台に同名 stat が存在することが前提。
2. **`SetStat(int)`** — `m_Int = val` → `SteamUserStats.SetStat(ID, m_Int)`、**`StoreStats` は呼ばれない**（Steam server には即時反映されない）。
3. **`AlterStat(int)`** — `GetStatInt() → m_Int += val → SteamUserStats.SetStat() → SteamUserStats.StoreStats()`、**`StoreStats` を呼ぶ**（server に反映、実績ポップアップを発動）。

### Float 版
`SetStat(float val)` は Int と同様で `m_Float` を使う。現状**対応する `AlterStat(float)` / `GetStatFloat()` は未実装**。

### 後台対応失敗
Steam 後台にこの ID がない場合、`GetStat` は `Debug.LogError` を出して false を返し、`SetStat` も失敗する（これが Simulation log 内 `Win_EnemyType_Elite` 等のエラーの根本原因）。

## 注意事項

*   **ID は Steam 後台と完全一致**：1 文字でも違うと stat は消え、自動同期もされない。
*   **Editor の AppId が正しいこと**：Editor の `steam_appid.txt = 480`（SpaceWar）だと、すべての stat が SpaceWar の後台に飛ぶ — 自分のゲームには反映されない。[`UCL_SteamConfigAsset`](UCL_SteamConfigAsset.md) で正式 AppID（1864830）に切り替えてあるか確認。
*   **`AlterStat` は自動で StoreStats を呼ぶが `SetStat` は呼ばない**：単に SetStat しただけだと値はローカルに残るだけで、次回 client 起動時に消える可能性。永続化したいなら AlterStat を使うか自分で `SteamUserStats.StoreStats` を呼ぶこと。
*   **Float と AVGRate は `m_Float` を共有**：AVGRate は Steam 後台では特殊型（自動でレート計算）だが、本データ上はただの float 扱い。
*   **`m_Inited` は private bool**：runtime cache、初回 GetStat 成功後 true になる。user 切替や reset 時には自動リセットされない。

---

## 付録：プログラマ向けリファレンス

### A.1 クラス情報
*   **ファイルパス**：`CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamUserStatAsset.cs`
*   **継承元**：`UCL_Asset<UCL_SteamUserStatAsset>`
*   **AssetGroup**：`AssetGroup.Steam` / sort = `UCL_SteamUserStatAsset`
*   **`#if UNITY_STANDALONE_*`** 条件コンパイル

### A.2 フィールド対応

| コードフィールド | エディタ表示 | 型 | 備考 |
|---|---|---|---|
| `m_ValueType` | ValueType | `EValueType` enum | `Int` / `Float` / `AVGRate` |
| `m_Int` | m_Int | `int` | `Conditional(Int)` |
| `m_Float` | m_Float | `float` | `Conditional(Float, AVGRate)` |
| `m_Inited` | — | `bool` (private) | runtime キャッシュフラグ |

### A.3 主要メソッド

*   **`GetStat()` (private)** → `SteamUserStats.GetStat(ID, out val)`、現状**`Int` のみ実装**（`Float` / `AVGRate` は未実装）。
*   **`GetStatInt()`** → 初回は `GetStat`、以降は `m_Int` を返す。
*   **`SetStat()` (no arg)** → ValueType に応じて `SetStat(int)` / `SetStat(float)` に分岐。
*   **`SetStat(int val)`** → `m_Int` を書く → `SteamUserStats.SetStat(ID, m_Int)`、**StoreStats は呼ばない**。
*   **`SetStat(float val)`** → `m_Float` を書く → `SteamUserStats.SetStat(ID, m_Float)`。
*   **`AlterStat(int val)`** → `GetStatInt() + val` → `SetStat` → `StoreStats`、try-catch あり。

### A.4 他システムとの連携

*   **`SteamUserStats`** (Steamworks SDK) — `GetStat` / `SetStat` / `StoreStats`。
*   **`RCG_StatsAsset`** (EoV 側) — `m_SteamUserStat` フィールドで本 Asset を参照。
*   **`UCL_SteamUserStatAssetEntry`**（同ファイル）— Asset Entry ラッパー、デフォルト ID = `"Default"`。

### A.5 既知の問題

*   `GetStat` 内の `switch (m_ValueType)` は `Int` のみ処理、**`Float` / `AVGRate` は完全未実装**（false を返すだけで LogWarning も出ない）。
*   `SetStat(int)` は `StoreStats` を呼ばないが `AlterStat` は呼ぶ — 違いを忘れて server push を忘れがち。
*   `m_Inited` は一度 true になると自動リセットされず、user logout/login 後にキャッシュが古くなる可能性あり。
