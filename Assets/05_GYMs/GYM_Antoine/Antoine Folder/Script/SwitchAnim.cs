using System;
using UnityEngine;

public class SwitchAnim : MonoBehaviour
{
    [SerializeField] EnnemyBase ennemyBase;

    void SwitchAttack(int anim)
    {
        ennemyBase.AttackStart(anim);
    }
}
