using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace U3DEditorUtility
{
    public class BundleBuilder
    {
        [MenuItem("Tools/打包工具/生成资源打包名", false, 20)]
        static void SetResourcesAssetBundleName()
        {
            string appPath = Application.dataPath + "/";
            string projPath = appPath.Substring(0, appPath.Length - 7);
            string fullPath = projPath + "/Assets/Packages";
            string[] searchExtensions = new[] {".prefab", ".asset", ".mat", ".txt"};
            Regex[] excluseRules = new Regex[] 
            {
                //new Regex (@"(\S+)(\/Scenes\/Z-zhucheng)(\S+)"),
            };

            SetDirAssetBundleName(fullPath, searchExtensions, excluseRules);

            fullPath = projPath + "/Assets/Resources";

            SetDirAssetBundleName(fullPath, searchExtensions, excluseRules);
        }

        [MenuItem("Tools/打包工具/生成打包文件Android", false, 20)]
        static void BuildAllAssetBundlesAndroid()
        {
            UnityEngine.Debug.Log("=========Build AssetBundles Android start..");
            //用lz4格式压缩
            BuildAssetBundleOptions build_options = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.IgnoreTypeTreeChanges | BuildAssetBundleOptions.DeterministicAssetBundle;
            string assetBundleOutputDir = Application.dataPath + "/../AssetBundles/Android/";
            if (!Directory.Exists(assetBundleOutputDir))
            {
                Directory.CreateDirectory(assetBundleOutputDir);
            }

            string projPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
            BuildPipeline.BuildAssetBundles(assetBundleOutputDir.Substring(projPath.Length), build_options, BuildTarget.Android);

            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("=========Build AssetBundles Android finished..");
        }

        /// <summary>
        /// 设置某个目录及子目录下资源打包名称
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="searchExtensions"></param>
        /// <param name="excluseRules"></param>
        static void SetDirAssetBundleName(string fullPath, string[] searchExtensions, Regex[] excluseRules)
        {
            if (!Directory.Exists(fullPath))
            {
                return;
            }

            string appPath = Application.dataPath + "/";
            string projPath = appPath.Substring(0, appPath.Length - 7);
            DirectoryInfo dir = new DirectoryInfo(fullPath);
            var files = dir.GetFiles("*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; ++i)
            {
                var fileInfo = files[i];

                string ext = fileInfo.Extension.ToLower();
                bool isFound = false;
                foreach (var v in searchExtensions)
                {
                    if (ext == v)
                    {
                        isFound = true;
                        break;
                    }
                }

                if (!isFound)
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar("设置打包资源名称", "正在处理" + fileInfo.Name, 1f * i / files.Length);
                string fullName = fileInfo.FullName.Replace('\\', '/');
                bool isExcluse = false;
                foreach (Regex excluseRule in excluseRules)
                {
                    if (excluseRule.Match(fullName).Success)
                    {
                        isExcluse = true;
                        break;
                    }
                }
                if (isExcluse)
                    continue;

                string path = fileInfo.FullName.Replace('\\', '/').Substring(projPath.Length);
                var importer = AssetImporter.GetAtPath(path);
                if (importer)
                {
                    string name = path.Substring(fullPath.Substring(projPath.Length).Length);
                    string targetName = "";
                    targetName = name.ToLower() + ".unity3d";
                    if (importer.assetBundleName != targetName)
                    {
                        importer.assetBundleName = targetName;
                    }
                }
            }
        }
    }
}
