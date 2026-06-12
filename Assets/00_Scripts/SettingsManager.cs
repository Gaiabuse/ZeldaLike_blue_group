using System;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Scene = UnityEngine.SceneManagement.Scene;

public class SettingsManager : MonoBehaviour
{
    public float mainVolume;
    public float musicVolume;
    public float sfxVolume;
    public bool debugMode;
    public bool vSync;
    public bool isEnglish;

    public static SettingsManager Instance;
    public static event Action OnLanguageChanged;

    private FMOD.Studio.VCA mainVCA;
    private FMOD.Studio.VCA musicVCA;
    private FMOD.Studio.VCA sfxVCA;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return; // Stop execution so the rest of Awake doesn't run on the duplicate
        }

        Instance = this;
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }

        mainVCA = FMODUnity.RuntimeManager.GetVCA("vca:/Master");
        musicVCA = FMODUnity.RuntimeManager.GetVCA("vca:/MUSIC");
        sfxVCA = FMODUnity.RuntimeManager.GetVCA("vca:/SFX");

        LoadSettings();
    }
    
    private void OnEnable()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

// This runs automatically EVERY time a new scene finishes loading
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyVolumesToFMOD();
        OnLanguageChanged?.Invoke();
    
        // Re-apply the debug mode state to whatever DebugScreen exists in the new scene
        if (DebugScreen.Instance != null)
        {
            if (debugMode) DebugScreen.Instance.Activate();
            else DebugScreen.Instance.Disactivate();
        }
    }

    private void Start()
    {
        ApplyVolumesToFMOD();
    }

    public void SetMainVolume(Slider slider)
    {
        mainVolume = slider.value;
        PlayerPrefs.SetFloat("MainVolume", mainVolume);
        mainVCA.setVolume(mainVolume);
    }

    public void SetMusicVolume(Slider slider)
    {
        musicVolume = slider.value;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        musicVCA.setVolume(musicVolume);
    }

    public void SetSfxVolume(Slider slider)
    {
        sfxVolume = slider.value;
        PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
        sfxVCA.setVolume(sfxVolume);
    }

    public void SetDebugMode(Toggle toggle)
    {
        debugMode = toggle.isOn;
    
        if (DebugScreen.Instance != null)
        {
            if (debugMode)
            {
                if (SceneManager.GetActiveScene().name == "MainMenu") return;
                DebugScreen.Instance.Activate();
                Cursor.lockState = CursorLockMode.None; 
                Cursor.visible = true; 
            }
            else
            {
                DebugScreen.Instance.Disactivate();
                Cursor.lockState = CursorLockMode.Locked; 
                Cursor.visible = false;
            }
        }
        else
        {
            Debug.LogWarning("DebugScreen instance is missing in this scene! " +
                             "Make sure the DebugScreen prefab/object is placed in the Game scene.");
        }
    }

    private void LoadSettings()
    {
        mainVolume = PlayerPrefs.GetFloat("MainVolume", 1.0f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1.0f);
        vSync = PlayerPrefs.GetFloat("vSync", 1.0f) != 0;
        isEnglish = PlayerPrefs.GetInt("isEnglish", 0) != 0;
        
        QualitySettings.vSyncCount = vSync ? 1 : 0;
        OnLanguageChanged?.Invoke();
    }

    private void ApplyVolumesToFMOD()
    {
        mainVCA.setVolume(mainVolume);
        musicVCA.setVolume(musicVolume);
        sfxVCA.setVolume(sfxVolume);
    }

    public void ToggleVSync(Toggle vSyncToggle)
    {
        vSync = vSyncToggle.isOn;
        QualitySettings.vSyncCount = vSync ? 1 : 0;
        PlayerPrefs.SetFloat("vSync", vSync ? 1 : 0);
    }
    
    public void ToggleLanguage(Toggle languageToggle)
    {
        isEnglish = languageToggle.isOn;
        PlayerPrefs.SetInt("isEnglish", isEnglish ? 1 : 0);
        OnLanguageChanged?.Invoke();
    }
}
