
// RCG_AutoHeader
// to change the auto header please go to RCG_AutoHeader.cs
// Create time : 12/20 2024
using Cysharp.Threading.Tasks;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UCL.Core.JsonLib;
using UCL.Core.LocalizeLib;
using UCL.Core.Page;
using UCL.Core.UI;
using UCL.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UCL.SteamLib
{

    /// <summary>
    /// Steam相關設定
    /// </summary>
    [UCL.Core.ATTR.UCL_GroupIDAttribute(AssetGroup.Steam)]
    [UCL.Core.ATTR.UCL_Sort((int)AssetGroup.EditConfigType.UCL_SteamConfigAsset)]
    public class UCL_SteamConfigAsset : UCL_Asset<UCL_SteamConfigAsset>
    {
        /// <summary>
        /// AppId
        /// </summary>
        public string m_AppId = default;

        /// <summary>
        /// 抓取AppId
        /// </summary>
        public AppId_t AppId
        {
            get
            {
                if(uint.TryParse(m_AppId, out var result))
                {
                    return new AppId_t(result);
                }
                return new AppId_t();
            }
        }
        public static string GetConfigPath(string path) => Path.Combine(path, "steam_appid.txt");

        public static string ConfigPath => GetConfigPath(UCL.Core.FileLib.Lib.GameFolder);
        public UCL_SteamConfigAsset()
        {
            ID = "Asset ID";
        }
        /// <summary>
        /// 將AppId寫檔到steam_appid.txt
        /// </summary>
        public void ApplyAppId(string path)
        {
            //Debug.LogError($"ApplyAppId ConfigPath:{ConfigPath},m_AppId:{m_AppId}");
            File.WriteAllText(GetConfigPath(path), m_AppId);
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
                
                GUILayout.Label($"{UCL_LocalizeManager.Get("Preview")}({ID})[AppId:{m_AppId}]", UCL.Core.UI.UCL_GUIStyle.LabelStyle);
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
                
                if (GUILayout.Button(UCL_LocalizeManager.Get("Apply AppId"), UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                {
                    ApplyAppId(UCL.Core.FileLib.Lib.GameFolder);
                }
                GUILayout.Label($"ConfigPath:{ConfigPath}", UCL.Core.UI.UCL_GUIStyle.LabelStyle);

                if (GUILayout.Button(UCL_LocalizeManager.Get("SteamAPI Shutdown"), UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                {
                    SteamAPI.Shutdown();
                }
                //if (!Application.isPlaying)
                //{
                //    GUILayout.Label("!Application.isPlaying", UCL_GUIStyle.LabelStyle);
                //    return;
                //}
            }
        }
    }


    public class UCL_SteamConfigAssetEntry : UCL_AssetEntryDefault<UCL_SteamConfigAsset>
    {
        public const string DefaultID = "Default";

        public UCL_SteamConfigAssetEntry() { m_ID = DefaultID; }
        public UCL_SteamConfigAssetEntry(string iID) { m_ID = iID; }

    }
}
