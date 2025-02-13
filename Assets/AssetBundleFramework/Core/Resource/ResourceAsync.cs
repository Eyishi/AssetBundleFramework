using System;
using AssetBundleFramework.Core.Bundle;
using UnityEngine;
using Object = UnityEngine.Object;
namespace AssetBundleFramework.Core.Resource
{
    internal class ResourceAsync : AResourceAsync
    {
        public override bool keepWaiting => !done;
        
        /// <summary>
        /// 异步加载资源的AssetBundleRequest
        /// </summary>
        private AssetBundleRequest m_AssetBundleRequest;
        
        /// <summary>
        /// 加载资源
        /// </summary>
        internal override void Load()
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException($"{nameof(Resource)}.{nameof(Load)}() {nameof(url)} is null.");

            if (bundle != null)
                throw new Exception($"{nameof(Resource)}.{nameof(Load)}() {nameof(bundle)} not null.");

            string bundleUrl = null;
            if (!ResourceManager.instance.ResourceBunldeDic.TryGetValue(url, out bundleUrl))
                throw new Exception($"{nameof(ResourceAsync)}.{nameof(Load)}() {nameof(bundleUrl)} is null.");

            bundle = BundleManager.instance.LoadAsync(bundleUrl);
        }
        /// <summary>
        /// 加载资源
        /// </summary>
        internal override void LoadAsset()
        {
            if (bundle == null)
                throw new Exception($"{nameof(ResourceAsync)}.{nameof(LoadAsset)}() {nameof(bundle)} is null.");

            if (!bundle.isStreamedSceneAssetBundle)
            {
                if (m_AssetBundleRequest != null)
                {
                    asset = m_AssetBundleRequest.asset;
                }
                else
                {
                    asset = bundle.LoadAsset(url, typeof(Object));
                }
            }

            done = true;

            if (finishedCallback != null)
            {
                Action<AResource> tempCallback = finishedCallback;
                finishedCallback = null;
                tempCallback.Invoke(this);
            }
        }
    }
}