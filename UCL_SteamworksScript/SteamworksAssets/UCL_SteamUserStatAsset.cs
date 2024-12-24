
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
    [UCL.Core.ATTR.UCL_Sort((int)AssetGroup.EditConfigType.UCL_SteamUserStatAsset)]
    public class UCL_SteamUserStatAsset : UCL_Asset<UCL_SteamUserStatAsset>
    {
        public enum EValueType
        {
            Int = 0,
            Float,
            /// <summary>
            /// 這種類型的統計數據用於計算平均速率，例如每分鐘的擊殺數或每小時的得分
            /// </summary>
            AVGRate,
        }
        public EValueType m_ValueType = EValueType.Int;

        [UCL.Core.PA.Conditional(nameof(m_ValueType), false, EValueType.Int)]
        public int m_Int;

        [UCL.Core.PA.Conditional(nameof(m_ValueType), false, EValueType.Float, EValueType.AVGRate)]
        public float m_Float;
        /// <summary>
        /// 初始值需要從Steam獲取
        /// 透過SteamUserStats.GetStat("NumGames", out m_NumGamesStat);
        /// </summary>
        private bool m_Inited = false;

        public UCL_SteamUserStatAsset()
        {
            ID = "UserStat ID";
        }


        /// <summary>
        /// 從Steam同步Stat
        /// </summary>
        private bool GetStat()
        {
            bool success = false;
            switch (m_ValueType)
            {
                case EValueType.Int:
                    {
                        if(SteamUserStats.GetStat(ID, out int val))
                        {
                            m_Int = val;
                            success = true;
                        }
                        else
                        {
                            Debug.LogError($"{GetType().Name}.GetStat, ID:{ID}, m_Int:{m_Int}, fail");
                        }
                        break;
                    }
            }
            return success;
        }
        public int GetStatInt()
        {
            if(m_ValueType != EValueType.Int)
            {
                Debug.LogError($"{GetType().Name}.GetStateInt(), ID:{ID}, m_ValueType:{m_ValueType}");
                return 0;
            }
            if (!m_Inited)//尚未初始化 要從Steam同步資訊
            {
                bool success = GetStat();
                if (!success)
                {
                    Debug.LogError($"{GetType().Name}.GetStateInt(), ID:{ID}, !success");
                }
                else
                {
                    m_Inited = true;
                    Debug.LogError($"{GetType().Name}.GetStateInt(), ID:{ID}, m_Int:{m_Int}");//Test
                }
            }
            return m_Int;
        }
        public void SetStat()
        {
            switch (m_ValueType)
            {
                case EValueType.Int:
                    {
                        SetStat(m_Int);
                        break;
                    }
                case EValueType.Float:
                case EValueType.AVGRate:
                    {
                        SetStat(m_Float);
                        break;
                    }
            }
        }
        public void SetStat(int val)
        {
            if (m_ValueType != EValueType.Int)
            {
                Debug.LogError($"{GetType().Name}.SetState(int), ID:{ID}, val:{val}, m_ValueType:{m_ValueType}");
                return;
            }
            m_Int = val;
            bool success = SteamUserStats.SetStat(ID, m_Int);
            if (!success)
            {
                Debug.LogError($"{GetType().Name}.SetState(), ID:{ID}, val:{val}, m_ValueType:{m_ValueType}, fail");
            }
        }
        public void SetStat(float val)
        {
            if (m_ValueType == EValueType.Int)
            {
                Debug.LogError($"{GetType().Name}.SetState(float), ID:{ID}, val:{val}, m_ValueType:{m_ValueType}");
                return;
            }
            m_Float = val;
            bool success = SteamUserStats.SetStat(ID, m_Float);
            if (!success)
            {
                Debug.LogError($"{GetType().Name}.SetState(), ID:{ID}, val:{val}, m_ValueType:{m_ValueType}, fail");
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
                GUILayout.Label($"ValueType:{m_ValueType}", UCL_GUIStyle.LabelStyle);
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


    public class UCL_SteamUserStatAssetEntry : UCL_AssetEntryDefault<UCL_SteamUserStatAsset>
    {
        public const string DefaultID = "Default";

        public UCL_SteamUserStatAssetEntry() { m_ID = DefaultID; }
        public UCL_SteamUserStatAssetEntry(string iID) { m_ID = iID; }

    }
}