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
    
    [SerializeField] private GameObject neutralSpell;
    
    [SerializeField] private GameObject nightmareSpell;
    [SerializeField] private GameObject grabIcon;
    [SerializeField] private GameObject eatIcon;
    [SerializeField] private GameObject spitIcon;
    
    [SerializeField] private GameObject dreamSpell;
    [SerializeField] private GameObject baitIcon;
    [SerializeField] private GameObject explodeIcon;
    
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
            Destroy(this);
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
                neutralSpell.SetActive(true);
                nightmareSpell.SetActive(false);
                dreamSpell.SetActive(false);
                break;
            case Form.nightmare:
                lifeGauge.sprite = lifeFullSprites[1];
                emptyLifeGauge.sprite = lifeEmptySprites[1];
                neutralSpell.SetActive(false);
                nightmareSpell.SetActive(true);
                dreamSpell.SetActive(false);
                break;
            case Form.dream:
                lifeGauge.sprite = lifeFullSprites[2];
                emptyLifeGauge.sprite = lifeEmptySprites[2];
                neutralSpell.SetActive(false);
                nightmareSpell.SetActive(false);
                dreamSpell.SetActive(true);
                break;
        }
    }

    public void DisplayBaitIcon()
    {
        baitIcon.SetActive(true);
        explodeIcon.SetActive(false);
    }

    public void DisplayExplodeIcon()
    {
        explodeIcon.SetActive(true);
        baitIcon.SetActive(false);
    }

    public void DisplayNightmareIcon(int icon)
    {
        switch (icon)
        {
            case 0:
                grabIcon.SetActive(true);
                eatIcon.SetActive(false);
                spitIcon.SetActive(false);
                break;
            case 1:
                grabIcon.SetActive(false);
                eatIcon.SetActive(true);
                spitIcon.SetActive(false);
                break;
            case 2:
                grabIcon.SetActive(false);
                eatIcon.SetActive(false);
                spitIcon.SetActive(true);
                break;
        }
    }
}
