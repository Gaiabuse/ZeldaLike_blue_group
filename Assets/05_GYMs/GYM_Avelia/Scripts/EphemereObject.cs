using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class EphemereObject : MonoBehaviour
{
    [SerializeField]
    float lifeTimeSeconds;


    private void Start()
    {
        StartCoroutine(DestroyCoroutine());
    }

    IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(lifeTimeSeconds);
        Destroy(gameObject);
    }

}
