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
#if UNITY_STANDALONE_LINUX
        if (SteamManager.Initialized)
        {
            InputHandle_t[] handles = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
            int controllerCount = SteamInput.GetConnectedControllers(handles);

            if (controllerCount > 0)
            {
                InputHandle_t activeController = handles[0];

                ushort leftMotor = (ushort)(Mathf.Clamp01(lowFreq) * ushort.MaxValue);
                ushort rightMotor = (ushort)(Mathf.Clamp01(highFreq) * ushort.MaxValue);

                SteamInput.TriggerVibration(activeController, leftMotor, rightMotor);
                return; 
            }
        }
#endif
        
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
        InputHandle_t[] handles = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
        int controllerCount = SteamInput.GetConnectedControllers(handles);
        
        if (controllerCount > 0)
        {
            InputHandle_t activeController = handles[0];
            SteamInput.TriggerVibration(activeController, 0, 0);
            return;
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

