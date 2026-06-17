using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsUIBridge : MonoBehaviour
{
    [Header("UI Elements in this Scene")]
    public Slider mainSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle debugToggle;
    public Toggle vSyncToggle;
    public Toggle languageToggle;

    private void Start()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager manager = SettingsManager.Instance;
            
            if (mainSlider != null) mainSlider.value = manager.mainVolume;
            if (musicSlider != null) musicSlider.value = manager.musicVolume;
            if (sfxSlider != null) sfxSlider.value = manager.sfxVolume;
            if (debugToggle != null) debugToggle.isOn = manager.debugMode;
            if (vSyncToggle != null) vSyncToggle.isOn = manager.vSync;
            if (languageToggle != null) languageToggle.isOn = manager.isEnglish;
            
            if (mainSlider != null) mainSlider.onValueChanged.AddListener(delegate { manager.SetMainVolume(mainSlider); });
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(delegate { manager.SetMusicVolume(musicSlider); });
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(delegate { manager.SetSfxVolume(sfxSlider); });
            if (debugToggle != null) debugToggle.onValueChanged.AddListener(delegate { manager.SetDebugMode(debugToggle); });
            if (vSyncToggle != null) vSyncToggle.onValueChanged.AddListener(delegate { manager.ToggleVSync(vSyncToggle); });
            if (SceneManager.GetActiveScene().name == "MainMenu")
                if (debugToggle != null)
                {
                    debugToggle.isOn = false;
                    manager.SetDebugMode(debugToggle);
                }
            
        }
        else
        {
            Debug.LogWarning("SettingsManager instance not found in this scene!");
        }
    }
}