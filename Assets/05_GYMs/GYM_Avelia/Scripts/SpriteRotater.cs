using UnityEngine;

public class SpriteRotater : MonoBehaviour
{
    [SerializeField]
    PlayerController controller;

    public delegate void ChangeDir(Direction dir);

    public enum Direction
    {
        NoChange,
        South,
        North,
        East,
        West,
    }

    public event ChangeDir OnChangeDir;

    private Direction currentDir;

    [SerializeField]
    private float northDirectionRight, northDirectionLeft,
                southDirectionRight, southDirectionLeft,
                eastDirectionUp, eastDirectionDown,
                westDirectionUp, westDirectionDown;

    void Start()
    {

    }

    private Direction GetDirection(Vector3 camera, Vector3 player)
    {
        var cameraProjectedDirection = new Vector2(camera.x, camera.z).normalized;
        var playerProjectedDirection = new Vector2(player.x, player.z).normalized;

        var signedAngle = Vector2.SignedAngle(cameraProjectedDirection, playerProjectedDirection);

        if (signedAngle < northDirectionLeft && signedAngle > northDirectionRight) return Direction.North;
        if (signedAngle < westDirectionDown && signedAngle > westDirectionUp) return Direction.West;
        if (signedAngle < eastDirectionUp && signedAngle > eastDirectionDown) return Direction.East;
        if (signedAngle < southDirectionRight && signedAngle > southDirectionLeft) return Direction.South;

        return Direction.NoChange;
    }

    void OnMove()
    {
        var cameraDirection = controller.cameraRotation.forward;
        var playerDirection = transform.forward;

        var newDir = GetDirection(cameraDirection, playerDirection);

        if (newDir == Direction.NoChange || newDir == currentDir) return;

        currentDir = newDir;

        OnChangeDir?.Invoke(newDir);
    }
}
