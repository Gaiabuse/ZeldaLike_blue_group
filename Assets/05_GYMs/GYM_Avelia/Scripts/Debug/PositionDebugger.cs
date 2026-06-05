using UnityEngine;
using TMPro;

public class PositionDebugger : MonoBehaviour
{
    [SerializeField]
    PlayerController player;

    [SerializeField]
    TMP_Text positionText;
    [SerializeField]
    TMP_Text form;

    void Update()
    {
        positionText.text = $"Position {player.transform.position}";
    }
}
