using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public float playTime { get; private set; }
    public float CurrentTargetTimeScale { get; private set; } = 1f;
    [Range(0f, 1f)] 
    [SerializeField] private float targetSlowTime = 0.1f;
    [SerializeField] private float transitionInDuration = 0.05f;
    [SerializeField] private float transitionOutDuration = 0.05f; 

    private float originalFixedDeltaTime;
    private Coroutine timeLerpCoroutine;
    private bool ultTuto;

    public bool achDontDie = true;
    public bool achNoHit = true;
    public EnnemyBase firstZonyr;
    public bool achSpareZonyr = true;
    public bool achSpeedrun = true;
    public float achSpeedrunTime = 600;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Start()
    {
        if (SceneManager.GetSceneByBuildIndex(0).isLoaded) SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(0).name);
    }

    private void Update()
    {
        playTime += Time.unscaledDeltaTime;
        if (achSpareZonyr)
        {
            if (firstZonyr == null) achSpareZonyr = false;
        }

        if (achSpeedrun)
        {
            if (playTime > achSpeedrunTime) achSpeedrun =  false;
        }
    }

    public void TriggerSlowMotion()
    {
        CurrentTargetTimeScale = targetSlowTime;
        if (timeLerpCoroutine != null) StopCoroutine(timeLerpCoroutine);
        timeLerpCoroutine = StartCoroutine(LerpTime(targetSlowTime, transitionInDuration));
    }

    public void ResetTimeScale()
    {
        if (ultTuto) return;
        CurrentTargetTimeScale = 1f;
        if (timeLerpCoroutine != null) StopCoroutine(timeLerpCoroutine);
        timeLerpCoroutine = StartCoroutine(LerpTime(1f, transitionOutDuration));
    }

    public void EndUltTuto()
    {
        ultTuto = false;
    }
    
    public void StartUltTuto()
    {
        ultTuto = true;
    }

    private void SetTimeScaleInstant(float value)
    {
        CurrentTargetTimeScale = value;
        if (timeLerpCoroutine != null) StopCoroutine(timeLerpCoroutine);
        Time.timeScale = value;
        Time.fixedDeltaTime = originalFixedDeltaTime * value;
    }
    
    private IEnumerator LerpTime(float targetScale, float duration)
    {
        float startScale = Time.timeScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; 
            
            float newScale = Mathf.Lerp(startScale, targetScale, elapsed / duration);
            
            Time.timeScale = newScale;
            Time.fixedDeltaTime = originalFixedDeltaTime * newScale; 

            yield return null;
        }
        
        Time.timeScale = targetScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * targetScale;
    }

    public void CheckAchievements()
    {
        if (achDontDie)
        {
            SteamAchievements.Instance.UnlockDontDie();
        }

        if (achNoHit)
        {
            SteamAchievements.Instance.UnlockNoHit();
        }

        if (achSpareZonyr)
        {
            SteamAchievements.Instance.UnlockSpareZonyr();
        }

        if (achSpeedrun)
        {
            SteamAchievements.Instance.UnlockSpeedrun();
        }
    }
}