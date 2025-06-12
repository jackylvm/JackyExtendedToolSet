using JackyExtendedToolSet.Runtime;
using JackyExtendedToolSet.Runtime.Excel2SO;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace JackyExtendedToolSet.Editor.Excel2SO
{
    public class ExcelToolSettingWindow : OdinMenuEditorWindow
    {
        [MenuItem("Jacky的工具集/Excel2SO/Setting")]
        public static void ShowWindow()
        {
            var window = GetWindow<ExcelToolSettingWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(600, 256);
            window.titleContent = new GUIContent("配置项窗口");
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
            menuTree.Add("配置选项", _setting);
            
            return menuTree;
        }
    }
}