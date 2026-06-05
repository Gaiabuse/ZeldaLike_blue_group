using UnityEngine;
using TMPro;

public class PositionDebugger : MonoBehaviour
{
    [SerializeField]
    PlayerController player;
    FormSwitcher switcher;

    [SerializeField]
    TMP_Text positionText;
    [SerializeField]
    TMP_Text form;

    void Start()
    {
        switcher = player.GetComponent<FormSwitcher>();
    }

    void Update()
    {
        positionText.text = $"Position {player.transform.position}";
        form.text = getForm();
    }

    string getForm()
        => switcher.currentForm switch
        {
            Form.neutral => "dream form",
            Form.dream => "dream form",
            Form.nightmare => "dream form",
            _ => "ERROR UNKNOWN FORM",
        };
}
