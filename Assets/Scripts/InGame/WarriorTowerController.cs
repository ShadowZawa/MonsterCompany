using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorTowerController : MonoBehaviour
{
    public Team team;
    public GameObject SoilderPrefab;
    public Transform spawnPos;
    private int currentHealth; 
    public int maxHealth = 100;
    public float towerRadius = 5;
    public int soilderHealth = 30;
    public int soilderDamage = 10;
    public AudioClip attackAudioClip;
    public int getHealth => currentHealth;
    void Start()
    {
        currentHealth = maxHealth;
        InvokeRepeating("heal", 1f, 1f);
        //Summon 3 soilder from (spawnPos) and walk around
        StartCoroutine("InitSoilder");

    }
    public void setTag(string teamName)
    {
        team = (teamName == "Blue") ? Team.Blue : Team.Red;
        gameObject.tag = teamName;
    }
    IEnumerator InitSoilder()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject soilder = Instantiate(SoilderPrefab, spawnPos.position, Quaternion.identity);
            soilder.GetComponent<SoilderAI>().init(this);
            soilder.AddComponent<AudioSource>();
            soilder.GetComponent<AudioSource>().playOnAwake = false;
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
