using System;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_STANDALONE_LINUX
using Steamworks;
#endif

public class RumbleManager : MonoBehaviour
{
    public static RumbleManager Instance;

#if UNITY_STANDALONE_LINUX
    private bool isSteamInputInitialized = false;
#endif

    private void Awake()
    {
        if (Instance == null)
        {
            Time.timeScale = 1;
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

    private void Start()
    {
#if UNITY_STANDALONE_LINUX
        TryInitSteamInput();
#endif
    }

#if UNITY_STANDALONE_LINUX
    private bool TryInitSteamInput()
    {
        if (isSteamInputInitialized) return true;

        if (SteamManager.Initialized)
        {
            // Initialise le système d'input Steam
            isSteamInputInitialized = SteamInput.Init(false);
            Debug.Log($"RumbleManager: SteamInput Initialization status: {isSteamInputInitialized}");
            return isSteamInputInitialized;
        }
        
        Debug.LogWarning("RumbleManager: SteamManager n'est pas encore prêt.");
        return false;
    }
#endif

    public void TriggerVibration(float lowFreq, float highFreq)
    {
        bool vibrationTriggered = false;

#if UNITY_STANDALONE_LINUX
        Debug.Log("RumbleManager: Tentative de vibration sur Linux (SteamOS)...");

        // Essai d'initialisation tardive si Steam n'était pas prêt au Start
        if (!isSteamInputInitialized)
        {
            TryInitSteamInput();
        }

        if (SteamManager.Initialized && isSteamInputInitialized)
        {
            // OBLIGATOIRE : Force Steam à rafraîchir l'état des manettes connectées
            SteamInput.RunFrame();

            InputHandle_t[] handles = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
            int controllerCount = SteamInput.GetConnectedControllers(handles);

            Debug.Log($"RumbleManager: {controllerCount} manette(s) trouvée(s) via SteamInput.");

            if (controllerCount > 0)
            {
                InputHandle_t activeController = handles[0];


                float boostedLow = Mathf.Pow(Mathf.Clamp01(lowFreq), 0.75f);
                float boostedHigh = Mathf.Pow(Mathf.Clamp01(highFreq), 0.75f);
                
                ushort leftMotor = (ushort)(boostedLow * ushort.MaxValue);
                ushort rightMotor = (ushort)(Mathf.Clamp01(boostedHigh + (boostedLow * 0.3f)) * ushort.MaxValue); 

                SteamInput.TriggerVibration(activeController, leftMotor, rightMotor);

                SteamInput.TriggerVibration(activeController, leftMotor, rightMotor);
                vibrationTriggered = true;
                Debug.Log("RumbleManager: Vibration envoyée via SteamInput.");
            }
        }
        else
        {
            Debug.LogWarning("RumbleManager: Steam API non initialisée. Impossible d'utiliser SteamInput.");
        }

        // FALLBACK LINUX : Si SteamInput a échoué ou n'a pas trouvé de manette,
        // on tente l'Input System classique d'Unity (parfois traduit par Proton)
        if (!vibrationTriggered && Gamepad.current != null)
        {
            Debug.Log("RumbleManager: Fallback sur l'Input System Unity standard.");
            Gamepad.current.SetMotorSpeeds(lowFreq, highFreq);
            vibrationTriggered = true;
        }
#endif

#if UNITY_STANDALONE_WIN
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(lowFreq, highFreq);
            vibrationTriggered = true;
        }
#endif
    }

    public void StopVibration()
    {
#if UNITY_STANDALONE_LINUX
        if (SteamManager.Initialized && isSteamInputInitialized)
        {
            SteamInput.RunFrame();
            InputHandle_t[] handles = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
            int controllerCount = SteamInput.GetConnectedControllers(handles);
            
            if (controllerCount > 0)
            {
                SteamInput.TriggerVibration(handles[0], 0, 0);
            }
        }

        if (Gamepad.current != null)
        {
            Gamepad.current.ResetHaptics();
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