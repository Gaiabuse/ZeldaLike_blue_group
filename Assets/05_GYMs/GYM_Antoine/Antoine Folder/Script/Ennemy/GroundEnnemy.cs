using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroundEnnemy : EnnemyBase
{
    NavMeshAgent navMesh;

    [SerializeField] Transform LockOn;

    [SerializeField] float LookRange = 5f;
    [SerializeField] float DistanceAlwaysSeeEnnemy = 2f;
    [SerializeField] int RadiusLook = 45;

    [SerializeField] float LoseFocusDist = 1f;

    [SerializeField] protected Transform AttackTrigger;
    [SerializeField] float DistanceAttack = 2;

    Vector3 WhereToGoPos;

    [Header("Layer")]
    [SerializeField] LayerMask LayerBlockRay;

    [Header("Patrol Route")]
    [SerializeField] List<Vector3> PatrolPosition;
    int currentPatrolPose;


    protected override void Start()
    {
        base.Start();
        navMesh = GetComponent<NavMeshAgent>();

        navMesh.speed = speed.x;
        navMesh.acceleration = acceleration.x;
        navMesh.angularSpeed = SpeedRotate.x;

        MainHitBox.damage = data.strength;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        isPlayerInFieldOfView();

        if (TargetInFieldOfView || alwaysAgro && move != "stun")
        {
            if (move != "chase")
            {
                EyesSetColorTo(colorChase);
                navMesh.speed = speed.y;
                navMesh.acceleration = acceleration.y;
                navMesh.angularSpeed = SpeedRotate.y;
                move = "chase";
            }

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
                if (Vector3.Distance(transform.position, WhereToGoPos) < LoseFocusDist)
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

            if (Vector3.Distance(transform.position, WhereToGoPos) < 1.5f)
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
                }
                if (rangeChecks[i].CompareTag("Player"))
                {
                    if (Player == null) Player = rangeChecks[i].transform;
                }
            }

            if (leureDetected)
            {
                if (CanSeeObject(Leure))
                {
                    TargetInFieldOfView = true;
                    CurrentTarget = Leure;
                }

                else if (Player != null)
                {
                    if (CanSeeObject(Player))
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
                if (CanSeeObject(Player))
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

        if (HP > 0)
        {
            if (move != "stun")
            {
                move = "chase";
                EyesSetColorTo(colorChase);

                navMesh.speed = speed.y;
                navMesh.acceleration = acceleration.y;
                navMesh.angularSpeed = SpeedRotate.y;

                WhereToGoPos = Player.position;
            }
        }
    }

    protected virtual void AttackPatern()
    {
        if (Vector3.Distance(AttackTrigger.position, CurrentTarget.position) <= DistanceAttack && CurrentTarget != null)
        {
            AttackStart(1);
        }
    }

    protected override void AttackStart(int attackID)
    {
        base.AttackStart(attackID);
        navMesh.isStopped = true;
    }

    protected override void AttackAnimEnd()
    {
        base.AttackAnimEnd();
        navMesh.isStopped = false;

        if (CurrentTarget != null && move != "stun")
        {
            move = "chase";
        }
        else
        {
            PatrolStart();
        }
    }

    protected void PatrolStart()
    {
        EyesSetColorTo(colorNormal);

        WhereToGoPos = SelectPatrolPosition();
        move = "patrol";

        navMesh.speed = speed.x;
        navMesh.acceleration = acceleration.x;
        navMesh.angularSpeed = SpeedRotate.x;
    }

    public override void StunEnnemy(float stunTime, bool infiniteStun)
    {
        base.StunEnnemy(stunTime, infiniteStun);
        animator.SetBool("Stun", true);
        navMesh.isStopped = true;
    }

    protected override void EndStun()
    {
        base.EndStun();
        animator.SetBool("Stun", false);
        navMesh.isStopped = false;

        WhereToGoPos = Player.position;
        move = "chase";
        EyesSetColorTo(colorChase);

        navMesh.speed = speed.y;
        navMesh.acceleration = acceleration.y;
        navMesh.angularSpeed = SpeedRotate.y;
    }
}
