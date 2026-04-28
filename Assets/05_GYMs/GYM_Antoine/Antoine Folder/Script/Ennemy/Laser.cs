using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] Vector3 expandSize = Vector3.one;
    [SerializeField] float expandTime = 1f;
    [SerializeField] Vector3 reduceSize = -Vector3.one;

    int phase = 1;
    float timer = 0;

    private void FixedUpdate()
    {
        if (phase == 1)
        {
            transform.localScale += expandSize * Time.deltaTime;
            transform.Translate(0, 0, (expandSize.z / 2) * Time.deltaTime);
            timer += Time.deltaTime;
            if (timer > expandTime)
            {
                phase = 2;
            }
        }
        if (phase == 2)
        {
            transform.localScale += reduceSize * Time.deltaTime;
            transform.Translate(0, 0, (reduceSize.z / 2) * Time.deltaTime);

            if (transform.localScale.x <= 0 || transform.localScale.y <= 0 || transform.localScale.z <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
