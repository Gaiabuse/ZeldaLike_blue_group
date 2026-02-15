using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class NightmareAttack : SimpleAttack
{
   [SerializeField]private float timeBetweenAttacks = 1f;
   private Coroutine cooldownCoroutine;
   private bool canAttack;

   private void OnEnable()
   {
      canAttack = true;
   }

   public override IEnumerator OnAttackEnumerator()
   {
      if (!canAttack) return null;
      canAttack = false;
      cooldownCoroutine = StartCoroutine(AttackCoolDownEnumerator());
      return base.OnAttackEnumerator();
   }

   private IEnumerator AttackCoolDownEnumerator()
   {
      yield return new WaitForSeconds(timeBetweenAttacks);
      canAttack = true;
      cooldownCoroutine = null; 
   }
}
