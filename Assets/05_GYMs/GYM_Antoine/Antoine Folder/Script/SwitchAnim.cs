using System;
using UnityEngine;

public class SwitchAnim : MonoBehaviour
{
    [SerializeField] EnnemyBase ennemyBase;
    SheepEnnemySprite sheep;

    private void Start()
    {
        sheep = ennemyBase.GetComponent<SheepEnnemySprite>();
    }

    void SwitchAttack(int anim)
    {
        ennemyBase.AttackStart(anim);
    }

    void SheepSwitchShell(int anim)
    {
        if (sheep != null) sheep.SetShell(anim);
    }

    void DestroyEnnemy()
    {
        Destroy(ennemyBase.gameObject);
    }

    void AttackAnimEnd()
    {
        ennemyBase.AttackAnimEnd();
    }
}
