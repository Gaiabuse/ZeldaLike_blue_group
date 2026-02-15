using UnityEngine;

public class ShootableButton : Button
{
    private void Start()
    {
        canInteract = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Attack"))
        {
            Interaction();
            Destroy(other.gameObject);
        }
    }
    
}
