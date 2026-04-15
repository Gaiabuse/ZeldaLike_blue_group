using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TransformIndicator : MonoBehaviour
{
    [SerializeField] private Image lifeGauge;
    [SerializeField] private Image emptyLifeGauge;
    [SerializeField] private Image spellIndicator;
    [FormerlySerializedAs("iconsSprites")]
    [Tooltip("order : 0= neutre 1 = cauchemar 2 = onirique")]
    [SerializeField] private Sprite[] lifeFullSprites;
    [Tooltip("order : 0= neutre 1 = cauchemar 2 = onirique")]
    [SerializeField] private Sprite[] lifeEmptySprites;

    [SerializeField] private GameObject NeutralPower;
    
    [SerializeField] private GameObject DreamPower;
    [SerializeField] private GameObject baitInput;
    [SerializeField] private GameObject explodeInput;
    
    [SerializeField] private GameObject NightmarePower;
    
    [Tooltip("order : 0= l1 1= r1")]
    [SerializeField] private FormSwitcher formSwitcher;
    
    public static TransformIndicator Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SwitchIndicators(Form.neutral);
        FormSwitcher.SwitchForm += SwitchIndicators;
    }

    private void OnDisable()
    {
        FormSwitcher.SwitchForm -= SwitchIndicators;
    }

    private void SwitchIndicators(Form currentForm)
    {
        switch (currentForm)
        {
            case Form.neutral:
                lifeGauge.sprite = lifeFullSprites[0];
                emptyLifeGauge.sprite = lifeEmptySprites[0];
                NeutralPower.SetActive(true);
                DreamPower.SetActive(false);
                NightmarePower.SetActive(false);
                break;
            case Form.nightmare:
                lifeGauge.sprite = lifeFullSprites[1];
                emptyLifeGauge.sprite = lifeEmptySprites[1];
                NeutralPower.SetActive(false);
                DreamPower.SetActive(false);
                NightmarePower.SetActive(true);
                break;
            case Form.dream:
                lifeGauge.sprite = lifeFullSprites[2];
                emptyLifeGauge.sprite = lifeEmptySprites[2];
                NeutralPower.SetActive(false);
                DreamPower.SetActive(true);
                NightmarePower.SetActive(false);
                break;
        }
    }
    
    public void ShowExplodeInput()
    {
        baitInput.SetActive(false);
        explodeInput.SetActive(true);
    }
    
    public void ShowBaitInput()
    {
        baitInput.SetActive(true);
        explodeInput.SetActive(false);
    }
}
