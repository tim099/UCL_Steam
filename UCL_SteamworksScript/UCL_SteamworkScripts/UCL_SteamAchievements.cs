
// RCG_AutoHeader
// to change the auto header please go to RCG_AutoHeader.cs
// Create time : 12/24 2024
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
    /// Steam成就相關功能
    /// </summary>
    public static class UCL_SteamAchievements
    {
        public static bool Initialized
        {
            get
            {
                return s_Initialized;
            }
        }
        private static bool s_Initialized = false;
        /// <summary>
        /// return value of RequestStats
        /// </summary>
        private static bool s_Stats = false;
        public static void Init()
        {
            if (s_Initialized)
            {
                return;
            }
            s_Initialized = true;

            s_Stats = RequestStats();
        }

        /// <summary>
        /// 這個方法基本上只是將發向 ISteamUserStats::RequestCurrentStats 的呼叫包裝了起來。
        /// 這是一個不同步呼叫，用來向 Steam 要求目前使用者的統計與成就。 
        /// 您必須先進行此呼叫，才能設定統計或成就。 第一次呼叫這個方法是在建構函式裡面進行。 
        /// 往後需要檢查更新統計與成就時，可再呼叫此方法。
        /// https://partner.steamgames.com/doc/features/achievements/ach_guide?l=tchinese
        /// </summary>
        /// <returns>代表呼叫是否成功的 Bool 如果呼叫失敗，可能是因為Steam 尚未初始化。 
        /// 進行呼叫時請確定 Steam 用戶端為開啟狀態，而且已先呼叫了SteamAPI_Init</returns>
        public static bool RequestStats()
        {
            try
            {
                if (!Steamworks.SteamUser.BLoggedOn())//使用者是否已登入？  若沒有，則無法取得統計
                {
                    return false;
                }
                return Steamworks.SteamUserStats.RequestCurrentStats();
            }
            catch(System.Exception ex)
            {
                Debug.LogException(ex);
            }
            return false;
        }
    }
}