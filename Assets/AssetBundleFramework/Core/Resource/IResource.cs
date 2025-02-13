using UnityEngine;

namespace AssetBundleFramework.Core.Resource
{
    public interface IResource
    {
        Object GetAsset();
        T GetAsset<T>() where T : Object;
        GameObject Instantiate();
        
       
        GameObject Instantiate(Transform parent, bool instantiateInWorldSpace);
     
    }
}