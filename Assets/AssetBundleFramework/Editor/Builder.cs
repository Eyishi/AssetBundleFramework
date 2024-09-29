using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Mime;
using AssetBundleFramework.Core;
using AssetBundleFramework.Editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 编译器开发的东西
/// </summary>
public class Builder : MonoBehaviour
{
    public static readonly Vector2 collectRuleFileProgress = new Vector2(0, 0.2f);
    
    
    
    private static readonly Profiler ms_BuildProfiler = new Profiler(nameof(Builder));
    /// <summary>
    /// 配置
    /// </summary>
    private static readonly Profiler ms_LoadBuildSettingProfiler = ms_BuildProfiler.CreateChild(nameof(LoadSetting));
    /// <summary>
    /// 转换平台
    /// </summary>
    private static readonly Profiler ms_SwitchPlatformProfiler = ms_BuildProfiler.CreateChild(nameof(SwitchPlatform));
    
    /// <summary>
    /// 关于收集打包的
    /// </summary>
    private static readonly Profiler ms_CollectProfiler = ms_BuildProfiler.CreateChild(nameof(Collect));
    private static readonly Profiler ms_CollectBuildSettingFileProfiler = ms_CollectProfiler.CreateChild("CollectBuildSettingFile");
    //private static readonly Profiler ms_CollectDependencyProfiler = ms_CollectProfiler.CreateChild(nameof(CollectDependency));
    //private static readonly Profiler ms_CollectBundleProfiler = ms_CollectProfiler.CreateChild(nameof(CollectBundle));
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
    
    #region Path

    /// <summary>
    /// 打包配置
    /// </summary>
    public readonly static string BuildSettingPath = Path.GetFullPath("BuildSetting.xml").
        Replace("\\", "/");

    
    
    /// <summary>
    /// 打包目录
    /// </summary>
    public static string buildPath { get; set; }

    #endregion
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

    #endregion
    
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
        Debug.Log(settingPath);
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
    
    /// <summary>
    /// 搜集打包bundle的信息
    /// </summary>
    /// <returns></returns>
    private static Dictionary<string, List<string>> Collect()
    {
        //获取所有在打包设置的文件列表
        ms_CollectBuildSettingFileProfiler.Start();
        HashSet<string> files = buildSetting.Collect();
        ms_CollectBuildSettingFileProfiler.Stop();

        //搜集所有文件的依赖关系
        ms_CollectDependencyProfiler.Start();
        Dictionary<string, List<string>> dependencyDic = CollectDependency(files);
        ms_CollectDependencyProfiler.Stop();

        //标记所有资源的信息
        Dictionary<string, EResourceType> assetDic = new Dictionary<string, EResourceType>();

        //被打包配置分析到的直接设置为Direct
        foreach (string url in files)
        {
            assetDic.Add(url, EResourceType.Direct);
        }

        //依赖的资源标记为Dependency，已经存在的说明是Direct的资源
        foreach (string url in dependencyDic.Keys)
        {
            if (!assetDic.ContainsKey(url))
            {
                assetDic.Add(url, EResourceType.Dependency);
            }
        }

        //该字典保存bundle对应的资源集合
        ms_CollectBundleProfiler.Start();
        Dictionary<string, List<string>> bundleDic = CollectBundle(buildSetting, assetDic, dependencyDic);
        ms_CollectBundleProfiler.Stop();

        //生成Manifest文件
        ms_GenerateManifestProfiler.Start();
        GenerateManifest(assetDic, bundleDic, dependencyDic);
        ms_GenerateManifestProfiler.Stop();

        return bundleDic;
    }
    private static void Build()
    {
        ms_BuildProfiler.Start();

        ms_SwitchPlatformProfiler.Start();
        SwitchPlatform();
        ms_SwitchPlatformProfiler.Stop();

        ms_LoadBuildSettingProfiler.Start();
        buildSetting = LoadSetting(BuildSettingPath);
        ms_LoadBuildSettingProfiler.Stop();

        //搜集bundle信息
        ms_CollectProfiler.Start();
        Dictionary<string, List<string>> bundleDic = Collect();
        ms_CollectProfiler.Stop();
        
        // //打包assetbundle
        // ms_BuildBundleProfiler.Start();
        // BuildBundle(bundleDic);
        // ms_BuildBundleProfiler.Stop();
        //
        // //清空多余文件
        // ms_ClearBundleProfiler.Start();
        // ClearAssetBundle(buildPath, bundleDic);
        // ms_ClearBundleProfiler.Stop();
        //
        // //把描述文件打包bundle
        // ms_BuildManifestBundleProfiler.Start();
        // BuildManifest();
        // ms_BuildManifestBundleProfiler.Stop();
        //
        // EditorUtility.ClearProgressBar();

        ms_BuildProfiler.Stop();

        Debug.Log($"打包完成{ms_BuildProfiler} ");
    }
    
    
    
    
}
