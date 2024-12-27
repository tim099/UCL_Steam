
// RCG_AutoHeader
// to change the auto header please go to RCG_AutoHeader.cs
// Create time : 12/22 2024
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UCL.SteamLib
{
    public static class UCL_SteamUtil
    {
        private static string FolderPath = Path.Combine(Application.persistentDataPath, "InstalledMods");
        private static HashSet<ulong> s_InstalledMod = null;

        /// <summary>
        /// 已經安裝的模組
        /// </summary>
        public static HashSet<ulong> InstalledMods
        {
            get
            {
                if (s_InstalledMod == null)
                {
                    s_InstalledMod = new();
                    var path = FolderPath;
                    Directory.CreateDirectory(path);//讀檔抓取資料
                    var files = Directory.GetFiles(path);
                    foreach (var file in files)
                    {
                        string id = Path.GetFileName(file);
                        if(ulong.TryParse(id, out  ulong fileId))
                        {
                            s_InstalledMod.Add(fileId);
                        }
                    }
                }
                return s_InstalledMod;
            }
        }
        private static string GetFilePath(ulong id) => Path.Combine(FolderPath, id.ToString());
        /// <summary>
        /// 紀錄已安裝的Steam模組
        /// </summary>
        /// <param name="fileId"></param>
        public static void AddInstalledMod(ulong id)
        {
            var installedMods = InstalledMods;
            if (installedMods.Contains(id))//已經記錄過
            {
                return;
            }
            installedMods.Add(id);
            File.WriteAllText(GetFilePath(id), string.Empty);//存檔
        }
        /// <summary>
        /// 刪除安裝紀錄
        /// </summary>
        /// <param name="id"></param>
        public static void RemoveInstalledMod(ulong id)
        {
            var installedMods = InstalledMods;
            if (!installedMods.Contains(id))//未安裝
            {
                return;
            }
            installedMods.Remove(id);
            var path = GetFilePath(id);
            if(File.Exists(path))//刪除紀錄
            {
                File.Delete(path);
            }
        }
    }
}