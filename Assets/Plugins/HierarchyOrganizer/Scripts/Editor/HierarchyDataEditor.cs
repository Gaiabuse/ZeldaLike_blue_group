using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HierarchyData))]
public class HierarchyDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("Separator Settings", EditorStyles.boldLabel);
        DrawDefaultInspectorSection("separator");

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        SerializedProperty enabledProp = serializedObject.FindProperty("enabledSpecificFiles");
        EditorGUILayout.PropertyField(enabledProp, new GUIContent("Enable Specific Files ? "));

        if (enabledProp.boolValue)
        {
            SerializedProperty objectsProp = serializedObject.FindProperty("objects");

            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            // On utilise isExpanded pour stocker l'état ouvert/fermé du groupe
            objectsProp.isExpanded = EditorGUILayout.Foldout(objectsProp.isExpanded, "Object List", true, EditorStyles.foldoutHeader);
            
            if (GUILayout.Button("+ Add", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                objectsProp.arraySize++;
                objectsProp.isExpanded = true;
            }
            EditorGUILayout.EndHorizontal();
            
            if (objectsProp.isExpanded)
            {
                EditorGUILayout.Space(2);
                EditorGUI.indentLevel++; 

                for (int i = 0; i < objectsProp.arraySize; i++)
                {
                    SerializedProperty element = objectsProp.GetArrayElementAtIndex(i);
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Element {i}", EditorStyles.miniBoldLabel);
                    
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        objectsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.PropertyField(element, true);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
                
                EditorGUI.indentLevel--;
            }
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
    
    private void DrawDefaultInspectorSection(string propertyName)
    {
        SerializedProperty prop = serializedObject.FindProperty(propertyName);
        if (prop != null)
        {
            EditorGUILayout.PropertyField(prop, true);
        }
    }
}
[CustomPropertyDrawer(typeof(HierarchyData.SeparatorData))]
public class SeparatorEditor : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        #region setProperty

        SerializedProperty enabled = property.FindPropertyRelative("enabled");
        SerializedProperty startString = property.FindPropertyRelative("startString");
        SerializedProperty color = property.FindPropertyRelative("color");
        SerializedProperty style = property.FindPropertyRelative("style");

        #endregion
        EditorGUILayout.PropertyField(enabled, new GUIContent("Enabled ? "));
        if (enabled.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(startString, new GUIContent("Start String : "));
            EditorGUILayout.PropertyField(color, new GUIContent("Background Color : "));
            EditorGUILayout.PropertyField(style, new GUIContent("Style : "));
            EditorGUI.indentLevel--;
        }
        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(HierarchyData.SpecificFile))]
public class SpecificFileEditor : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0; 
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        
        EditorGUI.BeginProperty(position, label, property);

        #region SetProperty

        SerializedProperty byTag = property.FindPropertyRelative("byTag");
        SerializedProperty targetTag = property.FindPropertyRelative("targetTag");
        SerializedProperty byScripts = property.FindPropertyRelative("byScripts");
        SerializedProperty targetObject = property.FindPropertyRelative("targetObjects");
        SerializedProperty enabled = property.FindPropertyRelative("enabled");
        SerializedProperty withIcon = property.FindPropertyRelative("withIcon");
        SerializedProperty icon = property.FindPropertyRelative("icon");
        SerializedProperty withColor = property.FindPropertyRelative("withColor");
        SerializedProperty color = property.FindPropertyRelative("color");
        SerializedProperty style = property.FindPropertyRelative("style");
        #endregion
        
        #region byTag
        EditorGUILayout.PropertyField(byTag, new GUIContent("Filter by Tag ? "));
        if (byTag.boolValue)
        {
            EditorGUI.indentLevel++;
            targetTag.stringValue = EditorGUILayout.TagField(
                new GUIContent("Choose Tag :"), 
                targetTag.stringValue
            );
            EditorGUI.indentLevel--;
        }
        #endregion
        
        #region byScripts
        EditorGUILayout.PropertyField(byScripts, new GUIContent("Filter by Scripts ? "));

        if (byScripts.boolValue)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            targetObject.isExpanded = EditorGUILayout.Foldout(targetObject.isExpanded, "Associated Scripts", true);
            
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+ Add", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                targetObject.arraySize++;
                targetObject.GetArrayElementAtIndex(targetObject.arraySize - 1).objectReferenceValue = null;
                targetObject.isExpanded = true;
            }
            EditorGUILayout.EndHorizontal();
            
            if (targetObject.isExpanded)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < targetObject.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    SerializedProperty element = targetObject.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(element, new GUIContent($"Script {i}"));

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        targetObject.DeleteArrayElementAtIndex(i);
                        EditorGUILayout.EndHorizontal();
                        break; 
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.Space(5);
            }
            else 
            {
                EditorGUILayout.LabelField($"({targetObject.arraySize} scripts enregistrés)", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.EndVertical();
        }
        #endregion
        
        EditorGUI.indentLevel++;

        #region Color

        EditorGUILayout.PropertyField(withColor, new GUIContent("Use Color ? "));
        if (withColor.boolValue)
        {
            EditorGUILayout.PropertyField(color, new GUIContent("Background Color : "));
            EditorGUILayout.PropertyField(style, new GUIContent("Style : "));
        }

        #endregion
     
        #region Icon
        EditorGUILayout.PropertyField(withIcon, new GUIContent("Use Icon ?"));
        if (withIcon.boolValue)
        {
            EditorGUILayout.PropertyField(icon, new GUIContent("Icon : "));
            icon.objectReferenceValue = EditorGUILayout.ObjectField(
                "Icon Texture", 
                icon.objectReferenceValue, 
                typeof(Texture2D), 
                false
            );
        }
        #endregion

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }
}



