// ***********************************************************************************
// FileName: SOCreator.cs
// Description:
// 
// Version: v1.0.0
// Creator: Jacky(jackylvm@foxmail.com)
// CreationTime: 2025-06-09 22:28:47
// ==============================================
// History update record:
// 
// ==============================================
// *************************************************************************************

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace JackyExtendedToolSet.Runtime
{
    public static class SOCreator
    {
        public static T GetOrCreateSO<T>(string assetName) where T : ScriptableObject
        {
#if UNITY_EDITOR
            var guid = AssetDatabase.FindAssets($"t:{typeof(T)}");
            if (guid.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid[0]));
            }

            var dirName = Path.GetDirectoryName(assetName);
            FolderHelper.EnsureFolderExists(dirName);

            var so = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(so, assetName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return so;
#else
            return null;
#endif
        }

        public static T GetExistingSO<T>() where T : ScriptableObject
        {
#if UNITY_EDITOR
            var guid = AssetDatabase.FindAssets($"t:{typeof(T)}");
            if (guid.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid[0]));
            }
#else
            return null;
#endif
            return null;
        }

        public static object GetExistingSO(Type type)
        {
#if UNITY_EDITOR
            var guid = AssetDatabase.FindAssets($"t:{type.Name}");
            if (guid.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid[0]), type);
            }
#else
            return null;
#endif
            return null;
        }
    }
}