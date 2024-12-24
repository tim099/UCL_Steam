
// RCG_AutoHeader
// to change the auto header please go to RCG_AutoHeader.cs
// Create time : 12/20 2024
using System.Collections;
using System.Collections.Generic;
using UCL.Core.JsonLib;
using UCL.Core.LocalizeLib;
using UCL.Core.StringExtensionMethods;
using UnityEngine;
using UCL.Core.EditorLib.Page;
using UCL.Core;
using UCL.Core.UI;
using Cysharp.Threading.Tasks;
using System.IO;
using UnityEngine.Networking;
using Steamworks;

namespace UCL.SteamLib
{
    public class UCL_SteamAPI
    {
        public static event System.Action<string> s_OnLog;
        public static void OnLog(string msg)
        {
            s_OnLog?.Invoke(msg);
        }
    }
    /// <summary>
    /// 對Steamworks相關API的封裝
    /// </summary>
    public static class UCL_SteamUGC
    {

        /// <summary>
        /// 建立工作坊物品
        /// </summary>
        /// <param name="nConsumerAppId"></param>
        /// <param name="eFileType"></param>
        /// <returns></returns>
        public static async UniTask<(CreateItemResult_t pCallback, bool IOFailure)> CreateItem
            (AppId_t nConsumerAppId, EWorkshopFileType eFileType)
        {
            UniTaskCompletionSource<(CreateItemResult_t pCallback, bool IOFailure)> ucs = new();

            //建立工作坊物品的CallBack
            void OnCreateItemResult(CreateItemResult_t pCallback, bool bIOFailure)
            {
                UCL_SteamAPI.OnLog($"CreateItem Result:{pCallback.m_eResult},PublishedFileId:{pCallback.m_nPublishedFileId}" +
                    $", UserNeedsToAcceptWorkshopLegalAgreement:{pCallback.m_bUserNeedsToAcceptWorkshopLegalAgreement}");
                ucs.TrySetResult((pCallback, bIOFailure));
            }
            //建立工作坊物品
            using (var OnCreateItemResultCallResult = CallResult<CreateItemResult_t>.Create(OnCreateItemResult))
            {
                SteamAPICall_t handle = SteamUGC.CreateItem(nConsumerAppId, eFileType);
                OnCreateItemResultCallResult.Set(handle);

                return await ucs.Task;
            }
        }

        /// <summary>
        /// 刪除工作坊物品
        /// </summary>
        /// <param name="nConsumerAppId"></param>
        /// <param name="eFileType"></param>
        /// <returns></returns>
        public static async UniTask<bool> DeleteItem(PublishedFileId_t nPublishedFileID)
        {
            UniTaskCompletionSource<(DeleteItemResult_t result, bool IOFailure)> ucs = new();

            //建立工作坊物品的CallBack
            void OnCreateItemResult(DeleteItemResult_t pCallback, bool bIOFailure)
            {
                UCL_SteamAPI.OnLog($"DeleteItem Result:{pCallback.m_eResult}, bIOFailure{bIOFailure}");
                ucs.TrySetResult((pCallback, bIOFailure));
            }
            //建立工作坊物品
            using (var OnCreateItemResultCallResult = CallResult<DeleteItemResult_t>.Create(OnCreateItemResult))
            {
                SteamAPICall_t handle = SteamUGC.DeleteItem(nPublishedFileID);
                OnCreateItemResultCallResult.Set(handle);
                var result = await ucs.Task;
                return result.result.m_eResult == EResult.k_EResultOK;
            }
        }

        /// <summary>
        /// 抓取所有訂閱的工作坊物品
        /// </summary>
        /// <returns></returns>
        public static List<PublishedFileId_t> GetSubscribedItems()
        {
            uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
            PublishedFileId_t[] subscribedItems = new PublishedFileId_t[numSubscribedItems];
            uint retrievedItemsCount =  SteamUGC.GetSubscribedItems(subscribedItems, numSubscribedItems);
            List<PublishedFileId_t> results = new List<PublishedFileId_t>();
            for (int i = 0; i < retrievedItemsCount; i++)
            {
                results.Add(subscribedItems[i]);
            }
            UCL_SteamAPI.OnLog($"GetSubscribedItems: {subscribedItems.ConcatToString(item => item.m_PublishedFileId.ToString())}");
            return results;
        }
        /// <summary>
        /// 抓取物品的工作坊連結
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public static string GetItemURL(PublishedFileId_t fileId)
        {
            return $"https://steamcommunity.com/sharedfiles/filedetails/?id={fileId}";
        }
        /// <summary>
        /// 抓取安裝工作坊物品的狀態
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="cchFolderSize"></param>
        /// <returns></returns>
        public static ItemInstallInfo GetItemInstallInfo(PublishedFileId_t fileId, uint cchFolderSize = 260)
        {
            var info = new ItemInstallInfo();
            info.success = SteamUGC.GetItemInstallInfo(fileId, out info.punSizeOnDisk, out info.pchFolder, cchFolderSize, out info.punTimeStamp);
            if(!info.success) UCL_SteamAPI.OnLog($"GetItemInstallInfo fail fileId: {fileId.m_PublishedFileId}");
            return info;
        }
        /// <summary>
        /// 建立工作坊物品
        /// </summary>
        /// <param name="nConsumerAppId"></param>
        /// <param name="eFileType"></param>
        /// <returns></returns>
        public static async UniTask<(SubmitItemUpdateResult_t result, bool IOFailure)> SubmitItemUpdate
            (UGCUpdateHandle_t handle, string pchChangeNote)
        {
            UniTaskCompletionSource<(SubmitItemUpdateResult_t, bool)> ucs = new();

            //建立工作坊物品的CallBack
            void OnSubmitItemUpdate(SubmitItemUpdateResult_t pCallback, bool bIOFailure)
            {
                UCL_SteamAPI.OnLog($"Result:{pCallback.m_eResult}, PublishedFileId:{pCallback.m_nPublishedFileId}" +
                    $",UserNeedsToAcceptWorkshopLegalAgreement:{pCallback.m_bUserNeedsToAcceptWorkshopLegalAgreement}");
                ucs.TrySetResult((pCallback, bIOFailure));
            }
            //建立工作坊物品
            using (var OnSubmitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(OnSubmitItemUpdate))
            {
                OnSubmitItemUpdateResult.Set(SteamUGC.SubmitItemUpdate(handle, pchChangeNote));

                return await ucs.Task;
            }
        }
        /// <summary>
        /// 建立工作坊物品相依性
        /// </summary>
        /// <param name="nConsumerAppId"></param>
        /// <param name="eFileType"></param>
        /// <returns></returns>
        //public static async UniTask<bool> AddDependency
        //    (PublishedFileId_t publishedFileId, List<PublishedFileId_t> dependencies)
        //{
        //    int completeCount = 0;
        //    int successCount = 0;
        //    //建立工作坊物品的CallBack
        //    void OnAddUGCDependency(AddUGCDependencyResult_t pCallback, bool bIOFailure)
        //    {
        //        if (pCallback.m_eResult == EResult.k_EResultOK)
        //        {
        //            ++successCount;
        //        }
        //        ++completeCount;
        //        OnLogError($"OnAddUGCDependency Result:{pCallback.m_eResult}, completeCount:{completeCount}, successCount:{successCount}");
        //    }
        //    //建立工作坊物品
        //    using (var OnSubmitItemUpdateResult = CallResult<AddUGCDependencyResult_t>.Create(OnAddUGCDependency))
        //    {
        //        foreach (var dependency in dependencies)
        //        {
        //            //這個做法可能有問題 需要測試
        //            OnSubmitItemUpdateResult.Set(SteamUGC.AddDependency(dependency, publishedFileId));
        //        }


        //        await UniTask.WaitUntil(() => completeCount >= dependencies.Count);
        //    }
        //    return completeCount == successCount;//All success
        //}

        /// <summary>
        /// 建立工作坊物品相依性
        /// </summary>
        /// <param name="nConsumerAppId"></param>
        /// <param name="eFileType"></param>
        /// <returns></returns>
        public static async UniTask<(AddUGCDependencyResult_t, bool)> AddDependency(PublishedFileId_t publishedFileId, PublishedFileId_t dependency)
        {
            UniTaskCompletionSource<(AddUGCDependencyResult_t, bool)> ucs = new();
            //建立工作坊物品的CallBack
            void OnAddDependency(AddUGCDependencyResult_t pCallback, bool bIOFailure)
            {
                ucs.TrySetResult((pCallback, bIOFailure));
                UCL_SteamAPI.OnLog($"OnAddUGCDependency Result:{pCallback.m_eResult}, dependency:{dependency.m_PublishedFileId.ToString()}");
            }
            //建立工作坊物品
            using (var OnSubmitItemUpdateResult = CallResult<AddUGCDependencyResult_t>.Create(OnAddDependency))
            {
                OnSubmitItemUpdateResult.Set(SteamUGC.AddDependency(dependency, publishedFileId));


                return await ucs.Task;
            }
        }
        /// <summary>
        /// 移除工作坊物品相依性
        /// </summary>
        /// <param name="nConsumerAppId"></param>
        /// <param name="eFileType"></param>
        /// <returns></returns>
        public static async UniTask<(RemoveUGCDependencyResult_t, bool)> RemoveDependency(PublishedFileId_t publishedFileId, PublishedFileId_t dependency)
        {
            UniTaskCompletionSource<(RemoveUGCDependencyResult_t, bool)> ucs = new();
            //建立工作坊物品的CallBack
            void OnRemoveDependency(RemoveUGCDependencyResult_t pCallback, bool bIOFailure)
            {
                ucs.TrySetResult((pCallback, bIOFailure));
                UCL_SteamAPI.OnLog($"RemoveDependency Result:{pCallback.m_eResult}, dependency:{dependency.m_PublishedFileId.ToString()}");
            }
            //建立工作坊物品
            using (var OnSubmitItemUpdateResult = CallResult<RemoveUGCDependencyResult_t>.Create(OnRemoveDependency))
            {
                OnSubmitItemUpdateResult.Set(SteamUGC.RemoveDependency(dependency, publishedFileId));


                return await ucs.Task;
            }
        }

        /// <summary>
        /// 設定這次物品更新的名稱與說明語言
        /// https://partner.steamgames.com/doc/api/ISteamUGC#SetItemUpdateLanguage
        /// </summary>
        /// <param name="nConsumerAppId"></param>
        /// <param name="eFileType"></param>
        /// <returns></returns>
        public static bool SetItemUpdateLanguage(UGCUpdateHandle_t handle, SteamAPILangCode pchLanguage)
        {
            return SteamUGC.SetItemUpdateLanguage(handle, pchLanguage.ToString());
        }
        /// <summary>
        /// 設定物品的新名稱
        /// </summary>
        /// <param name="nConsumerAppId"></param>
        /// <param name="eFileType"></param>
        /// <returns></returns>
        //public static async UniTask<(CreateItemResult_t pCallback, bool IOFailure)> StartItemUpdate
        //    (AppId_t nConsumerAppId, PublishedFileId_t nPublishedFileID)
        //{
        //    UniTaskCompletionSource<(CreateItemResult_t pCallback, bool IOFailure)> ucs = new();

        //    //建立工作坊物品的CallBack
        //    void OnCreateItemResult(CreateItemResult_t pCallback, bool bIOFailure)
        //    {
        //        OnLogError($"[{CreateItemResult_t.k_iCallback} - CreateItemResult] - {pCallback.m_eResult} -- {pCallback.m_nPublishedFileId}" +
        //            $" -- {pCallback.m_bUserNeedsToAcceptWorkshopLegalAgreement}");
        //        ucs.TrySetResult((pCallback, bIOFailure));
        //    }
        //    //建立工作坊物品
        //    using (var OnCreateItemResultCallResult = CallResult<CreateItemResult_t>.Create(OnCreateItemResult))
        //    {
        //        OnCreateItemResultCallResult.Set(SteamUGC.StartItemUpdate(nConsumerAppId, nPublishedFileID));

        //        return await ucs.Task;
        //    }
        //}
    }

    public static class UCL_SteamUserStats
    {
        /// <summary>
        /// 從Steam後端擷取使用者統計和成就資料
        /// </summary>
        /// <returns></returns>
        public static async UniTask<(UserStatsReceived_t result, bool bIOFailure)> RequestUserStats()
        {
            return await RequestUserStats(SteamUser.GetSteamID());//Get UserStats
        }


        /// <summary>
        /// 從Steam後端擷取使用者統計和成就資料
        /// </summary>
        /// <returns></returns>
        public static async UniTask<(UserStatsReceived_t result, bool bIOFailure)> RequestUserStats(CSteamID steamIDUser)
        {
            UniTaskCompletionSource<(UserStatsReceived_t result, bool bIOFailure)> ucs = new();
            //建立工作坊物品的CallBack
            void OnRequestUserStats(UserStatsReceived_t result, bool bIOFailure)
            {
                ucs.TrySetResult((result, bIOFailure));
                UCL_SteamAPI.OnLog($"GetNumberOfCurrentPlayers Result:{result.m_eResult},bIOFailure:{bIOFailure}");
            }
            //建立工作坊物品
            using (var OnSubmitItemUpdateResult = CallResult<UserStatsReceived_t>.Create(OnRequestUserStats))
            {
                OnSubmitItemUpdateResult.Set(SteamUserStats.RequestUserStats(steamIDUser));

                return await ucs.Task;
            }
        }

        /// <summary>
        /// 當前玩家數量
        /// </summary>
        /// <returns></returns>
        public static async UniTask<(NumberOfCurrentPlayers_t result, bool bIOFailure)> GetNumberOfCurrentPlayers()
        {
            UniTaskCompletionSource<(NumberOfCurrentPlayers_t result, bool bIOFailure)> ucs = new();
            //建立工作坊物品的CallBack
            void OnAddUGCDependency(NumberOfCurrentPlayers_t result, bool bIOFailure)
            {
                ucs.TrySetResult((result, bIOFailure));
                UCL_SteamAPI.OnLog($"GetNumberOfCurrentPlayers Result:{result.m_cPlayers},bIOFailure:{bIOFailure}");
            }
            //建立工作坊物品
            using (var OnSubmitItemUpdateResult = CallResult<NumberOfCurrentPlayers_t>.Create(OnAddUGCDependency))
            {
                OnSubmitItemUpdateResult.Set(SteamUserStats.GetNumberOfCurrentPlayers());

                return await ucs.Task;
            }
        }
    }


    public class ItemInstallInfo : UnityJsonSerializable
    {
        public ulong punSizeOnDisk;
        public string pchFolder;
        //public uint cchFolderSize;
        public uint punTimeStamp;

        public bool success;

        #region PA
        public override int GetHashCode()
        {
            return punSizeOnDisk.GetHashCode() ^ pchFolder.GetHashCode() ^ punTimeStamp.GetHashCode();
        }
        public override bool Equals(object iObj)
        {
            return Equals(iObj as ItemInstallInfo);
        }

        public virtual bool Equals(ItemInstallInfo iObj)
        {
            if (iObj == null) return false;
            return iObj.punSizeOnDisk == punSizeOnDisk && iObj.pchFolder == pchFolder && iObj.punTimeStamp == punTimeStamp;
        }
        #endregion
    }
}
