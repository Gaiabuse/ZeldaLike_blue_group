using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerTuto: MonoBehaviour
{
    
    private bool tutoActivated = false;
    public Action ActivateTutoStep;
    [SerializeField] private bool isJustDialogue;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("TriggerTuto");
        if(tutoActivated || !other.CompareTag("Player"))return;
        tutoActivated = true;
        ActivateTutoStep?.Invoke();
    }

    private void Start()
    {
        if (isJustDialogue)
        {
            tutoActivated = true;
            ActivateTutoStep?.Invoke(); 
        }
    }
}