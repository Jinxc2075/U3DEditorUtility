using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CopyLua : AssetPostprocessor
{
    public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string str in importedAsset)
        {
            if (str.Contains("Assets/Lua/"))
            {
                string src = Application.dataPath + "/" + str.Remove(0, 7);
                string dst = Application.dataPath + "/Resources/" + str.Remove(0, 7) + ".txt";

                Debug.Log("copy lua " + dst);
                File.Copy(src, dst, true);
            }
        }

        foreach (string str in deletedAssets)
        {
            if (str.Contains("Assets/Lua/"))
            {
                string dst = Application.dataPath + "/Resources/" + str.Remove(0, 7) + ".txt";
                Debug.Log("delete lua " + dst);
                File.Delete(dst);
            }
        }

        AssetDatabase.Refresh();
    }
}

