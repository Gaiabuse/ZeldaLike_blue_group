using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Static instance so any script can access the time easily
    public static GameManager Instance { get; private set; }

    public float playTime { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // Using regular Update and Time.deltaTime is standard for clocks
        playTime += Time.unscaledDeltaTime; 
    }
}