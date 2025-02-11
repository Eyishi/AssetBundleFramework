using System.Collections;
using System.Collections.Generic;
using System.IO;
using AssetBundleFramework.Core.Resource;
using UnityEngine;

public class Test_Callback : MonoBehaviour
{
    private string PrefixPath { get; set; }
    private string Platform { get; set; }
    
    // Start is called before the first frame update
    void Start()
    {
        Platform = GetPlatform();
        PrefixPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../AssetBundle")).Replace("\\", "/");
        PrefixPath += $"/{Platform}";
        ResourceManager.instance.Initialize(GetPlatform(), GetFileUrl, false, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private string GetPlatform()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                return "Windows";
            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.IPhonePlayer:
                return "iOS";
            default:
                throw new System.Exception($"未支持的平台:{Application.platform}");
        }
    }
    private string GetFileUrl(string assetUrl)
    {
        return $"{PrefixPath}/{assetUrl}";
    }
}
