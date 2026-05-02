---
title: Steam 実績 (UCL_SteamAchievementAsset) 説明
description: Steam 後台の「実績」1 件分の設定に対応。達成時に server へ push し内蔵ポップアップを発動
last_updated: 2026-05-02
target_audience: [Build_Engineer, AI_Agent, Designer]
---

# Steam 実績

> プログラムクラス名：`UCL.SteamLib.UCL_SteamAchievementAsset`
>
> EoV 側の `RCG_AchievementAsset` と組み合わせて使う — RCG が「条件達成判定」、本 Asset が「Steam に push」を担当。

## 用途

**Steam 後台の「実績」1 件分の設定に対応する。** 使用前に**[Steamworks 後台](https://partner.steamgames.com/apps/) の「統計と実績」→「実績」で対応する ID を作成しておく必要がある**。本 Asset の `ID` は後台 ID と完全一致させること。達成時にシステムが `SetAchievement` + `StoreStats` を呼び、Steam client が内蔵の実績ポップアップを表示する。

`UCL_Asset<UCL_SteamAchievementAsset>` を継承。

## エディタ上の見え方

```
UCL_SteamAchievementAsset: <ID = 後台実績 ID>
    Flag        ← 取得済みかどうか（runtime キャッシュ）
    [ボタン] SetStat   ← 現在の Flag を Steam に push
    [ボタン] Get       ← Steam から最新状態を取得
```

## 主なフィールド

| エディタ表示 | 必須 | 説明 |
|---|---|---|
| **Flag** | — | 実績取得済みフラグ（runtime キャッシュ、`SteamUserStats.GetAchievement` の戻り値相当） |

## 動作

### 状態取得 (`GetStat`)
*   `m_Inited = false` → `SteamUserStats.GetAchievement(ID, out m_Flag)` で Steam から取得。
*   `m_Flag` を返す。

### 実績設定 (`SetStat(bool flag)`)
*   `flag = true`：
    *   `SteamUserStats.SetAchievement(ID)` → 取得済みとマーク。
    *   成功時 → `SteamUserStats.StoreStats()` で server に push、Steam ポップアップを発動。
*   `flag = false`（ほぼ使わない）：
    *   `SteamUserStats.ClearAchievement(ID)` → クリア（テスト用、本番では使わないはず）。
*   全体 try-catch で保護。

### RCG_AchievementAsset との関係
EoV 側の `RCG_AchievementAsset` は `m_SteamAchievement` フィールドで本 Asset を参照する。RCG 側の条件が満たされると `steamAchievement.GetData().SetStat(true)` を呼んで Steam に push する。

## 注意事項

*   **ID は Steam 後台と完全一致**：誤字があると実績は一切発動しない。
*   **Editor の AppId が正しいこと**：`steam_appid.txt = 480` だとすべての実績が SpaceWar に飛ぶ — 自分のゲームの後台には何も記録されない。
*   **`SetAchievement` は冪等**：同じ実績 ID を繰り返し呼んでもポップアップは複数回出ない（Steam 内蔵の重複排除）。
*   **後台で未公開の実績**：partner site で draft 状態の実績は、runtime で `SetAchievement` してもポップアップも記録もされない。後台で「Publish」が必要。
*   **`GetStat` は LogError で出力**：呼び出すたびに `GetStat ID:... m_Flag:..., success:...` を `LogError` 扱いで出す — log 上 success=true でも error として表示される。実装上の小さな不備。

---

## 付録：プログラマ向けリファレンス

### A.1 クラス情報
*   **ファイルパス**：`CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamAchievementAsset.cs`
*   **継承元**：`UCL_Asset<UCL_SteamAchievementAsset>`
*   **AssetGroup**：`AssetGroup.Steam` / sort = `UCL_SteamAchievementAsset`
*   **`#if UNITY_STANDALONE_*`** 条件コンパイル

### A.2 フィールド対応

| コードフィールド | エディタ表示 | 型 | 備考 |
|---|---|---|---|
| `m_Flag` | Flag | `bool` | runtime キャッシュ |
| `m_Inited` | — | `bool` (private) | Steam と同期済みか |

### A.3 主要メソッド

*   **`GetStat()` → bool** — 初回は Steam から取得、以降はキャッシュを返す。try-catch あり。
*   **`SetStat()` (no arg)** → `SetStat(m_Flag)` を呼ぶ。
*   **`SetStat(bool flag)`** — `m_Flag = flag`：
    *   `flag=true` → `SteamUserStats.SetAchievement(ID)` → 成功時 `SteamUserStats.StoreStats()`。
    *   `flag=false` → `SteamUserStats.ClearAchievement(ID)`（テスト用）。
*   **OnGUI** → `SetStat` / `Get` ボタンを提供（手動テスト用）。

### A.4 他システムとの連携

*   **`SteamUserStats`** (Steamworks SDK) — `GetAchievement` / `SetAchievement` / `ClearAchievement` / `StoreStats`。
*   **`RCG_AchievementAsset`** (EoV) — `m_SteamAchievement: UCL_SteamAchievementEntry` で本 Asset を参照。
*   **`UCL_SteamAchievementEntry`**（同ファイル）— Asset Entry ラッパー、デフォルト ID = `"Default"`。

### A.5 既知の問題

*   `GetStat` 内の成功パスでも `Debug.LogError` でログ出力（ファイル 56, 96 行）、エラーに見えるが実際はそうではない。
*   `m_Inited` の設定箇所が一致していない：53 行の `if (!m_Inited)` で値取得を囲っているが、`m_Inited = true` がその中に書かれていない — **毎回 Steam API を再度叩いている**（バグ疑い、要注意）。
*   `SetStat` と `UCL_SteamUserStatAsset` で OnGUI ボタン名（`SetStat` / `Get`）が共通、UI は統一されているが achievement 側では「Get」ボタンは bool を返すだけで何も書き戻さない。
