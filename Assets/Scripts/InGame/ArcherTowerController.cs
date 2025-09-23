using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherTowerController : MonoBehaviour
{
    public Team team;
    public GameObject SoilderPrefab;
    public Transform spawnPos;
    private int currentHealth;
    public int maxHealth = 100;
    public float towerRadius = 3;
    public int soilderHealth = 30;
    public int soilderDamage = 10;
    public float soilderAttackRange = 5;
    public int getHealth => currentHealth;
    void Start()
    {
        currentHealth = maxHealth;
        InvokeRepeating("heal", 1f, 1f);
        //Summon 3 soilder from (spawnPos) and walk around
        StartCoroutine("InitSoilder");

    }
    IEnumerator InitSoilder()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject soilder = Instantiate(SoilderPrefab, spawnPos.position, Quaternion.identity);
            soilder.GetComponent<ArcherAI>().init(this);
            yield return new WaitForSeconds(0.5f);
        }
    }



    public void takeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            //Destroy the tower
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
