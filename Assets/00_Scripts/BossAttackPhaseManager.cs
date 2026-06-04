using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackPhaseManager : MonoBehaviour
{
    public enum AttackPattern
    {
        SingleLaunch,
        StrikeZone,
        CircleLaunch,
        RandomLaunch
    }

    [System.Serializable]
    public class AttackAction
    {
        [Tooltip("Name for designer organization")]
        public string actionName = "New Attack";
        
        [Tooltip("Which attack method from BomberAttack to run")]
        public AttackPattern pattern;

        [Tooltip("Index of the prefab to pick from BomberAttack's pool")]
        public int projectileIndex = 0;

        // Custom Drawer handles visibility for these fields:
        public Vector2 projectileCountRange = new Vector2(5, 5);
        public float radius = 4f;
        public Vector2 launchSpeedRange = new Vector2(0.2f, 0.5f);
        public bool useCircleDelayed = false;

        [Header("Flow Control")]
        [Tooltip("Time to wait BEFORE this specific attack fires")]
        public float delayBeforeAttack = 0f;
        [Tooltip("Time to wait AFTER this specific attack fires before proceeding")]
        public float delayAfterAttack = 2f;
    }

    [System.Serializable]
    public class BossPhase
    {
        public string phaseName = "New Phase";
        
        [Tooltip("Loop Phase: Loops until HP drops below this percentage. Run Once Phase: Used as a fallback check when exiting.")]
        [Range(0f, 100f)] 
        public float healthPercentageThreshold = 100f;
        
        [Tooltip("If true, actions play sequentially. If false, picks one random action per sequence step.")]
        public bool playSequentially = true;

        [Tooltip("If checked, plays its sequence exactly once, then drops directly to the phase corresponding to the current HP.")]
        public bool runOnlyOnce = false;

        [Tooltip("Downtime pacing delay applied AFTER this phase completely finishes before the next phase begins.")]
        public float waitTimeAfterPhase = 3f;
        
        public List<AttackAction> attacksInPhase = new List<AttackAction>();
    }

    [Header("References")]
    [SerializeField] private DreamCoreManager coreManager;
    [SerializeField] private BomberAttack bomberScript;

    [Header("Phase Configuration")]
    [Tooltip("Order your phases sequentially from top to bottom (Phase 1, Phase 2, Phase 3, etc.)")]
    [SerializeField] private List<BossPhase> bossPhases = new List<BossPhase>();

    private int currentPhaseIndex = 0;
    private bool isExecutingPhaseRoutine = false;
    private float maxHpCache = 1000f; 

    public void StartBossAttack()
    {
        if (coreManager == null || bomberScript == null || bossPhases.Count == 0)
        {
            Debug.LogError("BossAttackPhaseManager setup is incomplete!", this);
            enabled = false;
            return;
        }

        // Cache Max HP via reflection
        var hpField = typeof(DreamCoreManager).GetField("maxHP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (hpField != null) maxHpCache = Convert.ToSingle(hpField.GetValue(coreManager));

        currentPhaseIndex = 0;
        StartCoroutine(MasterPhaseLoop());
    }

    private IEnumerator MasterPhaseLoop()
    {
        while (currentPhaseIndex < bossPhases.Count)
        {
            BossPhase activePhase = bossPhases[currentPhaseIndex];
            Debug.Log($"[Phase Manager] Beginning Phase: {activePhase.phaseName}");

            isExecutingPhaseRoutine = true;
            StartCoroutine(RunPhaseSequences(activePhase));

            // Wait until the phase explicitly completes its execution block
            while (isExecutingPhaseRoutine)
            {
                yield return null;
            }

            // Phase has finished! Apply the designer's pacing downtime before starting the next one
            if (activePhase.waitTimeAfterPhase > 0f && currentPhaseIndex < bossPhases.Count)
            {
                Debug.Log($"[Phase Manager] Pacing Downtime: Waiting {activePhase.waitTimeAfterPhase}s before next phase.");
                coreManager.SwitchPhase(activePhase.waitTimeAfterPhase);
                yield return new WaitForSeconds(activePhase.waitTimeAfterPhase);
            }
        }

        Debug.Log("[Phase Manager] All configured boss phases have been exhausted.");
    }

    private IEnumerator RunPhaseSequences(BossPhase phase)
    {
        // Keep running sequences if it's a loop phase, or run exactly once if runOnlyOnce is true
        while (true)
        {
            // Execute all actions inside this sequence iteration
            if (phase.playSequentially)
            {
                for (int i = 0; i < phase.attacksInPhase.Count; i++)
                {
                    yield return StartCoroutine(ExecuteAttackAction(phase.attacksInPhase[i]));
                }
            }
            else
            {
                // Non-sequential execution runs the same total number of actions, but randomly selected
                for (int i = 0; i < phase.attacksInPhase.Count; i++)
                {
                    int randomIndex = UnityEngine.Random.Range(0, phase.attacksInPhase.Count);
                    yield return StartCoroutine(ExecuteAttackAction(phase.attacksInPhase[randomIndex]));
                }
            }

            // Sequence iteration is finished! Evaluate what to do next based on Phase rules
            float currentHpPercent = GetCurrentHealthPercentage();

            if (phase.runOnlyOnce)
            {
                // Run Once Rule: Scan the ordered list to find which phase matches our current HP
                int targetPhaseIndex = currentPhaseIndex + 1; // Default fallback: next item
                for (int i = 0; i < bossPhases.Count; i++)
                {
                    if (currentHpPercent <= bossPhases[i].healthPercentageThreshold)
                    {
                        targetPhaseIndex = i;
                    }
                }

                // Protect against getting stuck in a loop if it resolves back to itself
                currentPhaseIndex = Mathf.Max(targetPhaseIndex, currentPhaseIndex + 1);
                break; 
            }
            else
            {
                // Loop Rule: Check if our HP dropped below the threshold required to maintain this loop
                if (currentHpPercent <= phase.healthPercentageThreshold)
                {
                    Debug.Log($"[Phase Manager] HP dropped below threshold ({phase.healthPercentageThreshold}%). Breaking loop.");
                    currentPhaseIndex++;
                    break;
                }
                // If health is still high, the while(true) loop continues on this phase!
            }
        }

        isExecutingPhaseRoutine = false;
    }

    private float GetCurrentHealthPercentage()
    {
        var hpField = typeof(DreamCoreManager).GetField("hp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (hpField == null) return 100f;

        float currentHp = Convert.ToSingle(hpField.GetValue(coreManager));
        return (currentHp / maxHpCache) * 100f;
    }

    private IEnumerator ExecuteAttackAction(AttackAction action)
    {
        if (action.delayBeforeAttack > 0)
        {
            yield return new WaitForSeconds(action.delayBeforeAttack);
        }

        // Sync projectile index
        var indexField = typeof(BomberAttack).GetField("currentProjectileIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (indexField != null) indexField.SetValue(bomberScript, action.projectileIndex);

        int countValue = (int)UnityEngine.Random.Range(action.projectileCountRange.x, action.projectileCountRange.y + 1);
        Transform playerTransform = (Transform)typeof(BomberAttack).GetField("player", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(bomberScript);
        float yTarget = playerTransform != null ? playerTransform.position.y : 0f;

        switch (action.pattern)
        {
            case AttackPattern.SingleLaunch:
                yield return StartCoroutine((IEnumerator)typeof(BomberAttack).GetMethod("LaunchProcedure", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(bomberScript, null));
                break;

            case AttackPattern.StrikeZone:
                yield return StartCoroutine((IEnumerator)typeof(BomberAttack).GetMethod("StartStrikeZone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(bomberScript, new object[] { countValue, playerTransform, action.radius }));
                break;

            case AttackPattern.CircleLaunch:
                var delayedBoolField = typeof(BomberAttack).GetField("isCircleDelayed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (delayedBoolField != null) delayedBoolField.SetValue(bomberScript, action.useCircleDelayed);

                float speed = action.useCircleDelayed ? action.launchSpeedRange.x : 0f;
                yield return StartCoroutine((IEnumerator)typeof(BomberAttack).GetMethod("StartCircleLaunch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(bomberScript, new object[] { countValue, action.radius, speed, yTarget }));
                break;

            case AttackPattern.RandomLaunch:
                yield return StartCoroutine((IEnumerator)typeof(BomberAttack).GetMethod("StartRandomLaunch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(bomberScript, new object[] { action.projectileCountRange, action.radius, action.launchSpeedRange, yTarget }));
                break;
        }

        if (action.delayAfterAttack > 0)
        {
            yield return new WaitForSeconds(action.delayAfterAttack);
        }
    }
    
    /// <summary>
    /// Shuts down all logic entirely and cleans up every spawned visual indicator and projectile.
    /// </summary>
    public void StopAndCleanAllAttacks()
    {
        Debug.Log($"[Boss Manager] Clean Shutdown Triggered! Stopping all attack sequences.");
        
        // Fix: Declared at the function scope level so both step 2 and step 3 can use it safely
        System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        // 1. Stop the phase tracker coroutines instantly
        StopAllCoroutines();
        isExecutingPhaseRoutine = false;

        // 2. Clear any lingering settings on the launcher base itself
        if (bomberScript != null)
        {
            bomberScript.StopAllCoroutines();
            
            // Re-set launching states to false via reflection to prevent hanging states
            typeof(BomberAttack).GetField("isLaunching", flags)?.SetValue(bomberScript, false);
            typeof(BomberAttack).GetField("isStriking", flags)?.SetValue(bomberScript, false);
            typeof(BomberAttack).GetField("isCircleLaunch", flags)?.SetValue(bomberScript, false);
            typeof(BomberAttack).GetField("isRandomLaunch", flags)?.SetValue(bomberScript, false);
        }

        // 3. Scrub every StarBomb currently ticking down in the scene
        StarBomb[] activeBombs = FindObjectsByType<StarBomb>(FindObjectsSortMode.None);
        foreach (StarBomb bomb in activeBombs)
        {
            if (bomb != null)
            {
                // Force target rings to destroy if they detached to player parent
                var targetPreviewField = typeof(StarBomb).GetField("targetPreview", flags);
                if (targetPreviewField != null)
                {
                    GameObject previewObj = (GameObject)targetPreviewField.GetValue(bomb);
                    if (previewObj != null) Destroy(previewObj);
                }
                
                Destroy(bomb.gameObject);
            }
        }

        // 4. Scrub any Airborne enemies spawned by the boss launcher that haven't landed yet
        EnnemyBase[] activeEnemies = FindObjectsByType<EnnemyBase>(FindObjectsSortMode.None);
        foreach (EnnemyBase enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        
        // Disable this manager completely so it doesn't try to run anymore
        this.enabled = false;
    }
}