using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace AssetBundleFramework.Editor
{
    /// <summary>
    /// 打包构建
    /// </summary>
    public class BuildSetting : ISupportInitialize
    {
        
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
    }
}