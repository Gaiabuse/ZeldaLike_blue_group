using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


public class ObjectPainter : EditorWindow
{
    private bool hitSomething;
    private Vector3 hitPoint;
    private Vector3 hitNormal;

    private GameObject prefab;

    private Transform parent;
    private bool rangeDrawer;
    private float rotation;
    private float scale;
    private float minRotation;
    private float maxRotation;
    private float minScale;
    private float maxScale;
    private bool fixRotation;
    private bool fixScale;
    private string tagHandle;
    private string tagOfTheObject;
    private float range;
    private float minNumber;
    private float maxNumber;
    private void OnEnable()
    {
        SceneView.duringSceneGui += GetPositionInWorld;
        tagHandle = PlayerPrefs.GetString("tagHandle");
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= GetPositionInWorld;
    }
    [MenuItem("Tools/ObjectPainter")]
    public static void ShowWindow()
    {
        EditorWindow painter = GetWindow(typeof(ObjectPainter));
        painter.titleContent = new GUIContent("Object Painter");
    }
    private void GetPositionInWorld(SceneView sceneView)
    {
        if (Event.current.type != EventType.MouseMove && Event.current.type != EventType.Repaint && Event.current.type != EventType.MouseDown)
            return;
        Event e = Event.current;
        if(e.type == EventType.MouseDown && prefab != null)
        {
            if (e.button == 0)
            {
               CreateObject();
            }
        }
        if (e.type == EventType.MouseMove)
        {
            Ray raycast = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(raycast, out RaycastHit hit))
            {
                if (hit.transform.gameObject.CompareTag(tagHandle))
                {
                    hitPoint = hit.point;
                    hitNormal = hit.normal;
                    hitSomething = true;
                }
            }
            else
            {
                hitSomething = false;
            }
           
            sceneView.Repaint();
        }

     

        if (e.type == EventType.Repaint &&  hitSomething)
        {
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(hitPoint, hitNormal, 1f);
        }
       
    }
    
    
    private void CreateObject()
    {
        Undo.IncrementCurrentGroup();
        Object theObject = PrefabUtility.InstantiatePrefab(prefab);
        if (!fixRotation)
        {
            rotation = Random.Range(minRotation, maxRotation);
        }

        if (!fixScale)
        {
            scale = Random.Range(minScale, maxScale);
        }
        Undo.RegisterCreatedObjectUndo(theObject, "Create my GameObject");
        if (theObject != null && theObject is GameObject obj)
        {
            obj.transform.position = hitPoint;
            if (parent != null)
            {
                obj.transform.SetParent(parent);
            }
            obj.transform.localScale = new Vector3(scale, scale, scale);
            obj.transform.localRotation = Quaternion.FromToRotation(Vector3.up, hitNormal);
            var transformRotation = obj.transform.rotation;
            var angles = transformRotation.eulerAngles;
            angles.y = rotation;
            transformRotation.eulerAngles = angles; 
            obj.transform.rotation = transformRotation;
            obj.tag = tagOfTheObject;
        }
    }

    private void OnGUI()
    {
        ObjectPainterGUI();
    }

    private void ObjectPainterGUI()
    {
        GUILayout.Label("Object Painter", EditorStyles.boldLabel);
        tagHandle = EditorGUILayout.TagField("Tag for paint : ", tagHandle);
        prefab = EditorGUILayout.ObjectField("Object at paint : ", prefab, typeof(GameObject), false) as GameObject;
        parent = EditorGUILayout.ObjectField("Object parent : ", parent, typeof(Transform), true) as Transform;
        fixRotation = EditorGUILayout.Toggle("Fix Rotation : ", fixRotation);
        if (fixRotation)
        {
            rotation = EditorGUILayout.FloatField("Rotation : ", rotation);
        }
        else
        {
            minRotation = EditorGUILayout.Slider("Min Rotation : ", minRotation, 0f, maxRotation);
            maxRotation = EditorGUILayout.Slider("Max Rotation : ", maxRotation, minRotation, 360f);
        }
        fixScale = EditorGUILayout.Toggle("Fix Scale : ", fixScale);
        if (fixScale)
        {
            scale = EditorGUILayout.FloatField("Scale : ", scale);
        }
        else
        {
            minScale = EditorGUILayout.Slider("Min Scale : ", minScale, 0f, maxScale);
            maxScale = EditorGUILayout.FloatField("Max Scale : ", maxScale);
        }
        tagOfTheObject = EditorGUILayout.TagField("Tag of the object : ", tagOfTheObject);
        PlayerPrefs.SetString("tagHandle", tagHandle);
        PlayerPrefs.Save();
    }

   

}
