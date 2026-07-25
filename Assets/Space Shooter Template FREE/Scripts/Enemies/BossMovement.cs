using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [Tooltip("Movement speed in world units/second")]
    public float speed = 3f;

    [Tooltip("Min/max seconds to hold a chosen direction before picking a new one")]
    public float minDirectionHoldTime = 1f;
    public float maxDirectionHoldTime = 3f;

    [Tooltip("How quickly the boss eases its velocity toward a newly picked direction, instead of snapping instantly (units/sec of speed gained per second)")]
    public float turnAcceleration = 4f;

    [Tooltip("Offset from viewport borders for the boss's roaming area")]
    public float minXOffset = 1.5f, maxXOffset = 1.5f, minYOffset = 1.5f, maxYOffset = 3f;

    float minX, maxX, minY, maxY;
    Camera mainCamera;

    public BossDirection CurrentDirection { get; private set; } = BossDirection.Idle;
    float directionTimer;
    Vector2 currentVelocity;

    public float CurrentSpeed => currentVelocity.magnitude;

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

        Vector2 targetVelocity = BossMovementPattern.ToVector(CurrentDirection) * speed;
        currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, turnAcceleration * speed * Time.deltaTime);

        Vector3 nextPosition = transform.position + (Vector3)(currentVelocity * Time.deltaTime);
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
