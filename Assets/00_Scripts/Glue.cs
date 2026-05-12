using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Glue : MonoBehaviour
{
    [SerializeField] private GameObject platform;
    [SerializeField] private Animation anim;
    [SerializeField] private Vector3 bakeDimensions;
    
    public void CleanGlue()
    {
        MovePlatform();
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void MovePlatform()
    {
        anim.Play();
        StartCoroutine(WaitForEndOfAnimation());
    }

    IEnumerator WaitForEndOfAnimation()
    {
        yield return new WaitForSeconds(anim.clip.length);
        platform.tag = "Ground";
        platform.layer = LayerMask.NameToLayer("Ground");
        Bounds bakeBounds = new Bounds(transform.position, bakeDimensions);
        NavMeshManager.Instance.Rebake(bakeBounds);
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
        yield return null;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, bakeDimensions);
    }
}
