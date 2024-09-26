using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 编译器开发的东西
/// </summary>
public class Builder : MonoBehaviour
{
    #region Build MenuItem

    [MenuItem("Tools/ResBuild/Windows")]
    public static void BuildWindows()
    {
        Build();
    }

    [MenuItem("Tools/ResBuild/Android")]
    public static void BuildAndroid()
    {
        Build();
    }

    [MenuItem("Tools/ResBuild/iOS")]
    public static void BuildIos()
    {
        Build();
    }

    #endregion
    
    
}
