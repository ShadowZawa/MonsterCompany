using UnityEngine;


public class CastleModel : MonoBehaviour
{
    public Team team;
    public int getHealth => currentHealth;
    private int currentHealth;
    public int maxHealth=1000;
    void Start() {
        currentHealth = maxHealth;
    }
    public void takeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);

            // game Over
            GameManager.instance.redTeam.score += (team == Team.Blue) ? 5000 : 0;
            GameManager.instance.blueTeam.score += (team == Team.Blue) ? 0 : 5000;
            GameManager.instance.GameOver();
        }
    }
}