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
    [Tooltip("order : 0= neutre 1 = cauchemar 2 = onirique")]
    [SerializeField] private Sprite[] spellIndicatorSprites;
    [Tooltip("order : 0= l1 1= r1")]
    [SerializeField] private FormSwitcher formSwitcher;

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
                spellIndicator.sprite = spellIndicatorSprites[0];
                break;

            case Form.nightmare:
                lifeGauge.sprite = lifeFullSprites[1];
                emptyLifeGauge.sprite = lifeEmptySprites[1];
                spellIndicator.sprite = spellIndicatorSprites[1];
                break;

            case Form.dream:
                lifeGauge.sprite = lifeFullSprites[2];
                emptyLifeGauge.sprite = lifeEmptySprites[2];
                spellIndicator.sprite = spellIndicatorSprites[2];
                break;
        }
    }
}
