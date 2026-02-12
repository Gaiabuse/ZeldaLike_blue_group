using System;
using UnityEngine;

public class BrokenObject : MonoBehaviour
{
    [SerializeField] private GameObject intactObject;
    [SerializeField] private GameObject brokenObject;
    
    BoxCollider collider;

    private void Awake()
    {
        intactObject.SetActive(true);
        brokenObject.SetActive(false);
        
        collider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Break();
        }
    }

    private void Break()
    {
        intactObject.SetActive(false);
        brokenObject.SetActive(true);
        collider.enabled = false;
    }
}
