using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public class PunchingBag : Ennemy
{
    protected override void Start()
    {
        HP = 10000;

        hitValueDisplay.text = "";
        hitValueDisplay.transform.localScale = Vector3.zero;
    }

    protected override void FixedUpdate()
    {
        if (move == "sleep")
        {
            timerGeneral -= Time.deltaTime;
            if (timerGeneral <= 0)
            {
                EndSleep();
            }
        }
    }

    public override void TakeDamage(int damage)
    {
        if (dotween != null)
        {
            dotween.Kill();
            if (hitValueDisplay) hitValueDisplay.transform.localScale = Vector3.zero;
        }

        dotween = null;
        if (hitValueDisplay)
        {
            hitValueDisplay.text = damage.ToString();
            ShowHitDisplay();
        }
    }
}
