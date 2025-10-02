using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VisualScripting;
/*
    幫我重建TowerBuilder放置Tower的判定 只能放在Tile[name=Buildable]上
    並且每個Tower上面都會有collider了
*/
public class TowerModel
{
    public GameObject towerPrefab;
    public GameObject ghostPrefab;
    public int woodCost = 0;
    public int goldCost = 0;
    public int meatCost = 0;
    public int towerWidth = 3;
    public int towerHeight = 2;
}
public class TowerBuilder : MonoBehaviour
{
    [Header("Build Area Limit")]
    public float minX = 0f;
    public float maxX = 50f;
    public UnityEngine.Tilemaps.Tilemap buildTilemap; // 需要在Inspector指定

    public bool isAI = false;       // AI模式標記
    public Team team = Team.Blue;   
    [Header("Grid Settings")]
    public float gridSize = 1f;          // 格子大小
    public Color validColor = new Color(0, 1, 0, 0.3f);    // 有效位置顏色
    public Color invalidColor = new Color(1, 0, 0, 0.3f);  // 無效位置顏色
    public GameObject gridPrefab;         // 用於顯示格線的預製體
    public LayerMask buildBlockLayer;     // 用於檢查建造區域是否有障礙物

    [Header("Tower Settings")]
    public GameObject ghostPrefab;
    public TowerModel[] towers;          // 塔的預製體數組

    private void InitializeTowersFromCardLoader()
    {
        if (!CardLoader.instance.hasInit)
        {
            CardLoader.instance.Init();
        }

        var towerDataList = CardLoader.instance.mobData.towers;
        towers = new TowerModel[towerDataList.Length];

        for (int i = 0; i < towerDataList.Length; i++)
        {
            var towerData = towerDataList[i];
            towers[i] = new TowerModel
            {
                towerPrefab = towerData.prefab,
                ghostPrefab = CreateGhostPrefab(towerData, team),
                woodCost = towerData.woodCost,
                meatCost = towerData.meatCost,
                goldCost = towerData.goldCost,
                towerWidth = 3,  // 可以根據需要調整
                towerHeight = 2  // 可以根據需要調整
            };
        }
    }

    private GameObject CreateGhostPrefab(mobLevelData towerData, Team team)
    {
        GameObject p = ghostPrefab;
        p.GetComponent<SpriteRenderer>().sprite = team == Team.Blue ? towerData.previewBlueImage : towerData.previewRedImage;
        return p;
    }

    private GameObject ghostInstance;      // 虛影實例
    private SpriteRenderer ghostRenderer;  // 虛影的渲染器
    public static bool IsBuilding { get { return _isBuilding; } }  // 公開唯讀屬性
    private static bool _isBuilding = false;      // 是否正在建造
    private bool canBuild = false;        // 當前位置是否可以建造
    private Vector3 currentPos;           // 當前格線位置（自動對齊整數座標）
    private int selectedTowerIndex = 0;   // 當前選擇的塔索引
    private TowerModel currentTower;      // 當前選擇的塔模型
    private Vector2 lastValidPosition;    // 最後一個有效的位置

    private Camera mainCamera;
    public void setCurrentTower(TowerModel tower)
    {
        currentTower = tower;
    }
    public TowerModel getTowerByName(string towerName)
    {
        return System.Array.Find(towers, t => t.towerPrefab.name == towerName);
    }   
    void Start()
    {
        mainCamera = Camera.main;
        InitializeTowersFromCardLoader();
    }
    public void StartBuild(int index)
    {
        if (index < 0 || index >= towers.Length) return;
        _isBuilding = true;
        selectedTowerIndex = index;
        currentTower = towers[index];

        // 在相機中心創建預覽
        Vector3 centerPos = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, mainCamera.nearClipPlane));
        Vector2 gridPos = new Vector2(
            Mathf.Round(centerPos.x / gridSize) * gridSize,
            Mathf.Round(centerPos.y / gridSize) * gridSize
        );
        CreateGhost(gridPos);
    }

    public void CancelBuild()
    {
        if (ghostInstance != null)
        {
            Destroy(ghostInstance);
        }
        _isBuilding = false;
        canBuild = false;
    }

    private void CreateGhost(Vector2 pos)
    {
        // AI模式下不創建幽靈預覽
        if (isAI) return;

        if (ghostInstance != null)
        {
            ghostInstance.transform.position = new Vector3(pos.x, pos.y, 0);
            return;
        }

        ghostInstance = Instantiate(currentTower.ghostPrefab);
        ghostInstance.GetComponentsInChildren<Button>()[0].onClick.AddListener(BuildTower);
        ghostInstance.GetComponentsInChildren<Button>()[1].onClick.AddListener(CancelBuild);
        ghostInstance.transform.position = new Vector3(pos.x, pos.y, 0);
        ghostRenderer = ghostInstance.GetComponent<SpriteRenderer>();
        if (ghostRenderer == null)
        {
            ghostRenderer = ghostInstance.AddComponent<SpriteRenderer>();
        }
        UpdateGhostColor(pos);
    }

    private void UpdateGhostColor(Vector2 pos)
    {
        canBuild = IsValidBuildPosition(pos);
        if (ghostRenderer != null)
        {
            ghostRenderer.color = canBuild ? validColor : invalidColor;
        }
    }

    public bool IsValidBuildPosition(Vector2 pos)
    {
        // 1. x軸上下限制
        if (pos.x < minX || pos.x > maxX)
            return false;

        // 2. 檢查建築範圍每格的Tile必須在Buildable tilemap有tile
        if (buildTilemap == null || currentTower == null)
            return false;

        Vector2 offset = new Vector2(1, 2); // 根據tower尺寸調整
        for (int x = 0; x < currentTower.towerWidth; x++)
        {
            for (int y = 0; y < currentTower.towerHeight; y++)
            {
                Vector3 checkWorldPos = new Vector3(pos.x + x - offset.x, pos.y - offset.y + y, 0);
                var cellPos = buildTilemap.WorldToCell(checkWorldPos);
                var tile = buildTilemap.GetTile(cellPos);
                if (tile == null)
                    return false;
            }
        }

        // 3. 檢查Collider重疊（使用Prefab的Collider設定）
        if (currentTower.towerPrefab == null)
            return false;

        Collider2D prefabCollider = currentTower.towerPrefab.GetComponent<Collider2D>();
        if (prefabCollider == null)
            return false;

        // 計算放置後Collider的中心與大小
        Vector2 colliderOffset = prefabCollider.offset;
        Vector2 colliderSize = prefabCollider.bounds.size;
        Vector2 checkCenter = (Vector2)pos + colliderOffset;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(checkCenter, colliderSize, 0f);
        foreach (var col in colliders)
        {
            if (col.gameObject.CompareTag("Tower"))
                return false;
        }

        return true;
    }

    private Bounds GetBuildArea()
    {
        // 這裡你需要根據你的地圖大小來設定建造區域
        // 這是一個示例值，你應該根據實際地圖大小調整
        return new Bounds(Vector3.zero, new Vector3(50f, 50f, 1f));
    }

    void Update()
    {
        if (!_isBuilding || isAI) return; 

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended) return;
            
            // 檢查是否點擊到UI元素
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }
            
            // 處理觸控移動
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Began)
            {
                Vector3 touchPos = touch.position;
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(touchPos);
                Vector2 gridPos = new Vector2(
                    Mathf.Round(worldPos.x / gridSize) * gridSize,
                    Mathf.Round(worldPos.y / gridSize) * gridSize
                );

                // 強制座標為整數（去除小數點）
                Vector3 alignedPos = new Vector3(
                    Mathf.RoundToInt(gridPos.x),
                    Mathf.RoundToInt(gridPos.y),
                    0
                );
                CreateGhost(alignedPos);
                UpdateGhostColor(alignedPos);
                currentPos = alignedPos;
            }
        }
    }

    public bool BuildTowerAt(string towerName, Vector3 position)
    {
        // 找到對應的塔
        TowerModel tower = getTowerByName(towerName);
        if (tower == null) return false;

        // 檢查位置是否有效
        if (!IsValidBuildPosition(position)) return false;

        // 檢查資源是否足夠
        if (GameManager.instance.getResource(team, ResourceType.Wood) < tower.woodCost
        || GameManager.instance.getResource(team, ResourceType.Meat) < tower.meatCost
        || GameManager.instance.getResource(team, ResourceType.Gold) < tower.goldCost)
        {
            print("資源不足，無法建造！");
            return false;
        }

        // 扣除資源
        GameManager.instance.costResource(team, ResourceType.Wood, tower.woodCost);
        GameManager.instance.costResource(team, ResourceType.Meat, tower.meatCost);
        GameManager.instance.costResource(team, ResourceType.Gold, tower.goldCost);

        // 創建實際的塔
        GameObject towerInstance = Instantiate(tower.towerPrefab, position, Quaternion.identity);
        return true;
    }

    public void BuildTower()
    {
        if (!canBuild || currentTower == null) return;
        if (ghostInstance == null) return;

        // 在這裡檢查資源是否足夠
        if (GameManager.instance.getResource(team, ResourceType.Wood) <= currentTower.woodCost
        || GameManager.instance.getResource(team, ResourceType.Meat) <= currentTower.meatCost
        || GameManager.instance.getResource(team, ResourceType.Gold) <= currentTower.goldCost)
        {
            MessageBox.instance.ShowMessage("資源不足，無法建造！", Color.red);
            CancelBuild();
            return;
        }

        // 在這裡扣除資源
        GameManager.instance.costResource(team, ResourceType.Wood, currentTower.woodCost);
        GameManager.instance.costResource(team, ResourceType.Meat, currentTower.meatCost);
        GameManager.instance.costResource(team, ResourceType.Gold, currentTower.goldCost);
        // 創建實際的塔

        GameObject tower = Instantiate(currentTower.towerPrefab, currentPos, Quaternion.identity);
        
        // 清理預覽並重置狀態
        CancelBuild();
    }

#if UNITY_EDITOR
void OnDrawGizmos()
{
    if (_isBuilding && currentTower != null)
    {
        Gizmos.color = canBuild ? Color.green : Color.red;
        Vector3 drawPos = currentPos - new Vector3(1, 2, 0);
        Vector3 size = new Vector3(currentTower.towerWidth, currentTower.towerHeight, 0.1f);
        Gizmos.DrawWireCube(drawPos + size * 0.5f, size);
    }
}
#endif
}