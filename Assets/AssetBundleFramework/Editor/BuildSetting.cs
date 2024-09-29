using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;

namespace AssetBundleFramework.Editor
{
    /// <summary>
    /// 打包构建
    /// </summary>
    public class BuildSetting : ISupportInitialize
    {
        [DisplayName("项目名称")]
        [XmlAttribute("ProjectName")]
        public string projectName { get; set; }

        [DisplayName("后缀列表")]
        [XmlAttribute("SuffixList")]
        public List<string> suffixList { get; set; } = new List<string>();

        [DisplayName("打包文件的目标文件夹")]
        [XmlAttribute("BuildRoot")]
        public string buildRoot { get; set; }
        
        [DisplayName("打包选项")]
        [XmlElement("BuildItem")]
        public List<BuildItem> items { get; set; } = new List<BuildItem>();

        [XmlIgnore]
        public Dictionary<string, BuildItem> itemDic = new Dictionary<string, BuildItem>();
        
        public void BeginInit()
        {
            throw new System.NotImplementedException();
        }

        public void EndInit()
        {
            buildRoot = Path.GetFullPath(buildRoot).Replace("\\", "/");
            
            itemDic.Clear();
            //针对不同的  builditem 打包选项 不同的处理
            for (int i = 0; i < items.Count; i++)
            {
                BuildItem buildItem = items[i];

                if (buildItem.bundleType == EBundleType.All || buildItem.bundleType == EBundleType.Directory)
                {
                    if (!Directory.Exists(buildItem.assetPath))
                    {
                        throw new Exception($"不存在资源路径:{buildItem.assetPath}");
                    }
                }

                //处理后缀
                string[] prefixes = buildItem.suffix.Split('|');
                for (int ii = 0; ii < prefixes.Length; ii++)
                {
                    string prefix = prefixes[ii].Trim();
                    if (!string.IsNullOrEmpty(prefix))
                        buildItem.suffixes.Add(prefix);
                }

                if (itemDic.ContainsKey(buildItem.assetPath))
                {
                    throw new Exception($"重复的资源路径:{buildItem.assetPath}");
                }
                itemDic.Add(buildItem.assetPath, buildItem);
            }
        }
        /// <summary>
        /// 获取所有在打包设置的文件列表
        /// </summary>
        /// <returns>文件列表</returns>
        public HashSet<string> Collect()
        {
            float min = Builder.collectRuleFileProgress.x;
            float max = Builder.collectRuleFileProgress.y;

            //进度条
            EditorUtility.DisplayProgressBar($"{nameof(Collect)}", "搜集打包规则资源", min);

            //处理每个规则忽略的目录,如路径A/B/C,需要忽略A/B
            for (int i = 0; i < items.Count; i++)
            {
                BuildItem buildItem_i = items[i];
                
                if (buildItem_i.resourceType != EResourceType.Direct)
                    continue;

                buildItem_i.ignorePaths.Clear();
                for (int j = 0; j < items.Count; j++)
                {
                    BuildItem buildItem_j = items[j];
                    //两个资源不同  并且是打包资源
                    if (i != j && buildItem_j.resourceType == EResourceType.Direct)
                    {
                        // 是否以 指定路径前缀 开头
                        if (buildItem_j.assetPath.StartsWith(buildItem_i.assetPath, StringComparison.InvariantCulture))
                        {
                            buildItem_i.ignorePaths.Add(buildItem_j.assetPath);
                        }
                    }
                }
            }

            //存储被规则分析到的所有文件
            HashSet<string> files = new HashSet<string>();

            for (int i = 0; i < items.Count; i++)
            {
                BuildItem buildItem = items[i];

                EditorUtility.DisplayProgressBar($"{nameof(Collect)}", "搜集打包规则资源", min + (max - min) * ((float)i / (items.Count - 1)));

                if (buildItem.resourceType != EResourceType.Direct)
                    continue;

                List<string> tempFiles = Builder.GetFiles(buildItem.assetPath, 
                    null, buildItem.suffixes.ToArray());
                for (int j = 0; j < tempFiles.Count; j++)
                {
                    string file = tempFiles[j];

                    //过滤被忽略的
                    if (IsIgnore(buildItem.ignorePaths, file))
                        continue;

                    files.Add(file);
                }

                EditorUtility.DisplayProgressBar($"{nameof(Collect)}", "搜集打包设置资源", (float)(i + 1) / items.Count);
            }

            return files;
        }
        /// <summary>
        /// 文件是否在忽略列表
        /// </summary>
        /// <param name="ignoreList">忽略路径列表</param>
        /// <param name="file">文件路径</param>
        /// <returns></returns>
        public bool IsIgnore(List<string> ignoreList, string file)
        {
            for (int i = 0; i < ignoreList.Count; i++)
            {
                string ignorePath = ignoreList[i];
                if (string.IsNullOrEmpty(ignorePath))
                    continue;
                if (file.StartsWith(ignorePath, StringComparison.InvariantCulture))
                    return true;
            }

            return false;
        }
        
        /// <summary>
        /// 获取BundleName
        /// </summary>
        /// <param name="assetUrl">资源路径</param>
        /// <param name="resourceType">资源类型</param>
        /// <returns>BundleName</returns>
        public string GetBundleName(string assetUrl, EResourceType resourceType)
        {
            return "";
        }
    }
}