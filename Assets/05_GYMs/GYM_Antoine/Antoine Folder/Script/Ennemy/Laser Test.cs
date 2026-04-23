using UnityEngine;

public class LaserTest : MonoBehaviour
{
    [SerializeField] float laserIncreaseDuration = 1;
    [SerializeField] Vector3 increaseSize = new Vector3(0.5f, 0.5f, 3);
    [SerializeField] Vector3 decreaseSize = new Vector3(-0.25f, -0.25f, 0.5f);

    bool canHurt = true;
    float timer = 0;
    int currentPhase = 1;

    PlayerHP playerHp;

    private void Start()
    {
        transform.localScale = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (currentPhase == 1)
        {
            transform.localScale += increaseSize * Time.deltaTime;
            transform.Translate(0, 0, (increaseSize.z / 2) * Time.deltaTime);
            timer += Time.deltaTime;
            if (timer >= laserIncreaseDuration)
            {
                currentPhase = 2;
            }
        }
        if (currentPhase == 2)
        {
            transform.localScale += decreaseSize * Time.deltaTime;
            transform.Translate(0, 0, (decreaseSize.z / 2) * Time.deltaTime);

            if (transform.localScale.x <= 0 || transform.localScale.y <= 0 || transform.localScale.z <= 0) Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerHp == null) playerHp = other.transform.GetComponent<PlayerHP>();
            playerHp.TakeDamage(1);
        }
    }
}
