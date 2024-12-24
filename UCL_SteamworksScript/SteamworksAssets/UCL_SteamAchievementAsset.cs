
// RCG_AutoHeader
// to change the auto header please go to RCG_AutoHeader.cs
// Create time : 12/24 2024
using Cysharp.Threading.Tasks;
using Steamworks;
using UCL.Core.JsonLib;
using UCL.Core.LocalizeLib;
using UCL.Core.Page;
using UCL.Core.UI;
using UCL.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UCL.SteamLib
{

    /// <summary>
    /// SteamUserStats相關設定
    /// 需要先在後台的"統計與成就" => "統計" 新增統計中創建對應的ID
    /// </summary>
    [UCL.Core.ATTR.UCL_GroupIDAttribute(AssetGroup.Steam)]
    [UCL.Core.ATTR.UCL_Sort((int)AssetGroup.EditConfigType.UCL_SteamAchievementAsset)]
    public class UCL_SteamAchievementAsset : UCL_Asset<UCL_SteamAchievementAsset>
    {
        /// <summary>
        /// 是否已經獲得此成就
        /// </summary>
        public bool m_Flag = false;

        /// <summary>
        /// 初始值需要從Steam獲取
        /// 透過SteamUserStats.GetStat("NumGames", out m_NumGamesStat);
        /// </summary>
        private bool m_Inited = false;

        public UCL_SteamAchievementAsset()
        {
            ID = "UserStat ID";
        }


        /// <summary>
        /// 從Steam同步Stat
        /// </summary>
        private bool GetStat()
        {
            if (!m_Inited)
            {
                bool success = SteamUserStats.GetAchievement(ID, out m_Flag);
                Debug.LogError($"GetStat ID:{ID} m_Flag:{m_Flag},success:{success}");
            }
            return m_Flag;
        }
        /// <summary>
        /// 設置成就
        /// </summary>
        public void SetStat()
        {
            SetStat(m_Flag);
        }
        /// <summary>
        /// 設定成就
        /// </summary>
        /// <param name="flag"></param>
        public void SetStat(bool flag)
        {
            m_Flag = flag;
            if (m_Flag)
            {
                bool success = SteamUserStats.SetAchievement(ID);
                Debug.LogError($"SetStat ID:{ID} m_Flag:{m_Flag},success:{success}");
            }
            else//清除成就(測試用 實際應該不會用到)
            {
                bool success = SteamUserStats.ClearAchievement(ID);
                Debug.LogError($"SetStat ID:{ID} m_Flag:{m_Flag},success:{success}");
            }
        }
        /// <summary>
        /// Preview(OnGUI)
        /// </summary>
        /// <param name="iIsShowEditButton">Show edit button in preview window?</param>
        override public void Preview(UCL.Core.UCL_ObjectDictionary iDataDic, bool iIsShowEditButton = false)
        {
            //GUILayout.BeginHorizontal();
            using (var aScope = new GUILayout.VerticalScope("box", GUILayout.ExpandWidth(false)))
            {
                GUILayout.Label($"{UCL_LocalizeManager.Get("Preview")}({ID})", UCL.Core.UI.UCL_GUIStyle.LabelStyle);
                GUILayout.Label($"Flag:{m_Flag}", UCL_GUIStyle.LabelStyle);
                if (iIsShowEditButton)
                {
                    if (GUILayout.Button(UCL_LocalizeManager.Get("Edit"), UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                    {
                        UCL_CommonEditPage.Create(this);
                    }
                }
            }
            //GUILayout.EndHorizontal();
        }

        public override void OnGUI(UCL_ObjectDictionary iDataDic)
        {
            using (var scope = new GUILayout.VerticalScope("box"))//, GUILayout.Width(500)
            {
                UCL.Core.UI.UCL_GUILayout.DrawObjectData(this, iDataDic, string.Empty, true, LocalizeFieldName);

                if (GUILayout.Button(UCL_LocalizeManager.Get("SetStat"), UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                {
                    SetStat();
                }
                if (GUILayout.Button(UCL_LocalizeManager.Get("Get"), UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                {
                    GetStat();
                }
            }
        }
    }


    public class UCL_SteamAchievementEntry : UCL_AssetEntryDefault<UCL_SteamAchievementAsset>
    {
        public const string DefaultID = "Default";

        public UCL_SteamAchievementEntry() { m_ID = DefaultID; }
        public UCL_SteamAchievementEntry(string iID) { m_ID = iID; }

    }
}
