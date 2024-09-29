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
    private static readonly Vector2 ms_GetDependencyProgress = new Vector2(0.2f, 0.4f);
    private static readonly Vector2 ms_CollectBundleInfoProgress = new Vector2(0.4f, 0.5f);
    
    
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
    private static readonly Profiler ms_CollectDependencyProfiler = ms_CollectProfiler.CreateChild(nameof(CollectDependency));
    private static readonly Profiler ms_CollectBundleProfiler = ms_CollectProfiler.CreateChild(nameof(CollectBundle));
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
        
        

        ms_BuildProfiler.Stop();

        Debug.Log($"打包完成{ms_BuildProfiler} ");
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
        
        
    }
    
    /// <summary>
    /// 收集指定文件集合所有的依赖信息
    /// </summary>
    /// <param name="files">文件集合</param>
    /// <returns>依赖信息</returns>
    private static Dictionary<string, List<string>> CollectDependency(ICollection<string> files)
    {
        float min = ms_GetDependencyProgress.x;
        float max = ms_GetDependencyProgress.y;

        Dictionary<string, List<string>> dependencyDic = new Dictionary<string, List<string>>();

        //声明fileList后，就不需要递归了
        List<string> fileList = new List<string>(files);

        for (int i = 0; i < fileList.Count; i++)
        {
            string assetUrl = fileList[i];

            if (dependencyDic.ContainsKey(assetUrl))
                continue;

            if (i % 10 == 0)
            {
                //只能大概模拟进度
                float progress = min + (max - min) * ((float)i / (files.Count * 3));
                EditorUtility.DisplayProgressBar($"{nameof(CollectDependency)}", "搜集依赖信息", progress);
            }

            string[] dependencies = AssetDatabase.GetDependencies(assetUrl, false);
            List<string> dependencyList = new List<string>(dependencies.Length);

            //过滤掉不符合要求的asset
            for (int ii = 0; ii < dependencies.Length; ii++)
            {
                string tempAssetUrl = dependencies[ii];
                string extension = Path.GetExtension(tempAssetUrl).ToLower();
                if (string.IsNullOrEmpty(extension) || extension == ".cs" || extension == ".dll")
                    continue;
                dependencyList.Add(tempAssetUrl);
                if (!fileList.Contains(tempAssetUrl))
                    fileList.Add(tempAssetUrl);
            }

            dependencyDic.Add(assetUrl, dependencyList);
        }

        return dependencyDic;
    }
        /// <summary>
        /// 搜集bundle对应的ab名字
        /// </summary>
        /// <param name="buildSetting"></param>
        /// <param name="assetDic">资源列表</param>
        /// <param name="dependencyDic">资源依赖信息</param>
        /// <returns>bundle包信息</returns>
        private static Dictionary<string, List<string>> CollectBundle(BuildSetting buildSetting, 
            Dictionary<string, EResourceType> assetDic, Dictionary<string, List<string>> dependencyDic)
        {
            float min = ms_CollectBundleInfoProgress.x;
            float max = ms_CollectBundleInfoProgress.y;

            EditorUtility.DisplayProgressBar($"{nameof(CollectBundle)}", "搜集bundle信息", min);

            Dictionary<string, List<string>> bundleDic = new Dictionary<string, List<string>>();
            //外部资源
            List<string> notInRuleList = new List<string>();

            int index = 0;
            foreach (KeyValuePair<string, EResourceType> pair in assetDic)
            {
                index++;
                string assetUrl = pair.Key;
                string bundleName = buildSetting.GetBundleName(assetUrl, pair.Value);

                //没有bundleName的资源为外部资源
                if (bundleName == null)
                {
                    notInRuleList.Add(assetUrl);
                    continue;
                }

                List<string> list;
                if (!bundleDic.TryGetValue(bundleName, out list))
                {
                    list = new List<string>();
                    bundleDic.Add(bundleName, list);
                }

                list.Add(assetUrl);

                EditorUtility.DisplayProgressBar($"{nameof(CollectBundle)}", "搜集bundle信息", min + (max - min) * ((float)index / assetDic.Count));
            }

            //todo...  外部资源
            if (notInRuleList.Count > 0)
            {
                string massage = string.Empty;
                for (int i = 0; i < notInRuleList.Count; i++)
                {
                    massage += "\n" + notInRuleList[i];
                }
                EditorUtility.ClearProgressBar();
                throw new Exception($"资源不在打包规则,或者后缀不匹配！！！{massage}");
            }

            //排序
            foreach (List<string> list in bundleDic.Values)
            {
                list.Sort();
            }

            return bundleDic;
        }
    /// <summary>
    /// 获取指定路径的文件
    /// </summary>
    /// <param name="path">指定路径</param>
    /// <param name="prefix">前缀</param>
    /// <param name="suffixes">后缀集合</param>
    /// <returns>文件列表</returns>
    public static List<string> GetFiles(string path, string prefix, params string[] suffixes)
    {
        //获取指定路径下的文件
        string[] files = Directory.GetFiles(path, $"*.*", SearchOption.AllDirectories);
        List<string> result = new List<string>(files.Length);
        
        for (int i = 0; i < files.Length; ++i)
        {
            string file = files[i].Replace('\\', '/');
            
            //处理前缀
            if (prefix != null && !file.StartsWith(prefix, StringComparison.InvariantCulture))
            {
                continue;
            }
            //是否有其中一个后缀
            if (suffixes != null && suffixes.Length > 0)
            {
                bool exist = false;

                for (int ii = 0; ii < suffixes.Length; ii++)
                {
                    string suffix = suffixes[ii];
                    if (file.EndsWith(suffix, StringComparison.InvariantCulture))
                    {
                        exist = true;
                        break;
                    }
                }

                if (!exist)
                    continue;
            }

            result.Add(file);
        }
        return result;
        
    }
    
    
}
