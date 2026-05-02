using System.IO;
using UCL.Core;
using UnityEngine;

namespace UCL.SteamLib
{
    /// <summary>
    /// [職責] 將 UCL_Steam 自家的 docs 模組（"ucl_steam:" prefix）註冊到 <see cref="UCL_DocsModuleRegistry"/>。
    /// [物理意義] 讓 [HelpURL("ucl_steam:Docs~/{lang}/UCL_SteamAssets/...")] 在 Editor 連到本地 UCL_Steam/Docs~/，
    ///           Build 連到 GitHub UCL_Steam repo 的 blob 連結。與 UCL_Core / EoV 走同一條 Registry 流程。
    /// [數值影響] 不直接影響遊戲狀態；僅決定 [HelpURL("ucl_steam:...")] 的解析行為。
    /// </summary>
    public static class UCL_SteamDocsBootstrap
    {
        // [常數] UCL_Steam 的 prefix、雲端 URL、manifest 名稱；集中此處避免散落於程式各處。
        private const string PREFIX = "ucl_steam";
        private const string BUILD_BASE_URL = "https://github.com/tim099/UCL_Steam/blob/Dev/";
        private const string MANIFEST_NAME = "UCL_Steam_LocalizedDocsManifest";
        private const string DOCS_SUBFOLDER = "Docs~";
        private const string DISPLAY_NAME = "UCL_Steam";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        private static void Register()
        {
            // 區塊職責：建立 UCL_Steam 的 docs 模組描述並註冊。
            // 物理意義：Editor 端透過 UCL_SteamEditorPath.SteamPath 自動定位模組根；Build 端拼接 GitHub Dev 分支 URL。
            UCL_DocsModuleRegistry.Register(new UCL_DocsModule
            {
                Prefix = PREFIX,
                DisplayName = DISPLAY_NAME,
                DocsSubfolder = DOCS_SUBFOLDER,
                ManifestResourceName = MANIFEST_NAME,
                BuildBaseUrl = BUILD_BASE_URL,
#if UNITY_EDITOR
                ResolveBaseProvider = () => UCL_SteamEditorPath.SteamPath,
                // [Resources 寫入位置] UCL_Steam 自家 Resources/，與 UCL_Core 同模式（manifest 隨模組走）。
                ResourcesFolderProvider = () =>
                {
                    string aRoot = UCL_SteamEditorPath.SteamPath;
                    return string.IsNullOrEmpty(aRoot) ? null : Path.Combine(aRoot, "Resources");
                },
#endif
            });
        }
    }
}
