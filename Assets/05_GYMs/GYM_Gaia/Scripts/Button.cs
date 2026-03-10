using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public abstract class Button : MonoBehaviour
{
    protected bool canInteract = false;
    [SerializeField] protected GameObject buttonMoving;
    [Tooltip("direction is like [0,0,1] or [0,-1,0] etc...")]
    [SerializeField] protected bool animation;
    [SerializeField] protected Vector3 finalPosition;
    [SerializeField]protected float speed = 5f;
    
    [SerializeField]protected UnityEvent onInteract;
    
    protected virtual void Interaction()
    {
        if(!canInteract)return;
        canInteract = false;
        onInteract.Invoke();
        if (animation)
        {
            MoveButton();
        }
  
    }

    private void MoveButton()
    {
   
        buttonMoving.transform.DOLocalMove(finalPosition, speed);
        
    }
}
