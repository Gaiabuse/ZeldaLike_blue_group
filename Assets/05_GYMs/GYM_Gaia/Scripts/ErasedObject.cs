using System;
using UnityEngine;

public class ErasedObject : MonoBehaviour
{
    [SerializeField]private GameObject erasedObject;
    [SerializeField]private GameObject createdObject;

    private bool erased = false;

    private void Start()
    {
        erased = false;
        erasedObject.SetActive(erased);
        createdObject.SetActive(!erased);
    }

    public void EraseOrCreate()
    {
        erased = !erased;
        
        erasedObject.SetActive(erased);
        createdObject.SetActive(!erased);
    }
}
