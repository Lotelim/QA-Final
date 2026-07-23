using UnityEngine;

/// <summary>
/// Adds a damage-absorbing shield to whatever it's attached to (typically an Enemy).
/// Enemy checks for this component and routes incoming damage through it before
/// touching its own health, so existing enemies without a Shield are unaffected.
/// </summary>
public class Shield : MonoBehaviour
{
    [Tooltip("Shield hit points. Absorbs incoming damage before the owner's health is touched.")]
    public int shieldHealth;

    [Tooltip("Optional visual shown while the shield is active; disabled once depleted")]
    public GameObject shieldVFX;

    public bool IsActive => shieldHealth > 0;

    private void Start()
    {
        if (shieldVFX != null)
            shieldVFX.SetActive(IsActive);
    }

    /// <summary>
    /// Absorbs as much of the incoming damage as the shield has points for and
    /// returns whatever overflows through to the owner's own health.
    /// </summary>
    public int AbsorbDamage(int incomingDamage)
    {
        if (incomingDamage <= 0 || shieldHealth <= 0)
            return incomingDamage;

        int absorbed = Mathf.Min(shieldHealth, incomingDamage);
        shieldHealth -= absorbed;

        if (shieldHealth <= 0 && shieldVFX != null)
            shieldVFX.SetActive(false);

        return incomingDamage - absorbed;
    }
}
