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

    private bool _phaseCappedMidAttack = false;

    private void OnEnable()
    {
        DreamCoreManager.OnPhaseCapped += HandlePhaseCappedMidAttack;
    }

    private void OnDisable()
    {
        DreamCoreManager.OnPhaseCapped -= HandlePhaseCappedMidAttack;
    }

    private void HandlePhaseCappedMidAttack()
    {
        _phaseCappedMidAttack = true;
        // FIX: We no longer call bomberScript.AbortPendingLaunches() here. 
        // This ensures the boss finishes actively shooting before the animation starts.
    }

    public void StartBossAttack()
    {
        if (coreManager == null || bomberScript == null || bossPhases.Count == 0) return;

        this.enabled = true;
        StopAllCoroutines();

        bomberScript.fireCoroutineHost = this;

        var hpField = typeof(DreamCoreManager).GetField("maxHP",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (hpField != null) maxHpCache = Convert.ToSingle(hpField.GetValue(coreManager));

        currentPhaseIndex = 0;
        _phaseCappedMidAttack = false;

        UpdateHealthGatingThreshold();

        StartCoroutine(MasterPhaseLoop());
    }

    private IEnumerator MasterPhaseLoop()
    {
        while (currentPhaseIndex < bossPhases.Count)
        {
            BossPhase phase = bossPhases[currentPhaseIndex];
            _phaseCappedMidAttack = false;

            // --- PHASE ENTRY ANIMATION ---
            if (phase.hasAnimation)
            {
                coreManager.SetInvincible(true);
                yield return StartCoroutine(coreManager.SwitchPhaseCoroutine());
                MusicManager.Instance.StartBossMusic();
                coreManager.SetInvincible(false);
            }
            else
            {
                coreManager.SetInvincible(false);
            }

            // --- ATTACK LOOP ---
            if (phase.runOnlyOnce)
            {
                if (phase.playSequentially)
                {
                    foreach (var action in phase.attacksInPhase)
                    {
                        yield return StartCoroutine(ExecuteAttackAction(action));
                        // Break early if we hit the animation health cap mid-sequence
                        if (_phaseCappedMidAttack) break;
                    }
                }
                else
                {
                    // Random non-repeating shuffle for Run Only Once
                    List<int> indices = new List<int>();
                    for (int i = 0; i < phase.attacksInPhase.Count; i++) indices.Add(i);

                    // Shuffle list
                    for (int i = 0; i < indices.Count; i++)
                    {
                        int temp = indices[i];
                        int randomIndex = UnityEngine.Random.Range(i, indices.Count);
                        indices[i] = indices[randomIndex];
                        indices[randomIndex] = temp;
                    }

                    foreach (int idx in indices)
                    {
                        yield return StartCoroutine(ExecuteAttackAction(phase.attacksInPhase[idx]));
                        if (_phaseCappedMidAttack) break;
                    }
                }
            }
            else
            {
                // Looping phase
                while (true)
                {
                    bool phaseShouldEnd = false;

                    if (phase.playSequentially)
                    {
                        foreach (var action in phase.attacksInPhase)
                        {
                            yield return StartCoroutine(ExecuteAttackAction(action));
                            if (_phaseCappedMidAttack || GetCurrentHealthPercentage() <= phase.healthPercentageThreshold)
                            {
                                phaseShouldEnd = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        int randomIndex = UnityEngine.Random.Range(0, phase.attacksInPhase.Count);
                        yield return StartCoroutine(ExecuteAttackAction(phase.attacksInPhase[randomIndex]));

                        if (_phaseCappedMidAttack || GetCurrentHealthPercentage() <= phase.healthPercentageThreshold)
                        {
                            phaseShouldEnd = true;
                        }
                    }

                    if (phaseShouldEnd) break;
                }
            }

            // --- EVALUATE NEXT PHASE ---
            float currentHP = GetCurrentHealthPercentage();
            int nextPhase = currentPhaseIndex + 1;

            // FIX: Skip minor phases if player dealt massive damage, 
            // but always stop if a phase requires an animation or is a special RunOnce sequence.
            while (nextPhase < bossPhases.Count)
            {
                if (bossPhases[nextPhase].hasAnimation) break;
                if (bossPhases[nextPhase].runOnlyOnce) break;
                if (currentHP > bossPhases[nextPhase].healthPercentageThreshold) break;
                
                nextPhase++;
            }

            currentPhaseIndex = nextPhase;
            
            if (currentPhaseIndex < bossPhases.Count)
            {
                UpdateHealthGatingThreshold();
            }
            else
            {
                break;
            }
        }

        Debug.Log("Fight Ended: All phases exhausted.");
    }

    private void UpdateHealthGatingThreshold()
    {
        float targetPercentage = 0f;
        bool foundCap = false;

        // Look ahead to find the next phase that requires an animation transition
        for (int i = currentPhaseIndex + 1; i < bossPhases.Count; i++)
        {
            if (bossPhases[i].hasAnimation)
            {
                // The cap is the threshold of the phase immediately PRECEDING the animation phase
                // This represents the HP at which the animation phase begins.
                targetPercentage = bossPhases[i - 1].healthPercentageThreshold;
                foundCap = true;
                break;
            }
        }

        if (foundCap)
        {
            float minHpAllowed = (targetPercentage / 100f) * maxHpCache;
            coreManager.SetHealthCap(minHpAllowed);
        }
        else
        {
            // No future animation phases, boss can die naturally
            coreManager.SetHealthCap(0f);
        }
    }

    private float GetCurrentHealthPercentage()
    {
        var hpField = typeof(DreamCoreManager).GetField("hp",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (hpField == null) return 0f;
        return (Convert.ToSingle(hpField.GetValue(coreManager)) / maxHpCache) * 100f;
    }

    private IEnumerator ExecuteAttackAction(AttackAction action)
    {
        isExecutingAction = true;

        if (action.delayBeforeAttack > 0)
        {
            yield return new WaitForSeconds(action.delayBeforeAttack);
        }

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(BomberAttack).GetField("currentProjectileIndex", flags)?.SetValue(bomberScript, action.projectileIndex);

        int count = (int)UnityEngine.Random.Range(action.projectileCountRange.x, action.projectileCountRange.y + 1);
        Transform player = (Transform)typeof(BomberAttack).GetField("player", flags)?.GetValue(bomberScript);
        float yTarget = player != null ? player.position.y : 0f;

        switch (action.pattern)
        {
            case AttackPattern.SingleLaunch:
                yield return StartCoroutine((IEnumerator)typeof(BomberAttack)
                    .GetMethod("LaunchProcedure", flags).Invoke(bomberScript, null));
                break;

            case AttackPattern.StrikeZone:
                yield return StartCoroutine((IEnumerator)typeof(BomberAttack)
                    .GetMethod("StartStrikeZone", flags).Invoke(bomberScript, new object[] { count, player, action.radius }));
                break;

            case AttackPattern.CircleLaunch:
                typeof(BomberAttack).GetField("isCircleDelayed", flags)?.SetValue(bomberScript, action.useCircleDelayed);
                float speed = action.useCircleDelayed ? action.launchSpeedRange.x : 0f;
                yield return StartCoroutine((IEnumerator)typeof(BomberAttack)
                    .GetMethod("StartCircleLaunch", flags).Invoke(bomberScript, new object[] { count, action.radius, speed, yTarget }));
                break;

            case AttackPattern.RandomLaunch:
                yield return StartCoroutine((IEnumerator)typeof(BomberAttack)
                    .GetMethod("StartRandomLaunch", flags).Invoke(bomberScript, new object[] { action.projectileCountRange, action.radius, action.launchSpeedRange, yTarget }));
                break;
        }

        if (action.delayAfterAttack > 0)
        {
            float elapsed = 0f;
            while (elapsed < action.delayAfterAttack)
            {
                // FIX: If the cap is hit mid-delay, we break out immediately to skip the dead time 
                // and transition directly into the animation.
                if (_phaseCappedMidAttack) break;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        isExecutingAction = false;
    }

    public void StopAndCleanAllAttacks(bool disableManager = true)
    {
        Camera.main.transform.DOShakePosition(0.5f, 0.5f);
        
        if (disableManager)
        {
            StopAllCoroutines();
        }

        bomberScript.AbortPendingLaunches();

        foreach (var bomb in FindObjectsByType<StarBomb>(FindObjectsSortMode.None))
        {
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            GameObject preview = (GameObject)typeof(StarBomb).GetField("targetPreview", flags)?.GetValue(bomb);
            if (preview != null) Destroy(preview);
            Destroy(bomb.gameObject);
        }

        foreach (var enemy in FindObjectsByType<EnnemyBase>(FindObjectsSortMode.None))
            enemy.Kill();

        currentPhaseIndex = 0;
        isExecutingAction = false;
        _phaseCappedMidAttack = false;

        if (disableManager)
        {
            coreManager.KillBoss();
            enabled = false;
        }
    }
}