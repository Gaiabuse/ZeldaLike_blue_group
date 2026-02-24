using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AcidicGlav : Ennemy
{
    [Header("Acidic Glav")]

    [SerializeField] int Sharpness;

    [SerializeField] ChainIKConstraint NeckRig;
    [SerializeField] List<GameObject> CrystalTail;

    bool IKchangeWeight; float timer = 0;
    int AddIK = 0;

    protected override void Start()
    {
        base.Start();
        SetTailSharpness(Sharpness);
    }

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

    protected void SetTailSharpness(int sharpness)
    {
        Sharpness = sharpness;

        if (Sharpness > CrystalTail.Count + 1)
        {
            Sharpness = CrystalTail.Count + 1;
        }

        for (int i = 0; i < CrystalTail.Count; i++)
        {
            if (i >= Sharpness) CrystalTail[i].SetActive(false);
            else CrystalTail[i].SetActive(true);
        }
    }

    protected override void AttackPatern()
    {
        float DistanceP = Vector3.Distance(AttackTrigger.position, Player.position);

        if (Sharpness >= CrystalTail.Count + 1)
        {
            if (DistanceP <= 2.5f)
            {
                AttackStart(3);
            }
        }
        else
        {
            if (DistanceP <= 2.5f)
            {
                AttackStart(1);
            }
            else if (DistanceP <= 3f)
            {
                AttackStart(4);

                Sharpness += 1;
                SetTailSharpness(Sharpness);
            }
        }
    }

    protected void RushForward(int move)
    {
        if (move == 1)
        {
            navMesh.isStopped = false;

            navMesh.speed = speed.y * 5;
            navMesh.acceleration = acceleration.y * 10;
            navMesh.angularSpeed = 5;
        }
        else
        {
            navMesh.isStopped = true;

            navMesh.speed = speed.y;
            navMesh.acceleration = acceleration.y;
            navMesh.angularSpeed = SpeedRotate.y;
        }
    }
}
