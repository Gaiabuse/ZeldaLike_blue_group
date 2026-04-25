using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class StarBomb : MonoBehaviour
{
    [SerializeField] private GameObject explodeZone;
    [SerializeField] private GameObject targetPreview;
    [SerializeField] private GameObject explodePreview;
    [SerializeField] private float timeToExplode;
    [SerializeField] private int damages;

    private MeshRenderer meshRenderer;
    private bool hasDealDamage = false;
    private bool isExploding = false;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void ShowPreview(Vector3 target, Transform player)
    {
        Vector3 pos = target;
        pos.y -= 1;
        targetPreview.transform.position = pos;
        targetPreview.transform.SetParent(player.parent);
        targetPreview.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !isExploding)
        {
            Explode();
        }
    }

    public void DealDamages(IPlayerDamageable player)
    {
        player.TakeDamage(damages);
    }

    public void StartCountdown()
    {
        if (isExploding || !this) return;
        if (targetPreview != null) Destroy(targetPreview);
        if (explodeZone != null) explodePreview.SetActive(true);
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
            if (isExploding || !this) yield return null;

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
        if (isExploding) return;
        if (targetPreview != null) Destroy(targetPreview);
        isExploding = true;

        Destroy(explodePreview);
        explodeZone.SetActive(true);

        await Task.Delay(1000);
        explodeZone.SetActive(false);
        Destroy(gameObject);
    }
}
