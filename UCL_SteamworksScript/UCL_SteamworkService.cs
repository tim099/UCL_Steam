
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
using System.Linq;
namespace UCL.SteamLib
{
    public class UCL_SteamworkService : UCL.Core.Game.UCL_GameService
    {
        public static UCL_SteamworkService Ins { get; protected set; }

        public List<PublishedFileId_t> m_SubscribedItems;
        public List<ItemInstallInfo> m_InstallItemsInfo = new List<ItemInstallInfo>();
        public List<string> m_Logs = new List<string>();
        /// <summary>
        /// 新訂閱的物品
        /// </summary>
        public List<string> m_NewItems = new();
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
            var installedMods = UCL_SteamUtil.InstalledMods;
            
            HashSet<ulong> subscribedItems = new ();
            if (!m_SubscribedItems.IsNullOrEmpty())//有訂閱的模組
            {
                foreach (var publishedFileID in m_SubscribedItems)
                {
                    ulong fileId = publishedFileID.m_PublishedFileId;
                    subscribedItems.Add(fileId);//紀錄訂閱的物品id
                    var item = UCL_SteamUGC.GetItemInstallInfo(publishedFileID);
                    if (!installedMods.Contains(fileId))//新訂閱的物品 記錄起來
                    {
                        m_NewItems.Add(fileId.ToString());
                    }
                    if(item.success)
                    {
                        m_InstallItemsInfo.Add(item);
                        try
                        {
                            CheckAndInstallModule(publishedFileID, item);
                        }
                        catch(System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                    else
                    {
                        Debug.LogError($"SubscribedItem:{publishedFileID}, !item.success, pchFolder:{item.pchFolder}");
                    }

                }
            }
            
            if (!installedMods.IsNullOrEmpty())//Uninstall unsubscribedItems
            {
                foreach(var itemId in installedMods.ToList())//移除取消訂閱的物品 因為會在foreach內修改到原本的Set所以複製一份
                {
                    //Debug.LogError($" installedMods itemId:{itemId},subscribedItems:{subscribedItems.ConcatToString()}");
                    if (!subscribedItems.Contains(itemId))//已經取消訂閱
                    {
                        //Debug.LogError($"itemId:{itemId}, uninstall");//移除模組
                        UnInstallModule(itemId);
                    }
                }
            }
            UCL_SteamAchievements.Init();

            //Steamworks.UserStatsReceived_t
            //使用者已登入
            //UCL_SteamAchievements.RequestStats();


            //Steamworks.SteamUserStats.GetAchievement
            //if
        }
        public override void Save()
        {
            base.Save();
        }
        /// <summary>
        /// 從Steam安裝模組
        /// </summary>
        private void CheckAndInstallModule(PublishedFileId_t publishedFileID, ItemInstallInfo item)
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

            UCL_SteamUtil.AddInstalledMod(publishedFileID.m_PublishedFileId);
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
        /// <summary>
        /// 解除安裝Steam模組
        /// </summary>
        /// <param name="modID"></param>
        private void UnInstallModule(ulong moduleID)
        {
            string id = moduleID.ToString();
            var moduleEntry = UCL_ModulePath.PersistantPath.GetModulesEntry(UCL_ModuleEditType.Runtime);
            string path = moduleEntry.GetModulePath(id);
            //Debug.LogError($"UnInstallModule:{id},path:{path}");

            UCL_SteamUtil.RemoveInstalledMod(moduleID);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
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
