using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using AssetBundleFramework.Core;
using AssetBundleFramework.Editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 编译器开发的东西
/// </summary>
public class Builder : MonoBehaviour
{
    
    private static readonly Profiler ms_BuildProfiler = new Profiler(nameof(Builder));
    /// <summary>
    /// 配置
    /// </summary>
    private static readonly Profiler ms_LoadBuildSettingProfiler = ms_BuildProfiler.CreateChild(nameof(LoadSetting));
    /// <summary>
    /// 转换平台
    /// </summary>
    private static readonly Profiler ms_SwitchPlatformProfiler = ms_BuildProfiler.CreateChild(nameof(SwitchPlatform));
    
#if UNITY_IOS
        private const string PLATFORM = "iOS";
#elif UNITY_ANDROID
        private const string PLATFORM = "Android";
#else
    private const string PLATFORM = "Windows";
#endif
    
    /// <summary>
    /// 打包设置
    /// </summary>
    public static BuildSetting buildSetting { get; private set; }
    
    /// <summary>
    /// 打包目录
    /// </summary>
    public static string buildPath { get; set; }
    
    #region Build MenuItem

    [MenuItem("Tools/ResBuild/Windows")]
    public static void BuildWindows()
    {
        Build();
    }

    [MenuItem("Tools/ResBuild/Android")]
    public static void BuildAndroid()
    {
        Build();
    }

    [MenuItem("Tools/ResBuild/iOS")]
    public static void BuildIos()
    {
        Build();
    }

    /// <summary>
    /// 切换打包平台
    /// </summary>
    public static void SwitchPlatform()
    {
        string platform = PLATFORM;
        
        //切换平台
        switch (platform)
        {
            case "windows":
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, 
                    BuildTarget.StandaloneWindows64);
                break;
            case "android":
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, 
                    BuildTarget.Android);
                break;
            case "ios":
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, 
                    BuildTarget.iOS);
                break;
        }
    }

    /// <summary>
    /// 加载打包配置文件
    /// </summary>
    /// <param name="settingPath">打包配置路径</param>
    private static BuildSetting LoadSetting(string settingPath)
    {
        buildSetting = XmlUtility.Read<BuildSetting>(settingPath);
        if (buildSetting == null)
        {
            throw new Exception($"Load buildSetting failed,SettingPath:{settingPath}.");
        }
        (buildSetting as ISupportInitialize)?.EndInit();

        //打包的根目录
        buildPath = Path.GetFullPath(buildSetting.buildRoot).Replace("\\", "/");
        if (buildPath.Length > 0 && buildPath[buildPath.Length - 1] != '/')
        {
            buildPath += "/";
        }
        buildPath += $"{PLATFORM}/";

        return buildSetting;
    }
    
    #endregion
    
    
}
