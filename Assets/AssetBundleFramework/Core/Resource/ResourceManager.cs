using System;
using System.Collections.Generic;
using System.IO;
using AssetBundleFramework.Core.Bundle;
using UnityEngine;

namespace AssetBundleFramework.Core.Resource
{
    public class ResourceManager
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static ResourceManager instance { get; } = new ResourceManager();

        private const string MANIFEST_BUNDLE = "manifest.ab";
        private const string RESOURCE_ASSET_NAME = "Assets/Temp/Resource.bytes";
        private const string BUNDLE_ASSET_NAME = "Assets/Temp/Bundle.bytes";
        private const string DEPENDENCY_ASSET_NAME = "Assets/Temp/Dependency.bytes";
        
        /// <summary>
        ///   保存资源对应的bundle
        /// </summary>
        internal Dictionary<string, string> ResourceBunldeDic = new Dictionary<string, string>();
        
        /// <summary>
        /// 保存资源的依赖关系
        /// </summary>
        internal Dictionary<string, List<string>> ResourceDependencyDic = new Dictionary<string, List<string>>();
        
        /// <summary>
        /// 是否使用AssetDataBase进行加载
        /// </summary>
        private bool m_Editor;
        
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="platform">平台</param>
        /// <param name="getFileCallback">获取资源真实路径回调</param>
        /// <param name="editor">是否使用AssetDataBase加载</param>
        /// <param name="offset">获取bundle的偏移</param>
        public void Initialize(string platform, Func<string, string> getFileCallback, bool editor, ulong offset)
        {
            m_Editor = editor;

            if (m_Editor)
                return;
            BundleManager.instance.Initialize(platform, getFileCallback, offset);
            
            string manifestBunldeFile = getFileCallback.Invoke(MANIFEST_BUNDLE);
            AssetBundle manifestAssetBundle = AssetBundle.LoadFromFile(manifestBunldeFile, 0, offset);
            
            TextAsset resourceTextAsset = manifestAssetBundle.LoadAsset(RESOURCE_ASSET_NAME) as TextAsset;
            TextAsset bundleTextAsset = manifestAssetBundle.LoadAsset(BUNDLE_ASSET_NAME) as TextAsset;
            TextAsset dependencyTextAsset = manifestAssetBundle.LoadAsset(DEPENDENCY_ASSET_NAME) as TextAsset;

            byte[] resourceBytes = resourceTextAsset.bytes;
            byte[] bundleBytes = bundleTextAsset.bytes;
            byte[] dependencyBytes = dependencyTextAsset.bytes;
            
            manifestAssetBundle.Unload(true);
            manifestAssetBundle = null;
            
            //保存id对应的asseturl
            Dictionary<ushort, string> assetUrlDic = new Dictionary<ushort, string>();
            #region 读取资源信息
            {
                MemoryStream resourceMemoryStream = new MemoryStream(resourceBytes);
                BinaryReader resourceBinaryReader = new BinaryReader(resourceMemoryStream);
                //获取资源个数
                ushort resourceCount = resourceBinaryReader.ReadUInt16();
                for (ushort i = 0; i < resourceCount; i++)
                {
                    string assetUrl = resourceBinaryReader.ReadString();
                    assetUrlDic.Add(i, assetUrl);
                }
            }
            #endregion
            
            #region 读取bundle信息
            {
                ResourceBunldeDic.Clear();
                MemoryStream bundleMemoryStream = new MemoryStream(bundleBytes);
                BinaryReader bundleBinaryReader = new BinaryReader(bundleMemoryStream);
                //获取bundle个数
                ushort bundleCount = bundleBinaryReader.ReadUInt16();
                for (int i = 0; i < bundleCount; i++)
                {
                    string bundleUrl = bundleBinaryReader.ReadString();
                    //string bundleFileUrl = getFileCallback(bundleUrl);
                    string bundleFileUrl = bundleUrl;
                    //获取bundle内的资源个数
                    ushort resourceCount = bundleBinaryReader.ReadUInt16();
                    for (int ii = 0; ii < resourceCount; ii++)
                    {
                        ushort assetId = bundleBinaryReader.ReadUInt16();
                        string assetUrl = assetUrlDic[assetId];
                        ResourceBunldeDic.Add(assetUrl, bundleFileUrl);
                    }
                }
            }
            #endregion
            
            #region 读取资源依赖信息
            {
                ResourceDependencyDic.Clear();
                MemoryStream dependencyMemoryStream = new MemoryStream(dependencyBytes);
                BinaryReader dependencyBinaryReader = new BinaryReader(dependencyMemoryStream);
                //获取依赖链个数
                ushort dependencyCount = dependencyBinaryReader.ReadUInt16();
                for (int i = 0; i < dependencyCount; i++)
                {
                    //获取资源个数
                    ushort resourceCount = dependencyBinaryReader.ReadUInt16();
                    ushort assetId = dependencyBinaryReader.ReadUInt16();
                    string assetUrl = assetUrlDic[assetId];
                    List<string> dependencyList = new List<string>(resourceCount);
                    for (int ii = 1; ii < resourceCount; ii++)
                    {
                        ushort dependencyAssetId = dependencyBinaryReader.ReadUInt16();
                        string dependencyUrl = assetUrlDic[dependencyAssetId];
                        dependencyList.Add(dependencyUrl);
                    }

                    ResourceDependencyDic.Add(assetUrl, dependencyList);
                }
            }
            #endregion
        }
    }
}