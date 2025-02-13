using System;
using System.IO;
using UnityEngine;

namespace AssetBundleFramework.Core.Bundle
{
    /// <summary>
    /// 异步加载的bundle
    /// </summary>
    internal class BundleAsync : ABundleAsync
    {
        /// <summary>
        /// 异步bundle的AssetBundleCreateRequest
        /// </summary>
        private AssetBundleCreateRequest m_AssetBundleCreateRequest;
        
        /// <summary>
        /// 加载AssetBundle
        /// </summary>
        internal override void Load()
        {
            if (m_AssetBundleCreateRequest != null)
            {
                throw new Exception($"{nameof(BundleAsync)}.{nameof(Load)}() {nameof(m_AssetBundleCreateRequest)} not null, {this}.");
            }

            string file = BundleManager.instance.GetFileUrl(url);

#if UNITY_EDITOR || UNITY_STANDALONE
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"{nameof(BundleAsync)}.{nameof(Load)}() {nameof(file)} not exist, file:{file}.");
            }
#endif

            m_AssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(file, 0, BundleManager.instance.offset);
        }

        internal override bool Update()
        {
            throw new System.NotImplementedException();
        }
    }
}