using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetBundleFramework.Core.Bundle
{
    internal abstract class ABundle
    {
        /// <summary>
        /// AssetBundle
        /// </summary>
        internal AssetBundle assetBundle { get; set; }
        
        /// <summary>
        /// 是否是场景，场景跟别的不太一样
        /// </summary>
        internal bool isStreamedSceneAssetBundle { get; set; }
        
        /// <summary>
        /// bundle url
        /// </summary>
        internal string url { get; set; }

        /// <summary>
        /// 引用计数器
        /// </summary>
        internal int reference { get; set; }

        //bundle是否加载完成
        internal bool done { get; set; }

        /// <summary>
        /// bundle依赖
        /// </summary>
        internal ABundle[] dependencies { get; set; }
        
        /// <summary>
        /// 加载bundle
        /// </summary>
        internal abstract void Load();
        
        
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="name">资源名称</param>
        /// <param name="type">资源Type</param>
        /// <returns>指定名字的资源</returns>
        internal abstract Object LoadAsset(string name, Type type);
        
        /// <summary>
        /// 增加引用
        /// </summary>
        internal void AddReference()
        {
            //自己引用+1
            ++reference;
        }
    }
}