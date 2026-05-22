using System;
using System.Collections.Generic;
using UnityEngine;

// We wrap this so it's only included in the Editor
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TutoManager : MonoBehaviour
{
    [SerializeField]private FormSwitcher formSwitcher;
    [SerializeField]private Textbox textBox;
    [SerializeField]private ErasedManager erasedManager;
    [SerializeField] private List<GameObject> atkIndicators;
    [SerializeField] private TutoStep[] steps;
    [SerializeField] private ChatHistory chatHistory;

    // Inside TutoManager.cs
    private void OnEnable()
    {
        foreach (TutoStep tutoStep in steps)
        {
            // Pass the atkIndicators list here
            tutoStep.OnEnableStep(formSwitcher, textBox, erasedManager, atkIndicators, chatHistory);
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
    [SerializeField] private List<Form> disponibleForms = new List<Form>();
    [SerializeField] private bool setForm;
    [SerializeField] private Form form;
    [SerializeField] private bool asDialogue;
    [TextArea(0,5)][SerializeField] private string dialogue;
    [SerializeField] private bool setNumberOfPointsForErased;
    [SerializeField] private bool changeIndicator; // The toggle
    [Range(1,5)][SerializeField] private int ativeAtk; // The int between 1-4
    [SerializeField] private int numberOfPointsForErased;
    
    private FormSwitcher _formSwitcher;
    private Textbox _textbox;
    private ErasedManager _erasedManager;
    private List<GameObject> _atkIndicators; // Reference stored here
    private ChatHistory chatHistory; // Reference stored here

    public void OnEnableStep(FormSwitcher formSwitcher, Textbox textbox, ErasedManager erasedManager, List<GameObject> indicators, ChatHistory _chatHistory)
    {
        _formSwitcher = formSwitcher;
        _textbox = textbox;
        _erasedManager = erasedManager;
        _atkIndicators = indicators; // Store reference
        colliderTrigger.ActivateTutoStep += StartTutoStep;
        chatHistory = _chatHistory;
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
            _formSwitcher.AvailableForms.Clear();
            _formSwitcher.AvailableForms = new List<Form>(disponibleForms);
            foreach (var formSwitcherDisponibleForm in _formSwitcher.AvailableForms)
            {
               Debug.Log(formSwitcherDisponibleForm); 
            }
        }

        if (setForm)
        {
            _formSwitcher.ChangeForm(form);
        }
        
        if (changeIndicator)
        {
            for (int i = 0; i < _atkIndicators.Count; i++)
            {
                if (_atkIndicators[i] == null) continue;
                _atkIndicators[i].SetActive(i == ativeAtk - 1);
            }
        }

        if (asDialogue)
        {
            _textbox.AppearText(dialogue);
            chatHistory.AddMessage(dialogue);
        }

        if (setNumberOfPointsForErased)
        {
            _erasedManager.maxPointsForCreate = numberOfPointsForErased;
        }
    }
}

// We wrap the entire custom editor class so the build completely ignores it
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TutoStep))]
public class TutoStepEditor : PropertyDrawer 
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        // Find properties
        SerializedProperty colliderTrigger = property.FindPropertyRelative("colliderTrigger");
        SerializedProperty disponibleForms = property.FindPropertyRelative("disponibleForms");
        SerializedProperty setForm = property.FindPropertyRelative("setForm");
        SerializedProperty form = property.FindPropertyRelative("form");
        SerializedProperty setNumberOfPointsForErased = property.FindPropertyRelative("setNumberOfPointsForErased");
        SerializedProperty numberOfPointsForErased = property.FindPropertyRelative("numberOfPointsForErased");
        SerializedProperty asDialogue = property.FindPropertyRelative("asDialogue");
        SerializedProperty dialogue = property.FindPropertyRelative("dialogue");
        SerializedProperty changeIndicator = property.FindPropertyRelative("changeIndicator");
        SerializedProperty ativeAtk = property.FindPropertyRelative("ativeAtk");

        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

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
            
            EditorGUILayout.PropertyField(changeIndicator);
            if (changeIndicator.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(ativeAtk);
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
        return property.isExpanded ? 0 : EditorGUIUtility.singleLineHeight;
    }
}
#endif