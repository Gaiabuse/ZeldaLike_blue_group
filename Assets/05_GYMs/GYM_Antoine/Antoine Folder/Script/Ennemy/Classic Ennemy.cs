using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ClassicEnnemy : EnnemyBase
{
    protected NavMeshAgent navMesh;

    [SerializeField] GameObject SpriteEnnemy;
    [SerializeField] Transform LockOn;

    [SerializeField] float LookRange = 5f;
    [SerializeField] float DistanceAlwaysSeeEnnemy = 2f;
    [SerializeField] int RadiusLook = 45;

    [SerializeField] float LoseFocusDist = 1f;

    [SerializeField] protected Transform AttackTrigger;
    [SerializeField] protected float DistanceAttack = 2;
    [SerializeField] float chargeAttackTime = 1.5f;
    [SerializeField] GameObject hitboxVFX;
    [SerializeField] protected float waitAfterAttack = 1.5f;

    [SerializeField] float waitBeforeDelete = 1.5f;

    protected Vector3 WhereToGoPos;

    [Header("Layer")]
    [SerializeField] LayerMask LayerBlockRay;

    [Header("Patrol Route")]
    [SerializeField] List<Vector3> PatrolPosition;
    int currentPatrolPose;

    [SerializeField] protected bool canLookAtPlayer = true;

    protected override void Start()
    {
        base.Start();
        navMesh = GetComponent<NavMeshAgent>();

        navMesh.speed = speed.x;
        navMesh.acceleration = acceleration.x;
        navMesh.angularSpeed = SpeedRotate.x;

        if (MainHitBox != null) MainHitBox.damage = data.strength;
        animator.SetBool("IsMoving", true);
        animator.SetBool("IsChasing", false);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (canLookAtPlayer) isPlayerInFieldOfView();

        if (move != "stun")
        {
            if (TargetInFieldOfView || alwaysAgro)
            {
                if (move != "chase" && (move == "lose chase" || move == "patrol" || move == "0"))
                {
                    EyesSetColorTo(colorChase);
                    navMesh.speed = speed.y;
                    navMesh.acceleration = acceleration.y;
                    navMesh.angularSpeed = SpeedRotate.y;
                    move = "chase";
                    animator.SetBool("IsChasing", true);
                }

                if (CurrentTarget == null) CurrentTarget = Player;
                WhereToGoPos = CurrentTarget.position;
            }
            else
            {
                if (move == "chase")
                {
                    move = "lose chase";
                }
                else if (move == "lose chase")
                {
                    if (Vector3.Distance(transform.position, WhereToGoPos) <= LoseFocusDist)
                    {
                        WhereToGoPos = SelectPatrolPosition();
                        PatrolStart();
                    }
                }
            }

            if (move == "chase" || move == "lose chase")
            {
                navMesh.destination = WhereToGoPos;

                AttackPatern();
            }

            if (move == "patrol")
            {
                navMesh.destination = WhereToGoPos;

                if (Vector3.Distance(transform.position, WhereToGoPos) <= 1.5f)
                {
                    currentPatrolPose += 1;

                    if (currentPatrolPose < PatrolPosition.Count) WhereToGoPos = PatrolPosition[currentPatrolPose];
                    else
                    {
                        currentPatrolPose = 0;
                        WhereToGoPos = PatrolPosition[0];
                    }
                }
            }

            if (move == "charge")
            {
                timerGeneral -= Time.deltaTime;
                if (timerGeneral <= 0)
                {
                    move = "attack";
                    timerGeneral = waitAfterAttack;
                    hitboxVFX.SetActive(false);
                    hitboxVFX.SetActive(true);
                    animator.SetTrigger("tAttack");
                }
            }

            if (move == "attack")
            {
                timerGeneral -= Time.deltaTime;
                if (timerGeneral <= 0)
                {
                    AttackAnimEnd();
                    animator.SetBool("IsMoving", true);
                }
            }
        }
    }

    void isPlayerInFieldOfView()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(LockOn.position, LookRange, LayerBlockRay);
        if (rangeChecks.Length > 0)
        {
            bool leureDetected = false;

            for (int i = 0; i < rangeChecks.Length; i++)
            {
                if (rangeChecks[i].CompareTag("Leure"))
                {
                    if (Leure != rangeChecks[i].transform) Leure = rangeChecks[i].transform;
                    leureDetected = true;
                    Debug.Log("Leure");
                }
                if (rangeChecks[i].CompareTag("Player"))
                {
                    if (Player == null) Player = rangeChecks[i].transform;
                }
            }

            if (leureDetected)
            {
                if (CanSeeObject(Leure) || alwaysAgro)
                {
                    TargetInFieldOfView = true;
                    CurrentTarget = Leure;
                }
                else if (Player != null)
                {
                    if (CanSeeObject(Player) || alwaysAgro)
                    {
                        TargetInFieldOfView = true;
                        CurrentTarget = Player;
                    }
                    else TargetInFieldOfView = false;
                }

                return;
            }
            else if (Player != null)
            {
                if (CanSeeObject(Player) || alwaysAgro)
                {
                    TargetInFieldOfView = true;
                    CurrentTarget = Player;
                }
                else TargetInFieldOfView = false;

                return;
            }
        }

        TargetInFieldOfView = false;
    }

    bool CanSeeObject(Transform Target)
    {
        if (Vector3.Distance(Target.position, transform.position) <= DistanceAlwaysSeeEnnemy)
        {
            return true;
        }
        else
        {
            if (Vector3.Distance(Target.position, transform.position) <= LookRange)
            {
                Vector3 anglePose1 = Target.position - LockOn.position;
                Vector3 anglePose2 = LockOn.position + (LockOn.forward * 0.5f) - LockOn.position;

                if (Vector3.Angle(anglePose1, anglePose2) < RadiusLook)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }

    Vector3 SelectPatrolPosition()
    {
        Vector3 whereTo;

        if (PatrolPosition.Count > 0)
        {
            currentPatrolPose = 0;
            whereTo = PatrolPosition[0];
            float min = Mathf.Infinity;

            for (int i = 0; i < PatrolPosition.Count; i++)
            {
                float dist = Vector3.Distance(transform.position, PatrolPosition[i]);

                if (dist < min)
                {
                    whereTo = PatrolPosition[i];
                    currentPatrolPose = i;
                    min = dist;
                }
            }
        }
        else whereTo = transform.position;

        return whereTo;
    }

    public override void TakeDamage(int damage, float stun)
    {
        base.TakeDamage(damage, stun);
        //hitVFX.transform.SetParent(transform.parent);
        hitVFX.transform.position = transform.position;
        Vector3 lookTarget = new Vector3(Player.transform.position.x, hitVFX.transform.position.y, Player.transform.position.z);
        hitVFX.transform.LookAt(lookTarget);
        hitVFX.transform.Rotate(0, 90, 0);

        hitVFX.SetActive(true);

        animator.SetBool("IsChasing", false);
        animator.SetBool("IsMoving", false);

        animator.SetTrigger("tHit");

        navMesh.isStopped = false;
        if (stun > 0)
        {
            navMesh.velocity = Vector3.zero;
        }
        else
        {
            animator.SetBool("IsMoving", true);
        }

        if (HP > 0)
        {
            if (move != "stun")
            {
                move = "chase";
                animator.SetBool("IsChasing", true);

                EyesSetColorTo(colorChase);

                navMesh.speed = speed.y;
                navMesh.acceleration = acceleration.y;
                navMesh.angularSpeed = SpeedRotate.y;

                canLookAtPlayer = true;
                WhereToGoPos = Player.position;
            }
        }
    }

    protected virtual void AttackPatern()
    {
        if (Vector3.Distance(AttackTrigger.position, CurrentTarget.position) <= DistanceAttack && CurrentTarget != null)
        {
            navMesh.speed = 0;
            navMesh.velocity = Vector3.zero;
            timerGeneral = chargeAttackTime;
            move = "charge";
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsChasing", false);
            animator.SetTrigger("tCharge");
        }
    }

    public override void AttackStart(int attackID)
    {
        base.AttackStart(attackID);
        navMesh.isStopped = true;
    }

    public override void AttackAnimEnd()
    {
        navMesh.isStopped = false;
        navMesh.speed = speed.y;
        navMesh.acceleration = acceleration.y;
        navMesh.angularSpeed = SpeedRotate.y;
        canLookAtPlayer = true;

        if (Vector3.Distance(transform.position, CurrentTarget.position) > DistanceAlwaysSeeEnnemy) PatrolStart();
        else
        {
            WhereToGoPos = CurrentTarget.position;
            move = "chase";
        }
    }

    protected void PatrolStart()
    {
        EyesSetColorTo(colorNormal);

        animator.SetBool("IsChasing", false);
        WhereToGoPos = SelectPatrolPosition();
        move = "patrol";

        navMesh.speed = speed.x;
        navMesh.acceleration = acceleration.x;
        navMesh.angularSpeed = SpeedRotate.x;
    }

    public override void StunEnnemy(float stunTime, bool infiniteStun)
    {
        base.StunEnnemy(stunTime, infiniteStun);
        animator.SetTrigger("tHit");
        animator.SetInteger("Attack", 0);
        navMesh.isStopped = true;
    }

    protected override void EndStun()
    {
        base.EndStun();
        animator.SetBool("Stun", false);
        navMesh.isStopped = false;

        WhereToGoPos = Player.position;
        move = "chase";
        animator.SetBool("IsMoving", true);
        animator.SetBool("IsChasing", true);

        EyesSetColorTo(colorChase);
        canLookAtPlayer = true;

        navMesh.speed = speed.y;
        navMesh.acceleration = acceleration.y;
        navMesh.angularSpeed = SpeedRotate.y;
    }

    protected override void Death()
    {
        move = "death";
        timerGeneral = waitBeforeDelete;
        navMesh.isStopped = true;
        navMesh.speed = 0;
        navMesh.velocity = Vector3.zero;

        if (EnnemyManager.Instance != null)
        {
            Debug.Log("remove");
            EnnemyManager.Instance.enemies.Remove(this);
            EnnemyManager.Instance.Check();
        }
        animator.SetBool("IsDead", true);
        OnDeath?.Invoke(this);
    }
    
    // Add this to the bottom of your ClassicEnnemy class
    private void OnDrawGizmos()
    {
        if (LockOn == null) return;

        // 1. Draw Look Range (Yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(LockOn.position, LookRange);

        // 2. Draw Distance Always See (Green)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, DistanceAlwaysSeeEnnemy);

        // 3. Draw Radius Look (FOV Cone)
        Gizmos.color = Color.blue;
        Vector3 forward = LockOn.forward;
        // Calculate the left and right boundaries of the FOV
        Vector3 leftRayDirection = Quaternion.AngleAxis(-RadiusLook, Vector3.up) * forward;
        Vector3 rightRayDirection = Quaternion.AngleAxis(RadiusLook, Vector3.up) * forward;

        Gizmos.DrawRay(LockOn.position, leftRayDirection * LookRange);
        Gizmos.DrawRay(LockOn.position, rightRayDirection * LookRange);

        // 4. Draw Lose Focus Distance (Gray)
        // This is checked against the WhereToGoPos when losing chase
        if (move == "lose chase")
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(WhereToGoPos, LoseFocusDist);
            Gizmos.DrawLine(transform.position, WhereToGoPos);
        }

        // 5. Draw Attack Distance (Red)
        if (AttackTrigger != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(AttackTrigger.position, DistanceAttack);
        }
    }

    protected override void DeathVFXAppear()
    {
        base.DeathVFXAppear();
        SpriteEnnemy.SetActive(false);
    }
}
