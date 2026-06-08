using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class DreamBait : MonoBehaviour
{
    [SerializeField] private DreamBaitProps BaitPrefab;
    [SerializeField] private Animator animator;
    [SerializeField] private bool isTutoActionDone = false;
    [SerializeField] private TutoIndicatorBlink tutoIndicator;

    private bool canExpode;

    private DreamBaitProps currentBaitInstance;

    async Task OnSecondPower(InputValue _input)
    {
        if (!_input.isPressed) return;
        if (currentBaitInstance == null)
        {
            DoBaitSpawn();
            return;
        }

        if (canExpode)
        {
            animator.SetTrigger("usingAbility");
            canExpode = false;
            // probably needs another way to do it but this will do it for now
            await currentBaitInstance.Explode();
            TransformIndicator.Instance.DisplayBaitIcon();
            animator.SetBool("isBombPlanted", false);
            if (!isTutoActionDone)
            {
                if (tutoIndicator == null) return;
                tutoIndicator.StopBlink();
            }
        }
    }

    void DoBaitSpawn()
    {
        animator.SetTrigger("usingAbility");
        currentBaitInstance = Instantiate(BaitPrefab, transform.position, Quaternion.identity);
        TransformIndicator.Instance.DisplayExplodeIcon();
        canExpode = true;
        StartCoroutine(WaitEndAnimation());
        
    }

    private IEnumerator WaitEndAnimation()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        animator.SetBool("isBombPlanted", true);
        yield return null;
    }

    void OnDisable()
    {
        // pop the bait
    }
}
