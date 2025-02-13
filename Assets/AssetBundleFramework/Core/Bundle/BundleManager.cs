using System;
using System.Collections.Generic;
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
        /// bundle依赖管理信息
        /// </summary>
        private AssetBundleManifest m_AssetBundleManifest;
        
        /// <summary>
        /// 所有已加载的bundle
        /// </summary>
        private Dictionary<string, ABundle> m_BundleDic = new Dictionary<string, ABundle>();

        //异步创建的bundle加载时候需要先保存到该列表
        private List<ABundleAsync> m_AsyncList = new List<ABundleAsync>();

        /// <summary>
        /// 需要释放的bundle
        /// </summary>
        private LinkedList<ABundle> m_NeedUnloadList = new LinkedList<ABundle>();
        
        /// <summary>
        /// 获取资源真实路径回调
        /// </summary>
        private Func<string, string> m_GetFileCallback;
        
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
        
        /// <summary>
        /// 同步加载bundle
        /// </summary>
        /// <param name="url">asset路径</param>
        internal ABundle Load(string url)
        {
            return LoadInternal(url, false);
        }
        
        /// <summary>
        /// 内部加载bundle
        /// </summary>
        /// <param name="url">asset路径</param>
        /// <param name="async">是否异步</param>
        /// <returns>bundle对象</returns>
        private ABundle LoadInternal(string url, bool async)
        {
            ABundle bundle;
            if (m_BundleDic.TryGetValue(url, out bundle))
            {
                if (bundle.reference == 0)
                {
                    m_NeedUnloadList.Remove(bundle);
                }

                //从缓存中取并引用+1
                bundle.AddReference();

                return bundle;
            }

            //创建ab  异步
            if (async)
            {
                bundle = new BundleAsync();
                bundle.url = url;
                m_AsyncList.Add(bundle as ABundleAsync);
            }
            //同步
            else
            {
                bundle = new Bundle();
                bundle.url = url;
            }

            m_BundleDic.Add(url, bundle);

            //加载依赖
            string[] dependencies = m_AssetBundleManifest.GetDirectDependencies(url);
            if (dependencies.Length > 0)
            {
                bundle.dependencies = new ABundle[dependencies.Length];
                for (int i = 0; i < dependencies.Length; i++)
                {
                    string dependencyUrl = dependencies[i];
                    ABundle dependencyBundle = LoadInternal(dependencyUrl, async);
                    bundle.dependencies[i] = dependencyBundle;
                }
            }

            bundle.AddReference();

            bundle.Load();

            return bundle;
        }
        
        /// <summary>
        /// 获取bundle的绝对路径
        /// </summary>
        /// <param name="url"></param>
        /// <returns>bundle的绝对路径</returns>
        internal string GetFileUrl(string url)
        {
            if (m_GetFileCallback == null)
            {
                throw new Exception($"{nameof(BundleManager)}.{nameof(GetFileUrl)}() {nameof(m_GetFileCallback)} is null.");
            }

            //交到外部处理
            return m_GetFileCallback.Invoke(url);
        }
    }
}