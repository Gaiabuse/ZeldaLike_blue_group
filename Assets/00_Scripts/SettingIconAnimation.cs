using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingIconAnimation : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite[] sprites;

    private void Start()
    {
        Slider slider = GetComponent<Slider>();
        if (slider != null) UpdateImage(slider);
    }

    public void UpdateImage(Slider slider)
    {
        switch (slider.value)
        {
            case 0:
                image.sprite = sprites[0];
                break;
            case <=0.5f:
                image.sprite = sprites[1];
                break;
            case >0.5f:
                image.sprite = sprites[2];
                break;
        }
    }
}
