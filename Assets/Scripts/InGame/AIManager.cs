using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AIAction
{
    public enum ActionType
    {
        BuildTower,
        SpawnMob,
        StartBoat,
    }

    public ActionType type;
    public string unitName;  // 塔或怪物的名稱
    public int goldCost;     // 需要的金
    public int woodCost;     // 需要的木
    public int meatCost;     // 需要的肉
    public int mobCount;     // 需要的怪物數量
    public Vector2 position;  // 建造位置（如果是固定位置）
}

[System.Serializable]
public class AIPhase
{
    public AIAction[] initialActions;  // 開場固定動作
    public AIAction[] randomActions;   // 隨機動作池
    public int resourceThreshold;      // 觸發隨機動作的資源閾值
}

public class AIManager : MonoBehaviour
{
    public Vector2 minPos1;
    public Vector2 minPos2;
    public Team team = Team.Red;  // AI所屬隊伍
    public TowerBuilder towerBuilder;
    public MobManager mobManager;
    private float checkInterval = 1f;  // 檢查資源間隔
    private AIPhase currentPhase;     // 當前階段配置

    private bool initialActionsComplete = false;
    private int currentActionIndex = 0;
    private List<Vector2> availableBuildPositions = new List<Vector2>();

    void Start()
    {
        if (towerBuilder)
        {
            towerBuilder.team = team;
            towerBuilder.isAI = true;
        }
        if (mobManager)
        {
            mobManager.team = team;
            mobManager.isAI = true;
        }

    LoadPhaseConfig(); // 讀取關卡配置
    StartCoroutine(AIRoutine());
    }

    void LoadPhaseConfig()
    {
        // TODO: 從JSON讀取配置
        // 示例配置
        currentPhase = new AIPhase
        {
            initialActions = new AIAction[]
            {
                new AIAction { 
                    type = AIAction.ActionType.BuildTower,
                    unitName = "House",
                    goldCost = 100,
                    position = new Vector2(10, 5)
                }
            },
            randomActions = new AIAction[]
            {
                new AIAction {
                    type = AIAction.ActionType.SpawnMob,
                    unitName = "Paddle_Fish",
                    goldCost = 50
                },
                new AIAction {
                    type = AIAction.ActionType.BuildTower,
                    unitName = "House",
                    goldCost = 150
                },
                new AIAction {
                    type = AIAction.ActionType.BuildTower,
                    unitName = "Warrior_Tower",
                    goldCost = 150
                },
                new AIAction {
                    type = AIAction.ActionType.StartBoat,
                    mobCount = 4
                }
            },
            resourceThreshold = 50
        };
    }

    Vector2 GetRandomBuildPosition(TowerModel tower)
    {
        if (towerBuilder == null || towerBuilder.buildTilemap == null || tower == null)
            return Vector2.zero;

        towerBuilder.setCurrentTower(tower);
        List<Vector2> validPositions = new List<Vector2>();
        var tilemap = towerBuilder.buildTilemap;

        // 只在 minPos1 與 minPos2 的矩形範圍內取 tile
        float minX = Mathf.Min(minPos1.x, minPos2.x);
        float maxX = Mathf.Max(minPos1.x, minPos2.x);
        float minY = Mathf.Min(minPos1.y, minPos2.y);
        float maxY = Mathf.Max(minPos1.y, minPos2.y);

        // 逐格檢查tilemap範圍
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                Vector3 worldPos = tilemap.CellToWorld(cellPos) + tilemap.tileAnchor;
                // 檢查是否在指定範圍內
                if (worldPos.x < minX || worldPos.x > maxX || worldPos.y < minY || worldPos.y > maxY)
                    continue;
                if (tilemap.GetTile(cellPos) == null)
                    continue;
                if (towerBuilder.IsValidBuildPosition(worldPos))
                {
                    validPositions.Add(worldPos);
                }
            }
        }

        // 隨機選取一個可用位置
        if (validPositions.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, validPositions.Count);
            return validPositions[idx];
        }
        else
        {
            Debug.Log("沒有可用的建造位置");
        }
        return Vector2.zero;

    }


    IEnumerator AIRoutine()
    {
        while (true)
        {
            // Debug: 顯示目前階段與資源
            string phase = initialActionsComplete ? "隨機階段" : "初始階段";
            string actionInfo = "";
            if (!initialActionsComplete)
            {
                if (currentActionIndex < currentPhase.initialActions.Length)
                {
                    AIAction action = currentPhase.initialActions[currentActionIndex];
                    actionInfo = $"目標: {action.type} {action.unitName} 需求資源: Gold={action.goldCost}, Wood={action.woodCost}, Meat={action.meatCost}";
                }
                else
                {
                    actionInfo = "初始動作已完成";
                }
            }
            else
            {
                AIAction randomAction = currentPhase.randomActions[UnityEngine.Random.Range(0, currentPhase.randomActions.Length)];
                actionInfo = $"目標: {randomAction.type} {randomAction.unitName} 需求資源: Gold={randomAction.goldCost}, Wood={randomAction.woodCost}, Meat={randomAction.meatCost}";
            }

            int gold = GameManager.instance.getResource(team, ResourceType.Gold);
            int wood = GameManager.instance.getResource(team, ResourceType.Wood);
            int meat = GameManager.instance.getResource(team, ResourceType.Meat);
            //Debug.Log($"[AIManager] 階段: {phase} | {actionInfo} | 資源: Gold={gold}, Wood={wood}, Meat={meat}");

            if (!initialActionsComplete)
            {
                // 執行開場固定動作
                if (currentActionIndex < currentPhase.initialActions.Length)
                {
                    AIAction action = currentPhase.initialActions[currentActionIndex];
                    bool canDo = gold >= action.goldCost
                        && wood >= action.woodCost
                        && meat >= action.meatCost;
                    if (canDo)
                    {
                        ExecuteAction(action);
                        currentActionIndex++;
                    }
                }
                else
                {
                    initialActionsComplete = true;
                }
            }
            else
            {
                AIAction randomAction = currentPhase.randomActions[UnityEngine.Random.Range(0, currentPhase.randomActions.Length)];
                bool canDo = gold >= randomAction.goldCost
                    && wood >= randomAction.woodCost
                    && meat >= randomAction.meatCost;
                if (canDo)
                {
                    bool success = ExecuteAction(randomAction);
                    if (success)
                    {
                        // 執行成功才冷卻
                        yield return new WaitForSeconds(checkInterval);
                        continue;
                    }
                }
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    bool ExecuteAction(AIAction action)
    {
        switch (action.type)
        {
            case AIAction.ActionType.BuildTower:
                if (towerBuilder == null) return false;

                TowerModel tower = towerBuilder.getTowerByName(action.unitName);
                if (tower == null)
                {
                    //Debug.LogError($"AI建塔失敗：找不到塔 {action.unitName}");
                    return false;
                }
                towerBuilder.setCurrentTower(tower); // 確保currentTower同步

                Vector2 buildPos = action.position;
                if (buildPos == Vector2.zero)
                {
                    buildPos = GetRandomBuildPosition(tower);
                }

                if (buildPos != Vector2.zero)
                {
                    // Debug: 印出建塔資訊
                    //Debug.Log($"AI建塔嘗試：{tower.towerName} at {buildPos} team={towerBuilder.team}");
                    bool valid = towerBuilder.IsValidBuildPosition(buildPos);
                    //Debug.Log($"IsValidBuildPosition={valid}");
                    if (valid && towerBuilder.BuildTowerAt(action.unitName, new Vector3(buildPos.x, buildPos.y, 0)))
                    {
                        Debug.Log("建造成功" + buildPos);
                        return true;
                    }
                    //Debug.LogWarning($"建造失敗，條件不符！座標:{buildPos} 塔:{action.unitName} team:{towerBuilder.team}");
                }
                return false;

            case AIAction.ActionType.SpawnMob:
                if (mobManager == null) return false;
                //print("AI召喚怪物" + action.unitName);
                if (mobManager.EnqueueMobByName(action.unitName))
                {
                    //mobManager.StartBoat();
                    return true;
                }
                //print("AI召喚怪物失敗" + action.unitName);
                return false;
            case AIAction.ActionType.StartBoat:
                if (mobManager == null) return false;
                if (action.mobCount > mobManager.mobCount) return false;
                //print("AI開始出航");
                mobManager.StartBoat();
                return true;
        }
        return false;
    }
}
