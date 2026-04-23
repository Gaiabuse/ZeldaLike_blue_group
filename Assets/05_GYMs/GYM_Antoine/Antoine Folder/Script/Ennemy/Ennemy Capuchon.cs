using Unity.VisualScripting;
using UnityEngine;

public class CapuchonEnnemy : GroundEnnemy
{
    [SerializeField] float DistStartAttack = 5f;
    [SerializeField] Transform SpawnLaserWhere;
    [SerializeField] GameObject Laser;
    [SerializeField] bool repositionToAttack = false;

    [SerializeField] float chargeTime = 2f;
    [SerializeField] float prepShoot = 0.5f;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (move == "aim")
        {
            Vector3 relativePos = new Vector3(CurrentTarget.position.x, transform.position.y, CurrentTarget.position.z) - transform.position;
            Quaternion lookAtTarget = Quaternion.LookRotation(relativePos, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookAtTarget, 0.065f);

            timerGeneral -= Time.deltaTime;
            if (timerGeneral <= 0)
            {
                timerGeneral = prepShoot;
                move = "readyShoot";
            }
        }
        if (move == "readyShoot")
        {
            timerGeneral -= Time.deltaTime;
            if (timerGeneral <= 0)
            {
                move = "shoot";
                AttackStart(3);
                GameObject laser = Instantiate(Laser);
                laser.transform.position = SpawnLaserWhere.position;
                laser.transform.rotation = SpawnLaserWhere.rotation;
            }
        }
        if (repositionToAttack)
        {
            if (Vector3.Distance(transform.position, CurrentTarget.position) > DistStartAttack)
            {
                repositionToAttack = false;
                canLookAtPlayer = true;

                AttackStart(1);
                move = "aim roll";
                navMesh.speed = 0;
            }
        }
    }

    protected override void AttackPatern()
    {
        if (CurrentTarget != null)
        {
            float distTarget = Vector3.Distance(transform.position, CurrentTarget.position);
            if (distTarget >= DistStartAttack && TargetInFieldOfView && !repositionToAttack)
            {
                AttackStart(1);
                move = "aim start";
            }
            else if (distTarget < DistStartAttack && !repositionToAttack)
            {
                move = "reposition";

                repositionToAttack = true;
                canLookAtPlayer = false;
                WhereToGoPos = transform.position + (CurrentTarget.transform.forward * (DistStartAttack + 5));
                navMesh.destination = WhereToGoPos;
            }
        }
    }

    public override void AttackStart(int attackID)
    {
        base.AttackStart(attackID);
        if (attackID == 2)
        {
            move = "aim";
            timerGeneral = chargeTime;
        }
        if (attackID == 4)
        {
            animator.SetInteger("Attack", 0);
            PatrolStart();
            navMesh.isStopped = false;
        }
    }
}