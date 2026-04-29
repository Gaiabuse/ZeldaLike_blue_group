using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UI;

public class ErasedObject : MonoBehaviour
{
    [SerializeField]private GameObject erasedObject;
    [SerializeField]private GameObject createdObject;
    [SerializeField]private Image eraseIcon;
    [SerializeField]private Image createIcon;
    [SerializeField]private Image createPointsIcon;
    [SerializeField]private List<Sprite> createPointsSprite;
    [Range(1,3)] public int creationCost;
    private bool _isCreated;
    public bool Erased { get; private set; }

    private void Start()
    {
        Erase();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !_isCreated)
        {
            createPointsIcon.sprite = createPointsSprite[creationCost-1];
            createPointsIcon.enabled = true;
            createIcon.enabled = true;
        }
        else if (other.tag == "Player" && _isCreated)
        {
            eraseIcon.enabled = true;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            createPointsIcon.enabled = false;
            createIcon.enabled = false;
            eraseIcon.enabled = false;

        }
    } 

    public void Erase()
    {
        _isCreated = false;
        createPointsIcon.enabled = false;
        createIcon.enabled = false;
        eraseIcon.enabled = false;
        Erased = true;
        createdObject.SetActive(!Erased);
        erasedObject.SetActive(Erased);
        
    }

    public void Create()
    {
        _isCreated = true;
        createPointsIcon.enabled = false;
        createIcon.enabled = false;
        eraseIcon.enabled = false;
        Erased = false;
        erasedObject.SetActive(Erased);
        createdObject.SetActive(!Erased);
    }
}
