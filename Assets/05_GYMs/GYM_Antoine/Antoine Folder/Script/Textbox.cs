using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Textbox : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textBox;
    [SerializeField] private Image textImg;
    //[SerializeField] TextMeshProUGUI nameBox;
    //[SerializeField] GameObject portrait;

    [SerializeField] Animator animator;

    float timer;
    float disapearTimer;
    float maxTimer;
    int TextPhase = 0;

    string textShow; string nameShow;
    string currentText; string currentName;
    [SerializeField] int currentLetter = 0;

    [Header("TextBox Assets")]

    [SerializeField] List<Sprite> PortraitIcon;
    [SerializeField] List<Vector2> PortraitSize;
    [SerializeField] List<Vector2> PortraitPosition;

    private void Start()
    {
        animator = GetComponent<Animator>();

        textBox.text = null;
        //nameBox.text = null;
        Debug.Log("ntm");
        AppearText(0, "You What ?!", "V2", 0.03f, 2);
    }

    private void FixedUpdate()
    {
        if (TextPhase == 1)
        {
            timer += Time.deltaTime;
            if (timer > maxTimer)
            {
                textBox.gameObject.SetActive(false);
                if (currentText != textShow) currentText += textShow[currentLetter];
                if (currentName != nameShow) currentName += nameShow[currentLetter];

                textBox.text = currentText;
                //nameBox.text = currentName;

                if (textShow.Length > nameShow.Length)
                {
                    if (currentLetter == textShow.Length || TextPhase != 1)
                    {
                        TextPhase = 2;
                        currentLetter = 0;
                        maxTimer = disapearTimer;
                    }
                }
                else
                {
                    if (currentLetter == nameShow.Length || TextPhase != 1)
                    {
                        TextPhase = 2;
                        currentLetter = 0;
                        maxTimer = disapearTimer;
                    }
                }

                timer = 0;
                currentLetter += 1;
            }
        }

        if (TextPhase == 2)
        {
            timer += Time.deltaTime;
            if (timer >= maxTimer)
            {
                animator.SetBool("Show", false);
                textBox.text = null;
                //nameBox.text = null;

                TextPhase = 0;
            }
        }
    }
    

    public void AppearText(int Portrait, string text, string name, float waitBeforeLetter, float waitDisapear)
    {
        textImg.enabled = true;
        //portrait.GetComponent<UnityEngine.UI.Image>().sprite = PortraitIcon[Portrait];

        //RectTransform portraitTransform = portrait.GetComponent<RectTransform>();

        //portraitTransform.anchoredPosition = PortraitPosition[Portrait];
        //portraitTransform.localScale = PortraitSize[Portrait];

        animator.SetBool("Show", true);
        textBox.text = null;
        //nameBox.text = null;

        currentLetter = 0;
        currentText = null;
        currentName = null;

        textShow = text;
        nameShow = name;

        maxTimer = waitBeforeLetter;
        TextPhase = 0;

        disapearTimer = waitDisapear;
    }

    protected void ShowText()
    {
        TextPhase = 1;
    }
}
