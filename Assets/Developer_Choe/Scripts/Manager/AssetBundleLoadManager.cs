using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AssetBundleLoadManager : PivotalManager
{
    public readonly static Dictionary<string, GameObject> ObjDic = new Dictionary<string, GameObject>();

    public static List<string> NameList = new List<string>();

    public static void LoadFromFile()
    {
        AssetBundle bundle = AssetBundle.LoadFromFile("Bundle/test");

        if (bundle != null)
        {
            GameObject[] obj = bundle.LoadAllAssets<GameObject>();

            for (int i = 0; i < obj.Length; i++)
            {
                ObjDic.Add(obj[i].name, obj[i]);
                NameList.Add(obj[i].name);
                Debug.Log(string.Format("{0} is Loaded from AssetBundle", obj[i].name));
            }
        }
    }
}
