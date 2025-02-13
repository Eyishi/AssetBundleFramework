﻿using System;
using AssetBundleFramework.Core.Bundle;
using UnityEngine;
using Object = System.Object;

namespace AssetBundleFramework.Core.Resource
{
    internal class Resource : AResource
    {
        public override bool keepWaiting => !done;
        
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
                throw new Exception($"{nameof(Resource)}.{nameof(Load)}() {nameof(bundleUrl)} is null.");

            bundle = BundleManager.instance.Load(bundleUrl);
            LoadAsset();
        }
        
        /// <summary>
        /// 卸载资源
        /// </summary>
        internal override void UnLoad()
        {
            if (bundle == null)
                throw new Exception($"{ nameof(Resource)}.{nameof(UnLoad)}() {nameof(bundle)} is null.");

            if (asset != null && !(asset is GameObject))
            {
                Resources.UnloadAsset(asset);
                asset = null;
            }

            BundleManager.instance.UnLoad(bundle);

            bundle = null;
            finishedCallback = null;
        }
        
        /// <summary>
        /// 加载资源
        /// </summary>
        internal override void LoadAsset()
        {
            if (bundle == null)
                throw new Exception($"{nameof(Resource)}.{nameof(LoadAsset)}() {nameof(bundle)} is null.");

            //正在异步加载的资源要变成同步
            FreshAsyncAsset();

            if (!bundle.isStreamedSceneAssetBundle)
                asset = bundle.LoadAsset(url, typeof(Object));

            asset = bundle.LoadAsset(url, typeof(Object));

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