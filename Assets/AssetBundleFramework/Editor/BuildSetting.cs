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
                
            }
        }
    }
}