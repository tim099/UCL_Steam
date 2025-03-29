
// RCG_AutoHeader
// to change the auto header please go to RCG_AutoHeader.cs
// Create time : 02/19 2025

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX

using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UnityEngine;
using UCL.BuildLib;

namespace UCL.SteamLib
{
    [UCL.Core.ATTR.UCL_Sort((int)AssetGroup.BuildSettingType.UCL_SteamVDFBuildSetting)]
    [UCL.Core.ATTR.EnableUCLEditor]
    public class UCL_SteamVDFBuildSetting : UCL_PreBuildSetting
    {
        public const string VDFFormat = @"
{
    ""appid"" ""{0}"",
    ""desc"" ""{1}"",
    ""buildoutput"" ""{2}"",
    ""contentroot"" """",
    ""setlive"" """",
    ""preview"" ""0"",
    ""local"" """",
    ""depots"" 
    {
        {3}
    }
}";

        public string m_AppId = "1864830";
        public string m_Desc = "Emblem of Valor_0.8.38";
        public string m_BuildOutput = "D:\\App\\steamworks_sdk_157\\sdk\\tools\\ContentBuilder\\output";
        public Dictionary<string, string> m_Depots = new();
        override public async UniTask OnBuild(BuildData iBuildData)
        {
            ExportVDF();
        }
        [UCL.Core.ATTR.UCL_FunctionButton]
        public void ExportVDF()
        {
            System.Text.StringBuilder depots = new();
            foreach (var depot in m_Depots.Keys)
            {
                depots.AppendLine($"\"{depot}\"   \"{m_Depots[depot]}\"");
            }
            Debug.LogError(VDFFormat);
            Debug.LogError(depots.ToString());
            string VDF = string.Format(VDFFormat, m_AppId, m_Desc, m_BuildOutput, depots.ToString(), "");
            Debug.LogError(VDF);
        }
    }
}

//Format example
//"appbuild"
//{
//    "appid" "1864830"
//    "desc" "Emblem of Valor_0.8.38"
//    "buildoutput" "D:\App\steamworks_sdk_157\sdk\tools\ContentBuilder\output"
//    "contentroot" ""
//    "setlive" ""
//    "preview" "0"
//    "local" ""
//    "depots"
//    {
//        "1864831"   "D:\App\steamworks_sdk_157\sdk\tools\ContentBuilder\scripts\depot_1864831.vdf"
//    }
//}

#endif