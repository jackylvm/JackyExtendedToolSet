// ***********************************************************************************
// FileName: AssemblyReloader.cs
// Description:
// 
// Version: v1.0.0
// Creator: Jacky(jackylvm@foxmail.com)
// CreationTime: 2025-06-11 22:12:12
// ==============================================
// History update record:
// 
// ==============================================
// *************************************************************************************

using System;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace JackyExtendedToolSet.Runtime
{
    public static class AssemblyReloader
    {
        /// <summary>
        /// 重新加载所有脚本
        /// </summary>
        public static void ReloadAllAssemblies()
        {
            // 标记需要重新加载
            EditorUtility.RequestScriptReload();
        }

        public static void RequestScriptCompilation()
        {
            // 标记需要重新编译
            CompilationPipeline.RequestScriptCompilation();
        }

        // 编译完成
        public static void RequestScriptCompilation(Action onCompilationComplete)
        {
            CompilationPipeline.compilationFinished += OnCompilationFinished;

            _onCompilationComplete = onCompilationComplete;

            // 标记需要重新编译
            CompilationPipeline.RequestScriptCompilation();
        }

        private static Action _onCompilationComplete;

        private static void OnCompilationFinished(object obj)
        {
            // 取消注册回调
            CompilationPipeline.compilationFinished -= OnCompilationFinished;

            // 执行完成回调
            _onCompilationComplete?.Invoke();

            Debug.Log($"AssemblyReloader:59>编译完成!");
        }
    }
}