using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatHistory : MonoBehaviour
{
    [SerializeField] private List<Message> messages;
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject messagePrefab;

    private void OnEnable()
    {
        SettingsManager.OnLanguageChanged += RefreshLanguage;
    }

    private void OnDisable()
    {
        SettingsManager.OnLanguageChanged -= RefreshLanguage;
    }

    private void OnValidate()
    {
        if (messages == null) return;
        foreach (var msg in messages)
        {
            msg.ClampText();
            msg.UpdateUI();
        }
    }

    private void Start()
    {
        UpdateChatHistory();
    }

    public void AddMessage(string fr, string en)
    {
        Message m = new Message();
        m.textFR = fr;
        m.textEN = en;
        messages.Add(m);

        RectTransform contentRect = content.GetComponent<RectTransform>();
        if (contentRect != null)
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, 225f * messages.Count);

        UpdateChatHistory();
    }

    private void RefreshLanguage()
    {
        foreach (Message msg in messages)
            msg.UpdateUI();
    }

    private void UpdateChatHistory()
    {
        bool spawnedNewObjects = false;

        if (content.transform.childCount < messages.Count)
        {
            int objectsToSpawn = messages.Count - content.transform.childCount;
            for (int i = 0; i < objectsToSpawn; i++)
                Instantiate(messagePrefab, content.transform);

            spawnedNewObjects = true;
        }

        if (spawnedNewObjects)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        }

        for (int i = 0; i < content.transform.childCount; i++)
        {
            GameObject currentBox = content.transform.GetChild(i).gameObject;

            if (i < messages.Count)
            {
                messages[i].textBox = currentBox;
                messages[i].UpdateUI();
            }
            else
            {
                currentBox.SetActive(false);
            }
        }

        Canvas.ForceUpdateCanvases();
    }
}

[Serializable]
public class Message
{
    [Header("Settings")]
    [SerializeField] private int maxCharacters = 100;
    public GameObject textBox;

    [TextArea(3, 10)] public string textFR;
    [TextArea(3, 10)] public string textEN;
    public bool _isSent;

    public string CurrentText => SettingsManager.Instance != null && SettingsManager.Instance.isEnglish
        ? textEN : textFR;

    public void ClampText()
    {
        if (textFR != null && textFR.Length > maxCharacters)
        {
            textFR = textFR.Substring(0, maxCharacters);
            Debug.LogWarning($"FR text exceeded limit! Clamped to {maxCharacters} characters.");
        }
        if (textEN != null && textEN.Length > maxCharacters)
        {
            textEN = textEN.Substring(0, maxCharacters);
            Debug.LogWarning($"EN text exceeded limit! Clamped to {maxCharacters} characters.");
        }
    }

    public void UpdateUI()
    {
        if (textBox == null) return;

        textBox.SetActive(true);

        TMP_Text textComponent = textBox.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
            textComponent.text = CurrentText;

        if (_isSent)
        {
            textBox.transform.localScale = new Vector3(-1f, 1f, 1f);
            if (textComponent != null)
                textComponent.transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else
        {
            textBox.transform.localScale = new Vector3(1f, 1f, 1f);
            if (textComponent != null)
                textComponent.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        Canvas.ForceUpdateCanvases();
    }
}