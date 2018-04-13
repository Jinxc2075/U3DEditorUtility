using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 几个打包使用的菜单功能实现
/// </summary>
namespace U3DEditorUtility
{
    public class BundleBuilder
    {
        [MenuItem(itemName: "Tools/打包工具/清空所有打包名", isValidateFunction: false, priority: 20)]
        private static void CleanResourcesAssetBundleName()
        {
            string appPath = Application.dataPath + "/";
            string projPath = appPath.Substring(0, appPath.Length - 7);
            string fullPath = projPath + "/Assets/Resources";

            DirectoryInfo dir = new DirectoryInfo(fullPath);
            var files = dir.GetFiles("*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; ++i)
            {
                var fileInfo = files[i];
                string path = fileInfo.FullName.Replace('\\', '/').Substring(projPath.Length);
                EditorUtility.DisplayProgressBar("清理打包资源名称", "正在处理" + fileInfo.Name, 1f * i / files.Length);
                var importer = AssetImporter.GetAtPath(path);
                if (importer)
                {
                    importer.assetBundleName = null;
                }
            }

            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();

            Debug.Log("=========clean Lua bundle name finished.." + files.Length + " processed");
        }

        [MenuItem(itemName: "Tools/打包工具/生成LUA打包名", isValidateFunction: false, priority: 20)]
        private static void SetLuaBundleName()
        {
            Debug.Log("=========Set Lua bundle name start..");

            string appPath = Application.dataPath + "/";
            string projPath = appPath.Substring(0, appPath.Length - 7);
            string fullPath = projPath + "/Assets/Resources/Lua";

            DirectoryInfo dir = new DirectoryInfo(fullPath);
            var files = dir.GetFiles("*.txt", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; ++i)
            {
                var fileInfo = files[i];
                string path = fileInfo.FullName.Replace('\\', '/').Substring(projPath.Length);
                EditorUtility.DisplayProgressBar("设置lua打包名称", "正在处理" + fileInfo.Name, 1f * i / files.Length);
                var importer = AssetImporter.GetAtPath(path);
                if (importer)
                {
                    string targetName = "lua.unity3d";
                    if (importer.assetBundleName != targetName)
                    {
                        importer.assetBundleName = targetName;
                    }
                }
            }

            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();

            Debug.Log("=========Set Lua bundle name finished.." + files.Length + " processed");
        }

        [MenuItem(itemName: "Tools/打包工具/生成资源打包名", isValidateFunction: false, priority: 20)]
        private static void SetResourcesAssetBundleName()
        {
            string appPath = Application.dataPath + "/";
            string projPath = appPath.Substring(0, appPath.Length - 7);
            
            string[] searchExtensions = new[] {".prefab", ".mat", ".txt"};
            Regex[] excluseRules = new Regex[] 
            {
                new Regex (@"^.*/Lua/.*$"), //忽略掉lua脚本，这些脚本会单独打包
            };

            string fullPath = projPath + "/Assets/Resources";

            SetDirAssetBundleName(fullPath, searchExtensions, excluseRules);

            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();

            Debug.Log("=========Set resource bundle name finished....");
        }

        [MenuItem(itemName: "Tools/打包工具/生成打包文件Android", isValidateFunction: false, priority: 20)]
        private static void BuildAllAssetBundlesAndroid()
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

            Debug.Log("=========Build AssetBundles Android finished..");
        }

        /// <summary>
        /// 设置某个目录及子目录下资源打包名称
        /// </summary>
        /// <param name="fullPath">搜索资源的目录路径</param>
        /// <param name="searchExtensions">要打包的资源扩展名</param>
        /// <param name="excluseRules">要排除掉的资源，用正则表达式</param>
        private static void SetDirAssetBundleName(string fullPath, string[] searchExtensions, Regex[] excluseRules)
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
                {
                    continue;
                }

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
