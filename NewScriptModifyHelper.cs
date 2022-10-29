using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace Diselorya.UnityPlugin.ModifyScriptTemplate
{

    /// <summary>
    /// 辅助脚本内容的改写
    /// </summary>
    public class NewScriptModifyHelper
    {
        public static readonly string ClassNamePattern = "public class ([A-Za-z0-9_]+)\\s*:\\s*MonoBehaviour";
        public static readonly string NamespacePattern = @"namespace\s+([A-Za-z0-9.]*)";
        public static readonly string UsingPattern = @"^using\s+([A-Za-z0-9.]+);\s*";

        // 类变量会被其他线程反复重写，直接函数内解决
        //private List<string> Lines = new List<string>();
        //private List<string> References = new List<string>();
        //private List<string> LinesWithoutReferences = new List<string>();
        //private string NamespaceName = null;
        //private string ClassName = null;

        public virtual NewScriptModifyHelper Reset()
        {
            return this;
        }

        public NewScriptModifyHelper() { }


        public static ScriptModifyInfo GetData()
        {
            return AssetDatabase.LoadAssetAtPath<ScriptModifyInfo>(Configs.Path.NAMESPACE_DATA_ASSET_PATH);
        }

        public static bool ModifyScript(string path)
        {
            if(string.IsNullOrEmpty(path) || !File.Exists(path)) return false;
            try
            {
                // 获取用系统模板生成的脚本文件内容
                string[] lines = File.ReadAllLines(path);
                if (lines is null) return false;
                File.WriteAllText("D:\\temp\\unity\\testRead.txt", DateTime.Now + Environment.NewLine + String.Join(Environment.NewLine, lines));

                List<string> Lines = new List<string>();
                List<string> References = new List<string>();
                List<string> LinesWithoutReferences = new List<string>();
                string NamespaceName = null;
                string ClassName = null;

                var op = GetData();

                int i = -1;
                foreach (string line in lines)
                {
                    i++;
                    Match m;

                    // 检索 using 引用
                    m = Regex.Match(line, NewScriptModifyHelper.UsingPattern);
                    if (m.Success)
                    {
                        if (!References.Contains(m.Groups[1].Value))
                            References.Add(m.Groups[1].Value);
                        continue;
                    }
                    else LinesWithoutReferences.Add(line);

                    // 检索类名
                    m = Regex.Match(line, NewScriptModifyHelper.ClassNamePattern);
                    if (m.Success)
                    {
                        ClassName = m.Groups[1].Value;
                        continue;
                    }

                    // 检索命名空间
                    m = Regex.Match(line, NewScriptModifyHelper.NamespacePattern);
                    if (m.Success)
                    {
                        NamespaceName = m.Groups[1].Value;
                        continue;
                    }
                }

                foreach(string r in op.ReferencesToAdd)
                {
                    if (!References.Contains(r)) References.Add(r);
                }

                // 写入脚本新内容
                string code = GenerateScriptContent(LinesWithoutReferences, op.NamespaceName, References);
                File.WriteAllText(path, code);

                return true;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }
        }

        public static string GenerateScriptContent(IEnumerable<string> linesWithoutRefs, string newNamespace, IEnumerable<string> references)
        {
            if (!string.IsNullOrEmpty(newNamespace))
            {
                newNamespace = newNamespace.Replace(" ", "");
                linesWithoutRefs = ReplaceNamespace(linesWithoutRefs, newNamespace);
            }

            StringBuilder sb = new StringBuilder();

            // 模板原有的和新增的 using 引用
            if (references != null)
            {
                foreach (string reference in references)
                {
                    sb.Append($"using {reference};").AppendLine();
                }
            }
            sb.AppendLine();

            // 除了 using 引用之外的行
            foreach (string line in linesWithoutRefs)
            {
                sb.Append(line).AppendLine();
            }

            return sb.ToString();
        }


        public static void GetScriptInfo(IEnumerable<string> lines, out string className, out string namespaceName,
            out int lastUsingLine, out List<string> usings)
        {
            className = null;
            namespaceName = null;
            lastUsingLine = 0;
            usings = new List<string>();
            int i = -1;

            if (lines is null) return;

            foreach(string line in lines)
            {
                i++;
                Match m;

                // 检索 using 引用
                m = Regex.Match(line, NewScriptModifyHelper.UsingPattern);
                if (m.Success)
                {
                    lastUsingLine = i + 1;
                    usings.Add(m.Groups[1].Value);
                    continue;
                }

                // 检索类名
                m = Regex.Match(line, NewScriptModifyHelper.ClassNamePattern);
                if (m.Success)
                {
                    className = m.Groups[1].Value;
                    return;
                }

                // 检索命名空间
                m = Regex.Match(line, NewScriptModifyHelper.NamespacePattern);
                if(m.Success)
                {
                    className = m.Groups[1].Value;
                    continue;
                }
            }

        }

        public static string[] ReplaceNamespace(IEnumerable<string> lines, string newNamespace)
        {
            GetScriptInfo(lines, out string className, out string namespaceName, out int lastUsingLineNumber, out List<string> usings);
            List<string> newLines = new List<string>();
            newNamespace = newNamespace.Replace(" ", "");
            if (!string.IsNullOrEmpty(namespaceName))
            {
                foreach(string line in lines)
                {
                    var m = Regex.Match(line, NewScriptModifyHelper.NamespacePattern);
                    if (m.Success) newLines.Add(line.Replace(m.Groups[1].Value, newNamespace));
                    else newLines.Add(line);
                }
            }
            else
            {
                int i = -1;
                bool addIndent = false;
                foreach (string line in lines)
                {
                    i++;
                    if (i == lastUsingLineNumber)
                    {
                        newLines.Add($"namespace {newNamespace}");
                        newLines.Add("{");
                        addIndent = true;
                    }
                    else newLines.Add((addIndent ? "\t" : "") + line);
                }
                newLines.Add("}");
                addIndent = false;
            }

            return newLines.ToArray();
        }

        // 将设置保存到文件中
        public static ScriptModifyInfo SaveOptions(bool enable, string namespaceName = null, IEnumerable<string> referencesToAdd = null)
        {
            // 创建对象
            ScriptModifyInfo data = new ScriptModifyInfo();

            // 填充数据
            data.IsOn = enable;
            data.NamespaceName = namespaceName;

            data.ReferencesToAdd.Clear();
            //data.ReferencesSwitch.Clear();
            if (referencesToAdd != null)
            {
                foreach (var reference in referencesToAdd)
                {
                    data.ReferencesToAdd.Add(reference);
                    //data.ReferencesSwitch.Add(true);
                }
            }

            // 写入数据
            Directory.CreateDirectory(Configs.Path.NAMESPACE_DATA_ROOT_PATH);
            AssetDatabase.CreateAsset(data, Configs.Path.NAMESPACE_DATA_ASSET_PATH);

            return data;
        }

        public static bool IsOn()
        {
            ScriptModifyInfo data = NewScriptModifyHelper.GetData();
            if (data != null)
            {
                return data.IsOn;
            }

            return false;
        }
    }
}