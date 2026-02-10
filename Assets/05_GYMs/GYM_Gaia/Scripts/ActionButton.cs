using System;
using UnityEngine;

public class ActionButton : MonoBehaviour
{
    [SerializeField] private GameObject Ui;
    [SerializeField] private GameObject buttonMoving;
    [SerializeField] private float downValue;
    [SerializeField]private float speed = 5f;
    private bool canInteract = false;
    public Action OnInteraction;
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

    private void Interaction()
    {
        if(!canInteract)return;
        Ui.SetActive(false);
        canInteract = false;
        var targetPos = transform.position + Vector3.down * downValue;
        MoveButton(targetPos,Vector3.down);
        OnInteraction?.Invoke();
    }
    
    private void MoveButton(Vector3 finalPosition, Vector3 direction)
    {
        while (Vector3.Distance(buttonMoving.transform.position, finalPosition) > 0.1f)
        {
            buttonMoving.transform.Translate(direction * speed * Time.deltaTime);
        }
    }
}

