using UnityEngine;
using UnityEngine.InputSystem;

public class testAutoAim : MonoBehaviour, IHasProjectPoints
{
    [SerializeField] bool MouseMode = true, IsActiveFilter = true;
    Plane plane = new Plane(inNormal: Vector3.down, inPoint: Vector3.zero);
    [SerializeField]
    Camera sceneCamera;

    [SerializeField]
    Transform thingy;
    [SerializeField]
    DirectionFilter filter;

    Vector3 mousePosition;
    Vector2 direction;
    float directionfiltered;


    void Start() { }

    void Update()
    {
        UpdateMouseDirection();
        directionfiltered = filter.FilterStickInputToAngle(direction);

        if (IsActiveFilter)
            transform.eulerAngles = new(0f, directionfiltered, 0f);
        else
            transform.eulerAngles = new(0f, Mathf.Atan2(direction.x, direction.y), 0f);
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
            direction = Vector3.Normalize(new(thingy.position.x, thingy.position.z));
        }
    }

    void OnStickMoved(InputValue _input)
    {
        if (MouseMode) return;
        direction = _input.Get<Vector2>();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Ray raydir = new Ray(origin: Vector3.up * 2, direction: new(direction.x, 0f, direction.y));

        Gizmos.DrawRay(raydir);

        Gizmos.color = Color.greenYellow;
        Ray rayfiltdir = new Ray(origin: Vector3.up * 2, direction: transform.forward);

        Gizmos.DrawRay(rayfiltdir);
    }
}
