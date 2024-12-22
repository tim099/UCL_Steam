
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
    public static class AssetGroup
    {
        public const string Steam = "Steam";
        public enum EditConfigType : int
        {
            UCL_SteamConfigAsset = 1,
            UCL_SteamWorkshopAsset,
        }
    }


    [UCL.Core.ATTR.UCL_GroupIDAttribute(AssetGroup.Steam)]
    [UCL.Core.ATTR.UCL_Sort((int)AssetGroup.EditConfigType.UCL_SteamWorkshopAsset)]
    public class UCL_SteamWorkshopAsset : UCL_Asset<UCL_SteamWorkshopAsset>
    {
        public enum UploadState
        {
            /// <summary>
            /// 目前無任何上傳動作
            /// </summary>
            None,
            /// <summary>
            /// 正在創建物品
            /// </summary>
            CreateItem,
            /// <summary>
            /// 正在刪除物品
            /// </summary>
            DeleteItem,

            /// <summary>
            /// 上傳工作坊物品
            /// </summary>
            StartItemUpdate,
        }
        /// <summary>
        /// 要上傳的模組
        /// </summary>
        public UCL_ModuleEntry m_Module = new UCL_ModuleEntry();
        /// <summary>
        /// Workshop上的模組名稱
        /// </summary>
        public string m_Title = "Workshop Item Title";

        /// <summary>
        /// Workshop上的模組描述
        /// </summary>
        public string m_Description = "Mod Description";

        /// <summary>
        /// 模組語言(Steam)
        /// </summary>
        public SteamAPILangCode m_SteamAPILangCode = SteamAPILangCode.english;

        /// <summary>
        /// 檔案類型
        /// </summary>
        public EWorkshopFileType m_EWorkshopFileType = EWorkshopFileType.k_EWorkshopFileTypeCommunity;
        /// <summary>
        /// 物品的可見度
        /// </summary>
        public ERemoteStoragePublishedFileVisibility m_Visibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic;
        
        /// <summary>
        /// Workshop上的模組標籤
        /// </summary>
        public List<string> m_Tags = new List<string>();

        /// <summary>
        /// 表示更新的變更說明。這個說明會顯示在工作坊項目的更新日誌中，讓用戶了解這次更新的內容
        /// </summary>
        public string m_ChangeLog = "Change Log";

        /// <summary>
        /// 相依模組
        /// </summary>
        public List<PublishedFileId_t> m_Dependencies = new();

        /// <summary>
        /// 透過SteamUGC.CreateItem生成的工作坊物品ID
        /// (要存回到Module內!!)
        /// </summary>
        //[UCL.Core.ATTR.UCL_HideOnGUI]
        public PublishedFileId_t m_PublishedFileId = default;






        #region OnGUI

        /// <summary>
        /// 編輯器用 移除相依性
        /// </summary>
        [UCL.Core.ATTR.UCL_HideOnGUI]
        private ulong m_RemoveDependencyFileId = default;

        private List<string> m_Logs = new List<string>();

        private UploadState m_UploadState = UploadState.None;
        /// <summary>
        /// 顯示上傳進度用
        /// </summary>
        private string m_ProgressLog = string.Empty;
        /// <summary>
        /// 上傳的Handle
        /// </summary>
        private UGCUpdateHandle_t? m_Handle = null;
        #endregion

        public UCL_SteamWorkshopAsset()
        {
            ID = "Asset ID";
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
                if (!Application.isPlaying)
                {
                    GUILayout.Label("!Application.isPlaying", UCL_GUIStyle.LabelStyle);
                    return;
                }
                if (!SteamManager.Initialized)
                {
                    GUILayout.Label("!SteamManager.Initialized", UCL_GUIStyle.LabelStyle);
                    return;
                }
                try
                {
                    if (m_UploadState != UploadState.None)//上傳中
                    {
                        GUILayout.Label($"UploadState:{m_UploadState}", UCL_GUIStyle.LabelStyle);
                        if (m_Handle.HasValue)
                        {//顯示下載進度
                            ulong punBytesProcessed;
                            ulong punBytesTotal;
                            var status = SteamUGC.GetItemUpdateProgress(m_Handle.Value, out punBytesProcessed, out punBytesTotal);
                            float percentage = 0;
                            if (punBytesTotal > 0)
                            {
                                percentage = 100f * (float)punBytesProcessed / punBytesTotal;
                            }
                            m_ProgressLog = $"status:{status}, {percentage}%, punBytesProcessed:{punBytesProcessed}, punBytesTotal:{punBytesTotal}";
                        }

                        if (!string.IsNullOrEmpty(m_ProgressLog))
                        {
                            GUILayout.Label(m_ProgressLog, UCL_GUIStyle.LabelStyle);
                        }
                        return;
                    }

                    if (m_PublishedFileId == PublishedFileId_t.Invalid)//尚未生成工作坊物品 只能做創建操作
                    {
                        if (GUILayout.Button(UCL_LocalizeManager.Get("Create workshop item"), UCL_GUIStyle.ButtonStyle))//生成工作坊物品
                        {
                            CreateItem().Forget();
                        }
                        return;
                    }


                    GUILayout.Label($"PublishedFileId:{m_PublishedFileId.m_PublishedFileId}", UCL_GUIStyle.LabelStyle);

                    {//上傳物品
                        if (GUILayout.Button(UCL_LocalizeManager.Get("Update item to workshop"), UCL_GUIStyle.ButtonStyle))//生成工作坊物品
                        {
                            StartItemUpdate().Forget();
                        }
                    }




                    if (GUILayout.Button(UCL_LocalizeManager.Get("Open workshop URL"), UCL_GUIStyle.ButtonStyle))//開啟工作坊物品連結
                    {
                        string url = UCL_SteamUGC.GetItemURL(m_PublishedFileId);
                        Application.OpenURL(url);
                    }

                    using(var scopeH = new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(UCL_LocalizeManager.Get("Clear PublishedFileId"), UCL_GUIStyle.ButtonStyle))
                        {
                            m_PublishedFileId.m_PublishedFileId = 0;
                        }

                        if (GUILayout.Button(UCL_LocalizeManager.Get("Delete item"), UCL_GUIStyle.GetButtonStyle(Color.red)))//刪除工作坊物品
                        {
                            UCL.Core.Page.UCL_OptionPage.ConfirmDelete($"{m_Title}(PublishedFileId:{m_PublishedFileId.m_PublishedFileId})", () =>
                            {
                                DeleteItem().Forget();
                            });
                        }
                    }
                    using (var scopeH = new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(UCL_LocalizeManager.Get("Remove Dependency"), UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                        {
                            UCL_SteamUGC.RemoveDependency(new PublishedFileId_t(m_RemoveDependencyFileId), m_PublishedFileId).Forget();
                        }
                        m_RemoveDependencyFileId = (ulong)UCL_GUILayout.DrawObjectData(m_RemoveDependencyFileId, iDataDic.GetSubDic(nameof(m_RemoveDependencyFileId)),"",true);
                    }

                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
                finally
                {

                    if (GUILayout.Button(UCL_LocalizeManager.Get("Clear Logs"), UCL_GUIStyle.ButtonStyle))
                    {
                        m_Logs.Clear();
                    }
                    foreach (var log in m_Logs)//顯示Log
                    {
                        GUILayout.Label(log, UCL_GUIStyle.LabelStyle);
                    }
                }
            }



        }
        protected void OnLog(string log)
        {
            Debug.LogWarning(log);
            m_Logs.Add(log);
        }
        /// <summary>
        /// 生成工作坊物品
        /// https://partner.steamgames.com/doc/features/workshop/implementation
        /// </summary>
        /// <returns></returns>
        protected async UniTask CreateItem()
        {
            if(m_UploadState != UploadState.None)
            {
                OnLog($"CreateItem m_UploadState:{m_UploadState}");
                return;
            }
            m_UploadState = UploadState.CreateItem;
            m_ProgressLog = string.Empty;
            try
            {
                var appID = SteamUtils.GetAppID();

                OnLog($"appID:{appID}, workshopFileType:{m_EWorkshopFileType}");

                var result = await UCL_SteamUGC.CreateItem(appID, m_EWorkshopFileType);

                OnLog($"Result:{result.pCallback.m_eResult},IOFailure:{result.IOFailure}");

                if (result.pCallback.m_eResult != EResult.k_EResultOK)//失敗 中斷後續流程
                {
                    OnLog($"CreateItem Fail, Result:{result.pCallback.m_eResult},IOFailure:{result.IOFailure}");
                    //m_UploadState = UploadState.None; 設定改到finally (try 區塊內發生什麼情況，finally 區塊內的程式碼都會被執行)
                    return;
                }
                m_PublishedFileId = result.pCallback.m_nPublishedFileId;
            }
            catch(System.Exception ex)
            {
                OnLog($"CreateItem Exception:{ex}");
                Debug.LogException(ex);
            }
            finally
            {
                m_UploadState = UploadState.None;
            }
        }
        protected async UniTask DeleteItem()
        {
            if (m_UploadState != UploadState.None)
            {
                OnLog($"DeleteItem m_UploadState:{m_UploadState}");
                return;
            }
            m_UploadState = UploadState.DeleteItem;
            m_ProgressLog = string.Empty;
            try
            {
                var appID = SteamUtils.GetAppID();

                //OnLog($"DeleteItem m_PublishedFileId:{m_PublishedFileId.m_PublishedFileId}");
                var success = await UCL_SteamUGC.DeleteItem(m_PublishedFileId);

                OnLog($"DeleteItem m_PublishedFileId:{m_PublishedFileId.m_PublishedFileId}, success:{success}");
                m_PublishedFileId = default;//clear
            }
            catch (System.Exception ex)
            {
                OnLog($"DeleteItem Exception:{ex}");
                Debug.LogException(ex);
            }
            finally
            {
                m_UploadState = UploadState.None;
            }
        }
        /// <summary>
        /// 生成工作坊物品
        /// https://partner.steamgames.com/doc/features/workshop/implementation
        /// </summary>
        /// <returns></returns>
        protected async UniTask StartItemUpdate()
        {
            if (m_UploadState != UploadState.None)
            {
                OnLog($"CreateItem m_UploadState:{m_UploadState}");
                return;
            }
            m_UploadState = UploadState.StartItemUpdate;
            m_ProgressLog = string.Empty;
            try
            {
                var appID = SteamUtils.GetAppID();

                //https://partner.steamgames.com/doc/api/ISteamUGC#StartItemUpdate
                var handle = SteamUGC.StartItemUpdate(appID, m_PublishedFileId);
                m_Handle = handle;

                bool success = SteamUGC.SetItemTitle(handle, m_Title);
                if (!success)//失敗 中斷後續流程
                {
                    OnLog($"SteamUGC.SetItemTitle Fail, Title:{m_Title}");
                    return;
                }
                success = SteamUGC.SetItemDescription(handle, m_Description);//設定物品的新說明
                if (!success)//失敗 中斷後續流程
                {
                    OnLog($"SteamUGC.SetItemDescription Fail, Description:{m_Description}");
                    return;
                }

                success = SteamUGC.SetItemUpdateLanguage(handle, m_SteamAPILangCode.ToString());
                if (!success) OnLog($"SteamUGC.SetItemUpdateLanguage Fail, m_SteamAPILangCode:{m_SteamAPILangCode}");//失敗

                success = SteamUGC.SetItemVisibility(handle, m_Visibility);
                if (!success) OnLog($"SteamUGC.SetItemVisibility Fail, visibility:{m_Visibility}");//失敗

                if (!m_Tags.IsNullOrEmpty())//有標籤
                {
                    success = SteamUGC.SetItemTags(handle, m_Tags.ToArray());
                    if (!success) OnLog($"SteamUGC.SetItemTags Fail, m_Tags:{m_Tags.ConcatToString()}");//失敗
                }


                var module = m_Module.Module;
                var config = module.m_Config;
                success = SteamUGC.AddItemKeyValueTag(handle, "Version", config.m_Version);
                if (!success) OnLog($"SteamUGC.AddItemKeyValueTag Fail, m_Version:{config.m_Version}");//失敗

                {
                    string path = module.ModuleEntry.RootFolder;
                    success = SteamUGC.SetItemContent(handle, path);
                    if (!success) OnLog($"SteamUGC.SetItemContent Fail, path:{path}");//失敗
                }
                {
                    string logoPath = module.ModuleEntry.LogoPath;
                    if (File.Exists(logoPath))
                    {
                        success = SteamUGC.SetItemPreview(handle, logoPath);
                        if (!success) OnLog($"SteamUGC.SetItemPreview Fail, logoPath:{logoPath}");//失敗
                    }
                }

                if(!m_Dependencies.IsNullOrEmpty())//有相依模組
                {
                    //SteamUGC.RemoveDependency

                    foreach (var dependency in m_Dependencies)
                    {
                        await UCL_SteamUGC.AddDependency(dependency, m_PublishedFileId);
                    }

                    //success = await UCL_SteamUGC.AddDependency(m_PublishedFileId, m_Dependencies);
                    //if (!success) OnLogError($"SteamUGC.AddDependency Fail," +
                    //    $" m_Dependencies:{m_Dependencies.ConcatToString(id => id.m_PublishedFileId.ToString())}");//失敗
                    //foreach (var dependency in m_Dependencies) {
                    //    //添加相依性
                    //    SteamUGC.AddDependency(dependency, m_PublishedFileId);
                    //}
                    //設定相依模組
                    //success = SteamUGC.AddDependency
                }

                {
                    //上傳!!
                    var result = await UCL_SteamUGC.SubmitItemUpdate(handle, m_ChangeLog);
                    OnLog($"SteamUGC.SubmitItemUpdate IOFailure:{result.IOFailure},result:{result.result.m_eResult}" +
                        $",UserNeedsToAcceptWorkshopLegalAgreement:{result.result.m_bUserNeedsToAcceptWorkshopLegalAgreement}");
                    if(result.result.m_eResult == EResult.k_EResultOK)
                    {
                        m_PublishedFileId = result.result.m_nPublishedFileId;//上傳成功 紀錄m_PublishedFileId
                    }
                }
            }
            catch (System.Exception ex)
            {
                OnLog($"CreateItem Exception:{ex}");
                Debug.LogException(ex);
            }
            finally
            {
                m_Handle = null;
                m_UploadState = UploadState.None;
            }
        }

    }


    [System.Serializable]
    public class UCL_SteamWorkshopAssetEntry : UCL_AssetEntryDefault<UCL_SteamWorkshopAsset>
    {
        public const string DefaultID = "Default";

        public UCL_SteamWorkshopAssetEntry() { m_ID = DefaultID; }
        public UCL_SteamWorkshopAssetEntry(string iID) { m_ID = iID; }

    }
}
