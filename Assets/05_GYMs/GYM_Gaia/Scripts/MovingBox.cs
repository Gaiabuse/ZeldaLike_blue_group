using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovingBox : MonoBehaviour
{
    public enum Side { Left, Right, Front, Back }
    Side side;
    [SerializeField] private GameObject Ui;
    [SerializeField] private Rigidbody rb;
    private bool canInteract = false;
    [SerializeField]private float speed = 5f;
    private PlayerController player;
    private Vector3 startPos;

    private void Awake()
    {
        startPos = transform.position;
    }
    private void Start()
    {
        Ui.SetActive(false);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Ui.SetActive(true);
            canInteract = true;
            if (player == null)
            {
                player = other.GetComponent<PlayerController>();
                player.OnCatch += CatchBox;
                player.OnRelease += ReleaseBox;
            }
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
            if (player != null)
            {
                ReleaseBox();
                
                player.OnCatch -= CatchBox;
                player.OnRelease -= ReleaseBox;
                player = null;
            }

            if (Ui != null)
            {
                Ui.SetActive(false);
                canInteract = false;
            }
          
        }
    }


    private void CatchBox()
    {
        if (canInteract)
        {
            Debug.Log(player.name);
            ChooseSide();
            transform.SetParent(player.transform);
            Ui.SetActive(false);
            player.Boxes = gameObject;
            player.CanRotate = false;
            player.currentAttackManager.CanAttack = false;
        }
    }

    public void OnRespawn()
    {
        rb.angularVelocity = Vector3.zero;
        rb.position = startPos;
        transform.position = startPos;
    }

    private void ReleaseBox()
    {
        Debug.Log("nique ton pere");
        if (canInteract)
        {
            Ui.SetActive(true);
            transform.SetParent(null);
            player.Boxes = null;
            player.CanRotate = true;
            player.currentAttackManager.CanAttack = true;
        }
    }

    private void ChooseSide()
    {
        player.side = side;
    }
  
}
