using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class BomberAttack : MonoBehaviour
{
    public enum ProjectileSelectionMode
    {
        UseSelectedIndex,
        PickRandomFromList
    }

    [Header("Projectile Pool")]
    [Tooltip("Add all your different StarBombs and Enemy prefabs here")]
    [SerializeField] private List<GameObject> projectilePool = new List<GameObject>();

    [Tooltip("How should the launcher choose from the list above?")]
    [SerializeField] private ProjectileSelectionMode selectionMode = ProjectileSelectionMode.UseSelectedIndex;

    [Tooltip("If selection mode is set to 'Use Selected Index', this specific element index will fire next.")]
    [SerializeField] private int currentProjectileIndex = 0;

    [Header("Flight & Setup Settings")]
    [SerializeField] private AnimationCurve bombCurve;
    [SerializeField] private Transform player;
    [SerializeField] private float chargeSpeed;
    [SerializeField] private float launchSpeed;
    [SerializeField] private float launchHeight;
    [SerializeField] private float launcherSafeRadius = 2.23f;

    [Header("Launch Modes")]
    [SerializeField] private bool isLaunching;

    [Header("Launch Around")]
    [SerializeField] private bool isStriking;
    [SerializeField] private int strikeNb = 3;
    [SerializeField] private float strikeRadius = 2;

    [Header("Circle Launch")]
    [SerializeField] private bool isCircleLaunch;
    [SerializeField] private bool isCircleDelayed;
    [SerializeField] private int nbCircleLaunched = 5;
    [SerializeField] private float circleRadius = 4;
    [SerializeField] private float circleLaunchSpeed = 0.5f;

    [Header("Random Launch")]
    [SerializeField] private bool isRandomLaunch;
    [SerializeField] private Vector2 nbRandomLaunched = new Vector2(5, 10);
    [SerializeField] private float randomRadius = 4;
    [SerializeField] private Vector2 randomLaunchSpeed = new Vector2(0.1f, 1);

    // FIX: The phase manager sets itself here so that Fire() coroutines
    // are owned by the phase manager, not by BomberAttack. This means
    // AbortPendingLaunches() can stop all BomberAttack coroutines (killing
    // the charge/setup phase) without killing already in-flight arcs,
    // because those Fire() coroutines live on the phase manager component.
    [HideInInspector] public MonoBehaviour fireCoroutineHost;

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            isLaunching = false;
            isStriking = false;
            isCircleLaunch = false;
            isRandomLaunch = false;
            return;
        }

        if (isLaunching) StartCoroutine(LaunchProcedure());
        if (isStriking) StartCoroutine(StartStrikeZone(strikeNb, player, strikeRadius));
        if (isCircleLaunch && isCircleDelayed) StartCoroutine(StartCircleLaunch(nbCircleLaunched, circleRadius, circleLaunchSpeed, player.position.y));
        if (isCircleLaunch && !isCircleDelayed) StartCoroutine(StartCircleLaunch(nbCircleLaunched, circleRadius, 0, player.position.y));
        if (isRandomLaunch) StartCoroutine(StartRandomLaunch(nbRandomLaunched, randomRadius, randomLaunchSpeed, player.position.y));
    }

    // FIX: Stops all setup/charge coroutines on this component immediately.
    // Fire() coroutines are NOT affected because they are hosted on the
    // phase manager (fireCoroutineHost), so in-flight projectile arcs
    // keep moving naturally through their animation until they land.
    public void AbortPendingLaunches()
    {
        StopAllCoroutines();
        isLaunching = false;
        isStriking = false;
        isCircleLaunch = false;
        isRandomLaunch = false;
    }

    private GameObject GetSelectedProjectilePrefab()
    {
        if (projectilePool == null || projectilePool.Count == 0)
        {
            Debug.LogError("Projectile Pool is empty on BomberAttack script!", gameObject);
            return null;
        }

        if (selectionMode == ProjectileSelectionMode.PickRandomFromList)
        {
            int randomIndex = Random.Range(0, projectilePool.Count);
            return projectilePool[randomIndex];
        }

        int safeIndex = Mathf.Clamp(currentProjectileIndex, 0, projectilePool.Count - 1);
        return projectilePool[safeIndex];
    }

    private void SetupProjectile(GameObject projectile, Vector3 targetPos)
    {
        if (projectile == null) return;

        if (projectile.TryGetComponent<EnnemyBase>(out EnnemyBase enemy))
        {
            enemy.isAirbone = true;

            if (projectile.GetComponent<Animator>() != null)
                projectile.GetComponent<Animator>().enabled = false;

            if (projectile.GetComponent<UnityEngine.AI.NavMeshAgent>() != null)
                projectile.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        }
    }

    private void ShowPreview(GameObject projectile, Vector3 targetPos)
    {
        if (projectile == null) return;

        if (projectile.TryGetComponent<StarBomb>(out StarBomb star))
        {
            star.ShowPreview(targetPos, player);
        }
        else if (projectile.TryGetComponent<EnnemyBase>(out EnnemyBase enemy))
        {
            enemy.ShowPreview(targetPos, player);
        }
    }

    private IEnumerator LaunchProcedure()
    {
        GameObject activePrefab = GetSelectedProjectilePrefab();
        if (activePrefab == null) { isLaunching = false; yield break; }

        Vector3 target = player.position;
        GameObject newProjectile = Instantiate(activePrefab, transform.position, activePrefab.transform.rotation);

        SetupProjectile(newProjectile, target);
        ShowPreview(newProjectile, target);

        yield return new WaitForSeconds(chargeSpeed);

        // FIX: Route Fire() through the host so it survives AbortPendingLaunches()
        MonoBehaviour host = fireCoroutineHost != null ? fireCoroutineHost : this;
        host.StartCoroutine(Fire(newProjectile, transform.position, target));
        isLaunching = false;
    }

    private IEnumerator StartStrikeZone(int nb, Transform target, float radius)
    {
        GameObject activePrefab = GetSelectedProjectilePrefab();
        if (activePrefab == null) { isStriking = false; yield break; }

        List<(GameObject, Vector3)> items = new List<(GameObject, Vector3)>();
        Vector3 launcherPos = transform.position;

        for (int i = 0; i < nb; i++)
        {
            Vector3 targetPos;
            int safetyCounter = 0;

            do
            {
                targetPos = target.position;
                targetPos += new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
                safetyCounter++;
            }
            while (Vector3.Distance(new Vector3(targetPos.x, launcherPos.y, targetPos.z), launcherPos) < launcherSafeRadius && safetyCounter < 10);

            GameObject newProjectile = Instantiate(activePrefab, transform.position, activePrefab.transform.rotation);
            items.Add((newProjectile, targetPos));

            SetupProjectile(newProjectile, targetPos);
            ShowPreview(newProjectile, targetPos);
        }

        yield return new WaitForSeconds(chargeSpeed);

        // FIX: Route Fire() through the host so it survives AbortPendingLaunches()
        MonoBehaviour host = fireCoroutineHost != null ? fireCoroutineHost : this;
        foreach (var item in items)
        {
            if (item.Item1 != null)
            {
                host.StartCoroutine(Fire(item.Item1, transform.position, item.Item2));
            }
        }
        isStriking = false;
    }

    private IEnumerator StartCircleLaunch(int nb, float radius, float lTime, float yTarget)
    {
        GameObject activePrefab = GetSelectedProjectilePrefab();
        if (activePrefab == null) { isCircleLaunch = false; yield break; }

        List<(GameObject obj, Vector3 target)> items = new List<(GameObject, Vector3)>();
        float effectiveRadius = Mathf.Max(radius, launcherSafeRadius + 0.5f);

        for (int i = 0; i < nb; i++)
        {
            float angle = (360f / nb) * i;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 targetPos = transform.position + (rotation * Vector3.forward * effectiveRadius);
            targetPos.y = yTarget;

            GameObject newProjectile = Instantiate(activePrefab, transform.position, activePrefab.transform.rotation);
            items.Add((newProjectile, targetPos));

            SetupProjectile(newProjectile, targetPos);
        }

        // FIX: Route Fire() through the host so it survives AbortPendingLaunches()
        MonoBehaviour host = fireCoroutineHost != null ? fireCoroutineHost : this;
        foreach (var item in items)
        {
            ShowPreview(item.obj, item.target);
            yield return new WaitForSeconds(lTime);

            if (item.obj != null)
            {
                host.StartCoroutine(Fire(item.obj, transform.position, item.target));
            }
        }

        isCircleLaunch = false;
    }

    private IEnumerator StartRandomLaunch(Vector2 rndNb, float radius, Vector2 rndLTime, float yTarget)
    {
        GameObject activePrefab = GetSelectedProjectilePrefab();
        if (activePrefab == null) { isRandomLaunch = false; yield break; }

        int nb = (int)Random.Range(rndNb.x, rndNb.y);
        List<(GameObject obj, Vector3 target)> items = new List<(GameObject, Vector3)>();
        Vector3 launcherPos = transform.position;

        for (int i = 0; i < nb; i++)
        {
            Vector3 targetPos;
            int safetyCounter = 0;

            do
            {
                targetPos = transform.position;
                targetPos += new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
                targetPos.y = yTarget;
                safetyCounter++;
            }
            while (Vector3.Distance(new Vector3(targetPos.x, launcherPos.y, targetPos.z), launcherPos) < launcherSafeRadius && safetyCounter < 10);

            GameObject newProjectile = Instantiate(activePrefab, transform.position, activePrefab.transform.rotation);
            items.Add((newProjectile, targetPos));

            SetupProjectile(newProjectile, targetPos);
        }

        // FIX: Route Fire() through the host so it survives AbortPendingLaunches()
        MonoBehaviour host = fireCoroutineHost != null ? fireCoroutineHost : this;
        foreach (var item in items)
        {
            ShowPreview(item.obj, item.target);
            float lTime = Random.Range(rndLTime.x, rndLTime.y);
            yield return new WaitForSeconds(lTime);

            if (item.obj != null)
            {
                host.StartCoroutine(Fire(item.obj, transform.position, item.target));
            }
        }

        isRandomLaunch = false;
    }

    // FIX: Made public so BossAttackPhaseManager can yield on it directly
    // via reflection, and so it can be started on the fireCoroutineHost.
    public IEnumerator Fire(GameObject projectile, Vector3 startPos, Vector3 targetPos)
    {
        if (projectile == null) yield break;

        float timePassed = 0f;
        Vector3 destination = targetPos;
        destination.y -= 1;

        Vector3 randomTumbleAxis = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        float tumbleSpeed = Random.Range(180f, 360f);
        Quaternion originalRotation = projectile.transform.rotation;

        while (timePassed < launchSpeed)
        {
            if (projectile == null) yield break;

            float linearT = timePassed / launchSpeed;
            float heightT = bombCurve.Evaluate(linearT);
            float heightOffset = heightT * launchHeight;

            Vector3 currentPos = Vector3.Lerp(startPos, destination, linearT);
            currentPos.y += heightOffset;
            projectile.transform.position = currentPos;

            if (linearT < 0.8f)
            {
                projectile.transform.Rotate(randomTumbleAxis, tumbleSpeed * Time.deltaTime, Space.Self);
            }
            else
            {
                float settleT = (linearT - 0.8f) / 0.2f;
                projectile.transform.rotation = Quaternion.Slerp(projectile.transform.rotation, originalRotation, settleT);
            }

            timePassed += Time.deltaTime;
            yield return null;
        }

        if (projectile != null)
        {
            projectile.transform.position = destination;
            projectile.transform.rotation = originalRotation;

            if (projectile.TryGetComponent<StarBomb>(out StarBomb star))
            {
                star.StartCountdown();
            }
            else if (projectile.TryGetComponent<EnnemyBase>(out EnnemyBase enemy))
            {
                if (projectile.GetComponent<Animator>() != null)
                    projectile.GetComponent<Animator>().enabled = true;

                if (projectile.GetComponent<UnityEngine.AI.NavMeshAgent>() != null)
                    projectile.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;

                enemy.isAirbone = false;
                enemy.move = "chase";
                enemy.alwaysAgro = true;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(origin, Quaternion.identity, new Vector3(1, 0.01f, 1));

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, launcherSafeRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Vector3.zero, circleRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, randomRadius);

        Gizmos.matrix = oldMatrix;
        if (player != null)
        {
            Gizmos.matrix = Matrix4x4.TRS(player.position, Quaternion.identity, new Vector3(1, 0.01f, 1));
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Vector3.zero, strikeRadius);
            Gizmos.matrix = oldMatrix;
        }
    }
}