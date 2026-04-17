using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class StarBomb : MonoBehaviour
{
    [SerializeField] private GameObject explodeZone;
    [SerializeField] private float timeToExplode;
    
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("explode now");
            Explode();
        }
    }


    public void StartCountdown()
    {
        StartCoroutine(SelfExplodeCountdown());
    }

    private IEnumerator SelfExplodeCountdown()
    {
        float elapsed = 0;
        //Animation placeholder
        float currentBlinkInterval = 1f;
        float lastBlinkTime = 0;

        while (elapsed < timeToExplode)
        {
            elapsed += Time.deltaTime;
            
            if (elapsed - lastBlinkTime >= currentBlinkInterval)
            {
                lastBlinkTime = elapsed;
                meshRenderer.enabled = !meshRenderer.enabled;
                currentBlinkInterval *= 0.75f; 
                
                currentBlinkInterval = Mathf.Max(currentBlinkInterval, 0.05f);
            }

            yield return null;
        }
        Explode(); 
    }

    public async Task Explode()
    {
        explodeZone.SetActive(true);

        await Task.Delay(1000);
        explodeZone.SetActive(false);
        Destroy(gameObject);
    }
}
