using System;
using UnityEngine;
using Object = System.Object;

namespace AssetBundleFramework.Core.Bundle
{
    internal class BundleManager
    {
        public readonly static BundleManager instance = new BundleManager();

        /// <summary>
        /// 加载bundle开始的偏移
        /// </summary>
        internal ulong offset { get; private set; }
        
        /// <summary>
        /// 获取资源真实路径回调
        /// </summary>
        private Func<string, string> m_GetFileCallback;
        
        /// <summary>
        /// bundle依赖管理信息
        /// </summary>
        private AssetBundleManifest m_AssetBundleManifest;
        
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="platform">平台</param>
        /// <param name="getFileCallback">获取资源真实路径回调</param>
        /// <param name="offset">加载bundle偏移,一些资源不想让别人知道，相当于加密</param>
        internal void Initialize(string platform, Func<string, string> getFileCallback, ulong offset)
        {
            m_GetFileCallback = getFileCallback;
            this.offset = offset;
            
            string assetBundleManifestFile = getFileCallback.Invoke(platform);
            AssetBundle manifestAssetBundle = AssetBundle.LoadFromFile(assetBundleManifestFile);
            Object[] objs = manifestAssetBundle.LoadAllAssets();

            if (objs.Length == 0)
            {
                throw new Exception($"{nameof(BundleManager)}.{nameof(Initialize)}() AssetBundleManifest load fail.");
            }

            m_AssetBundleManifest = objs[0] as AssetBundleManifest;
        }
    }
}