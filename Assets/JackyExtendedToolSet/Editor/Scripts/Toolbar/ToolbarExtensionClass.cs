﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace JackyExtendedToolSet.Editor.Toolbar
{
    [InitializeOnLoad]
    public static class ToolbarExtensionClass
    {
#if UNITY_2019_3_OR_NEWER
        public const float space = 8;
#else
        public const float space = 10;
#endif
        public const float largeSpace = 20;
        public const float buttonWidth = 32;
        public const float dropdownWidth = 80;
#if UNITY_2019_1_OR_NEWER
        public const float playPauseStopWidth = 140;
#else
	public const float playPauseStopWidth = 100;
#endif

        public static int m_toolCount;
        public static GUIStyle m_commandStyle = null;

        public static readonly List<Action> LeftToolbarGUI = new();
        public static readonly List<Action> RightToolbarGUI = new();

        static ToolbarExtensionClass()
        {
            var toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
#if UNITY_2019_1_OR_NEWER
            const string fieldName = "k_ToolCount";
#else
			const string fieldName = "s_ShownToolIcons";
#endif
            var toolIcons = toolbarType.GetField(
                fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
            );

#if UNITY_2019_3_OR_NEWER
            m_toolCount = toolIcons != null ? ((int)toolIcons.GetValue(null)) : 8;
#elif UNITY_2019_1_OR_NEWER
			m_toolCount = toolIcons != null ? ((int) toolIcons.GetValue(null)) : 7;
#elif UNITY_2018_1_OR_NEWER
			m_toolCount = toolIcons != null ? ((Array) toolIcons.GetValue(null)).Length : 6;
#else
			m_toolCount = toolIcons != null ? ((Array) toolIcons.GetValue(null)).Length : 5;
#endif

            ToolbarExtensionCallback.OnToolbarGUI = OnGUI;
            ToolbarExtensionCallback.OnToolbarGUILeft = GUILeft;
            ToolbarExtensionCallback.OnToolbarGUIRight = GUIRight;
        }

        private static void OnGUI()
        {
            m_commandStyle ??= new GUIStyle("CommandLeft");

            var screenWidth = EditorGUIUtility.currentViewWidth;

            float playButtonsPosition = Mathf.RoundToInt((screenWidth - playPauseStopWidth) / 2);

            var leftRect = new Rect(0, 0, screenWidth, Screen.height);
            leftRect.xMin += space;
            leftRect.xMin += buttonWidth * m_toolCount;
#if UNITY_2019_3_OR_NEWER
            leftRect.xMin += space;
#else
			leftRect.xMin += largeSpace;
#endif
            leftRect.xMin += 64 * 2;
            leftRect.xMax = playButtonsPosition;

            var rightRect = new Rect(0, 0, screenWidth, Screen.height)
            {
                xMin = playButtonsPosition,
                xMax = screenWidth
            };
            rightRect.xMax -= space;
            rightRect.xMax -= dropdownWidth;
            rightRect.xMax -= space;
            rightRect.xMax -= dropdownWidth;
            rightRect.xMin += m_commandStyle.fixedWidth * 3;
#if UNITY_2019_3_OR_NEWER
            rightRect.xMax -= space;
#else
			rightRect.xMax -= largeSpace;
#endif
            rightRect.xMax -= dropdownWidth;
            rightRect.xMax -= space;
            rightRect.xMax -= buttonWidth;
            rightRect.xMax -= space;
            rightRect.xMax -= 78;

            leftRect.xMin += space;
            leftRect.xMax -= space;
            rightRect.xMin += space;
            rightRect.xMax -= space;
#if UNITY_2019_3_OR_NEWER
            leftRect.y = 4;
            leftRect.height = 22;
            rightRect.y = 4;
            rightRect.height = 22;
#else
			leftRect.y = 5;
			leftRect.height = 24;
			rightRect.y = 5;
			rightRect.height = 24;
#endif
            if (leftRect.width > 0)
            {
                GUILayout.BeginArea(leftRect);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                foreach (var handler in LeftToolbarGUI)
                {
                    handler();
                }

                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            if (rightRect.width > 0)
            {
                GUILayout.BeginArea(rightRect);
                GUILayout.BeginHorizontal();
                foreach (var handler in RightToolbarGUI)
                {
                    handler();
                }

                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }

        private static void GUILeft()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            foreach (var handler in LeftToolbarGUI)
            {
                GUILayout.Space(5);
                handler();
            }

            GUILayout.EndHorizontal();
        }

        private static void GUIRight()
        {
            GUILayout.BeginHorizontal();
            foreach (var handler in RightToolbarGUI)
            {
                GUILayout.Space(5);
                handler();
            }

            GUILayout.EndHorizontal();
        }
    }
}