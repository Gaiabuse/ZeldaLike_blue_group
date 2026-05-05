using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Glue : MonoBehaviour
{
    [SerializeField] private GameObject platform;
    [SerializeField] private Animation anim;
    
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
        Destroy(gameObject);
        yield return null;
    }
}
