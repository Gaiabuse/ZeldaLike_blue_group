using System;
using Unity.AI.Navigation;
using UnityEngine;

public class ErasedObject : MonoBehaviour
{
    [SerializeField]private GameObject erasedObject;
    [SerializeField]private GameObject createdObject;
    [Range(1,3)] public int creationCost;
    public bool Erased { get; private set; }

    private void Start()
    {
        Erase();
    }

    public void Erase()
    {
        Erased = true;
        createdObject.SetActive(!Erased);
        erasedObject.SetActive(Erased);
        
    }

    public void Create()
    {
        Erased = false;
        erasedObject.SetActive(Erased);
        createdObject.SetActive(!Erased);
    }
}
