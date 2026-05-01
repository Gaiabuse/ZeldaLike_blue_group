using System;
using UnityEngine;

public class SwitchAnim : MonoBehaviour
{
    [SerializeField] EnnemyBase ennemyBase;
    [SerializeField] SheepEnnemySprite sheep;

    void SwitchAttack(int anim)
    {
        ennemyBase.AttackStart(anim);
    }

    void SheepSwitchShell(int anim)
    {
        sheep.SetShell(anim);
    }

    void DestroyEnnemy()
    {
        Destroy(ennemyBase.gameObject);
    }
}
