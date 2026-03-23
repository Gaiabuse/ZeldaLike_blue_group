using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class KnockBackFeedback : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float knockBackForce = 5f, delay = 0.15f;
    

    private Coroutine resetCoroutine;
    
    public void PlayKnockBack(GameObject sender)
    {
        StopAllCoroutines();
        Vector2 direction = (transform.position - sender.transform.position).normalized;
        rb.AddForce(direction * knockBackForce, ForceMode.Impulse);
        resetCoroutine = StartCoroutine(Reset());
    }

    public void PlayKnockBack(Transform sender, float strength)
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }
        Vector3 direction = (transform.position - sender.position);
        
        direction.y = 0;
        
        direction = direction.normalized;
        
        rb.linearVelocity = Vector3.zero;

        rb.AddForce(direction * strength, ForceMode.Impulse);
        resetCoroutine = StartCoroutine(Reset());
    }
    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(delay);
        rb.linearVelocity = Vector3.zero;
    }
}
