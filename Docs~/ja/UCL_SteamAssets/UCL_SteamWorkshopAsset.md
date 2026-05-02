---
title: Steam Workshop アップロード (UCL_SteamWorkshopAsset) 説明
description: Mod を Steam Workshop にアップロードする設定：タイトル、説明、可視性、変更履歴、依存 Mod
last_updated: 2026-05-02
target_audience: [Build_Engineer, Modder, AI_Agent]
---

# Steam Workshop アップロード

> プログラムクラス名：`UCL.SteamLib.UCL_SteamWorkshopAsset`

## 用途

**Mod を Steam Workshop にアップロードするための設定**。以下を結びつける：
*   `UCL_ModuleEntry` 1 つ（アップロードする Mod 本体）
*   Workshop 上のタイトル / 説明 / 変更履歴 / タグ / 可視性
*   依存 Mod（プレイヤーがこの Mod を subscribe したとき Steam が一緒にインストールする）

各アップロードは `SteamUGC` フロー（CreateItem / SubmitItemUpdate）を通り、返ってきた `PublishedFileId` を本 Asset に書き戻す。

`UCL_Asset<UCL_SteamWorkshopAsset>` を継承。

## エディタ上の見え方

```
UCL_SteamWorkshopAsset: <ID>
    Module                   ← アップロードする Mod (UCL_ModuleEntry)
    Title                    ← Workshop に表示されるタイトル
    Description              ← Mod 説明
    SteamAPILangCode  ▾      ← Mod 言語 (english / schinese / japanese...)
    EWorkshopFileType  ▾     ← ファイル種別 (通常は Community)
    Visibility  ▾            ← 可視性 (Public / FriendsOnly / Private)
    Tags                     ← タグリスト (プレイヤーの検索用)
    ChangeLog                ← 変更内容説明 (更新履歴に表示)
    Dependencies             ← 依存 Mod の PublishedFileId リスト
    PublishedFileId          ← Steam が返す Workshop ID (初回アップロード後に書き込まれる)
    [アップロードボタン群]   ← 多段階：CreateItem / StartItemUpdate / DeleteItem
```

## 主なフィールド

| エディタ表示 | 必須 | 説明 |
|---|---|---|
| **Module** | はい | アップロードする Mod 本体 (`UCL_ModuleEntry`)、対応するフォルダがパッケージ化されアップロードされる |
| **Title** | はい | Workshop タイトル、プレイヤーが Workshop で見る名前 |
| **Description** | はい | Mod 詳細説明（Markdown 一部対応） |
| **SteamAPILangCode** | はい | Mod 対象言語（Workshop 内の言語フィルタに影響）、デフォルト `english` |
| **EWorkshopFileType** | はい | ファイル種別、本ゲームでは通常 `k_EWorkshopFileTypeCommunity` |
| **Visibility** | はい | `Public` / `FriendsOnly` / `Private`、デフォルト `Public` |
| **Tags** | いいえ | カスタムタグリスト、プレイヤーの検索分類用 |
| **ChangeLog** | いいえ | 今回の更新内容、Workshop の更新履歴に表示 |
| **Dependencies** | いいえ | 依存 Mod の `PublishedFileId_t` リスト、subscribe 時に Steam が一緒にインストール |
| **PublishedFileId** | — | 初回 `CreateItem` 後に Steam が返す ID、**次回以降のアップロードで保持しなければならない** |

## 動作

### アップロードフロー (`UploadState`)

1. **`None`**：アイドル状態。
2. **`CreateItem`**：初回アップロード時に `SteamUGC.CreateItem` を呼び、`PublishedFileId` を取得して本 Asset に書き戻す。
3. **`StartItemUpdate`**：以降の更新はこちら — Title / Description / Tags / ChangeLog / Visibility 設定 + コンテンツフォルダのアップロード → `SteamUGC.SubmitItemUpdate`。
4. **`DeleteItem`**：公開済みの Workshop item を削除（不可逆）。

### Module との対応
`m_Module: UCL_ModuleEntry` はローカル Mod を参照する。アップロード時にその Mod のフォルダを Steam Workshop の content としてパッケージ化する。**Mod は事前に `UCL_ModuleService` に登録されている必要がある**。

### Dependencies の役割
`m_Dependencies` に列挙された `PublishedFileId_t` は Steam Workshop に一緒に記録される。**プレイヤーが本 Mod を subscribe すると、Steam client が依存 Mod を自動ダウンロードする**（mod プラットフォーム共通の仕組み）。

## 注意事項

*   **初回アップロードでは必ず先に CreateItem**：いきなり StartItemUpdate すると失敗（PublishedFileId がない）。
*   **PublishedFileId を Asset に書き込んだら必ず commit**：初回アップロード後、新 ID が JSON に書かれる。commit を忘れると次回また `CreateItem` で別の Workshop item が新規作成されてしまう（重複）。
*   **Visibility はいつでも変更可能**：先に Private で内部テストし、正式リリース時に Public に切り替える。
*   **Editor の AppId が正しいこと**：`steam_appid.txt` は本ゲームの AppID（1864830 or 2263320）でなければならず、480 ではダメ。さもないと SteamUGC は SpaceWar の Workshop に飛ぶ。
*   **タグは Steam 後台で先に承認が必要**：プレイヤーから見えるタグリストは partner site で設定されている。Asset 内に勝手に書いたタグでプレイヤーは検索できない。
*   **ファイルサイズ制限**：Steam Workshop の単一 item の上限は通常数百 MB、大型 mod は注意。

---

## 付録：プログラマ向けリファレンス

### A.1 クラス情報
*   **ファイルパス**：`CardGame/Assets/UCL/UCL_Modules/UCL_Steam/UCL_SteamworksScript/UCL_SteamworkScripts/SteamworksAssets/UCL_SteamWorkshopAsset.cs`
*   **継承元**：`UCL_Asset<UCL_SteamWorkshopAsset>`
*   **AssetGroup**：`AssetGroup.Steam` / sort = `UCL_SteamWorkshopAsset`
*   **`#if UNITY_STANDALONE_*`** 条件コンパイル

### A.2 フィールド対応

| コードフィールド | エディタ表示 | 型 | 備考 |
|---|---|---|---|
| `m_Module` | Module | `UCL_ModuleEntry` | アップロードする Mod |
| `m_Title` | Title | `string` | Workshop タイトル |
| `m_Description` | Description | `string` | 詳細説明 |
| `m_SteamAPILangCode` | SteamAPILangCode | `SteamAPILangCode` enum | デフォルト `english` |
| `m_EWorkshopFileType` | EWorkshopFileType | `EWorkshopFileType` enum | デフォルト `Community` |
| `m_Visibility` | Visibility | `ERemoteStoragePublishedFileVisibility` enum | デフォルト `Public` |
| `m_Tags` | Tags | `List<string>` | |
| `m_ChangeLog` | ChangeLog | `string` | |
| `m_Dependencies` | Dependencies | `List<PublishedFileId_t>` | |
| `m_PublishedFileId` | PublishedFileId | `PublishedFileId_t` | 初回アップロード後に書き込み |

### A.3 主要メソッド

`UCL_SteamWorkshopAsset` には完全なアップロードフロー実装が含まれる（async / coroutine が中心）。主なメソッド：
*   **`CreateItem`** — `SteamUGC.CreateItem(AppId, FileType)` → `m_PublishedFileId` 取得。
*   **`StartItemUpdate`** — `SteamUGC.StartItemUpdate(AppId, m_PublishedFileId)` → metadata + content folder 設定 → `SubmitItemUpdate`。
*   **`DeleteItem`** — `SteamUGC.DeleteItem(m_PublishedFileId)`。
*   **`UploadState` ステートマシン** — UI フロー駆動、重複アップロード衝突を回避。

### A.4 他システムとの連携

*   **`SteamUGC`** (Steamworks SDK) — Workshop メインインターフェース。
*   **`UCL_ModuleEntry` / `UCL_ModuleService`** — Mod システム。
*   **`UCL_SteamConfigAsset`** — アップロード対象 AppID のソース。
*   **`UCL_SteamWorkshopAssetEntry`**（同ファイル、line 490）— Asset Entry ラッパー。

### A.5 既知の問題

*   初回アップロード後、`m_PublishedFileId` を手動で Asset に save し直す必要がある。自動 save の callback は漏れやすく、「2 つ目の Workshop item が再作成される」ケースが発生する。
*   `m_EWorkshopFileType` のリストに古い / 非推奨の型（`k_EWorkshopFileTypeFirst` 等）が含まれる。通常プロジェクトは `Community` のみ使う。
*   `m_Tags` には Steam 後台の承認済みタグとの同期メカニズムがない — 誤字があってもエラーは出ず、プレイヤーは検索できない。
