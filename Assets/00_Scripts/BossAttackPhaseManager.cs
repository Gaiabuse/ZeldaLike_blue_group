using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; 

public class BossAttackPhaseManager : MonoBehaviour
{
    public enum AttackPattern { SingleLaunch, StrikeZone, CircleLaunch, RandomLaunch }

    [System.Serializable]
    public class AttackAction
    {
        public string actionName = "New Attack";
        public AttackPattern pattern;
        public int projectileIndex = 0;
        public Vector2 projectileCountRange = new Vector2(5, 5);
        public float radius = 4f;
        public Vector2 launchSpeedRange = new Vector2(0.2f, 0.5f);
        public bool useCircleDelayed = false;
        public float delayBeforeAttack = 0f;
        public float delayAfterAttack = 2f;
    }

    [System.Serializable]
    public class BossPhase
    {
        public string phaseName = "New Phase";
        [Range(0f, 100f)] public float healthPercentageThreshold = 0f; 
        public bool playSequentially = true;
        public bool runOnlyOnce = false;
        public bool hasAnimation = true; 
        public List<AttackAction> attacksInPhase = new List<AttackAction>();
    }

    [Header("References")]
    [SerializeField] private DreamCoreManager coreManager;
    [SerializeField] private BomberAttack bomberScript;
    [SerializeField] private List<BossPhase> bossPhases = new List<BossPhase>();

    private int currentPhaseIndex = 0;
    private bool isExecutingAction = false;
    private float maxHpCache = 1000f;

    public void StartBossAttack()
    {
        if (coreManager == null || bomberScript == null || bossPhases.Count == 0) return;

        var hpField = typeof(DreamCoreManager).GetField("maxHP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (hpField != null) maxHpCache = Convert.ToSingle(hpField.GetValue(coreManager));

        currentPhaseIndex = 0;
        StartCoroutine(MasterPhaseLoop());
    }

    private IEnumerator MasterPhaseLoop()
    {
        while (currentPhaseIndex < bossPhases.Count)
        {
            BossPhase phase = bossPhases[currentPhaseIndex];

            // Update the Health Manager's Gatekeeper Threshold for the NEXT phase boundary
            UpdateHealthGatingThreshold();

            // 1. PHASE START: Trigger Roar/Cleanup if needed
            if (phase.hasAnimation)
            {
                coreManager.SetInvincible(true); // Secure invincibility state explicitly
                StopAndCleanAllAttacks(false);
                yield return StartCoroutine(coreManager.SwitchPhaseCoroutine());
                MusicManager.Instance.StartBossMusic();
                coreManager.SetInvincible(false); // Turn off invincibility after animation finishes
            }
            else
            {
                // Ensure they aren't stuck invincible if this phase skipped the animation
                coreManager.SetInvincible(false); 
            }

            // 2. RUN ACTIONS
            while (true)
            {
                bool phaseShouldEnd = false;

                if (phase.playSequentially)
                {
                    foreach (var action in phase.attacksInPhase)
                    {
                        yield return StartCoroutine(ExecuteAttackAction(action));
                        if (ShouldInterruptPhase(phase)) { phaseShouldEnd = true; break; }
                    }
                }
                else
                {
                    int randomIndex = UnityEngine.Random.Range(0, phase.attacksInPhase.Count);
                    yield return StartCoroutine(ExecuteAttackAction(phase.attacksInPhase[randomIndex]));
                    if (ShouldInterruptPhase(phase)) phaseShouldEnd = true;
                }

                // 3. CHECK EXIT CONDITIONS
                if (phase.runOnlyOnce || phaseShouldEnd)
                {
                    currentPhaseIndex++;
                    break;
                }
                
                yield return null;
            }
        }
        Debug.Log("Fight Ended: All phases exhausted.");
    }

    // --- NEW METHOD TO CALCULATE AND SEND HEALTH GATE TO CORE MANAGER ---
    private void UpdateHealthGatingThreshold()
    {
        // Look ahead to check the threshold requirement of the next phase index
        int nextPhaseIndex = currentPhaseIndex + 1;
        if (nextPhaseIndex < bossPhases.Count)
        {
            float targetPercentage = bossPhases[nextPhaseIndex].healthPercentageThreshold;
            float minHpAllowed = (targetPercentage / 100f) * maxHpCache;
            coreManager.SetHealthCap(minHpAllowed);
        }
        else
        {
            // If it's the last phase, let it drop all the way down to 0 HP
            coreManager.SetHealthCap(0f);
        }
    }

    private bool ShouldInterruptPhase(BossPhase phase)
    {
        if (phase.runOnlyOnce) return false;
        return GetCurrentHealthPercentage() <= phase.healthPercentageThreshold;
    }

    private float GetCurrentHealthPercentage()
    {
        var hpField = typeof(DreamCoreManager).GetField("hp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (hpField == null) return 0f;
        return (Convert.ToSingle(hpField.GetValue(coreManager)) / maxHpCache) * 100f;
    }

    private IEnumerator ExecuteAttackAction(AttackAction action)
    {
        isExecutingAction = true;
        if (action.delayBeforeAttack > 0) yield return new WaitForSeconds(action.delayBeforeAttack);

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(BomberAttack).GetField("currentProjectileIndex", flags)?.SetValue(bomberScript, action.projectileIndex);
        
        int count = (int)UnityEngine.Random.Range(action.projectileCountRange.x, action.projectileCountRange.y + 1);
        Transform player = (Transform)typeof(BomberAttack).GetField("player", flags)?.GetValue(bomberScript);
        float yTarget = player != null ? player.position.y : 0f;

        switch (action.pattern)
        {
            case AttackPattern.SingleLaunch:
                yield return StartCoroutine((IEnumerator)typeof(BomberAttack).GetMethod("LaunchProcedure", flags).Invoke(bomberScript, null));
                break;
            case AttackPattern.StrikeZone:
                yield return StartCoroutine((IEnumerator)typeof(BomberAttack).GetMethod("StartStrikeZone", flags).Invoke(bomberScript, new object[] { count, player, action.radius }));
                break;
            case AttackPattern.CircleLaunch:
                typeof(BomberAttack).GetField("isCircleDelayed", flags)?.SetValue(bomberScript, action.useCircleDelayed);
                float speed = action.useCircleDelayed ? action.launchSpeedRange.x : 0f;
                yield return StartCoroutine((IEnumerator)typeof(BomberAttack).GetMethod("StartCircleLaunch", flags).Invoke(bomberScript, new object[] { count, action.radius, speed, yTarget }));
                break;
            case AttackPattern.RandomLaunch:
                yield return StartCoroutine((IEnumerator)typeof(BomberAttack).GetMethod("StartRandomLaunch", flags).Invoke(bomberScript, new object[] { action.projectileCountRange, action.radius, action.launchSpeedRange, yTarget }));
                break;
        }

        if (action.delayAfterAttack > 0) yield return new WaitForSeconds(action.delayAfterAttack);
        isExecutingAction = false;
    }

    public void StopAndCleanAllAttacks(bool disableManager = true)
    {
        Debug.Log("[Boss Manager] Cleaning Arena...");
        
        Camera.main.transform.DOShakePosition(0.5f, 0.5f);

        if (disableManager) StopAllCoroutines();
        bomberScript.StopAllCoroutines();

        foreach (var bomb in FindObjectsByType<StarBomb>(FindObjectsSortMode.None))
        {
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            GameObject preview = (GameObject)typeof(StarBomb).GetField("targetPreview", flags)?.GetValue(bomb);
            if (preview != null) Destroy(preview);
            Destroy(bomb.gameObject);
        }

        foreach (var enemy in FindObjectsByType<EnnemyBase>(FindObjectsSortMode.None)) Destroy(enemy.gameObject);

        if (disableManager)
        {
            Debug.Log("Fight Ended");
            coreManager.KillBoss();
            this.enabled = false;
        }
    }
}