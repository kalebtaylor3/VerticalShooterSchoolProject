/// <summary>
/// Component to place on a Game Object to allow it to do damage.
/// Add damage types as needed. Game Object that receives damage 
/// must accept the damage type for damage to occur.
/// </summary>

using UnityEngine;

public enum DamageType
{
    Unassigned = -1,
    PlayerBody,
    PlayerBullet,
    EnemyBody,
    EnemyBullet,
    BOMB
}

public class Damage : MonoBehaviour
{
    [SerializeField]
    private DamageType damageType = DamageType.Unassigned;
    public DamageType Type
    {
        get { return damageType; }
    }

    [SerializeField]
    [Tooltip("Damage Value to be dealt; Use -1 to force death.")]
    private int damageAmount = 10;
    public int DamageAmount
    {
        get { return damageAmount; }
    }

    [SerializeField]
    [Tooltip("Set to true if you want the damager to be destroyed unpon death. (Typically set to true for bullets)")]
    private bool destroyAfterDamage = false;
    public bool DestroyAfterDamage
    {
        get { return destroyAfterDamage; }
    }
}
