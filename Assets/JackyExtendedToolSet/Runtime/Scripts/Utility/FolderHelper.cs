// ***********************************************************************************
// FileName: FolderHelper.cs
// Description:
// 
// Version: v1.0.0
// Creator: Jacky(jackylvm@foxmail.com)
// CreationTime: 2025-06-09 22:29:40
// ==============================================
// History update record:
// 
// ==============================================
// *************************************************************************************

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JackyExtendedToolSet.Runtime
{
    public static class FolderHelper
    {
        /// <summary>
        /// 获取项目路径
        /// </summary>
        /// <returns></returns>
        public static string GetProjectPath()
        {
            var dataPath = Application.dataPath;
            var projectPath = dataPath.Substring(0, dataPath.Length - "/Assets".Length);
            return projectPath;
        }

        /// <summary>
        /// 检查指定文件夹中是否存在指定文件
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool CheckFileExists(string folderPath, string fileName)
        {
            // 构建完整的文件路径
            var fullPath = Path.Combine(folderPath, fileName);

            // 去除路径中的多余反斜杠
            fullPath = fullPath.Replace("\\", "/");

            return File.Exists(fullPath);
        }

        /// <summary>
        /// 确保文件夹存在
        /// </summary>
        /// <param name="folderPath"></param>
        public static void EnsureFolderExists(string folderPath)
        {
            var tmp = folderPath;
            if (folderPath.StartsWith("Assets/"))
            {
                tmp = Path.Combine(GetProjectPath(), folderPath);
            }

            // 检查文件夹是否存在
            if (Directory.Exists(tmp)) return;

            // 若不存在，递归创建文件夹
            Directory.CreateDirectory(tmp);

#if UNITY_EDITOR
            // 刷新AssetDatabase，让Unity更新资源视图
            AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// 判断文件是否被打开
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool IsExcelFileOpen(string filePath)
        {
            try
            {
                var tmp = filePath;
                if (filePath.StartsWith("Assets/"))
                {
                    tmp = GetProjectPath() + filePath;
                }

                using var fs = File.Open(tmp, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                // 文件未被打开，流可以成功打开
                return false;
            }
            catch (IOException)
            {
                // 文件已被打开，无法以独占方式打开流
                return true;
            }
        }

        public static string[] GetExcelFiles(string folderPath)
        {
            // 获取所有.xlsx和.xls文件
            var excelFiles = Directory.GetFiles(
                folderPath, "*.xlsx"
            ).Concat(
                Directory.GetFiles(folderPath, "*.xls")
            ).ToArray();

            return excelFiles;
        }
    }
}