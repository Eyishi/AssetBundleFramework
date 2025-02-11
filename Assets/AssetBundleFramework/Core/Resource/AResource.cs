﻿using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetBundleFramework.Core.Resource
{
    internal abstract class AResource:CustomYieldInstruction, IResource
    {
        /// <summary>
        /// Asset对应的Url
        /// </summary>
        public string url { get; set; }
        
        /// <summary>
        /// 加载完成的资源
        /// </summary>
        public virtual Object asset { get; protected set; }
        
        /// <summary>
        /// 依赖资源
        /// </summary>
        internal AResource[] dependencies { get; set; }
        
        /// <summary>
        /// 引用计数器
        /// </summary>
        internal int reference { get; set; }
        
        
        /// <summary>
        /// 增加引用
        /// </summary>
        internal void AddReference()
        {
            ++reference;
        }

        //是否加载完成
        internal bool done { get; set; }

        /// <summary>
        /// 减少引用
        /// </summary>
        internal void ReduceReference()
        {
            --reference;

            if (reference < 0)
            {
                //throw new Exception($"{GetType()}.{nameof(ReduceReference)}() less than 0,{nameof(url)}:{url}.");
            }
        }
        
        public Object GetAsset()
        {
            return asset;
        }

        // public abstract T GetAsset<T>() where T : Object;

        public GameObject Instantiate()
        {
            Object obj = asset;

            if (!obj)
                return null;

            if (!(obj is GameObject))
                return null;

            return Object.Instantiate(obj) as GameObject;
        }

        
        public GameObject Instantiate(Vector3 position, Quaternion rotation)
        {
            Object obj = asset;

            if (!obj)
                return null;

            if (!(obj is GameObject))
                return null;

            return Object.Instantiate(obj, position, rotation) as GameObject;
        }

       

        public GameObject Instantiate(Transform parent, bool instantiateInWorldSpace)
        {
            Object obj = asset;

            if (!obj)
                return null;

            if (!(obj is GameObject))
                return null;

            return Object.Instantiate(obj, parent, instantiateInWorldSpace) as GameObject;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        internal abstract void Load();
    }
}