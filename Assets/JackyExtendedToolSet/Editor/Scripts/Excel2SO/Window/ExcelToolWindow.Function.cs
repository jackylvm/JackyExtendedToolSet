// ***********************************************************************************
// FileName: ExcelToolWindow.Function.cs
// Description:
// 
// Version: v1.0.0
// Creator: Jacky(jackylvm@foxmail.com)
// CreationTime: 2025-06-11 23:50:13
// ==============================================
// History update record:
// 
// ==============================================
// *************************************************************************************

using System;
using System.IO;
using JackyExtendedToolSet.Runtime;
using UnityEditor;
using UnityEngine;

namespace JackyExtendedToolSet.Editor.Excel2SO
{
    public partial class ExcelToolWindow
    {
        private void CreateAllDataClassScriptableObjects()
        {
            var excelPath = Path.Combine(FolderHelper.GetProjectPath(), _setting.excelPath);
            var excelFiles = FolderHelper.GetExcelFiles(excelPath);
            if (excelFiles == null || excelFiles.Length == 0)
            {
                return;
            }

            var total = excelFiles.Length;
            var current = 0;
            EditorUtility.DisplayProgressBar("Excel文件", "正在生成数据结构", 0);
            try
            {
                foreach (var excelFile in excelFiles)
                {
                    ExcelTool.BuildScriptableObjectFile(excelFile, _setting.outputPath01);

                    current++;
                    EditorUtility.DisplayProgressBar("Excel文件", "正在生成数据结构...", current / (float)total);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                AssemblyReloader.RequestScriptCompilation(EditorUtility.ClearProgressBar);
            }
            catch (Exception e)
            {
                Debug.LogError($"UnityExcelMapperWindow.Excel:34>{e.Message}");
                EditorUtility.ClearProgressBar();
                throw;
            }
        }

        private void ImportAllExcelData()
        {
            var excelPath = Path.Combine(FolderHelper.GetProjectPath(), _setting.excelPath);
            var excelFiles = FolderHelper.GetExcelFiles(excelPath);
            if (excelFiles == null || excelFiles.Length == 0)
            {
                return;
            }

            var total = excelFiles.Length;
            var current = 0;
            EditorUtility.DisplayProgressBar("提示", "正在导出数据表", 0);
            try
            {
                foreach (var excelFile in excelFiles)
                {
                    ExcelTool.BuildScriptableObjectData(excelFile, _setting.outputPath02);

                    current++;
                    EditorUtility.DisplayProgressBar("提示", "正在导出数据表...", current / (float)total);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError($"[ExcelToolWindow.Function:88]>{e.Message}");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void DeleteAllDataClassScriptableObjects()
        {
            var projectPath = FolderHelper.GetProjectPath();
            projectPath = projectPath.Replace('/', Path.DirectorySeparatorChar);

            var filePath = Path.Combine(projectPath, _setting.outputPath01);
            var filePath1 = Path.Combine(filePath, "Custom");
            // 删除文件夹下面的所有文件
            if (Directory.Exists(filePath1))
            {
                Directory.Delete(filePath1, true);
            }

            filePath1 = Path.Combine(filePath, "Container");
            if (Directory.Exists(filePath1))
            {
                Directory.Delete(filePath1, true);
            }

            filePath1 = Path.Combine(filePath, "Data");
            if (Directory.Exists(filePath1))
            {
                Directory.Delete(filePath1, true);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AssemblyReloader.RequestScriptCompilation();
        }

        private void DeleteAllContainerClassScriptableObjects()
        {
            var projectPath = FolderHelper.GetProjectPath();
            projectPath = projectPath.Replace('/', Path.DirectorySeparatorChar);

            var filePath = Path.Combine(projectPath, _setting.outputPath02);
            // 删除文件夹下面的所有文件
            if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath, true);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}