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
    
    [Header("HDR Blink Colors")]
    [ColorUsage(true, true)] [SerializeField] private Color colorA;
    [ColorUsage(true, true)] [SerializeField] private Color colorB;

    private MeshRenderer meshRenderer;
    private MeshRenderer childRenderer;
    private bool isExploding = false;
    private MaterialPropertyBlock _propertyBlock;
    
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor"); 
    
    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        
        // Safely cache the first child's MeshRenderer if it exists
        if (transform.childCount > 0)
        {
            childRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        }
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
        if (other.CompareTag("Player") && !isExploding)
        {
            _ = Explode();
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
        float currentBlinkInterval = 1f;
        float lastBlinkTime = 0;
        bool useColorA = true; // Toggle flag to switch between colors

        while (elapsed < timeToExplode)
        {
            if (isExploding || !this) yield return null;

            elapsed += Time.deltaTime;

            if (elapsed - lastBlinkTime >= currentBlinkInterval)
            {
                lastBlinkTime = elapsed;
                
                Color activeColor = useColorA ? colorA : colorB;
                useColorA = !useColorA;
                
                if (meshRenderer != null)
                {
                    meshRenderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor(EmissionColorID, activeColor);
                    meshRenderer.SetPropertyBlock(_propertyBlock);
                }

                if (childRenderer != null)
                {
                    childRenderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor(EmissionColorID, activeColor);
                    childRenderer.SetPropertyBlock(_propertyBlock);
                }
                
                currentBlinkInterval *= 0.75f;
                currentBlinkInterval = Mathf.Max(currentBlinkInterval, 0.05f);
            }

            yield return null;
        }
        _ = Explode();
    }

    public async Task Explode()
    {
        if (isExploding) return;
        if (targetPreview != null) Destroy(targetPreview);
        isExploding = true;

        if (explodePreview != null) Destroy(explodePreview);
        if (explodeZone != null) explodeZone.SetActive(true);
        GetComponent<MeshRenderer>().enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);

        await Task.Delay(1000);
        
        if (explodeZone != null) explodeZone.SetActive(false);
        if (gameObject != null) Destroy(gameObject);
    }
}