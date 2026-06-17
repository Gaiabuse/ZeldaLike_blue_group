using UnityEngine;
using Steamworks;

/// <summary>
/// Handles 5 binary (on/off) achievements + 1 stat-based achievement (0-100)
/// using Steamworks.NET. Attach this to a persistent GameObject in your first scene,
/// alongside (or after) your SteamManager.
/// </summary>
public class SteamAchievements : MonoBehaviour
{
    public static SteamAchievements Instance { get; private set; }

    // ---- Achievement API names: must match EXACTLY what you set in the Steamworks dashboard ----
    public const string ACH_FINISH_GAME     = "ACHIEVEMENT_FINISH_GAME";
    public const string ACH_ULT_NEUTRAL = "ACHIEVEMENT_ULTIMATE_NEUTRAL";
    public const string ACH_ULT_NIGHTMARE  = "ACHIEVEMENT_ULTIMATE_NIGHT";
    public const string ACH_ULT_DREAM   = "ACHIEVEMENT_ULTIMATE_DREAM";
    public const string ACH_FINISH_TUTO      = "ACHIEVEMENT_FINISH_TUTO";
    public const string ACH_CLEAN_ALL_DUST      = "ACHIEVEMENT_CLEAN_ALL";
    

    private bool statsInitialized = false;

    protected Callback<UserStatsStored_t> userStatsStoredCallback;
    protected Callback<UserAchievementStored_t> userAchievementStoredCallback;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SteamAPI.Init();
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("[Steam] SteamManager not initialized, achievements disabled.");
            return;
        }

        // Stats are automatically synced by Steam before launch in current SDK versions
        statsInitialized = true;

        userStatsStoredCallback = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        userAchievementStoredCallback = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

        Debug.Log("[Steam] Achievements manager ready.");
    }

    private void OnUserStatsStored(UserStatsStored_t pCallback)
    {
        if (pCallback.m_nGameID != (ulong)SteamUtils.GetAppID()) return;
        if (pCallback.m_eResult != EResult.k_EResultOK)
            Debug.LogWarning("[Steam] Failed to store stats: " + pCallback.m_eResult);
    }

    private void OnAchievementStored(UserAchievementStored_t pCallback)
    {
        Debug.Log("[Steam] Achievement stored: " + pCallback.m_rgchAchievementName);
    }

    // ===================== Binary (on/off) achievements =====================

    /// <summary>Unlocks an achievement by its API name. Safe to call multiple times.</summary>
    public void UnlockAchievement(string achievementId)
    {
        if (!SteamManager.Initialized || !statsInitialized) return;

        bool alreadyUnlocked;
        if (SteamUserStats.GetAchievement(achievementId, out alreadyUnlocked) && alreadyUnlocked)
            return; // already unlocked, nothing to do

        SteamUserStats.SetAchievement(achievementId);
        SteamUserStats.StoreStats();
    }

    // Convenience wrappers — call these directly from your gameplay code
    public void UnlockEndGame()     => UnlockAchievement(ACH_FINISH_GAME);
    public void UnlockUltNeutral() => UnlockAchievement(ACH_ULT_NEUTRAL);
    public void UnlockUltNightmare()  => UnlockAchievement(ACH_ULT_NIGHTMARE);
    public void UnlockUltDream()   => UnlockAchievement(ACH_ULT_DREAM);
    public void UnlockEndTuto()      => UnlockAchievement(ACH_FINISH_TUTO);
    public void UnlockAllCleaned()      => UnlockAchievement(ACH_CLEAN_ALL_DUST);


    // ===================== Debug / testing =====================

    [ContextMenu("DEBUG: Reset All Stats & Achievements")]
    public void ResetAllStats()
    {
        if (!SteamManager.Initialized) return;
        
        if (SteamUserStats.ResetAllStats(true)) 
        {
            string[] allAchievements = { 
                ACH_FINISH_GAME, 
                ACH_ULT_NEUTRAL, 
                ACH_ULT_NIGHTMARE, 
                ACH_ULT_DREAM, 
                ACH_FINISH_TUTO, 
                ACH_CLEAN_ALL_DUST 
            };

            foreach (string ach in allAchievements)
            {
                SteamUserStats.ClearAchievement(ach);
            }
            
            SteamUserStats.StoreStats();

            Debug.Log("[Steam] All stats reset and local cache hard-cleared instantly!");
        }
        else
        {
            Debug.LogError("[Steam] Failed to issue ResetAllStats.");
        }
    }
}