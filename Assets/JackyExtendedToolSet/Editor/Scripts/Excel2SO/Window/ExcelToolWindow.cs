using System.IO;
using JackyExtendedToolSet.Runtime;
using JackyExtendedToolSet.Runtime.Excel2SO;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace JackyExtendedToolSet.Editor.Excel2SO
{
    public partial class ExcelToolWindow : OdinMenuEditorWindow
    {
        [MenuItem("Jacky的工具集/Excel2SO/Excel列表")]
        private static void ShowWindow()
        {
            var window = GetWindow<ExcelToolWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1024, 768);
            window.titleContent = new GUIContent("Excel列表");
            window.Show();
        }

        private ExcelToolSetting _setting;

        protected override OdinMenuTree BuildMenuTree()
        {
            var menuTree = new OdinMenuTree(false)
            {
                Config =
                {
                    DrawSearchToolbar = true,
                    AutoScrollOnSelectionChanged = false,
                    SearchToolbarHeight = 28
                }
            };

            _setting = SOCreator.GetOrCreateSO<ExcelToolSetting>("Assets/ExcelToolSetting.asset");
            if (string.IsNullOrEmpty(_setting.excelPath))
            {
                if (EditorUtility.DisplayDialog("警告!", "没有指定Excel文件夹", "确定"))
                {
                    ExcelToolSettingWindow.ShowWindow();
                }

                return menuTree;
            }

            if (string.IsNullOrEmpty(_setting.outputPath01) || string.IsNullOrEmpty(_setting.outputPath02))
            {
                if (EditorUtility.DisplayDialog("警告!", "没有指定输出文件夹", "确定"))
                {
                    ExcelToolSettingWindow.ShowWindow();
                }

                return menuTree;
            }

            if (
                string.IsNullOrEmpty(_setting.scriptNamespace) ||
                string.IsNullOrEmpty(_setting.dataClassSuffix) ||
                string.IsNullOrEmpty(_setting.containerClassSuffix) ||
                string.IsNullOrEmpty(_setting.customScriptName)
            )
            {
                if (EditorUtility.DisplayDialog("警告!", "没有指定命名空间、脚本后缀、容器脚本后缀、自定义脚本名", "确定"))
                {
                    ExcelToolSettingWindow.ShowWindow();
                }

                return menuTree;
            }

            var excelFolder = Path.Combine(FolderHelper.GetProjectPath(), _setting.excelPath);
            var excelFiles = FolderHelper.GetExcelFiles(excelFolder);
            if (excelFiles == null || excelFiles.Length == 0)
            {
                EditorUtility.DisplayDialog("警告!", "没有找到Excel文件", "确定");
                return menuTree;
            }

            menuTree.Add("Excel列表", null);
            foreach (var excelFile in excelFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(excelFile);
                var menuItem = new OdinMenuItem(
                    menuTree,
                    fileName,
                    new ExcelFileWrapper(excelFile, fileName, _setting.outputPath01, _setting.outputPath02)
                );
                menuTree.AddMenuItemAtPath($"Excel列表", menuItem);
            }

            return menuTree;
        }

        protected override void OnBeginDrawEditors()
        {
            if (MenuTree == null)
                return;

            var toolbarHeight = MenuTree.Config.SearchToolbarHeight;

            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            {
                if (SirenixEditorGUI.ToolbarButton(new GUIContent(">刷新左侧列表<")))
                {
                    ForceMenuTreeRebuild();
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent(">打开Excel文件夹<")))
                {
                    var excelPath = Path.Combine(FolderHelper.GetProjectPath(), _setting.excelPath);
                    Application.OpenURL(excelPath);
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent(">删除所有生成的脚本文件<")))
                {
                    DeleteAllDataClassScriptableObjects();
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent(">删除所有生成表数据<")))
                {
                    DeleteAllContainerClassScriptableObjects();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();

            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            {
                if (SirenixEditorGUI.ToolbarButton(new GUIContent(">1.创建所有数据结构<")))
                {
                    CreateAllDataClassScriptableObjects();
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent(">2.导出所有Excel表<")))
                {
                    ImportAllExcelData();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();

            base.OnBeginDrawEditors();
        }
    }
}