using UnityEngine;
using UnityEngine.UIElements;

public class CapuchonEnnemy : GroundEnnemy
{
    [SerializeField] float DistStartAttack = 5f;
    [SerializeField] GameObject Laser;
    [SerializeField] Transform laserSpawn;

    [SerializeField] bool repositionToAttack = false;

    float chargeTime = 2f;

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
                AttackStart(3);
                GameObject laser = Instantiate(Laser);
                laser.transform.position = laserSpawn.position;
                laser.transform.rotation = laserSpawn.rotation;
            }
        }
        if (move == "reposition")
        {
            if (Vector3.Distance(WhereToGoPos, transform.position) <= 2.5f)
            {
                WhereToGoPos = transform.position + (CurrentTarget.transform.forward * (DistStartAttack + 5));
                navMesh.destination = WhereToGoPos;
            }
            if (Vector3.Distance(CurrentTarget.position, transform.position) > DistStartAttack)
            {
                repositionToAttack = false;
                canLookAtPlayer = true;

                AttackStart(1);
                move = "aim start";
            }
        }
    }

    protected override void AttackPatern()
    {
        if (CurrentTarget != null)
        {
            float distTarget = Vector3.Distance(AttackTrigger.position, CurrentTarget.position);
            if (distTarget >= DistStartAttack && TargetInFieldOfView && !repositionToAttack)
            {
                AttackStart(1);
                move = "aim start";
            }
            else if (distTarget < DistanceAttack && move != "reposition")
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