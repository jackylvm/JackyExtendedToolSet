using JackyExtendedToolSet.Editor.Toolbar;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace JackyExtendedToolSet.Example
{
    [InitializeOnLoad]
    public class ToolbarExample
    {
        static ToolbarExample()
        {
            ToolbarExtensionClass.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            if (GUILayout.Button("测试场景", GUILayout.Width(96)))
            {
                EditorSceneManager.OpenScene("Assets/JackyExtendedToolSet/Example/Scenes/ExampleScene.unity");
            }
        }
    }
}