using UnityEngine;

public class DirectionSampler : MonoBehaviour
{
    [SerializeField]
    GameObject ToClone;
    [SerializeField]
    int AmountofPoints = 60;
    [SerializeField]
    float height = 1f, radius = 2f;

    [SerializeField]
    DirectionFilter filter;

    async void Start()
    {
        for (float i = 0; i < AmountofPoints; i++)
        {
            var box = Instantiate(ToClone);
            var angle = i / AmountofPoints * 2f * Mathf.PI;
            box.transform.position = GetPosition(angle);
            var correctedAngle = filter.FilterStickInputToAngle(FromAngle(angle)) + 180;
            box.GetComponent<Renderer>().material.color = Color.HSVToRGB(correctedAngle / 360f, 1f, 1f);
        }
    }

    Vector3 GetPosition(float radiants)
        => new Vector3(Mathf.Cos(radiants), 0f, Mathf.Sin(radiants)) * radius + Vector3.up * height;

    Vector2 FromAngle(float radiants)
        => new Vector2(Mathf.Cos(radiants), Mathf.Sin(radiants));
}
