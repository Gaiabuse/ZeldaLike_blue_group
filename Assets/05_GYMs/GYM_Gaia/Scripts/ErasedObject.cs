using System;
using UnityEngine;

public class ErasedObject : MonoBehaviour
{
    [SerializeField]private GameObject erasedObject;
    [SerializeField]private GameObject createdObject;

    public bool Erased { get; private set; }

    private void Start()
    {
        Erase();
    }

    public void Erase()
    {
        Erased = true;
        
        erasedObject.SetActive(Erased);
        createdObject.SetActive(!Erased);
    }

    public void Create()
    {
        Erased = false;
        erasedObject.SetActive(Erased);
        createdObject.SetActive(!Erased);
    }
}
