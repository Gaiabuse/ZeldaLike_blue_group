using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    
    [SerializeField] private TutoStep comboStep;
    [Range(0f, 1f)] 
    [SerializeField] private float targetSlowTime = 0.1f;
    [SerializeField] private float transitionInDuration = 0.05f;
    [SerializeField] private float transitionOutDuration = 0.05f; 

    private float originalFixedDeltaTime;
    private Coroutine timeLerpCoroutine;

    private void Awake()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }
    public void TriggerSlowMotion()
    {
        if (timeLerpCoroutine != null) StopCoroutine(timeLerpCoroutine);
        timeLerpCoroutine = StartCoroutine(LerpTime(targetSlowTime, transitionInDuration));
    }
    public void ResetTimeScale()
    {
        if (timeLerpCoroutine != null) StopCoroutine(timeLerpCoroutine);
        timeLerpCoroutine = StartCoroutine(LerpTime(1f, transitionOutDuration));
    }
    
    public void SetTimeScaleInstant(float value)
    {
        if (timeLerpCoroutine != null) StopCoroutine(timeLerpCoroutine);
        Time.timeScale = value;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
    }

    private IEnumerator LerpTime(float targetScale, float duration)
    {
        float startScale = Time.timeScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; 
            
            float newScale = Mathf.Lerp(startScale, targetScale, elapsed / duration);
            
            Time.timeScale = newScale;
            Time.fixedDeltaTime = originalFixedDeltaTime * newScale; 

            yield return null;
        }
        
        Time.timeScale = targetScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * targetScale;
    }
    private void OnEnable()
    {
        foreach (TutoStep tutoStep in steps)
        {
            tutoStep.OnEnableStep(formSwitcher, textBox, erasedManager, atkIndicators, chatHistory);
        }
        comboStep.OnEnableStep(formSwitcher, textBox, erasedManager, atkIndicators, chatHistory);
        formSwitcher.FirstUltimateTime += StartComboStep;
        formSwitcher.EndFirstUltimateTime += EndComboStep;
    }

    private void OnDisable()
    {
        foreach (TutoStep tutoStep in steps)
        {
            tutoStep.OnDisableStep();
        }
        formSwitcher.FirstUltimateTime -= StartComboStep;
        formSwitcher.EndFirstUltimateTime -= EndComboStep;
    }

    private void StartComboStep()
    {
        comboStep.StartTutoStep();
        TriggerSlowMotion();
    }

    private void EndComboStep()
    {
        SetTimeScaleInstant(1f);
        formSwitcher.EndFirstUltimateTime -= EndComboStep;
    }
}


[Serializable]
public class TutoStep
{
    [Tooltip("ne pas mettre pour le combo steps")]
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
    [SerializeField] private bool hadUI;
    [SerializeField] private GameObject uiObject;
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
        if(colliderTrigger != null) colliderTrigger.ActivateTutoStep += StartTutoStep;
        chatHistory = _chatHistory;
        /*if (hadUI)
        {
            uiObject.SetActive(false);
        }*/
    }

    public void OnDisableStep()
    {
        colliderTrigger.ActivateTutoStep -= StartTutoStep;
    }
    
    public void StartTutoStep()
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

        if (hadUI)
        {
            uiObject.SetActive(true);
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
        SerializedProperty hadUI = property.FindPropertyRelative("hadUI");
        SerializedProperty uiObject = property.FindPropertyRelative("uiObject");
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
            EditorGUILayout.PropertyField(hadUI);
            if (hadUI.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(uiObject);
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