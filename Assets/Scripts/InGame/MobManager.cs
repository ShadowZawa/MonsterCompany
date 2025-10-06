using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class MobModel
{
    public string mobName;
    public GameObject mobPrefab;
    public int goldCost;
    public int woodCost;
    public int meatCost;

}

[System.Serializable]
public class MobManager : MonoBehaviour
{
    public Team team;
    public bool isAI = false;  // AI模式標記

    public GameObject Boat;
    public MobModel[] mobs;
    public bool isShipping = false;

    void Start()
    {
        InitializeMobsFromCardLoader();
    }

    private void InitializeMobsFromCardLoader()
    {
        if (!CardLoader.instance.hasInit)
        {
            CardLoader.instance.Init();
        }

        var mobDataList = CardLoader.instance.mobData.mobs;
        mobs = new MobModel[mobDataList.Length];

        for (int i = 0; i < mobDataList.Length; i++)
        {
            var mobData = mobDataList[i];
            mobs[i] = new MobModel
            {
                mobName = mobData.name,
                mobPrefab = mobData.prefab,
                woodCost = mobData.woodCost,
                meatCost = mobData.meatCost,
                goldCost = mobData.goldCost
            };
        }
    }

    private List<GameObject> _mobQueue = new List<GameObject>();
    public int mobCount => _mobQueue.Count;
    public bool EnqueueMobByName(string mobName)
    {
        // 找到對應的怪物
        MobModel mob = System.Array.Find(mobs, m => m.mobName == mobName);
        if (mob == null)
        {
            print("找不到怪物：" + mobName);
            return false;
        }

        if (GameManager.instance.getResource(team, ResourceType.Gold) < mob.goldCost
            || GameManager.instance.getResource(team, ResourceType.Wood) < mob.woodCost
            || GameManager.instance.getResource(team, ResourceType.Meat) < mob.meatCost
        )
        {
            print("資源不足，無法召喚怪物：" + mobName);
            return false;
        }

        GameManager.instance.costResource(team, ResourceType.Gold, mob.goldCost);
        GameManager.instance.costResource(team, ResourceType.Wood, mob.woodCost);
        GameManager.instance.costResource(team, ResourceType.Meat, mob.meatCost);

        // 推送貨幣更新事件
        EventBus.Instance.Publish(new CurrencyUpdateEvent());

        return AddQueue(mob);
    }

    public void EnqueueMob(int mobIndex)
    {
        MobModel mob = mobs[mobIndex];
        if (GameManager.instance.getResource(team, ResourceType.Gold) <= mob.goldCost
            || GameManager.instance.getResource(team, ResourceType.Wood) <= mob.woodCost
            || GameManager.instance.getResource(team, ResourceType.Meat) <= mob.meatCost
        )
        {
            if (!isAI) // AI模式下不顯示訊息
            {
                MessageBox.instance.ShowMessage("資源不足，無法召喚！", Color.red);
            }
            return;
        }
        GameManager.instance.costResource(team, ResourceType.Gold, mob.goldCost);
        GameManager.instance.costResource(team, ResourceType.Wood, mob.woodCost);
        GameManager.instance.costResource(team, ResourceType.Meat, mob.meatCost);
        
        // 推送貨幣更新事件
        EventBus.Instance.Publish(new CurrencyUpdateEvent());
        
        AddQueue(mob);
    }
    private bool AddQueue(MobModel mob)
    {
        if (isShipping) return false;
        if (_mobQueue.Count >= 10) return false;
        int ind = _mobQueue.Count;
        if (ind % 2 == 0)
        {
            //row1
            GameObject mobInstance = Instantiate(mob.mobPrefab, Boat.transform);
            mobInstance.transform.localScale = Vector3.one;
            mobInstance.GetComponent<EnemyAI>().team = team;
            mobInstance.GetComponent<EnemyAI>().isBoating = true;
            mobInstance.transform.localPosition = new Vector3(0.4f - (ind / 2) * 0.1f, 0.6f, 0);
            _mobQueue.Add(mobInstance);
        }
        else
        {
            //row2
            GameObject mobInstance = Instantiate(mob.mobPrefab, Boat.transform);
            mobInstance.transform.localScale = Vector3.one;
            mobInstance.GetComponent<EnemyAI>().team = team;
            mobInstance.GetComponent<EnemyAI>().isBoating = true;
            mobInstance.transform.localPosition = new Vector3(0.4f - (ind / 2) * 0.1f, 0.4f, 0);
            _mobQueue.Add(mobInstance);
        }
        return true;
    }
    void OnDestroy()
    {

    }
    public void StartBoat()
    {
        if (isShipping) return;
        if (_mobQueue.Count == 0)
        {
            if (!isAI) // AI模式下不顯示訊息
            {
                MessageBox.instance.ShowMessage("沒有可出航的怪物！", Color.red);
            }
            return;
        }
        isShipping = true;
        LeanTween.move(Boat, team == Team.Red ? GameManager.instance.blueLoc.transform.position : GameManager.instance.redLoc.transform.position, 15f).setOnComplete(() =>
        {
            //處理怪物
            foreach (GameObject mob in _mobQueue)
            {
                mob.transform.parent = null;
                mob.GetComponent<EnemyAI>().isBoating = false;
            }

            _mobQueue.Clear();
            LeanTween.move(Boat, team == Team.Red ? GameManager.instance.redLoc.transform.position : GameManager.instance.blueLoc.transform.position, 15f).setOnComplete(() =>
            {
                isShipping = false;
            });
        });
    }



}