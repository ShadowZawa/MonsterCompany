using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorTowerController : MonoBehaviour
{
    public Team team;
    public GameObject SoilderPrefab;
    public Transform spawnPos;
    private EntityModel _model;
    public EntityModel getModel => _model;
    public AudioClip attackAudioClip;
    void Start()
    {
        //InvokeRepeating("heal", 1f, 1f);
        //Summon 3 soilder from (spawnPos) and walk around
        _model = GetComponent<EntityModel>();
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

        
    

}
