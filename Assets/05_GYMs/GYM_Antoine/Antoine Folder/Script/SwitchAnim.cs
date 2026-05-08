using System;
using UnityEngine;

public class SwitchAnim : MonoBehaviour
{
    [SerializeField] EnnemyBase ennemyBase;
    SheepEnnemySprite sheep;
    BookEnnemy book;

    private void Start()
    {
        if (ennemyBase.gameObject.GetComponent<SheepEnnemySprite>() != null) sheep = ennemyBase.gameObject.GetComponent<SheepEnnemySprite>();
        if (ennemyBase.gameObject.GetComponent<BookEnnemy>() != null) book = ennemyBase.gameObject.GetComponent<BookEnnemy>();
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

    void ToogleHitBox(int toogle)
    {
        ennemyBase.ToogleMainAttack(toogle);
    }

    void RecoverStunBook()
    {
        book.RecoverStun();
    }
}
