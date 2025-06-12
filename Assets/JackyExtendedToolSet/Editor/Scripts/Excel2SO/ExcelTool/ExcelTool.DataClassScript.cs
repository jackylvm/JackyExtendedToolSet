using System.Collections.Generic;
using System.IO;
using System.Text;
using JackyExtendedToolSet.Runtime;
using UnityEngine;

namespace JackyExtendedToolSet.Editor.Excel2SO
{
    public static partial class ExcelTool
    {
        private static string CreateDataClassScript(
            string className, List<string> fieldNames, List<string> dataTypes, List<string> comments, string excelPath, string outputPath
        )
        {
            var listFlag = false;
            var classMembers = new StringBuilder();
            for (var i = 0; i < fieldNames.Count; i++)
            {
                var fieldName = fieldNames[i];
                if (string.IsNullOrEmpty(fieldName))
                {
                    if (dataTypes[i].Contains("notation"))
                    {
                        continue;
                    }
                }

                var dataType = MapDataTypeToString(dataTypes[i]);
                if (dataType.Contains("List<"))
                {
                    listFlag = true;
                }

                var comment = i < comments.Count ? comments[i] : "";

                if (!string.IsNullOrEmpty(comment))
                {
                    classMembers.Append($"        /// <summary> {comment} </summary>\n");
                }

                classMembers.Append($"        public {dataType} {fieldName};\n");

                if (i < fieldNames.Count - 1)
                {
                    classMembers.Append("\n");
                }
            }

            var usingContent = new StringBuilder("using System;\n");
            if (listFlag)
            {
                usingContent.Append("using System.Collections.Generic;\n");
            }

            usingContent.Append("\n");

            var classHead = new StringBuilder($"namespace {_excelToolSetting.scriptNamespace}\n");
            classHead.Append("{\n");
            classHead.Append("    /// 由Excel自动生成的数据类\n");
            classHead.Append("    /// 源文件: " + Path.GetFileName(excelPath) + "\n");
            classHead.Append("    /// </summary>\n");
            classHead.Append("    [Serializable]\n");
            classHead.Append("    public partial class " + className + "\n");
            classHead.Append("    {\n");

            var scriptContent = usingContent.ToString() + classHead + classMembers + "    }\n}\n";

            var dataPath = Path.Combine(outputPath, "Data");
            FolderHelper.EnsureFolderExists(dataPath);
            var scriptPath = Path.Combine(dataPath, $"{className}.cs");
            scriptPath = scriptPath.Replace(Application.dataPath, "Assets");

            File.WriteAllText(scriptPath, scriptContent);

            var customPath = Path.Combine(outputPath, "Custom");
            FolderHelper.EnsureFolderExists(customPath);
            var customScriptPath = Path.Combine(customPath, $"{className}.{_excelToolSetting.customScriptName}.cs");
            var a = FolderHelper.CheckFileExists(FolderHelper.GetProjectPath(), customScriptPath);
            if (!a)
            {
                var customScript = new StringBuilder($"namespace {_excelToolSetting.scriptNamespace}\n");
                customScript.Append("{\n");
                customScript.Append("    public partial class " + className + "\n");
                customScript.Append("    {\n    }\n}\n");
                customScriptPath = customScriptPath.Replace(Application.dataPath, "Assets");
                File.WriteAllText(customScriptPath, customScript.ToString());
            }

            return scriptPath;
        }
    }
}