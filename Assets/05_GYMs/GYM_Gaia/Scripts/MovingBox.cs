using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovingBox : MonoBehaviour
{
    public enum Side { Left, Right, Front, Back }
    Side side;
    [SerializeField] private GameObject Ui;
    [SerializeField] private LayerMask layersObstacles;
    private bool canInteract = false;
    [SerializeField]private float speed = 5f;
    private PlayerController player;
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
                Debug.Log(player.name);
            }
        }

        if (other.CompareTag("Wall"))
        {
            if (player != null)
            {
                player.OnWallWithBox = true;
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
            ReleaseBox();
            player.OnCatch -= CatchBox;
            player.OnRelease -= ReleaseBox;
            player = null;
            Ui.SetActive(false);
            canInteract = false;
        }
        if (other.CompareTag("Wall"))
        {
            if (player != null)
            {
                player.OnWallWithBox = false;
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
            player.IsWithBox = true;
            player.CanRotate = false;
            player.currentAttackManager.CanAttack = false;
        }
    }

    private void ReleaseBox()
    {
        if (canInteract)
        {
            Ui.SetActive(true);
            transform.SetParent(null);
            player.IsWithBox = false;
            player.CanRotate = true;
            player.currentAttackManager.CanAttack = true;
        }
    }

    private void ChooseSide()
    {
        player.side = side;
    }
  
}
