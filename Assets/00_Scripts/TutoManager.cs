using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TutoManager : MonoBehaviour
{
    [SerializeField]private FormSwitcher formSwitcher;
    [SerializeField]private Textbox textBox;
    [SerializeField]private ErasedManager erasedManager;
    [SerializeField]private TutoStep[] steps;

    private void OnEnable()
    {
        foreach (TutoStep tutoStep in steps)
        {
            tutoStep.OnEnableStep(formSwitcher, textBox, erasedManager);
        }
    }

    private void OnDisable()
    {
        foreach (TutoStep tutoStep in steps)
        {
            tutoStep.OnDisableStep();
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


[Serializable]
public class TutoStep
{
    [SerializeField] private TriggerTuto colliderTrigger;
    [SerializeField]private List<Form> disponibleForms = new List<Form>();
    [SerializeField] private bool setForm;
    [SerializeField] private Form form;
    [SerializeField] private bool asDialogue;
    [TextArea(0,5)][SerializeField]private string dialogue;
    [SerializeField] private bool setNumberOfPointsForErased;
    [SerializeField] private int numberOfPointsForErased;
    private FormSwitcher _formSwitcher;
    private Textbox _textbox;
    private ErasedManager _erasedManager;
    public void OnEnableStep(FormSwitcher formSwitcher, Textbox textbox, ErasedManager erasedManager)
    {
        _formSwitcher = formSwitcher;
        _textbox = textbox;
        _erasedManager = erasedManager;
        colliderTrigger.ActivateTutoStep += StartTutoStep;
    }

    public void OnDisableStep()
    {
        colliderTrigger.ActivateTutoStep -= StartTutoStep;
    }
    private void StartTutoStep()
    {
        Debug.Log("startTutoStep");
        if (disponibleForms.Count != 0)
        {
            _formSwitcher.DisponibleForms.Clear();
            _formSwitcher.DisponibleForms = new List<Form>(disponibleForms);
            foreach (var formSwitcherDisponibleForm in _formSwitcher.DisponibleForms)
            {
               Debug.Log(formSwitcherDisponibleForm); 
            }
        }

        if (setForm)
        {
            _formSwitcher.ChangeForm(form);
        }

        if (asDialogue)
        {
            _textbox.AppearText(dialogue);
        }

        if (setNumberOfPointsForErased)
        {
            _erasedManager.maxPointsForCreate = numberOfPointsForErased;
        }
    }

}

[CustomPropertyDrawer(typeof(TutoStep))]
public class TutoStepEditor : PropertyDrawer 
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        SerializedProperty disponibleForms = property.FindPropertyRelative("disponibleForms");
        SerializedProperty colliderTrigger  = property.FindPropertyRelative("colliderTrigger");
        SerializedProperty setForm = property.FindPropertyRelative("setForm");
        SerializedProperty setNumberOfPointsForErased =property.FindPropertyRelative("setNumberOfPointsForErased");
        SerializedProperty asDialogue = property.FindPropertyRelative("asDialogue");
        SerializedProperty form = property.FindPropertyRelative("form");
        SerializedProperty numberOfPointsForErased = property.FindPropertyRelative("numberOfPointsForErased");
        SerializedProperty dialogue = property.FindPropertyRelative("dialogue");
        
 
        position.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);


        if (property.isExpanded)
        {
            EditorGUILayout.PropertyField(colliderTrigger);
            EditorGUILayout.PropertyField(disponibleForms, new GUIContent("Formes Disponibles"), true);
            EditorGUILayout.PropertyField(setForm);
            if (setForm.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(form);
                EditorGUI.indentLevel--;
            }
        
            EditorGUILayout.PropertyField(setNumberOfPointsForErased);
            if (setNumberOfPointsForErased.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(numberOfPointsForErased);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(asDialogue);
            if (asDialogue.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(dialogue);
                EditorGUI.indentLevel--;
            }
        }

        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
        
        SerializedProperty setForm = property.FindPropertyRelative("setForm");
        SerializedProperty setNumberOfPointsForErased = property.FindPropertyRelative("setNumberOfPointsForErased");
        SerializedProperty asDialogue = property.FindPropertyRelative("asDialogue");
        int lines = 2;
        return lines * (EditorGUIUtility.singleLineHeight + 2);
    }
}


