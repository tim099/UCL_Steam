
// RCG_AutoHeader
// to change the auto header please go to RCG_AutoHeader.cs
// Create time : 12/21 2024
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Cysharp.Threading.Tasks;
using UCL.Core.UI;
using System.Threading;
using System.IO;
using UCL.Core;
using UCL.Core.JsonLib;
namespace UCL.SteamLib
{
    public class UCL_SteamworkService : UCL.Core.Game.UCL_GameService
    {
        public static UCL_SteamworkService Ins { get; protected set; }

        public List<PublishedFileId_t> m_SubscribedItems;
        public List<ItemInstallInfo> m_InstallItemsInfo = new List<ItemInstallInfo>();
        public List<string> m_Logs = new List<string>();
        //[SerializeField] private AppId_t m_AppID;
        public override async UniTask InitAsync(CancellationToken iToken)
        {
            Ins = this;
            var aToken = gameObject.GetCancellationTokenOnDestroy();
            if (SteamManager.Initialized)
            {
                await UniTask.WaitUntil(() => SteamManager.Initialized, cancellationToken: aToken);
            }
            UCL_SteamAPI.s_OnLog += OnLog;

            string name = SteamFriends.GetPersonaName();
            Debug.LogWarning($"SteamManager.Initialized name:{name}");
            
            m_SubscribedItems = UCL_SteamUGC.GetSubscribedItems();
            if (!m_SubscribedItems.IsNullOrEmpty())
            {
                foreach (var publishedFileID in m_SubscribedItems)
                {
                    var item = UCL_SteamUGC.GetItemInstallInfo(publishedFileID);
                    
                    if(item.success)
                    {
                        m_InstallItemsInfo.Add(item);
                        try
                        {
                            CheckAndInstallModule(item);
                        }
                        catch(System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }

                }
            }
        }
        /// <summary>
        /// 從Steam安裝模組
        /// </summary>
        private void CheckAndInstallModule(ItemInstallInfo item)
        {
            //string configPath = Path.Combine(item.pchFolder, UCL_ModulePath.ConfigFileName);
            ////Load config
            //if(!File.Exists(configPath))
            //{
            //    Debug.LogError($"{GetType().Name}.CheckAndInstallModule configPath:{configPath}, !File.Exists(configPath)");
            //    return;
            //}
            //{
            //    string json = File.ReadAllText(configPath);
            //    JsonData jsonData = JsonData.ParseJson(json);
            //    UCL_Module.Config config = new UCL_Module.Config();
            //    config.DeserializeFromJson(jsonData);
            //    Debug.LogWarning($"CheckAndInstallModule config:{config.AllFieldToString()}");
            //}
            //檢查是否安裝
            string id = Path.GetFileName(item.pchFolder);
            Debug.LogWarning($"Steam mod ID:{id}");
            //安裝到模組資料夾
            var moduleEntry = UCL_ModulePath.PersistantPath.GetModulesEntry(UCL_ModuleEditType.Runtime);
            string path = moduleEntry.GetModulePath(id);
            string itemInstallInfoPath = Path.Combine(path, "ItemInstallInfo.json");
            Debug.LogWarning($"GetModulePath:{path}");
            Debug.LogWarning($"itemInstallInfoPath:{itemInstallInfoPath}");
            bool needInstall = false;
            bool directoryExists = Directory.Exists(path);
            if (!directoryExists)//not installed, install
            {
                needInstall = true;
            }
            else//Check install version
            {
                if(!File.Exists(itemInstallInfoPath))//not install by steam
                {
                    needInstall = true;
                }
                else//compare version
                {
                    try
                    {
                        string json = File.ReadAllText(itemInstallInfoPath);
                        JsonData jsonData = JsonData.ParseJson(json);
                        ItemInstallInfo prev = new ItemInstallInfo();
                        prev.DeserializeFromJson(jsonData);
                        if (!prev.Equals(item))//version different
                        {
                            Debug.LogWarning($"{GetType().Name}.CheckAndInstallModule prev:{prev.AllFieldToString()}, item:{item.AllFieldToString()}");
                            needInstall = true;
                        }
                    }
                    catch(System.Exception ex)
                    {
                        Debug.LogException(ex);
                        needInstall = true;
                    }

                }
            }
            if(!needInstall)
            {
                return;
            }
            if (directoryExists)//Delete Old Version
            {
                Directory.Delete(path, true);
            }
            UCL.Core.FileLib.Lib.CopyDirectory(item.pchFolder, path);

            File.WriteAllText(itemInstallInfoPath, item.SerializeToJson().ToJsonBeautify());//Save ItemInstallInfo
            
            //Install
            //UCL_ModuleService.Ins.
        }
        virtual protected void OnDestroy()
        {
            UCL_SteamAPI.s_OnLog -= OnLog;
            SteamAPI.Shutdown();
        }
        virtual protected void OnLog(string log)
        {
            Debug.LogWarning(log);
            m_Logs.Add(log);
        }
        //private void OnCreateItem(CreateItemResult_t result, bool bIOFailure)
        //{
        //    Debug.LogError($"OnCreateItem bIOFailure:{bIOFailure},result.m_eResult:{result.m_eResult}" +
        //        $",result.m_nPublishedFileId:{result.m_nPublishedFileId},result.m_bUserNeedsToAcceptWorkshopLegalAgreement:{result.m_bUserNeedsToAcceptWorkshopLegalAgreement},");
        //}
    }
}
