using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Diselorya.UnityPlugin.ModifyScriptTemplate
{

    /// <summary>
    /// 自动添加命名空间的，可视化设置窗口脚本
    /// </summary>
    public class MSTToolEditorWindow : EditorWindow
    {
        private static string _name;
        public static bool _isOn;
        protected static string _refs;
        protected static List<string> _references = new List<string>();

        [MenuItem("Tools/ModifyScriptTemplate")]
        public static void OpenWindow()
        {
            var window = GetWindow(typeof(MSTToolEditorWindow));
            window.minSize = new Vector2(500, 300);
            window.Show();
            Init();
        }

        private static void Init()
        {
            ScriptModifyInfo data = NewScriptModifyHelper.GetData();
            if (data != null)
            {
                _name = data.NamespaceName;
                _isOn = data.IsOn;
                _references.Clear();
                _references.AddRange(data.ReferencesToAdd);
                _refs = string.Join(';', _references);
            }
        }

        private void OnGUI()
        {
            _isOn = GUILayout.Toggle(_isOn, "自动修改新建脚本模板");
            //GUILayout.FlexibleSpace();
            GUILayout.Space(10.0f);

            GUILayout.Label("默认命名空间名称：");
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(200));
            _name = EditorGUI.TextField(rect, _name);

            GUILayout.Label("自动引用命名空间：");
            Rect rectNs = EditorGUILayout.GetControlRect(GUILayout.Width(200));
            _references.Clear();
            _refs = EditorGUI.TextField(rectNs, _refs);
            if(!string.IsNullOrEmpty(_refs))
                _references.AddRange(_refs.Replace(" ", "").Split(',', ';'));


            if (GUILayout.Button("应用", GUILayout.MaxWidth(100)))
            {
                NewScriptModifyHelper.SaveOptions(_isOn, _name, _references);
            }
        }
    }
}