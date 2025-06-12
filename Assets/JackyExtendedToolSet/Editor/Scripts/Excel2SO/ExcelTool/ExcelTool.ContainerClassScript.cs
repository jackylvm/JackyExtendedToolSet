using System.IO;
using System.Text;
using JackyExtendedToolSet.Runtime;
using UnityEngine;

namespace JackyExtendedToolSet.Editor.Excel2SO
{
    public static partial class ExcelTool
    {
        // 创建容器类脚本
        private static string CreateContainerClassScript(string className, string dataClassName, string excelPath, string outputPath)
        {
            var usingContent = new StringBuilder("using UnityEngine;\n");
            usingContent.Append("using System.Collections.Generic;\n");
            usingContent.Append("\n");

            var classHead = new StringBuilder($"namespace {_excelToolSetting.scriptNamespace}\n");
            classHead.Append("{\n");
            classHead.Append("    /// <summary>\n");
            classHead.Append("    /// 由Excel自动生成的数据类\n");
            classHead.Append("    /// 源文件: " + Path.GetFileName(excelPath) + "\n");
            classHead.Append("    /// </summary>\n");
            classHead.Append("    [CreateAssetMenu(fileName = ");
            classHead.Append("\"" + className + "\", menuName = \"ScriptableObjects/" + className + "\")]\n");
            classHead.Append("    public partial class " + className + " : ScriptableObject\n");
            classHead.Append("    {\n");
            classHead.Append("        public List<" + dataClassName + "> dataList = new();\n");
            classHead.Append("    }\n}\n");

            var scriptContent = usingContent.ToString() + classHead;

            var containerPath = Path.Combine(outputPath, "Container");
            FolderHelper.EnsureFolderExists(containerPath);
            var scriptPath = Path.Combine(containerPath, $"{className}.cs");
            scriptPath = scriptPath.Replace(Application.dataPath, "Assets");

            File.WriteAllText(scriptPath, scriptContent);

            var customPath = Path.Combine(outputPath, "Custom");
            FolderHelper.EnsureFolderExists(customPath);
            var customScriptPath = Path.Combine(customPath, $"{className}.{_excelToolSetting.customScriptName}.cs");
            var a = FolderHelper.CheckFileExists(FolderHelper.GetProjectPath(), customScriptPath);
            if (!a)
            {
                usingContent = new StringBuilder("using UnityEngine;\n\n");
                var customScript = new StringBuilder($"namespace {_excelToolSetting.scriptNamespace}\n");
                customScript.Append("{\n");
                customScript.Append("    public partial class " + className + " : ScriptableObject\n");
                customScript.Append("    {\n    }\n}\n");
                customScriptPath = customScriptPath.Replace(Application.dataPath, "Assets");

                scriptContent = usingContent.ToString() + customScript;
                File.WriteAllText(customScriptPath, scriptContent);
            }

            return scriptPath;
        }
    }
}