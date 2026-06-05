using System;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public float mainVolume;
    public float musicVolume;
    public float sfxVolume;
    public bool debugMode;

    public static SettingsManager Instance;

    private FMOD.Studio.VCA mainVCA;
    private FMOD.Studio.VCA musicVCA;
    private FMOD.Studio.VCA sfxVCA;

    DebugScreen debugScreen;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);

        Instance = this;
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }

        mainVCA = FMODUnity.RuntimeManager.GetVCA("vca:/Master");
        musicVCA = FMODUnity.RuntimeManager.GetVCA("vca:/MUSIC");
        sfxVCA = FMODUnity.RuntimeManager.GetVCA("vca:/SFX");

        LoadSettings();

        debugScreen = FindAnyObjectByType<DebugScreen>();
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
        PlayerPrefs.SetInt("DebugMode", debugMode ? 1 : 0);
        debugScreen.gameObject.SetActive(debugMode);
    }

    private void LoadSettings()
    {
        mainVolume = PlayerPrefs.GetFloat("MainVolume", 1.0f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1.0f);

        int debugInt = PlayerPrefs.GetInt("DebugMode", 0);
        debugMode = (debugInt == 1);

        debugScreen.gameObject.SetActive(debugMode);
    }

    private void ApplyVolumesToFMOD()
    {
        mainVCA.setVolume(mainVolume);
        musicVCA.setVolume(musicVolume);
        sfxVCA.setVolume(sfxVolume);
    }
}
