using UnityEngine;
using System.Collections;

public class ManaDrainer : MonoBehaviour
{
    [SerializeField]
    float ManaToGain = 0, numberOfSecondBetweenTicks = 1;

    void OnEnable()
    {
        _ = StartCoroutine(DrainMana());
    }

    IEnumerator DrainMana()
    {
        while (true)
        {
            //print($"lost {ManaToGain} mana TO IMPLEMENT");
            yield return new WaitForSeconds(1f);
        }
    }
}
