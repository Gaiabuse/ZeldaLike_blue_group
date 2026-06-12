using System;
using TMPro;
using UnityEngine;

public class SwitchLanguageText : MonoBehaviour
{
    [SerializeField] private GameObject frenchText;
    [SerializeField] private GameObject englishText;

    private void Start()
    {
        if (frenchText == null) frenchText = transform.GetChild(0).gameObject;
        if (englishText == null) englishText = transform.GetChild(1).gameObject;
        SwitchLanguage();
    }

    private void OnEnable()
    {
        SettingsManager.OnLanguageChanged += SwitchLanguage;
        if (frenchText == null) frenchText = transform.GetChild(0).gameObject;
        if (englishText == null) englishText = transform.GetChild(1).gameObject;
        SwitchLanguage();
    }

    private void OnDisable()
    {
        SettingsManager.OnLanguageChanged -= SwitchLanguage;
    }
    
    private void SwitchLanguage()
    {
        if (frenchText == null || englishText == null) return;
        if (SettingsManager.Instance == null) return;
        frenchText.SetActive(!SettingsManager.Instance.isEnglish);
        englishText.SetActive(SettingsManager.Instance.isEnglish);
    }
}
