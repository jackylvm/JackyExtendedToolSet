// ***********************************************************************************
// FileName: ExcelToolSetting.cs
// Description:
// 
// Version: v1.0.0
// Creator: Jacky(jackylvm@foxmail.com)
// CreationTime: 2025-06-09 21:49:16
// ==============================================
// History update record:
// 
// ==============================================
// *************************************************************************************

using System;
using System.Collections;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JackyExtendedToolSet.Runtime.Excel2SO
{
    [CreateAssetMenu(menuName = "ScriptableObjects/ExcelToolSetting", fileName = "ExcelToolSetting")]
    public class ExcelToolSetting : ScriptableObject
    {
        [FolderPath] [LabelText("Excel路径")] public string excelPath = "";
        [FolderPath] [LabelText("脚本输出路径")] public string outputPath01 = "";
        [FolderPath] [LabelText("资源输出路径")] public string outputPath02 = "";
        [LabelText("脚本命名空间")] public string scriptNamespace = "ExcelClass";
        [LabelText("数据脚本后缀")] public string dataClassSuffix = "Data";
        [LabelText("容器脚本后缀")] public string containerClassSuffix = "Container";
        [LabelText("Custom脚本名")] public string customScriptName = "Custom";

        [ValueDropdown("AssemblyNameDropdown")] [LabelText("输出脚本所在程序集")]
        public string assemblyName = "Assembly-CSharp";

        private IEnumerable AssemblyNameDropdown()
        {
            var result = new ValueDropdownList<string>();
            var projectPath = FolderHelper.GetProjectPath();
            projectPath = projectPath.Replace('/', Path.DirectorySeparatorChar);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    if (!assembly.Location.Contains(projectPath))
                    {
                        Debug.Log($"[ExcelToolSetting:44]> {assembly.Location}");
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"[ExcelToolSetting:53]> {assembly.FullName}");
                }

                var aName = assembly.GetName().Name;
                if (
                    aName.Contains("mscorlib") ||
                    aName.Contains("netstandard") ||
                    aName.Contains("nunit.framework") ||
                    aName.Contains("Unity") ||
                    aName.StartsWith("System") ||
                    aName.StartsWith("Bee") ||
                    aName.StartsWith("Mono") ||
                    aName.StartsWith("Sirenix") ||
                    aName.StartsWith("JetBrains") ||
                    aName.StartsWith("ClosedXML") ||
                    aName.StartsWith("NuGetForUnity") ||
                    aName.StartsWith("NugetForUnity") ||
                    aName.StartsWith("DocumentFormat") ||
                    aName.StartsWith("Anonymously") ||
                    aName.StartsWith("PlayerBuildProgram") ||
                    aName.StartsWith("ExCSS") ||
                    aName.StartsWith("RBush") ||
                    aName.StartsWith("PPv2") ||
                    aName.StartsWith("ExcelNumberFormat") ||
                    aName.StartsWith("SixLabors")
                )
                {
                    continue;
                }

                result.Add(aName, aName);
            }

            return result;
        }
    }
}