using UnityEngine;
using UnityEngine.UIElements;

public class CapSpriteEnnemy : ClassicEnnemy
{
    [SerializeField] float DistStartAttack = 5f;
    [SerializeField] GameObject Laser;
    [SerializeField] Transform laserSpawn;

    [SerializeField] bool repositionToAttack = false;
    [SerializeField] float afterAttackWait = 3;

    [SerializeField] GameObject LaserPrevisuArrow;

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
                move = "shoot";
                animator.SetTrigger("tLaunch");
                timerGeneral = waitAfterAttack;

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

                animator.SetTrigger("tCharge");
                timerGeneral = chargeTime;
                move = "aim";
                navMesh.isStopped = true;
                LaserPrevisuArrow.SetActive(true);
            }
        }
        if (move == "shoot")
        {
            timerGeneral -= Time.deltaTime;
            if (timerGeneral <= 0)
            {
                animator.SetTrigger("tLaunch");
                timerGeneral = afterAttackWait;
                move = "after shoot";
                LaserPrevisuArrow.SetActive(false);
            }
        }
        if (move == "after shoot")
        {
            timerGeneral -= Time.deltaTime;
            if (timerGeneral <= 0)
            {
                PatrolStart();
                navMesh.isStopped = false;
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
                animator.SetTrigger("tCharge");
                timerGeneral = chargeTime;
                move = "aim";
                navMesh.isStopped = true;
                LaserPrevisuArrow.SetActive(true);
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
}