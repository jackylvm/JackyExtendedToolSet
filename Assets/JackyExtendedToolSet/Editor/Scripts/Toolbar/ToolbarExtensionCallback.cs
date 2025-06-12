using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JackyExtendedToolSet.Editor.Toolbar
{
    public static class ToolbarExtensionCallback
    {
        public static Action OnToolbarGUI;
        public static Action OnToolbarGUILeft;
        public static Action OnToolbarGUIRight;

        private static Type ToolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static Type GUIViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GUIView");
        private static Type IWindowBackendType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.IWindowBackend");

        private static PropertyInfo WindowBackendProperty = GUIViewType.GetProperty(
            "windowBackend", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );

        private static PropertyInfo ViewVisualTreeProperty = IWindowBackendType.GetProperty(
            "visualTree", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );

        private static FieldInfo IMGUIContainerOnGUIField = typeof(IMGUIContainer).GetField(
            "m_OnGUIHandler", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );

        private static ScriptableObject CurrentToolbar;

        static ToolbarExtensionCallback()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        static void OnGUI()
        {
            var handler = OnToolbarGUI;
            if (handler != null)
            {
                handler();
            }
        }

        private static void OnUpdate()
        {
            if (CurrentToolbar) return;

            var toolbars = Resources.FindObjectsOfTypeAll(ToolbarType);
            CurrentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;

            if (!CurrentToolbar) return;

#if UNITY_2021_1_OR_NEWER
            var root = CurrentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            var rawRoot = root.GetValue(CurrentToolbar);
            var mRoot = rawRoot as VisualElement;
            RegisterCallback("ToolbarZoneLeftAlign", OnToolbarGUILeft);
            RegisterCallback("ToolbarZoneRightAlign", OnToolbarGUIRight);

            void RegisterCallback(string root, Action cb)
            {
                var toolbarZone = mRoot.Q(root);

                var parent = new VisualElement()
                {
                    style =
                    {
                        flexGrow = 1,
                        flexDirection = FlexDirection.Row,
                    }
                };
                var container = new IMGUIContainer();
                container.style.flexGrow = 1;
                container.onGUIHandler += () => { cb?.Invoke(); };
                parent.Add(container);
                toolbarZone.Add(parent);
            }
#else
#if UNITY_2020_1_OR_NEWER
					var windowBackend = WindowBackendProperty.GetValue(CurrentToolbar);
					var visualTree = (VisualElement) ViewVisualTreeProperty.GetValue(windowBackend, null);
#else
					var visualTree = (VisualElement) ViewVisualTreeProperty.GetValue(CurrentToolbar, null);
#endif
					var container = (IMGUIContainer) visualTree[0];
					var handler = (Action) IMGUIContainerOnGUIField.GetValue(container);
					handler -= OnGUI;
					handler += OnGUI;
					IMGUIContainerOnGUIField.SetValue(container, handler);
#endif
        }
    }
}