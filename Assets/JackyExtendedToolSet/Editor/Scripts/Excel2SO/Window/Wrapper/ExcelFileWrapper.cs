// ***********************************************************************************
// FileName: ExcelFileWrapper.cs
// Description:
// 
// Version: v1.0.0
// Creator: Jacky(jackylvm@foxmail.com)
// CreationTime: 2025-06-09 22:58:15
// ==============================================
// History update record:
// 
// ==============================================
// *************************************************************************************

using System;
using JackyExtendedToolSet.Runtime;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace JackyExtendedToolSet.Editor.Excel2SO
{
    public class ExcelFileWrapper
    {
        [ShowInInspector] [LabelText("文件名")] private string _fileName;

        private readonly string _filePath;
        private readonly string _scriptOutputPath;
        private readonly string _assetOutputPath;

        public ExcelFileWrapper(string filePath, string fileName, string scriptOutputPath, string assetOutputPath)
        {
            _filePath = filePath;
            _fileName = fileName;

            _scriptOutputPath = scriptOutputPath;
            _assetOutputPath = assetOutputPath;
        }

        [Button("构建数据结构", ButtonSizes.Large)]
        private void ExcelToScriptableObject()
        {
            if (EditorUtility.DisplayDialog("警告", "是否要覆盖已有的数据结构？", "确定", "取消"))
            {
                EditorUtility.DisplayProgressBar("提示", "等待中...", 0);
                try
                {
                    ExcelTool.BuildScriptableObjectFile(_filePath, _scriptOutputPath);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    AssemblyReloader.RequestScriptCompilation(EditorUtility.ClearProgressBar);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ExcelFileWrapper:56]> {e.Message}");
                    EditorUtility.ClearProgressBar();
                    throw;
                }
            }
        }

        [Button("导入Excel数据", ButtonSizes.Large)]
        private void ImportExcelData()
        {
            if (EditorUtility.DisplayDialog("警告", "是否要覆盖已有的数据？", "确定", "取消"))
            {
                EditorUtility.DisplayProgressBar("提示", "等待中...", 0);
                try
                {
                    ExcelTool.BuildScriptableObjectData(_filePath, _assetOutputPath);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ExcelFileWrapper:75]> {e.Message}");
                    throw;
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }
    }
}