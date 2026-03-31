using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "HierarchyData", menuName = "Hierarchy Data", order = 1)]
public class HierarchyData : ScriptableObject
{
    public bool enabled = true;
    #region separator 
    [System.Serializable]
    public class SeparatorData
    {
        public bool enabled = true;
        public string startString = "-";
        public Color color = new Color(0, 1,1, .15f);
        public GUIStyle style = new GUIStyle();
    }
        
    public SeparatorData separator;
    #endregion
    #region specific files
    public bool enabledSpecificFiles = true;
    [System.Serializable]
    public class SpecificFile
    {
        public bool byTag;
        public string targetTag = "Untagged";
        public bool byScripts;
        public MonoScript[] targetObjects;
        
        public bool withIcon = false;
        public Texture2D icon = null;
        public bool withColor = false;
        public Color color = Color.red;
        public GUIStyle style = new GUIStyle();
    }
    
    public SpecificFile[] objects = new SpecificFile[0];
    #endregion
    
}
