using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FlyingEnnemy : EnnemyBase
{
    [SerializeField] float LookRange = 5f;

    [Header("Layer")]
    [SerializeField] LayerMask LayerTarget;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        isPlayerInFieldOfView();
        if (TargetInFieldOfView)
        {
            if (move != "attack")
            {
                EyesSetColorTo(colorChase);
                move = "attack";
            }
        }
        else
        {
            if (move != "wait")
            {
                EyesSetColorTo(colorNormal);
                move = "wait";
            }
        }

        if (move == "attack")
        {
            Vector3 relativePos = CurrentTarget.position - transform.position;
            Quaternion lookAtTarget = Quaternion.LookRotation(relativePos, Vector3.up);

            transform.rotation = Quaternion.Slerp(transform.rotation, lookAtTarget, 0.25f);
            
        }
    }

    void isPlayerInFieldOfView()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, LookRange, LayerTarget);
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
                TargetInFieldOfView = true;
                CurrentTarget = Leure;

                return;
            }
            else if (Player != null)
            {
                TargetInFieldOfView = true;
                CurrentTarget = Player;

                return;
            }
        }

        TargetInFieldOfView = false;
    }
}
