using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Diselorya.UnityPlugin.ModifyScriptTemplate
{

    /// <summary>
    /// 自动添加命名空间的脚本
    /// 其实是重写脚本内容
    /// </summary>
    public class CreateScriptEventHandle : UnityEditor.AssetModificationProcessor
    {
        private static void OnWillCreateAsset(string path)
        {
            if (!NewScriptModifyHelper.IsOn())
                return;

            path = path.Replace(".meta", "");
            if (path.EndsWith(".cs"))
            {
                NewScriptModifyHelper.ModifyScript(path);

                // 刷新会导致再次调用 OnWillCreateAsset 事件
                AssetDatabase.Refresh();
            }

        }
    }
}