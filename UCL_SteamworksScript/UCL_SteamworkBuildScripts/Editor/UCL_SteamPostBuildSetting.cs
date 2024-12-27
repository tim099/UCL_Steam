
// RCG_AutoHeader
// to change the auto header please go to RCG_AutoHeader.cs
// Create time : 12/27 2024
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UnityEngine;
using UCL.BuildLib;

namespace UCL.SteamLib
{
    [UCL.Core.ATTR.UCL_Sort((int)AssetGroup.BuildSettingType.UCL_SteamPostBuildSetting)]
    public class UCL_SteamPostBuildSetting : UCL_PreBuildSetting
    {
        public UCL_SteamConfigAssetEntry m_SteamConfig = new UCL_SteamConfigAssetEntry();
        override public async UniTask OnBuild(BuildData iBuildData)
        {
            var data = m_SteamConfig.GetData();
            data.ApplyAppId(iBuildData.m_OutputPath);
        }
    }
}
