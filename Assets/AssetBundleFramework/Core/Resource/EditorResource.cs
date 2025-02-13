using System;

using Object = UnityEngine.Object;
namespace AssetBundleFramework.Core.Resource
{
    internal class EditorResource : AResource
    {
        public override bool keepWaiting { get; }
        
        /// <summary>
        /// 加载资源
        /// </summary>
        internal override void Load()
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException($"{nameof(EditorResource)}.{nameof(Load)}() {nameof(url)} is null.");

            LoadAsset();
        }
        
        /// <summary>
        /// 加载资源
        /// </summary>
        internal override void LoadAsset()
        {
#if UNITY_EDITOR
            asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(url);
#endif
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