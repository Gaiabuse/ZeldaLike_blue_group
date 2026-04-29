using UnityEngine;
using UnityEngine.InputSystem;

public class testAutoAim : MonoBehaviour, IHasProjectPoints
{
    [SerializeField] bool MouseMode = true;
    Plane plane = new Plane(inNormal: Vector3.down, inPoint: Vector3.zero);
    [SerializeField]
    Camera sceneCamera;

    [SerializeField]
    Transform thingy;
    [SerializeField]
    DirectionFilter filter;

    Vector3 mousePosition;
    Vector2 direction;


    void Start() { }

    void Update()
    {
        UpdateMouseDirection();
        var directionfiltered = filter.FilterStickInputToAngle(direction);

        transform.eulerAngles = new(0f, -directionfiltered, 0f);
    }

    Vector3 IHasProjectPoints.ProjectPoint(Vector2 dir)
    {
        Vector3 camRight = sceneCamera.transform.right;
        Vector3 camForward = sceneCamera.transform.forward;
        Vector3 moveDirRight = Vector3.ProjectOnPlane(camRight, transform.up).normalized;
        Vector3 moveDirForward = Vector3.ProjectOnPlane(camForward, transform.up).normalized;
        return (moveDirForward * dir.y) + (moveDirRight * dir.x);
    }

    void UpdateMouseDirection()
    {
        if (!MouseMode) return;
        var raycam = sceneCamera.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(raycam, out float len))
        {
            thingy.position = raycam.GetPoint(len);
            direction = new(thingy.position.x, thingy.position.z);
        }
    }

    void OnStickMoved(InputValue _input)
    {
        if (MouseMode) return;
        direction = _input.Get<Vector2>();
    }
}
