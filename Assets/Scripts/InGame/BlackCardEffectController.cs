using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 黑市卡牌效果控制器 - 負責執行具體的卡牌效果
/// 使用delegate模式，保持單一職責
/// </summary>
public class BlackCardEffectController : MonoBehaviour
{
    [Header("效果設定")]
    public Team playerTeam = Team.Blue;
    
    // 卡牌效果委託定義
    public delegate void CardEffectDelegate(BlackCardModel card);
    private Dictionary<string, CardEffectDelegate> effectMethods;
    
    // 正在生效的效果追蹤
    private List<ActiveBlackCardEffect> activeEffects = new List<ActiveBlackCardEffect>();
    
    void Awake()
    {
    }
    
    /// <summary>
    /// 執行卡牌效果 - 主要入口點
    /// </summary>
    public bool ExecuteEffect(BlackCardModel card)
    {
        if (effectMethods.TryGetValue(card.effectFunction, out CardEffectDelegate method))
        {
            method(card);
            Debug.Log($"[BlackCardEffect] 執行效果: {card.name} - {card.effectFunction}");
            return true;
        }
        
        Debug.LogWarning($"[BlackCardEffect] 未找到效果方法: {card.effectFunction}");
        return false;
    }
    
    // ===== 配合現有 BlackCards.json 的具體效果實現 =====
    
    /// <summary>
    /// ID 1: 黑市補給 - 立即獲得一批隨機資源（木材與肉）
    /// </summary>
    private void GainRandomResources(BlackCardModel card)
    {
        int randomWood = UnityEngine.Random.Range(50, 151); // 50-150 木材
        int randomMeat = UnityEngine.Random.Range(30, 101); // 30-100 肉類

        GameManager.instance.addResource(playerTeam, ResourceType.Wood, randomWood);
        GameManager.instance.addResource(playerTeam, ResourceType.Meat, randomMeat);
        
        EventBus.Instance.Publish(new ResourceUpdateEvent(playerTeam, ResourceType.Wood));
        EventBus.Instance.Publish(new ResourceUpdateEvent(playerTeam, ResourceType.Meat));
        
        MessageBox.instance.ShowMessage($"獲得隨機資源：木材+{randomWood} 肉類+{randomMeat}！", Color.green);
    }
    
    /// <summary>
    /// ID 2: 偷渡軍火 - 所有防禦塔攻擊力+15%，持續2回合
    /// </summary>
    private void IncreaseTowerDamage(BlackCardModel card)
    {
        // 提升己方防禦塔攻擊力
        var entities = FindObjectsByType<EntityModel>(FindObjectsSortMode.None);
        
        List<EntityModel> towerModels = new List<EntityModel>();

        foreach (var entity in entities)
        {
            if (entity.team == playerTeam && entity.entityType == EntityType.Tower)
            {
                towerModels.Add(entity);
            }
        }
        
        
        StartCoroutine(BuffSpecificUnitsCoroutine(towerModels, "damage", 1.15f, card.duration * 60f)); // 2回合 = 120秒
        MessageBox.instance.ShowMessage("己方防禦塔攻擊力提升15%！", Color.cyan);
    }
    
    /// <summary>
    /// ID 3: 非法徵召 - 立即生成一名臨時士兵（限本回合）
    /// </summary>
    private void SpawnTemporaryUnit(BlackCardModel card)
    {
        // 在己方城堡附近生成臨時士兵
        Vector3 spawnPos = (playerTeam == Team.Blue) ? 
            GameManager.instance.blueCastle.transform.position : 
            GameManager.instance.redCastle.transform.position;

        spawnPos += new Vector3(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f), 0);

        // 這裡需要一個臨時士兵的prefab，暫時用訊息代替
        MessageBox.instance.ShowMessage("召喚臨時士兵協助防禦！", Color.blue);
        AddTimedEffect(card);
    }
    
    /// <summary>
    /// ID 4: 暗殺令 - 隨機擊殺一名精英敵人
    /// </summary>
    private void InstantKillEliteEnemy(BlackCardModel card)
    {
        Team enemyTeam = (playerTeam == Team.Blue) ? Team.Red : Team.Blue;
        
        // 尋找敵方單位
    var enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        List<EnemyAI> enemyTargets = new List<EnemyAI>();
        
        foreach (var enemy in enemies)
        {
            if (enemy.team == enemyTeam && enemy.getModel.maxHealth > 50) // 血量高的算精英
            {
                enemyTargets.Add(enemy);
            }
        }
        
        if (enemyTargets.Count > 0)
        {
            var target = enemyTargets[UnityEngine.Random.Range(0, enemyTargets.Count)];
            target.getModel.takeDamage(9999, null); // 直接擊殺
            MessageBox.instance.ShowMessage("暗殺成功！隨機擊殺一名敵人！", Color.red);
        }
        else
        {
            MessageBox.instance.ShowMessage("沒有找到適合的暗殺目標", Color.gray);
        }
    }
    
    /// <summary>
    /// ID 5: 毒霧陷阱 - 所有敵人中毒，每秒造成3點傷害，持續5秒
    /// </summary>
    private void ApplyPoisonToEnemies(BlackCardModel card)
    {
        Team enemyTeam = (playerTeam == Team.Blue) ? Team.Red : Team.Blue;
        StartCoroutine(PoisonDamageCoroutine(enemyTeam, 3, card.duration));
        MessageBox.instance.ShowMessage("毒霧籠罩戰場！敵方中毒！", Color.green);
    }
    
    /// <summary>
    /// ID 6: 黑暗契約 - 犧牲10%生命換取200金幣
    /// </summary>
    private void TradeHealthForGold(BlackCardModel card)
    {
        // 對己方所有單位造成10%血量損失
        var playerUnits = CollectTeamUnits(playerTeam);
        foreach (var unit in playerUnits)
        {
            if (unit != null)
            {
                int damage = Mathf.RoundToInt(unit.maxHealth * 0.1f);
                unit.takeDamage(damage, 999);
            }
        }
        
        // 獲得金幣
        GameManager.instance.addResource(playerTeam, ResourceType.Gold, 200);
        EventBus.Instance.Publish(new ResourceUpdateEvent(playerTeam, ResourceType.Gold));
        
        MessageBox.instance.ShowMessage("黑暗契約生效！犧牲生命力獲得200金幣！", Color.black);
    }
    
    /// <summary>
    /// ID 7: 走私網路 - 開啟黑市特賣，下一次購買價格減半
    /// </summary>
    private void NextCardHalfPrice(BlackCardModel card)
    {
        // 這個效果需要在BlackMarketManager中實作
        // 暫時用標記來實現
        AddTimedEffect(card);
        MessageBox.instance.ShowMessage("走私網路啟動！下次黑市購買半價！", Color.yellow);
    }
    
    /// <summary>
    /// ID 8: 血肉儀式 - 所有單位生命+20%，但攻速-10%，持續3回合
    /// </summary>
    private void BuffHPReduceAS(BlackCardModel card)
    {
        float duration = card.duration * 60f; // 3回合 = 180秒
        ApplyBuffToAllUnits(playerTeam, "health", 0.2f, duration);
        ApplyBuffToAllUnits(playerTeam, "attackSpeed", -0.1f, duration);
        
        MessageBox.instance.ShowMessage("血肉儀式：生命力提升20%，攻速降低10%", Color.darkRed);
    }
    /*
    /// <summary>
    /// ID 9: 禁忌科技 - 立即隨機升級一座防禦塔，但有30%機率爆炸毀壞
    /// </summary>
    private void RandomUpgradeOrDestroyTower(BlackCardModel card)
    {
        // 收集己方塔
        var houses = FindObjectsByType<HouseController>(FindObjectsSortMode.None);
        var archerTowers = FindObjectsByType<ArcherTowerController>(FindObjectsSortMode.None);
        var warriorTowers = FindObjectsByType<WarriorTowerController>(FindObjectsSortMode.None);
        
        List<GameObject> playerTowers = new List<GameObject>();
        
        foreach (var house in houses)
            if (house.team == playerTeam) playerTowers.Add(house.gameObject);
            
        foreach (var tower in archerTowers)
            if (tower.team == playerTeam) playerTowers.Add(tower.gameObject);
            
        foreach (var tower in warriorTowers)
            if (tower.team == playerTeam) playerTowers.Add(tower.gameObject);
        
        if (playerTowers.Count > 0)
        {
            var targetTower = playerTowers[UnityEngine.Random.Range(0, playerTowers.Count)];
            
            if (UnityEngine.Random.Range(0f, 1f) < 0.3f) // 30% 機率爆炸
            {
                Destroy(targetTower);
                MessageBox.instance.ShowMessage("禁忌科技失控！塔被摧毀了！", Color.red);
            }
            else // 70% 機率升級
            {
                var model = targetTower.GetComponent<EntityModel>();
                if (model != null)
                {
                    model.damage = Mathf.RoundToInt(model.damage * 1.5f);
                    model.maxHealth = Mathf.RoundToInt(model.maxHealth * 1.3f);
                }
                MessageBox.instance.ShowMessage("禁忌科技成功！塔得到強化！", Color.purple);
            }
        }
        else
        {
            MessageBox.instance.ShowMessage("沒有可升級的防禦塔", Color.gray);
        }
    }
    */
    /// <summary>
    /// ID 10: 暗影交易 - 從黑市抽取三張隨機卡牌，可選擇其中一張購買
    /// </summary>
    private void DrawBlackMarketChoices(BlackCardModel card)
    {
        // 這個效果會觸發另一個黑市面板，需要特殊處理
        MessageBox.instance.ShowMessage("暗影交易啟動！獲得額外選擇機會！", Color.purple);

        // 可以在這裡觸發BlackMarketManager再次開啟
        var blackMarket = FindFirstObjectByType<BlackMarketManager>();
        if (blackMarket != null)
        {
            StartCoroutine(DelayedBlackMarketOpen(blackMarket, 2f));
        }
    }
    
    // ===== 輔助方法 =====
    
    //加入限時效果
    private void AddTimedEffect(BlackCardModel card)
    {
        var effect = new ActiveBlackCardEffect
        {
            card = card,
            startTime = Time.time,
            duration = card.duration
        };
        activeEffects.Add(effect);
    }
    
    private void ApplyBuffToAllUnits(Team team, string buffType, float multiplier, float duration)
    {
        StartCoroutine(BuffAllUnitsCoroutine(team, buffType, multiplier, duration));
    }
    
    /// <summary>
    /// 對特定單位列表應用Buff
    /// </summary>
    private IEnumerator BuffSpecificUnitsCoroutine(List<EntityModel> units, string effectType, float multiplier, float duration)
    {
        // 應用效果
        foreach (var unit in units)
        {
            if (unit == null) continue;
            ApplyUnitBuff(unit, effectType, multiplier);
        }
        // 等待持續時間
        yield return new WaitForSeconds(duration);
        // 恢復multiplier
        foreach (var unit in units)
        {
            if (unit == null) continue;
            RestoreUnitBuff(unit, effectType, multiplier);
        }
    }
    
    /// <summary>
    /// 毒霧傷害協程
    /// </summary>
    private IEnumerator PoisonDamageCoroutine(Team targetTeam, int damagePerSecond, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            var enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                if (enemy.team == targetTeam)
                {
                    enemy.getModel.takeDamage(damagePerSecond, 999);
                }
            }
            
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }
    }
    
    /// <summary>
    /// 延遲開啟黑市
    /// </summary>
    private IEnumerator DelayedBlackMarketOpen(BlackMarketManager blackMarket, float delay)
    {
        yield return new WaitForSeconds(delay);
        blackMarket.Open();
    }

    private IEnumerator BuffAllUnitsCoroutine(Team team, string effectType, float multiplier, float duration)
    {
        // 簡化版實作 - 收集並應用效果
        List<EntityModel> targetUnits = CollectTeamUnits(team);
        // 應用效果
        foreach (var unit in targetUnits)
        {
            if (unit == null) continue;
            ApplyUnitBuff(unit, effectType, multiplier);
        }
        // 等待持續時間
        yield return new WaitForSeconds(duration);
        // 恢復multiplier
        foreach (var unit in targetUnits)
        {
            if (unit == null) continue;
            RestoreUnitBuff(unit, effectType, multiplier);
        }
    }
    
    
    //未來須優化 不該使用FindObjectsByType (太吃效能)
    private List<EntityModel> CollectTeamUnits(Team team)
    {
        List<EntityModel> units = new List<EntityModel>();
        
        // 收集各類型單位
        foreach (var enemy in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
            if (enemy.team == team) units.Add(enemy.getModel);

        foreach (var farmer in FindObjectsByType<FarmerAI>(FindObjectsSortMode.None))
            if (farmer.team == team) units.Add(farmer.getModel);

        foreach (var soldier in FindObjectsByType<SoilderAI>(FindObjectsSortMode.None))
            if (soldier.team == team) units.Add(soldier.GetComponent<EntityModel>());

        foreach (var archer in FindObjectsByType<ArcherAI>(FindObjectsSortMode.None))
            if (archer.team == team) units.Add(archer.GetComponent<EntityModel>());

        return units;
    }
    
    private void ApplyUnitBuff(EntityModel unit, string effectType, float multiplier)
    {
        switch (effectType.ToLower())
        {
            case "damage":
                unit.damageMultiplier += multiplier;
                break;
            case "speed":
                unit.moveSpeedMultiplier += multiplier;
                break;
            case "health":
                unit.healthMultiplier += multiplier;
                break;
            case "attackSpeed":
                unit.attackIntervalMultiplier += multiplier;
                break;
        }
    }

    private void RestoreUnitBuff(EntityModel unit, string effectType, float multiplier)
    {
        switch (effectType.ToLower())
        {
            case "damage":
                unit.damageMultiplier -= multiplier;
                break;
            case "speed":
                unit.moveSpeedMultiplier -= multiplier;
                break;
            case "health":
                unit.healthMultiplier -= multiplier;
                break;
            case "attackSpeed":
                unit.attackIntervalMultiplier -= multiplier;
                break;
        }
    }
    
    void Update()
    {
        // 清理過期效果
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            if (Time.time - effect.startTime >= effect.duration)
            {
                activeEffects.RemoveAt(i);
            }
        }
    }
}

/// <summary>
/// 正在生效的黑市卡牌效果資料類
/// </summary>
[System.Serializable]
public class ActiveBlackCardEffect
{
    public BlackCardModel card;
    public float startTime;
    public float duration;
}