using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TransformIndicator : MonoBehaviour
{
    [SerializeField] private Image indicatorLeft;
    [SerializeField] private Image indicatorRight;
    [SerializeField] private Image iconLeft;
    [SerializeField] private Image iconRight;
    [Tooltip("order : 0= neutre 1 = cauchemar 2 = onirique")]
    [SerializeField] private Sprite[] iconsSprites;
    [Tooltip("order : 0= l1 1= r1")]
    [SerializeField] private Sprite[] indicatorNeutralSprites;
    [SerializeField] private Sprite[] indicatorNightmareSprites;
    [SerializeField] private Sprite[] indicatorDreamSprites;

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
                indicatorLeft.sprite = indicatorNightmareSprites[0];
                indicatorRight.sprite = indicatorDreamSprites[1];
                iconLeft.sprite = iconsSprites[1];
                iconRight.sprite = iconsSprites[2];
                break;
            case Form.nightmare:
                indicatorLeft.sprite = indicatorDreamSprites[0];
                indicatorRight.sprite = indicatorNeutralSprites[1];
                iconLeft.sprite = iconsSprites[2];
                iconRight.sprite = iconsSprites[0];
                break;
            case Form.dream:
                indicatorLeft.sprite = indicatorNightmareSprites[0];
                indicatorRight.sprite = indicatorNeutralSprites[1];
                iconLeft.sprite = iconsSprites[1];
                iconRight.sprite = iconsSprites[0];
                break;
        }
    }
}
