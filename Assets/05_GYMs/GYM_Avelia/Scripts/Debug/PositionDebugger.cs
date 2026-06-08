using UnityEngine;
using TMPro;

public class PositionDebugger : MonoBehaviour
{
    [Header ("Reference")]
    PlayerController player;
    FormSwitcher switcher;
    
    [Header ("FPS")]
    int m_frameCounter = 0;
    float m_timeCounter = 0.0f;
    float m_lastFramerate = 0.0f;
    public float m_refreshTime = 0.25f;

    [Header ("Text")]
    [SerializeField]
    TMP_Text positionText;
    [SerializeField]
    TMP_Text form;
    [SerializeField]
    TMP_Text FPS;

    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;
        switcher = player.GetComponent<FormSwitcher>();
    }

    void Update()
    {
        if (player == null) return;
        if (switcher == null) return;
        positionText.text = $"Position {player.transform.position}";
        form.text = GetForms();
        FrameCounter();
    }

    void FrameCounter()
    {
        m_timeCounter += Time.unscaledDeltaTime;
        m_frameCounter++;

        if (m_timeCounter >= m_refreshTime)
        {
            m_lastFramerate = m_frameCounter / m_timeCounter;
        
            FPS.text = $"FPS: {m_lastFramerate:F1}";
            
            m_frameCounter = 0;
            m_timeCounter = 0.0f;
        }
    }

    private string GetForms()
    {
        string forms = "Available Forms:\n";
        foreach (var form in switcher.AvailableForms)
        {
            forms += $"{form}\n";
        }

        return forms;
    }
}
