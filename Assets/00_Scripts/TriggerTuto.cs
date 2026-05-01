using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerTuto: MonoBehaviour
{
    
    private bool tutoActivated = false;
    public Action ActivateTutoStep;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("TriggerTuto");
        if(tutoActivated || !other.CompareTag("Player"))return;
        tutoActivated = true;
        ActivateTutoStep?.Invoke();
    }
}