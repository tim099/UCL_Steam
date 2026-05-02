---
title: Steam 設定 (UCL_SteamConfigAsset) 説明
description: Steamworks の接続先 AppID を設定する。Editor で Apply を押すと CardGame/steam_appid.txt を上書き
last_updated: 2026-05-02
target_audience: [Build_Engineer, AI_Agent, Designer]
---

# Steam 設定

> プログラムクラス名：`UCL.SteamLib.UCL_SteamConfigAsset`
>
> Steam AppID 全体のアーキテクチャ分析は 專案層級的 Steam AppID 架構文件 を参照。

## 用途

**Steamworks SDK がどの Steam app に接続するかを指定する。** 各 Asset は AppID 文字列を 1 つ持ち、`Apply AppId` ボタンを押すとその ID をプロジェクト直下の `CardGame/steam_appid.txt` に書き込む。Editor 再起動 / Domain Reload 後、SteamAPI が新しい ID で初期化される。

`UCL_Asset<UCL_SteamConfigAsset>` を継承。

本プロジェクトの現在のエントリ：

| Asset ID | AppID | 用途 |
|---|---|---|
| `Default` | 1864830 | 製品版 Emblem of Valor |
| `Demo` | 2263320 | Demo 版 |

> [!IMPORTANT]
> Editor が現在読んでいる `steam_appid.txt` は **`480`**（Steamworks SDK サンプルの SpaceWar）であり、本ゲームではない。Editor 内で実績 / 統計を検証したい場合は §「切り替え SOP」に従うこと。

## エディタ上の見え方

```
UCL_SteamConfigAsset: <ID>
    AppId    ← AppID（文字列、例 "1864830"）
    [ボタン] Apply AppId         ← CardGame/steam_appid.txt に書き込む
    ConfigPath: ...              ← 対象ファイルの絶対パスを表示
    [ボタン] SteamAPI Shutdown   ← 現在の SteamAPI 接続を閉じる
```

## 主なフィールド

| エディタ表示 | 必須 | 説明 |
|---|---|---|
| **AppId** | はい | Steam AppID 文字列（`uint.TryParse` で `AppId_t` に解析、失敗時は `Invalid`） |

## 動作

### 書き込みタイミング (`ApplyAppId`)
`Apply AppId` ボタン → `ApplyAppId(UCL.Core.FileLib.Lib.GameFolder)` を呼ぶ →
*   `GameFolder` = `Path.GetDirectoryName(Application.dataPath)`、Editor 内では = `<project>/CardGame/`。
*   書き込み先 = `<GameFolder>/steam_appid.txt`。
*   Build 時は `UCL_SteamPostBuildSetting` が AppID を build 出力先に書く（前提：その step の `IsEnable=True`、本プロジェクトでは現在 False）。

### Editor で AppID を切り替える SOP

1. Developer Page → `Steam` グループ → `UCL_SteamConfigAsset`
2. 切り替えたいエントリを選ぶ（`Default` / `Demo`）
3. `Apply AppId` を押す — `CardGame/steam_appid.txt` を上書き
4. `SteamAPI Shutdown` を押す — 現在の接続を閉じる
5. Domain Reload を起こす（任意の .cs を保存、または Editor 内 Ctrl+R）
6. `SteamManager.Awake` が新しい AppID で再度 `SteamAPI.Init()` を実行

## 注意事項

*   **`steam_appid.txt` は git 追跡対象**：Apply 後の変更は `git status` に出る。commit 前にどの ID を残したいか確認すること。
*   **対応するライセンスがないと切り替え不可**：あなたの Steam アカウントが対象 AppID のライセンス（partner site / 開発者付与）を所持している必要がある。
*   **Demo (2263320) と製品版 (1864830) は別 app**：実績 / 統計はそれぞれ独立しており互換性なし。
*   **頻繁に 480 に戻してテストする場合**：`Dev.json`（AppId=480）エントリを足しておくと、手動で steam_appid.txt を編集する誤字を避けられる。

---

## 付録：プログラマ向けリファレンス

### A.1 クラス情報
*   **ファイルパス**：`CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamConfigAsset.cs`
*   **継承元**：`UCL_Asset<UCL_SteamConfigAsset>`
*   **AssetGroup**：`AssetGroup.Steam` / sort = `UCL_SteamConfigAsset`
*   **`#if UNITY_STANDALONE_*`** 条件コンパイル、PC のみ有効

### A.2 フィールド対応

| コードフィールド | エディタ表示 | 型 | 備考 |
|---|---|---|---|
| `m_AppId` | AppId | `string` | uint.TryParse 失敗 → `AppId_t.Invalid` |

### A.3 主要メソッド

*   **`AppId` (property)** → `m_AppId` 文字列 → `AppId_t`、解析失敗時は `Invalid`。
*   **`ApplyAppId(string path)`** → `File.WriteAllText(GetConfigPath(path), m_AppId)`、AppID を `path/steam_appid.txt` に書く。
*   **`GetConfigPath(string path)` (static)** → `Path.Combine(path, "steam_appid.txt")`。
*   **`ConfigPath` (static property)** → デフォルト = `GameFolder/steam_appid.txt`。
*   **OnGUI** → `Apply AppId` と `SteamAPI Shutdown` のボタンを提供。

### A.4 他システムとの連携

*   **`SteamManager.Awake`** ([SteamManager.cs:99,122](../../../UCL_SteamworksScript/UCL_SteamworkScripts/Steamworks.NET/SteamManager.cs)) — `SteamAPI.RestartAppIfNecessary(AppId_t.Invalid)` + `SteamAPI.Init()`、`steam_appid.txt` に依存。
*   **`UCL_SteamPostBuildSetting`** — Build 後、対応する SteamConfig の AppId を build 出力先に書く（現状全 BuildAsset で disabled）。
*   **`UCL_SteamConfigAssetEntry`**（同ファイル）— Asset Entry ラッパー、デフォルト ID = `"Default"`。

### A.5 既知の問題

*   `m_AppId` が uint ではなく string なのは、空値と段階初期化（Asset 作成直後の ID は `"Asset ID"`）に対応するため。
*   `ApplyAppId` には try-catch がなく、書き込み権限がない場合は例外が投げられる（極めて稀）。
