using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public float mainVolume;
    public float musicVolume;
    public float sfxVolume;
    public bool debugMode;
    
    public static SettingsManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SetMainVolume(Slider slider)
    {
        mainVolume = slider.value;
        PlayerPrefs.SetFloat("MainVolume", mainVolume);
    }

    public void SetMusicVolume(Slider slider)
    {
        musicVolume = slider.value;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    public void SetSfxVolume(Slider slider)
    {
        sfxVolume = slider.value;
        PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
    }
    
    public void SetDebugMode(Toggle toggle)
    {
        debugMode = toggle.isOn;
        PlayerPrefs.SetInt("DebugMode", debugMode ? 1 : 0);
    }

    private void LoadSettings()
    {
        mainVolume = PlayerPrefs.GetFloat("MainVolume", 1.0f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1.0f);
        
        int debugInt = PlayerPrefs.GetInt("DebugMode", 0);
        debugMode = (debugInt == 1);
    }
}