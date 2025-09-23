using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AIAction
{
    public enum ActionType
    {
        BuildTower,
        SpawnMob
    }

    public ActionType type;
    public string unitName;  // 塔或怪物的名稱
    public int requiredResource;  // 需要的資源數量
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

        StartCoroutine(AIRoutine());
        LoadPhaseConfig(); // 讀取關卡配置
        UpdateBuildPositions(); // 更新可建造位置
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
                    unitName = "Basic_Tower",
                    requiredResource = 100,
                    position = new Vector2(10, 5)
                }
            },
            randomActions = new AIAction[]
            {
                new AIAction {
                    type = AIAction.ActionType.SpawnMob,
                    unitName = "Paddle_Shark",
                    requiredResource = 50
                },
                new AIAction {
                    type = AIAction.ActionType.BuildTower,
                    unitName = "Basic_Tower",
                    requiredResource = 150
                }
            },
            resourceThreshold = 50
        };
    }

    void UpdateBuildPositions()
    {
        if (towerBuilder == null || towerBuilder.buildTilemap == null || towerBuilder.towers == null) return;

        // 清除現有位置列表
        availableBuildPositions.Clear();

        // 從 TowerBuilder 的 buildTilemap 獲取可建造的位置
        var tilemap = towerBuilder.buildTilemap;
        BoundsInt bounds = tilemap.cellBounds;

        // 計算最大的塔尺寸，用於檢查建造空間
        int maxTowerWidth = 1;
        int maxTowerHeight = 1;
        foreach (var tower in towerBuilder.towers)
        {
            maxTowerWidth = Mathf.Max(maxTowerWidth, tower.towerWidth);
            maxTowerHeight = Mathf.Max(maxTowerHeight, tower.towerHeight);
        }

        for (int x = bounds.min.x; x < bounds.max.x - maxTowerWidth + 1; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y - maxTowerHeight + 1; y++)
            {
                // 檢查這個位置是否有足夠的空間放置最大的塔
                bool isValidPosition = true;
                for (int dx = 0; dx < maxTowerWidth && isValidPosition; dx++)
                {
                    for (int dy = 0; dy < maxTowerHeight && isValidPosition; dy++)
                    {
                        Vector3Int checkPosition = new Vector3Int(x + dx, y + dy, 0);
                        if (!tilemap.HasTile(checkPosition))
                        {
                            isValidPosition = false;
                        }
                    }
                }

                if (isValidPosition)
                {
                    Vector2 worldPosition = tilemap.CellToWorld(new Vector3Int(x, y, 0));
                    // 檢查該位置是否在建造區域限制內
                    if (worldPosition.x >= towerBuilder.minX && worldPosition.x <= towerBuilder.maxX)
                    {
                        // 檢查是否已經有塔在這個位置
                        Collider2D[] colliders = Physics2D.OverlapBoxAll(worldPosition, 
                            new Vector2(maxTowerWidth * towerBuilder.gridSize, maxTowerHeight * towerBuilder.gridSize), 
                            0f, 
                            towerBuilder.buildBlockLayer);

                        if (colliders.Length == 0)
                        {
                            availableBuildPositions.Add(worldPosition);
                        }
                    }
                }
            }
        }
    }

    Vector2 GetRandomBuildPosition()
    {
        if (availableBuildPositions.Count == 0)
            return Vector2.zero;
        
        int index = UnityEngine.Random.Range(0, availableBuildPositions.Count);
        return availableBuildPositions[index];
    }

    IEnumerator AIRoutine()
    {
        while (true)
        {
            if (!initialActionsComplete)
            {
                // 執行開場固定動作
                if (currentActionIndex < currentPhase.initialActions.Length)
                {
                    AIAction action = currentPhase.initialActions[currentActionIndex];
                    if (GameManager.instance.getResource(team, ResourceType.Gold) >= action.requiredResource)
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
                // 執行隨機動作
                if (GameManager.instance.getResource(team, ResourceType.Gold) >= currentPhase.resourceThreshold)
                {
                    AIAction randomAction = currentPhase.randomActions[UnityEngine.Random.Range(0, currentPhase.randomActions.Length)];
                    if (GameManager.instance.getResource(team, ResourceType.Gold) >= randomAction.requiredResource)
                    {
                        ExecuteAction(randomAction);
                    }
                }
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    void ExecuteAction(AIAction action)
    {
        switch (action.type)
        {
            case AIAction.ActionType.BuildTower:
                if (towerBuilder == null) break;

                Vector2 buildPos = action.position;
                if (buildPos == Vector2.zero)
                {
                    buildPos = GetRandomBuildPosition();
                }

                if (buildPos != Vector2.zero)
                {
                    if (towerBuilder.BuildTowerAt(action.unitName, buildPos))
                    {
                        // 建造成功後，從可用位置列表中移除
                        availableBuildPositions.Remove(buildPos);
                    }
                }
                break;

            case AIAction.ActionType.SpawnMob:
                if (mobManager == null) break;

                if (mobManager.EnqueueMobByName(action.unitName))
                {
                    // 如果怪物成功加入隊列，自動開始出航
                    mobManager.StartBoat();
                }
                break;
        }
    }
}
