using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovingBox : MonoBehaviour
{
    enum Side { Left, Right, Front, Back }
    Side side;
    [SerializeField] private GameObject Ui;
    [SerializeField] private LayerMask layersObstacles;
    private bool canInteract = false;
    [SerializeField]private float speed = 5f;
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

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 hitDirection = other.transform.position - transform.position;
            
            float dotForward = Vector3.Dot(hitDirection, transform.forward);
            float dotRight = Vector3.Dot(hitDirection, transform.right);
            
            if (Mathf.Abs(dotForward) > Mathf.Abs(dotRight))
            {
                side = dotForward > 0 ?  Side.Front : Side.Back;
            }
            else
            {
                side = dotRight > 0 ?Side.Right : Side.Left;
            }
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
        
        if (canInteract)
        {
            ChooseSide();
        }
    }
    private void ChooseSide()
    {
        Vector3 targetPosition = Vector3.zero;
        Vector3 direction = Vector3.zero;
        switch (side)
        {
            case Side.Left:
                targetPosition = transform.position - Vector3.left;
                direction = -Vector3.left;
                break;
            case Side.Right:
                targetPosition = transform.position -Vector3.right;
                direction = -Vector3.right;
                break;
            case Side.Front:
                targetPosition = transform.position - Vector3.forward;
                direction = -Vector3.forward;
                break;
            case Side.Back:
                targetPosition = transform.position - Vector3.back;
                direction = -Vector3.back;
                break;
        }
        Move(targetPosition,direction);
    }
    private void Move(Vector3 finalPosition, Vector3 direction)
    {
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, direction, out hit,1.5f,layersObstacles))
        { 
            while (Vector3.Distance(transform.position, finalPosition) > 0.1f)
            {
                transform.parent.transform.Translate(direction * speed * Time.deltaTime);
            }
        }
    }
}
