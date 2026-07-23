using UnityEngine;

/// <summary>
/// Marker component identifying an Enemy as a boss (huge health, BossMovement instead of a
/// fixed path). LevelCompletionTracker uses its presence to know a level isn't clear until
/// this specific enemy is also defeated.
/// </summary>
[RequireComponent(typeof(Enemy))]
public class Boss : MonoBehaviour
{
}
