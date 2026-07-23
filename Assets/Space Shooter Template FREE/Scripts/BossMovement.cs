using UnityEngine;

/// <summary>
/// Moves a boss enemy around the play area: it picks a direction (left/right/up/down/idle),
/// holds it for a random duration, then picks again, staying clamped within screen bounds.
/// Used instead of FollowThePath (which only ever moves along one fixed spline).
/// </summary>
public class BossMovement : MonoBehaviour
{
    [Tooltip("Movement speed in world units/second")]
    public float speed = 3f;

    [Tooltip("Min/max seconds to hold a chosen direction before picking a new one")]
    public float minDirectionHoldTime = 1f;
    public float maxDirectionHoldTime = 3f;

    [Tooltip("Offset from viewport borders for the boss's roaming area")]
    public float minXOffset = 1.5f, maxXOffset = 1.5f, minYOffset = 1.5f, maxYOffset = 3f;

    float minX, maxX, minY, maxY;
    Camera mainCamera;

    public BossDirection CurrentDirection { get; private set; } = BossDirection.Idle;
    float directionTimer;

    private void Start()
    {
        mainCamera = Camera.main;
        ResizeBounds();
        PickNewDirection();
    }

    void ResizeBounds()
    {
        minX = mainCamera.ViewportToWorldPoint(Vector2.zero).x + minXOffset;
        maxX = mainCamera.ViewportToWorldPoint(Vector2.right).x - maxXOffset;
        minY = mainCamera.ViewportToWorldPoint(Vector2.zero).y + minYOffset;
        maxY = mainCamera.ViewportToWorldPoint(Vector2.up).y - maxYOffset;
    }

    private void Update()
    {
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
            PickNewDirection();

        Vector2 delta = BossMovementPattern.ToVector(CurrentDirection) * speed * Time.deltaTime;
        Vector3 nextPosition = transform.position + (Vector3)delta;
        transform.position = new Vector3(
            Mathf.Clamp(nextPosition.x, minX, maxX),
            Mathf.Clamp(nextPosition.y, minY, maxY),
            nextPosition.z);
    }

    void PickNewDirection()
    {
        CurrentDirection = BossMovementPattern.PickNextDirection(Random.value);
        directionTimer = Random.Range(minDirectionHoldTime, maxDirectionHoldTime);
    }
}
