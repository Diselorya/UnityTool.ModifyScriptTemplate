using UnityEngine;
using System.Collections.Generic;

namespace Diselorya.UnityPlugin.ModifyScriptTemplate
{

    [System.Serializable]
    public class ScriptModifyInfo : ScriptableObject
    {
        [SerializeField]
        public string NamespaceName;

        [SerializeField]
        public bool IsOn;

        [SerializeField]
        public List<string> ReferencesToAdd = new List<string>();

        //[SerializeField]
        //public List<bool> ReferencesSwitch = new List<bool>();
    }
}