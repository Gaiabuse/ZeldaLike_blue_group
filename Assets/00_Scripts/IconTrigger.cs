using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class IconTrigger : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Sprite sprite;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            icon.sprite = sprite;
            icon.enabled = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            icon.enabled = false;
        }
    }
}
