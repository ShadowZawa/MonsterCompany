using UnityEngine;


public enum EntityType
{
    Enemy,
    Tower,
    Soilder,

}
public class EntityModel : MonoBehaviour
{
    public EntityType entityType;
    public int maxHealth = 100;
    private int currentHealth;
    public int getHealth => currentHealth;
    public int damage = 10;
    public float attackRange = 1f;
    public float moveSpeed = 2f;
    public float patrolRadius = 3f;
        public float patrolInterval = 2f;
    public float attackInterval = 1.2f;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void takeDamage(int damage)
    {
        if (damage < 0)
        {
            Debug.LogWarning("Damage cannot be negative");
            return;
        }
    
        currentHealth = Mathf.Max(0, currentHealth - damage);
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