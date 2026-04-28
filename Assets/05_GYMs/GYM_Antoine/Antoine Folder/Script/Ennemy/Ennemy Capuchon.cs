using UnityEngine;

public class CapuchonEnnemy : GroundEnnemy
{
    [SerializeField] float DistStartAttack = 5f;
    [SerializeField] GameObject Laser;
    bool repositionToAttack = false;

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
            else if (distTarget < DistanceAttack && !repositionToAttack)
            {

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
            move = "chase";
            navMesh.isStopped = false;
        }
    }
}