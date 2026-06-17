using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TutoManager : MonoBehaviour
{
    [SerializeField] private FormSwitcher formSwitcher;
    [SerializeField] private Textbox textBox;
    [SerializeField] private ErasedManager erasedManager;
    [SerializeField] private List<GameObject> atkIndicators;
    [SerializeField] private List<GameObject> tutoIndicator;
    [SerializeField] private TutoStep[] steps;
    [SerializeField] private ChatHistory chatHistory;
    [SerializeField] private TutoStep comboStep;

    private void OnEnable()
    {
        foreach (TutoStep tutoStep in steps)
        {
            tutoStep.OnEnableStep(formSwitcher, textBox, erasedManager, atkIndicators, chatHistory, tutoIndicator, this);
        }
        comboStep.OnEnableStep(formSwitcher, textBox, erasedManager, atkIndicators, chatHistory, tutoIndicator, this);
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

    public void SetActiveStep(TutoStep step)
    {
        textBox.SetActiveStep(step);
    }

    private void StartComboStep()
    {
        comboStep.StartTutoStep();
        GameManager.Instance.StartUltTuto();
        GameManager.Instance.TriggerSlowMotion();
    }

    private void EndComboStep()
    {
        GameManager.Instance.EndUltTuto();
        GameManager.Instance.ResetTimeScale();
        formSwitcher.EndFirstUltimateTime -= EndComboStep;
    }
}


[Serializable]
public class TutoStep
{
    private enum ActivatedTutoIndicator
    {
        None, Heal, AtkN, AtkNm, AtkD, Dash, Spell, Phone
    }

    [Tooltip("ne pas mettre pour le combo steps")]
    [SerializeField] private TriggerTuto colliderTrigger;
    [SerializeField] private List<Form> disponibleForms = new List<Form>();
    [SerializeField] private bool isTutoEnd;
    [SerializeField] private ActivatedTutoIndicator activeTutoUi = ActivatedTutoIndicator.None;
    [SerializeField] private bool setForm;
    [SerializeField] private Form form;
    [SerializeField] private bool asDialogue;
    [TextArea(0, 5)][SerializeField] private string dialogueFR;
    [TextArea(0, 5)][SerializeField] private string dialogueEN;
    public string CurrentDialogue => SettingsManager.Instance.isEnglish ? dialogueEN : dialogueFR;

    [SerializeField] private bool setNumberOfPointsForErased;
    [SerializeField] private bool changeIndicator;
    [Range(1, 5)][SerializeField] private int ativeAtk;
    [SerializeField] private int numberOfPointsForErased;
    [SerializeField] private bool hadUI;
    [SerializeField] private bool temporateUI;
    [SerializeField] private float timeOfUI;
    [SerializeField] private GameObject uiObject;

    private FormSwitcher _formSwitcher;
    private Textbox _textbox;
    private ErasedManager _erasedManager;
    private List<GameObject> _atkIndicators;
    private ChatHistory _chatHistory;
    private List<GameObject> _tutoIndicator;
    private TutoManager _manager;

    public void OnEnableStep(FormSwitcher formSwitcher, Textbox textbox, ErasedManager erasedManager,
        List<GameObject> indicators, ChatHistory chatHistory, List<GameObject> tutoIndicator, TutoManager manager)
    {
        _formSwitcher = formSwitcher;
        _textbox = textbox;
        _erasedManager = erasedManager;
        _atkIndicators = indicators;
        _chatHistory = chatHistory;
        _tutoIndicator = tutoIndicator;
        _manager = manager;

        if (colliderTrigger != null) colliderTrigger.ActivateTutoStep += StartTutoStep;
    }

    public void OnDisableStep()
    {
        if (colliderTrigger != null) colliderTrigger.ActivateTutoStep -= StartTutoStep;
    }

    public void StartTutoStep()
    {
        _manager.SetActiveStep(this);

        if (disponibleForms.Count != 0)
        {
            _formSwitcher.AvailableForms.Clear();
            _formSwitcher.AvailableForms = new List<Form>(disponibleForms);
        }

        if (setForm)
            _formSwitcher.ChangeForm(form);

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
            _textbox.AppearText(CurrentDialogue);
            _chatHistory.AddMessage(dialogueFR, dialogueEN);
        }

        if (isTutoEnd)
        {
            SteamAchievements.Instance.UnlockEndTuto();
        }

        if (hadUI)
            uiObject.SetActive(true);

        if (temporateUI)
            ShowTemporateUI();

        if (setNumberOfPointsForErased)
            _erasedManager.maxPointsForCreate = numberOfPointsForErased;

        switch (activeTutoUi)
        {
            case ActivatedTutoIndicator.Heal:   _tutoIndicator[0].SetActive(true); break;
            case ActivatedTutoIndicator.AtkN:   _tutoIndicator[1].SetActive(true); break;
            case ActivatedTutoIndicator.AtkNm:  _tutoIndicator[2].SetActive(true); break;
            case ActivatedTutoIndicator.AtkD:   _tutoIndicator[3].SetActive(true); break;
            case ActivatedTutoIndicator.Dash:   _tutoIndicator[4].SetActive(true); break;
            case ActivatedTutoIndicator.Spell:  _tutoIndicator[5].SetActive(true); _tutoIndicator[5].GetComponent<TutoIndicatorBlink>().StartBlink(); break;
            case ActivatedTutoIndicator.Phone:  _tutoIndicator[6].SetActive(true); break;
        }
    }

    private void ShowTemporateUI()
    {
        uiObject.SetActive(true);
        uiObject.transform.localScale = Vector3.zero;
        uiObject.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            uiObject.transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.OutQuad).SetDelay(timeOfUI);
        });
    }
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TutoStep))]
public class TutoStepEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty activeTutoUi        = property.FindPropertyRelative("activeTutoUi");
        SerializedProperty isTutoEnd           = property.FindPropertyRelative("isTutoEnd");
        SerializedProperty colliderTrigger     = property.FindPropertyRelative("colliderTrigger");
        SerializedProperty disponibleForms     = property.FindPropertyRelative("disponibleForms");
        SerializedProperty setForm             = property.FindPropertyRelative("setForm");
        SerializedProperty form                = property.FindPropertyRelative("form");
        SerializedProperty setNumberOfPoints   = property.FindPropertyRelative("setNumberOfPointsForErased");
        SerializedProperty numberOfPoints      = property.FindPropertyRelative("numberOfPointsForErased");
        SerializedProperty asDialogue          = property.FindPropertyRelative("asDialogue");
        SerializedProperty dialogueFR          = property.FindPropertyRelative("dialogueFR");
        SerializedProperty dialogueEN          = property.FindPropertyRelative("dialogueEN");
        SerializedProperty hadUI               = property.FindPropertyRelative("hadUI");
        SerializedProperty temporateUI         = property.FindPropertyRelative("temporateUI");
        SerializedProperty timeOfUI            = property.FindPropertyRelative("timeOfUI");
        SerializedProperty uiObject            = property.FindPropertyRelative("uiObject");
        SerializedProperty changeIndicator     = property.FindPropertyRelative("changeIndicator");
        SerializedProperty ativeAtk            = property.FindPropertyRelative("ativeAtk");

        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

        if (property.isExpanded)
        {
            EditorGUILayout.PropertyField(activeTutoUi, new GUIContent("Active Tuto UI"));
            EditorGUILayout.PropertyField(colliderTrigger);
            EditorGUILayout.PropertyField(isTutoEnd);
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

            EditorGUILayout.PropertyField(setNumberOfPoints);
            if (setNumberOfPoints.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(numberOfPoints);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(asDialogue);
            if (asDialogue.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(dialogueFR, new GUIContent("Dialogue FR"));
                EditorGUILayout.PropertyField(dialogueEN, new GUIContent("Dialogue EN"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(temporateUI);
            if (temporateUI.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(timeOfUI);
                EditorGUILayout.PropertyField(uiObject);
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