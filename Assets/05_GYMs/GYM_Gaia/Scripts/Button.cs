using System;
using DG.Tweening;
using UnityEngine;

public abstract class Button : MonoBehaviour
{
    protected bool canInteract = false;
    public Action OnInteraction;
    [SerializeField] protected GameObject buttonMoving;
    [Tooltip("direction is like [0,0,1] or [0,-1,0] etc...")]
    [SerializeField] protected bool animation;
    [SerializeField] protected Vector3 finalPosition;
    [SerializeField]protected float speed = 5f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    protected virtual void Interaction()
    {
        if(!canInteract)return;
        canInteract = false;
        OnInteraction?.Invoke();
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
