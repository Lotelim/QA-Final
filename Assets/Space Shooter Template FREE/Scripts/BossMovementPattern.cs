using UnityEngine;

/// <summary>Directions the boss can pick to move in for a while.</summary>
public enum BossDirection { Idle, Left, Right, Up, Down }

/// <summary>
/// Pure direction-selection logic for BossMovement, pulled out of the MonoBehaviour so it
/// can be unit tested with an injected RNG instead of UnityEngine.Random.
/// </summary>
public static class BossMovementPattern
{
    static readonly BossDirection[] Directions =
    {
        BossDirection.Left, BossDirection.Right, BossDirection.Up, BossDirection.Down, BossDirection.Idle
    };

    /// <summary>Picks the next direction to move in from a uniform [0,1) roll.</summary>
    public static BossDirection PickNextDirection(float roll01)
    {
        int index = Mathf.Clamp(Mathf.FloorToInt(roll01 * Directions.Length), 0, Directions.Length - 1);
        return Directions[index];
    }

    public static Vector2 ToVector(BossDirection direction)
    {
        switch (direction)
        {
            case BossDirection.Left: return Vector2.left;
            case BossDirection.Right: return Vector2.right;
            case BossDirection.Up: return Vector2.up;
            case BossDirection.Down: return Vector2.down;
            default: return Vector2.zero;
        }
    }
}
