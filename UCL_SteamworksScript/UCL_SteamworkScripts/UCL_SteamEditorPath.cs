#if UNITY_EDITOR
namespace UCL.SteamLib
{
    /// <summary>
    /// [職責] 自動定位 UCL_Steam 模組在專案中的根目錄路徑（Editor 端）。
    /// [物理意義] 用於將相對路徑轉換為絕對路徑，支援 UCL_Steam 在不同專案中的移植。
    ///           主要供 UCL_DocsModuleRegistry 註冊 "ucl_steam:" prefix 與 manifest 寫入位置使用。
    /// [數值影響] 不影響任何遊戲狀態；僅作為 Editor 端 docs 解析與 manifest 產生的根路徑來源。
    /// </summary>
    public static class UCL_SteamEditorPath
    {
        // [快取] 第一次計算後保存，避免反覆 AssetDatabase.FindAssets。
        private static string s_SteamPath = null;

        // [常數] UCL_Steam 模組資料夾名稱；用於從 Asset 路徑切出根目錄。
        private const string MODULE_FOLDER_NAME = "UCL_Steam";

        // [常數] 用來定位模組的「指紋」腳本檔名（不含副檔名）；該檔名須在整個專案內唯一且必定位於 UCL_Steam 之下。
        // [物理意義] AssetDatabase.FindAssets 透過此檔案的 GUID 反查路徑，再抓 "UCL_Steam" 字串切出模組根。
        private const string ANCHOR_SCRIPT_NAME = "UCL_SteamAPI";

        /// <summary>
        /// [職責] 取得 UCL_Steam 模組在專案中的根目錄（"Assets/.../UCL_Steam"）。
        /// [計算邏輯] 透過 AssetDatabase.FindAssets 找到 ANCHOR_SCRIPT_NAME 的 .cs 檔，從其路徑切出 "UCL_Steam" 之前的字串。
        /// [使用情境] 僅供 Editor 端使用；Build 端不應呼叫。
        /// </summary>
        public static string SteamPath
        {
            get
            {
                if (!string.IsNullOrEmpty(s_SteamPath)) return s_SteamPath;

                // 區塊職責：透過 AssetDatabase 反查 anchor 腳本路徑。
                // 物理意義：anchor 腳本必定位於 UCL_Steam 之下，藉此切出模組根。
                // 數值影響：找不到時 s_SteamPath 維持 null，呼叫端的 manifest / resolver 對應會失效。
                string[] aGuids = UnityEditor.AssetDatabase.FindAssets($"{ANCHOR_SCRIPT_NAME} t:Script");
                foreach (string aGuid in aGuids)
                {
                    string aPath = UnityEditor.AssetDatabase.GUIDToAssetPath(aGuid);
                    if (aPath.Contains(MODULE_FOLDER_NAME))
                    {
                        int aIndex = aPath.IndexOf(MODULE_FOLDER_NAME);
                        s_SteamPath = aPath.Substring(0, aIndex + MODULE_FOLDER_NAME.Length);
                        break;
                    }
                }
                return s_SteamPath;
            }
        }
    }
}
#endif
