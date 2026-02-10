using UnityEngine;
using UnityEngine.InputSystem;

public class FormSwitcher : MonoBehaviour
{
    public Form currentForm { get; private set; } = Form.neutral;

    [SerializeField]
    GameObject neutralFormObject, dreamFormObject, nightmareFormObject;

    void Start()
    {

    }

    public void ChangeForm(Form nextForm)
    {
        neutralFormObject.SetActive(false);
        dreamFormObject.SetActive(false);
        nightmareFormObject.SetActive(false);

        switch (nextForm)
        {
            case Form.neutral:
                neutralFormObject.SetActive(true);
                break;
            case Form.dream:
                dreamFormObject.SetActive(true);
                break;
            case Form.nightmare:
                nightmareFormObject.SetActive(true);
                break;
        }

        currentForm = nextForm;
    }

    void OnTransform(InputValue _input)
    {

        switch (currentForm)
        {
            case Form.neutral:
                ChangeForm(Form.dream);
                break;
            case Form.dream:
                ChangeForm(Form.nightmare);
                break;
            case Form.nightmare:
                ChangeForm(Form.neutral);
                break;
        }
    }
}

public enum Form
{
    neutral,
    dream,
    nightmare,
}
