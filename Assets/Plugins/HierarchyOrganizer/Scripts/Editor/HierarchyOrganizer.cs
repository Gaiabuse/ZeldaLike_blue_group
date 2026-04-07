#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


[InitializeOnLoad]
public static class HierarchyOrganizer
{
     private static HierarchyData data;
    private static bool initialized = false;
    

    private static readonly Dictionary<int, bool> validationCache = new Dictionary<int, bool>();

    static HierarchyOrganizer()
    {
        Initialize();
        EditorApplication.hierarchyChanged += ClearCache;
    }

    private static void ClearCache() => validationCache.Clear();

    public static void Initialize()
    {
        if (initialized)
            EditorApplication.hierarchyWindowItemOnGUI -= SetDataVisuel;

        initialized = false;
        data = Load();

        if (data == null) return;

        initialized = true;
        if (data.enabled)
            EditorApplication.hierarchyWindowItemOnGUI += SetDataVisuel;

        EditorApplication.RepaintHierarchyWindow();
    }

    static void SetDataVisuel(int instanceID, Rect selectionRect)
    {
        if (EditorApplication.isPlaying) return;
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;
        
        if (data.separator.enabled && obj.name.StartsWith(data.separator.startString))
        {
            Color themeColor = EditorGUIUtility.isProSkin ? 
                new Color(0.219f, 0.219f, 0.219f) : // Dark Mode
                new Color(0.784f, 0.784f, 0.784f);   // Light Mode
            EditorGUI.DrawRect(selectionRect, themeColor);
            Rect fullRect = new Rect(32, selectionRect.y, selectionRect.width + (selectionRect.x - 32), selectionRect.height);
            EditorGUI.DrawRect(fullRect, data.separator.color);
            
            EditorGUI.LabelField(fullRect, obj.name.ToUpper(), data.separator.style);
            return; 
        }

     
        if (data.enabledSpecificFiles && data.objects != null)
        {
            foreach (var specificFile in data.objects)
            {
                bool match = specificFile.byTag && obj.CompareTag(specificFile.targetTag);
                
                if (specificFile.byScripts)
                {
                    match = CheckScriptsWithCache(obj, instanceID, specificFile);
                }

                if (match)
                {
                    SetSpecificFileVisuel(specificFile, selectionRect, obj);
                    break;
                }
            }
        }
    }

    static bool CheckScriptsWithCache(GameObject obj, int id, HierarchyData.SpecificFile file)
    {
        int cacheKey = id ^ file.GetHashCode(); 

        if (validationCache.TryGetValue(cacheKey, out bool hasScripts))
            return hasScripts;

        bool allPresent = true;
        foreach (MonoScript ms in file.targetObjects)
        {
            if (ms == null) continue;
            var scriptType = ms.GetClass();
            if (scriptType == null || obj.GetComponent(scriptType) == null)
            {
                allPresent = false;
                break;
            }
        }

        validationCache[cacheKey] = allPresent;
        return allPresent;
    }

    static void SetSpecificFileVisuel(HierarchyData.SpecificFile file, Rect selectionRect, GameObject obj)
    {
        Rect fullRect = new Rect(32, selectionRect.y, selectionRect.width + (selectionRect.x - 32), selectionRect.height);
        
        if (file.withColor)
        {
            Color themeColor = EditorGUIUtility.isProSkin ? 
                new Color(0.219f, 0.219f, 0.219f) : // Dark Mode
                new Color(0.784f, 0.784f, 0.784f);   // Light Mode
            EditorGUI.DrawRect(selectionRect, themeColor);
            EditorGUI.DrawRect(fullRect, file.color);
            EditorGUI.LabelField(fullRect, obj.name, file.style);
        }

        if (file.withIcon && file.icon != null)
        {
            Rect iconRect = new Rect(fullRect.width-5, selectionRect.y, 18, 18);
            GUI.Label(iconRect, file.icon);
        }
    }


    [MenuItem("Tools/Custom Hierarchy/Initialize or Create", priority = 1)]
    public static void InitializeOrCreate()
    {
        if (Load()) 
        {
            Initialize();
            SelectData();
        }
        else
        {
            if (EditorUtility.DisplayDialog("Custom Hierarchy", "Do you want to create an Hierarchy Icon Data?", "Yes", "No"))
            {
                CreateAsset();
            }
            else
            {
                Debug.Log("Hierarchy Icon: Data creation was canceled.");
            }
        }
    }
    private const string fileName = "HierarchyData";
    static void CreateAsset()
    {
        if (Load())
        {
            Debug.LogWarning("HierarchyIcons: Data already exists, won't create a new one.");
            return;
        }
        if(!AssetDatabase.IsValidFolder("Assets/Editor Default Resources"))
            AssetDatabase.CreateFolder("Assets", "Editor Default Resources");

        string path = "Assets/Editor Default Resources";
        if (!AssetDatabase.IsValidFolder("Assets/Editor Default Resources"))
        {
            string guid = AssetDatabase.CreateFolder("Assets","Editor Default Resources");
            path = AssetDatabase.GUIDToAssetPath(guid);
        }

        try
        {
            var asset = ScriptableObject.CreateInstance<HierarchyData>();
            AssetDatabase.CreateAsset(asset, path + $"/{fileName}.asset");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        AssetDatabase.SaveAssets();

        Initialize();
        SelectData();
        Debug.Log("Hierarchy Data asset was created in the 'Assets/Editor Default Resources folder.");
    }
    static HierarchyData Load()
    {
        var result = EditorGUIUtility.Load($"Assets/{fileName}.asset") as HierarchyData;
        if (result != null)
            return result;

        var guids = UnityEditor.AssetDatabase.FindAssets("t:" + nameof(HierarchyData));
        if (guids.Length == 0)
            return null;

        return AssetDatabase.LoadAssetAtPath<HierarchyData>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }
    
    
    static bool SelectData()
    {
        var loaded = Load();
        if (loaded != null)
        {
            Selection.activeObject = loaded;
            return true;
        }

        return false;
    }

}
#endif