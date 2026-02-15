using System;
using UnityEngine;

public class ActionButton : Button
{
    [SerializeField] private GameObject Ui;
    
    
    
    
    private void Start()
    {
        Ui.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerController.OnInteract += Interaction;
    }

    private void OnDisable()
    {
        PlayerController.OnInteract -= Interaction;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Ui.SetActive(true);
            canInteract = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Ui.SetActive(false);
            canInteract = false;
        }
    }

    protected override void Interaction()
    { 
        base.Interaction();
        Ui.SetActive(false);
    }
    
    
}

