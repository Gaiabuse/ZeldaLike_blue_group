using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_STANDALONE_LINUX
using Steamworks; // Compilé uniquement sur Linux / Steam Deck
#endif

public class RumbleManager : MonoBehaviour
{
    public static RumbleManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject); 
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void TriggerVibration(float lowFreq, float highFreq)
    {
        // --- CODE POUR STEAM DECK / LINUX ---
#if UNITY_STANDALONE_LINUX
        if (SteamManager.Initialized)
        {
            InputHandle_t activeController = SteamInput.GetControllerForGamepadIndex(0);
            
            ushort leftMotor = (ushort)(Mathf.Clamp01(lowFreq) * ushort.MaxValue);
            ushort rightMotor = (ushort)(Mathf.Clamp01(highFreq) * ushort.MaxValue);
            
            SteamInput.TriggerVibration(activeController, leftMotor, rightMotor);
            return; 
        }
#endif


        // --- CODE POUR WINDOWS (PRODUIT DE SUBSTITUTION / REGULAR RUMBLE) ---
#if UNITY_STANDALONE_WIN
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(lowFreq, highFreq);
        }
#endif
    }
    
    public void StopVibration()
    {
#if UNITY_STANDALONE_LINUX
        if (SteamManager.Initialized)
        {
            InputHandle_t activeController = SteamInput.GetControllerForGamepadIndex(0);
            SteamInput.TriggerVibration(activeController, 0, 0);
        }
#endif

#if UNITY_STANDALONE_WIN
        if (Gamepad.current != null)
        {
            Gamepad.current.ResetHaptics();
        }
#endif
    }
}