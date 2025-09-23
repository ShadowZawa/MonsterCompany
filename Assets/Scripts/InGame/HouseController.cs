


using UnityEngine;
public enum FarmerTargetType
{
    meat,
    tree
}
public class HouseController : MonoBehaviour
{
    public Team team;
    public GameObject residentPrefab;
    public Transform spawnPos;
    private int currentHealth;
    public int maxHealth = 80;
    //public float houseRadius = 2.5f;
    public int farmerHealth = 20;
    public int farmerDamage = 5;
    public float farmerCollectSpeed = 1.0f;
    public FarmerTargetType farmerTarget = FarmerTargetType.meat;
    public int farmerMaxStorage = 10;
    public int getHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        InvokeRepeating("heal", 1f, 1f);
        // 生成1個居民
        GameObject resident = Instantiate(residentPrefab, spawnPos.position, Quaternion.identity);
        resident.SendMessage("init", this, SendMessageOptions.DontRequireReceiver);
    }


    public void takeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void heal()
    {
        currentHealth += 1;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
}