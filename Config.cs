namespace Diselorya.UnityPlugin.ModifyScriptTemplate
{
    /// <summary>
    /// 路径配置，根据自己实际路径进行配置
    /// </summary>
    public class Configs
    {
        public class Path
        {
            public static readonly string NAMESPACE_DATA_ROOT_PATH = "Assets/ModifyScriptTemplate/Editor/Cache/";
            public static readonly string NAMESPACE_DATA_ASSET_PATH = NAMESPACE_DATA_ROOT_PATH + "Data.asset";
        }

        public class Namespace
        {
            public static readonly string Default = "Diselorya.UnityGame";
        }
    }
}