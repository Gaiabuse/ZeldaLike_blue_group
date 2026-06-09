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

    // FIX: Set to true the instant DreamCoreManager clamps HP to the phase
    // floor and fires OnPhaseCapped. ExecuteAttackAction polls this flag at
    // every yield point so it exits as soon as the cap is hit, regardless
    // of where it is in the charge / delay / launch sequence.
    private bool _phaseCappedMidAttack = false;

    private void OnEnable()
    {
        DreamCoreManager.OnPhaseCapped += HandlePhaseCappedMidAttack;
    }

    private void OnDisable()
    {
        DreamCoreManager.OnPhaseCapped -= HandlePhaseCappedMidAttack;
    }

    // FIX: Fires the instant HP hits the floor mid-attack.
    // Aborts setup/charge coroutines on BomberAttack immediately,
    // while Fire() coroutines (hosted here) keep running so
    // already-airborne projectiles complete their arcs naturally.
    private void HandlePhaseCappedMidAttack()
    {
        _phaseCappedMidAttack = true;
        bomberScript.AbortPendingLaunches();
    }

    public void StartBossAttack()
    {
        if (coreManager == null || bomberScript == null || bossPhases.Count == 0) return;

        this.enabled = true;
        StopAllCoroutines();

        // FIX: Tell BomberAttack to host all Fire() coroutines here on the
        // phase manager, so AbortPendingLaunches() (which calls
        // bomberScript.StopAllCoroutines) cannot kill in-flight arcs.
        bomberScript.fireCoroutineHost = this;

        var hpField = typeof(DreamCoreManager).GetField("maxHP",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (hpField != null) maxHpCache = Convert.ToSingle(hpField.GetValue(coreManager));

        currentPhaseIndex = 0;
        _phaseCappedMidAttack = false;

        // FIX: Apply the health gate for phase 0 BEFORE any attack runs,
        // so a burst of damage at the very start of the fight is already
        // capped correctly. Previously this only happened inside the loop,
        // meaning the very first attack had no gate in place.
        UpdateHealthGatingThreshold();

        StartCoroutine(MasterPhaseLoop());
    }

    private IEnumerator MasterPhaseLoop()
    {
        while (currentPhaseIndex < bossPhases.Count)
        {
            BossPhase phase = bossPhases[currentPhaseIndex];

            // FIX: Clear the flag at the top of every phase so a cap event
            // from the previous phase does not immediately short-circuit this one.
            _phaseCappedMidAttack = false;

            // --- PHASE ENTRY ANIMATION ---
            if (phase.hasAnimation)
            {
                // Boss is already invincible (set by TakeDamages when it hit
                // the floor), but set it explicitly here to be safe.
                coreManager.SetInvincible(true);

                // Do NOT destroy projectiles here — let in-flight bombs land.
                yield return StartCoroutine(coreManager.SwitchPhaseCoroutine());

                MusicManager.Instance.StartBossMusic();
                coreManager.SetInvincible(false);
            }
            else
            {
                coreManager.SetInvincible(false);
            }

            // --- ATTACK LOOP ---
            while (true)
            {
                bool phaseShouldEnd = false;

                if (phase.playSequentially)
                {
                    foreach (var action in phase.attacksInPhase)
                    {
                        yield return StartCoroutine(ExecuteAttackAction(action));

                        // FIX: Check the cap flag after every single action.
                        // Previously only ShouldInterruptPhase was checked,
                        // which runs after a full natural completion — meaning
                        // heavy damage during a long attack could push HP below
                        // a threshold without the phase ever noticing until the
                        // next attack finished.
                        if (_phaseCappedMidAttack || ShouldInterruptPhase(phase))
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

                    if (_phaseCappedMidAttack || ShouldInterruptPhase(phase))
                        phaseShouldEnd = true;
                }

                if (phase.runOnlyOnce || phaseShouldEnd)
                {
                    currentPhaseIndex++;

                    // FIX: Update the health gate immediately after advancing
                    // the index, before looping back to the top where the
                    // animation plays. This ensures the new floor is in place
                    // for the very next phase from the first frame onwards.
                    UpdateHealthGatingThreshold();
                    break;
                }

                yield return null;
            }
        }

        Debug.Log("Fight Ended: All phases exhausted.");
    }

    // FIX: Called both at StartBossAttack (before phase 0 runs) AND
    // immediately after currentPhaseIndex is incremented. This guarantees
    // the health floor is always set for the current phase with no gap.
    private void UpdateHealthGatingThreshold()
    {
        int nextPhaseIndex = currentPhaseIndex + 1;
        if (nextPhaseIndex < bossPhases.Count)
        {
            float targetPercentage = bossPhases[nextPhaseIndex].healthPercentageThreshold;
            float minHpAllowed = (targetPercentage / 100f) * maxHpCache;
            coreManager.SetHealthCap(minHpAllowed);
        }
        else
        {
            // Last phase — let HP drop all the way to zero.
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
        var hpField = typeof(DreamCoreManager).GetField("hp",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (hpField == null) return 0f;
        return (Convert.ToSingle(hpField.GetValue(coreManager)) / maxHpCache) * 100f;
    }

    private IEnumerator ExecuteAttackAction(AttackAction action)
    {
        isExecutingAction = true;

        // FIX: If the cap was already hit during the previous action's
        // delayAfter, skip this action entirely rather than starting it
        // and immediately aborting — avoids spawning projectiles that
        // would be cleaned up one frame later.
        if (_phaseCappedMidAttack) { isExecutingAction = false; yield break; }

        // FIX: Replace WaitForSeconds with a manual loop so we can poll
        // _phaseCappedMidAttack every frame. A plain WaitForSeconds would
        // sit out the full duration even after the cap has been hit.
        if (action.delayBeforeAttack > 0)
        {
            float elapsed = 0f;
            while (elapsed < action.delayBeforeAttack)
            {
                if (_phaseCappedMidAttack) { isExecutingAction = false; yield break; }
                elapsed += Time.deltaTime;
                yield return null;
            }
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

        // FIX: Same polling loop for the post-attack delay. Without this,
        // the phase manager would sit here for the full delayAfter duration
        // even after the cap was hit, delaying the transition animation.
        if (action.delayAfterAttack > 0)
        {
            float elapsed = 0f;
            while (elapsed < action.delayAfterAttack)
            {
                if (_phaseCappedMidAttack) { isExecutingAction = false; yield break; }
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        isExecutingAction = false;
    }

    public void StopAndCleanAllAttacks(bool disableManager = true)
    {
        Debug.Log("[Boss Manager] Cleaning Arena...");
        Camera.main.transform.DOShakePosition(0.5f, 0.5f);

        if (disableManager)
        {
            MusicManager.Instance.StopBossMusic();
            StopAllCoroutines();
        }

        bomberScript.StopAllCoroutines();

        foreach (var bomb in FindObjectsByType<StarBomb>(FindObjectsSortMode.None))
        {
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            GameObject preview = (GameObject)typeof(StarBomb).GetField("targetPreview", flags)?.GetValue(bomb);
            if (preview != null) Destroy(preview);
            Destroy(bomb.gameObject);
        }

        foreach (var enemy in FindObjectsByType<EnnemyBase>(FindObjectsSortMode.None))
            Destroy(enemy.gameObject);

        currentPhaseIndex = 0;
        isExecutingAction = false;

        // FIX: Reset the mid-attack flag so a stale true value from a
        // previous fight does not short-circuit the next StartBossAttack.
        _phaseCappedMidAttack = false;

        if (disableManager)
        {
            Debug.Log("Fight Ended");
            coreManager.KillBoss();
            this.enabled = false;
        }
    }
}