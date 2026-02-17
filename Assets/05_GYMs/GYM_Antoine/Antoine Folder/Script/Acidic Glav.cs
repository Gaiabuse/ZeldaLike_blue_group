using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AcidicGlav : Ennemy
{
    [SerializeField] ChainIKConstraint NeckRig;

    bool IKchangeWeight; float timer = 0;
    int AddIK = 0;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (IKchangeWeight)
        {
            timer += Time.deltaTime * AddIK;
            NeckRig.weight = timer;

            if (AddIK > 0 && timer >= 1) IKchangeWeight = false;
            else if (AddIK > 1 && timer <= 0) IKchangeWeight = false;
        }
    }

    protected override void AttackStart(int attackID)
    {
        base.AttackStart(attackID);
        IKchangeWeight = true;
        timer = -1; AddIK = -1;
    }

    protected override void AttackAnimEnd()
    {
        base.AttackAnimEnd();
        IKchangeWeight = true;
        timer = 1; AddIK = 1;
    }

    protected override void Death()
    {
        navMesh.isStopped = true;
        NeckRig.weight = 0;
        animator.SetBool("Death", true);
    }

    protected override void TakeDamage(int damage)
    {
        if (move == "patrol") AttackStart(2);
        base.TakeDamage(damage);
    }
}
