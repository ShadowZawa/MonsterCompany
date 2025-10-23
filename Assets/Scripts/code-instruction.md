# MonsterCompany 專案程式碼分析報告

## 專案概述
本專案是一款塔防策略遊戲，玩家可以建造塔、召喚怪物、採集資源來對戰。遊戲包含藍隊和紅隊的對戰機制，具有AI系統、資源管理、黑市系統等功能。

## 程式碼結構分析

### 1. 核心架構設計

#### ✅ 優點：
- **事件系統 (EventBus)**: 使用良好的事件驅動架構，減少模組間的耦合
- **模組化設計**: 各功能模組分工明確 (GameManager, UIManager, TowerBuilder, MobManager等)
- **資料載入系統**: CardLoader統一管理遊戲資料載入
- **單例模式**: 適當使用單例管理全域狀態

#### ⚠️ 需要改進的地方：

##### 1.1 GameManager 責任過重
```csharp
// 問題：GameManager同時處理遊戲狀態、資源管理、計時器等多個職責
// 建議：拆分為多個管理器
public class ResourceManager : MonoBehaviour
{
    // 專門處理資源相關邏輯
}

public class GameStateManager : MonoBehaviour  
{
    // 專門處理遊戲狀態和計時
}
```

##### 1.2 MasterData 設計過於簡單
```csharp
// 問題：MasterData僅儲存currentStage，缺乏擴展性
// 建議：增加更多遊戲設定和配置管理
public class GameConfig
{
    public float gameTimeLimit = 1800f;
    public int startingResources = 200;
    public StageModel[] availableStages;
}
```

### 2. AI系統分析

#### ✅ 優點：
- AI行為模式清晰，支援建塔、召喚、啟航等操作
- 基於資源閾值的決策機制



### 3. 遊戲機制問題

#### 3.1 農民AI邏輯缺陷



#### 3.2 塔建造系統問題



#### 3.3 戰鬥系統問題

##### 問題1：友軍攻擊判斷不完整
```csharp
// SoilderAI.cs 已有GetTeamFromObject方法，但其他AI沒有統一使用
// 建議：創建統一的團隊判斷工具類
public static class TeamUtils
{
    public static Team? GetTeamFromGameObject(GameObject obj)
    {
        // 統一的團隊判斷邏輯
    }
    
    public static bool IsEnemyTeam(Team myTeam, GameObject target)
    {
        var targetTeam = GetTeamFromGameObject(target);
        return targetTeam.HasValue && targetTeam.Value != myTeam;
    }
}
```


### 4. UI系統問題

#### 4.1 UIManager責任過重
UIManager同時處理資源顯示、按鈕創建、物件詳情等多項職責，建議拆分：

```csharp
// 建議拆分為：
public class ResourceUIManager    // 專門處理資源顯示
public class BuildUIManager      // 專門處理建造UI
public class DetailUIManager     // 專門處理物件詳情
```



### 5. 資料管理問題

#### 5.1 資料庫連接硬編碼
```csharp
// DataBaseManager.cs Line 29
// 問題：資料庫連接資訊硬編碼，且被註解掉
//MongoClient client = new MongoClient("mongodb://db_user:Abc123456@125.228.7.197:27017/");
// 建議：使用配置文件管理連接資訊
```

#### 5.2 本地資料持久化缺失
遊戲缺乏本地存檔機制，建議增加：
```csharp
public class SaveSystem
{
    public static void SavePlayerProgress(PlayerData data)
    {
        // 本地存檔邏輯
    }
    
    public static PlayerData LoadPlayerProgress()
    {
        // 本地載入邏輯
    }
}
```

### 6. 性能優化建議

#### 6.1 頻繁的GameObject搜尋
```csharp
// 多處使用GameObject.FindGameObjectsWithTag，效能較差
// 建議：使用快取或管理器模式
public class EntityManager : MonoBehaviour
{
    private Dictionary<Team, List<GameObject>> teamEntities;
    // 快取並維護各隊伍的實體列表
}
```

#### 6.2 Update方法優化
許多AI腳本在Update中執行複雜邏輯，建議：
```csharp
// 使用協程或定時器減少Update頻率
private IEnumerator AIUpdateRoutine()
{
    while (true)
    {
        // AI邏輯
        yield return new WaitForSeconds(0.1f); // 降低更新頻率
    }
}
```

### 7. 遊戲平衡性問題

#### 7.1 資源生成速度
- 初始資源200，但缺乏動態平衡機制
- 建議：根據遊戲進度調整資源生成速度

#### 7.2 AI難度設計
- AI目前只有固定行為模式
- 建議：增加多個難度等級和動態調整

### 8. 程式碼品質改進

#### 8.1 魔術數字過多
```csharp
// 到處都有魔術數字，建議使用常數
public static class GameConstants
{
    public const float FARMER_SEARCH_RANGE = 20f;
    public const int DEFAULT_STARTING_RESOURCES = 200;
    public const float GAME_TIME_LIMIT = 1800f;
}
```


#### 8.3 註解和文檔
- 部分程式碼缺乏註解說明
- 建議：為複雜邏輯添加詳細註解

### 9. 安全性問題

#### 9.1 SendMessage的安全性
```csharp
// 大量使用SendMessage，缺乏型別安全
// 建議：使用強型別的介面或事件系統
public interface IDamageable
{
    void TakeDamage(int damage);
}
```

#### 9.2 公開欄位過多
許多類別將欄位設為public，建議：
```csharp
// 使用屬性代替公開欄位
[SerializeField] private int maxHealth;
public int MaxHealth => maxHealth;
```
