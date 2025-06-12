// ***********************************************************************************
// FileName: ExcelTool.cs
// Description:
// 
// Version: v1.0.0
// Creator: Jacky(jackylvm@foxmail.com)
// CreationTime: 2025-06-09 23:01:11
// ==============================================
// History update record:
// 
// ==============================================
// *************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ClosedXML.Excel;
using JackyExtendedToolSet.Runtime;
using JackyExtendedToolSet.Runtime.Excel2SO;
using UnityEditor;
using UnityEngine;

namespace JackyExtendedToolSet.Editor.Excel2SO
{
    public static partial class ExcelTool
    {
        private static ExcelToolSetting _excelToolSetting;

        public static void BuildScriptableObjectFile(string excelPath, string outputPath)
        {
            if (string.IsNullOrEmpty(excelPath))
            {
                EditorUtility.DisplayDialog("提示", "参数为空！", "确定");
                return;
            }

            if (FolderHelper.IsExcelFileOpen(excelPath))
            {
                var fileName = Path.GetFileName(excelPath);
                EditorUtility.DisplayDialog("错误", $"{fileName}文件正在被打开, 请关闭后再试!", "确定");
                return;
            }

            _excelToolSetting = SOCreator.GetExistingSO<ExcelToolSetting>();

            using var workbook = new XLWorkbook(excelPath);
            foreach (var worksheet in workbook.Worksheets)
            {
                if (worksheet.Name.Contains("#"))
                {
                    continue;
                }

                if (ReadExcelHeaderInformation(worksheet, out var excelData))
                {
                    GenerateScriptFile(worksheet.Name, excelData, excelPath, outputPath);
                }
            }
        }

        public static void BuildScriptableObjectData(string excelPath, string outputPath)
        {
            if (FolderHelper.IsExcelFileOpen(excelPath))
            {
                var fileName = Path.GetFileName(excelPath);
                EditorUtility.DisplayDialog("错误", $"{fileName}文件正在被打开, 请关闭后再试!", "确定");
                return;
            }

            _excelToolSetting = SOCreator.GetExistingSO<ExcelToolSetting>();

            using var workbook = new XLWorkbook(excelPath);
            foreach (var worksheet in workbook.Worksheets)
            {
                if (worksheet.Name.Contains("#"))
                {
                    continue;
                }

                if (ReadExcelHeaderInformation(worksheet, out var excelData))
                {
                    GenerateScriptableObjectData(worksheet, excelData, outputPath);
                }
            }
        }

        private static bool ReadExcelHeaderInformation(IXLWorksheet worksheet, out ExcelData excelData)
        {
            var fieldNames = new List<string>();
            var comments = new List<string>();
            var dataTypes = new List<string>();
            var separators = new List<string>();

            var row1 = worksheet.Row(1);
            var row2 = worksheet.Row(2);
            var row3 = worksheet.Row(3);
            var row4 = worksheet.Row(4);
            var colNum = 1;

            while (true)
            {
                var valRow1 = row1.Cell(colNum).GetValue<string>();
                var valRow2 = row2.Cell(colNum).GetValue<string>();
                var valRow3 = row3.Cell(colNum).GetValue<string>();
                var valRow4 = row4.Cell(colNum).GetValue<string>();

                if (
                    string.IsNullOrEmpty(valRow1) &&
                    string.IsNullOrEmpty(valRow2) &&
                    string.IsNullOrEmpty(valRow3) &&
                    string.IsNullOrEmpty(valRow4)
                )
                {
                    break;
                }

                fieldNames.Add(valRow1);
                comments.Add(valRow2);
                dataTypes.Add(valRow3);
                separators.Add(valRow4);

                colNum++;
            }

            excelData = new ExcelData
            {
                // 读取元数据
                fieldNames = fieldNames,
                comments = comments,
                dataTypes = dataTypes,
                separators = separators
            };

            if (excelData.fieldNames.Count == 0 || excelData.dataTypes.Count == 0)
            {
                Debug.LogWarning($"工作表 '{worksheet.Name}' 格式不正确!");
                return false;
            }

            return true;
        }

        private static void GenerateScriptFile(string worksheetName, ExcelData excelData, string excelPath, string outputPath)
        {
            // 读取元数据
            var fieldNames = excelData.fieldNames;
            var comments = excelData.comments;
            var dataTypes = excelData.dataTypes;

            // 创建数据类
            var className = worksheetName + _excelToolSetting.dataClassSuffix;
            CreateDataClassScript(className, fieldNames, dataTypes, comments, excelPath, outputPath);

            // 创建容器类
            var containerClassName = worksheetName + _excelToolSetting.containerClassSuffix;
            CreateContainerClassScript(containerClassName, className, excelPath, outputPath);
        }

        // 处理工作表
        private static void GenerateScriptableObjectData(IXLWorksheet worksheet, ExcelData excelData, string outputPath)
        {
            // 读取元数据
            var fieldNames = excelData.fieldNames;
            var dataTypes = excelData.dataTypes;
            var separators = excelData.separators;

            var className = worksheet.Name + _excelToolSetting.dataClassSuffix;
            var containerClassName = worksheet.Name + _excelToolSetting.containerClassSuffix;

            // 获取数据类的类型
            var assembly = GetAssemblyByName(_excelToolSetting.assemblyName);
            if (assembly == null)
            {
                Debug.LogError($"ExcelTools:177>无法获取{_excelToolSetting.assemblyName}.dll");
                return;
            }

            var dataType = assembly.GetType(_excelToolSetting.scriptNamespace + "." + className);

            if (dataType == null)
            {
                Debug.LogError($"无法获取类型: {className}");
                return;
            }

            containerClassName = _excelToolSetting.scriptNamespace + "." + containerClassName;
            // 获取容器类的类型
            var containerType = assembly.GetType(containerClassName);

            if (containerType == null)
            {
                Debug.LogError($"无法获取容器类型: {containerClassName}");
                return;
            }

            // 创建ScriptableObject实例
            var isExisting = true;
            var containerInstance = SOCreator.GetExistingSO(containerType) as ScriptableObject;
            if (!containerInstance)
            {
                isExisting = false;
                containerInstance = ScriptableObject.CreateInstance(containerClassName);
            }

            // 读取数据行
            var dataObjects = new List<object>();
            var rowNum = 7; // 数据从第7行开始

            while (HasDataInRow(worksheet, rowNum))
            {
                var dataObject = Activator.CreateInstance(dataType);
                FillObjectData(dataObject, dataType, worksheet, rowNum, fieldNames, dataTypes, separators);
                dataObjects.Add(dataObject);
                rowNum++;
            }

            // 设置容器中的数据
            var listField = containerType.GetField("dataList");
            if (listField != null)
            {
                var dataList = Activator.CreateInstance(listField.FieldType) as IList;
                foreach (var dataObj in dataObjects)
                {
                    dataList.Add(dataObj);
                }

                listField.SetValue(containerInstance, dataList);
            }

            // 保存ScriptableObject
            if (isExisting)
            {
                EditorUtility.SetDirty(containerInstance);
                AssetDatabase.SaveAssets();
            }
            else
            {
                var assetPath = Path.Combine(outputPath, $"SO_{worksheet.Name}.asset");
                assetPath = assetPath.Replace(Application.dataPath, "Assets");
                AssetDatabase.CreateAsset(containerInstance, assetPath);
            }
        }

        private static Assembly GetAssemblyByName(string assemblyName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies.FirstOrDefault(assembly => assembly.FullName.Contains(assemblyName));
        }

        // 检查行是否有数据
        private static bool HasDataInRow(IXLWorksheet worksheet, int rowNum)
        {
            var row = worksheet.Row(rowNum);
            return !string.IsNullOrEmpty(row.Cell(1).GetValue<string>());
        }
    }
}