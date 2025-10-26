using UnityEngine;


public enum EntityType
{
    Enemy,
    Tower,
    Soilder,

}
public class EntityModel : MonoBehaviour
{
    // -------- Base Stats --------
    public Team team;
    public EntityType entityType;
    public int maxHealth = 100;
    private int currentHealth;
    public int defense = 10;
    public float damage = 10;
    public float attackRange = 1f;
    public float moveSpeed = 2f;
    public float patrolRadius = 3f;
    public float patrolInterval = 2f;
    public float attackInterval = 1.2f;

    // -------- Current Stats --------

    public int getHealth => currentHealth;

    // -------- Multiplier Stats --------
    public float damageMultiplier = 1.0f;
    public float healthMultiplier = 1.0f;
    public float defenseMultiplier = 1.0f;
    public float moveSpeedMultiplier = 1.0f;
    public float attackRangeMultiplier = 1.0f;
    public float attackIntervalMultiplier = 1.0f;


    void Start()
    {
        currentHealth = maxHealth;
    }

    public void takeDamage(int damage, int? piercing)
    {
        if (damage < 0)
        {
            Debug.LogWarning("Damage cannot be negative");
            return;
        }
        float defRemain = 1.0f;
        if (piercing.HasValue)
        {
            defRemain = Mathf.Max(0, piercing.Value - defense);
            if (defRemain != 0)
            {
                defRemain = 1 - (defRemain / defense);
            }
        }
        currentHealth = Mathf.Max(0, currentHealth - (int)(damage * defRemain));
        if (currentHealth <= 0)
        {
            death();
        }
    }

    void death()
    {
        // 處理實體死亡邏輯
        Destroy(gameObject);
    }
}