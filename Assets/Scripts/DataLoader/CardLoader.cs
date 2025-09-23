using System;
using UnityEngine;
[Serializable]
public class mobLevel
{
    public int level;
    public int health;
    public int damage;
    public float speed;
    public float range;
    public float fireRate;
    public int towerHealth;
    public float towerRange;
}
[Serializable]
public class mobLevelData
{
    public string displayName;
    public string description;
    public string name;
    public int woodCost;
    public int meatCost;
    public int goldCost;
    public string mobSpriteName;
    public string blueSpriteName;
    public string redSpriteName;
    public string rarity;
    public GameObject prefab;
    public Sprite previewImage;
    public Sprite previewBlueImage;
    public Sprite previewRedImage;
    public mobLevel[] levels;
}
[Serializable]
public class mobSheetData
{
    public string version;
    public mobLevelData[] mobs;
    public mobLevelData[] towers;

}

public class CardLoader : MonoBehaviour
{
    public static CardLoader instance;
    public bool hasInit = false;




    public mobSheetData mobData;
    public GameObject cardPrefab;
    public GameObject cardPrefab2;
    public GameObject detailUIPrefab;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }
    void Start()
    { 
        Init(); 
    }
    public void Init()
    {
        hasInit = true;
        cardPrefab = Resources.Load<GameObject>("CardPrefab");
        cardPrefab2 = Resources.Load<GameObject>("CardPrefab2");
        detailUIPrefab = Resources.Load<GameObject>("DetailPrefab");
        TextAsset jsonData = Resources.Load<TextAsset>("mobLevelSheet");
        if (jsonData != null)
        {
            mobSheetData sheetData = JsonUtility.FromJson<mobSheetData>(jsonData.text);
            if (sheetData != null)
            {
                CardLoader.instance.mobData = sheetData;
                Debug.Log("Loaded " + sheetData.mobs.Length + " mobs.");
                Debug.Log("Loaded " + sheetData.towers.Length + " towers.");
                foreach (mobLevelData mob in sheetData.mobs)
                {
                    if (!string.IsNullOrEmpty(mob.mobSpriteName))
                    {
                        mob.previewImage = Resources.Load<Sprite>("mobs/" + mob.mobSpriteName);
                        if (mob.previewImage == null)
                        {
                            Debug.LogWarning("Could not find sprite: " + mob.mobSpriteName + " for mob: " + mob.name);
                        }
                        mob.prefab = Resources.Load<GameObject>("mobs/" + mob.name);
                    }
                    else
                    {
                        Debug.LogWarning("mobSpriteName is null or empty for mob: " + mob.name);
                    }
                }
                foreach (mobLevelData tower in sheetData.towers)
                {
                    tower.prefab = Resources.Load<GameObject>("towers/" + tower.name);
                    if (!string.IsNullOrEmpty(tower.blueSpriteName))
                    {
                        tower.previewBlueImage = Resources.Load<Sprite>("towers/" + tower.blueSpriteName);
                        if (tower.previewBlueImage == null)
                        {
                            Debug.LogWarning("Could not find blue sprite: " + tower.blueSpriteName + " for tower: " + tower.name);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("blueSpriteName is null or empty for tower: " + tower.name);
                    }
                    if (!string.IsNullOrEmpty(tower.redSpriteName))
                    {
                        tower.previewRedImage = Resources.Load<Sprite>("towers/" + tower.redSpriteName);
                        if (tower.previewRedImage == null)
                        {
                            Debug.LogWarning("Could not find red sprite: " + tower.redSpriteName + " for tower: " + tower.name);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("redSpriteName is null or empty for tower: " + tower.name);
                    }
                }
                
            }
            else
            {
                Debug.LogError("Failed to parse mob level JSON data.");
            }
        }
        else
        {
            Debug.LogError("Failed to load mobLevelSheet.json from Resources.");
        }
    }
}