using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTextModdifier : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Color baseColor;
    [SerializeField] private Color secondColor;
    
    private TMP_Text _text;
    
    private bool IsThisButtonSelected()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.currentSelectedGameObject == gameObject;
    }
    
    private void Start()
    {
        _text = GetComponentInChildren<TMP_Text>();
        
        if (IsThisButtonSelected())
        {
            _text.color = secondColor;
        }
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        if (_text == null) return;
        if (!_text.gameObject.activeSelf) _text = GetComponentInChildren<TMP_Text>();
        _text.color = secondColor;
    }
    
    public void OnDeselect(BaseEventData eventData)
    {
        if (_text == null) return;
        if (!_text.gameObject.activeSelf) _text = GetComponentInChildren<TMP_Text>();
        _text.color = baseColor;
    }
    
    public void OnDisable()
    {
        if (_text == null) return;
        if (!_text.gameObject.activeSelf) _text = GetComponentInChildren<TMP_Text>();
        _text.color = baseColor;
    }
}
