using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum BlackCardType
{
    SelfBuff,
    EnemyDebuff,
    Special
}

/// <summary>
/// 黑市管理器 - 負責UI顯示和用戶交互 (MVC中的View+Controller)
/// 職責：UI管理、卡牌選擇、資源檢查
/// </summary>
public class BlackMarketManager : MonoBehaviour
{
    [Header("UI組件")]
    public GameObject blackMarketPanel;
    public List<BlackChoiceUIModel> choices = new List<BlackChoiceUIModel>();
    
    [Header("遊戲設定")]
    public Team playerTeam = Team.Blue;
    
    // 依賴的效果控制器
    private BlackCardEffectController effectController;
    
    // 當前顯示的卡牌
    private List<BlackCardModel> currentCards = new List<BlackCardModel>();
    
    // 黑市狀態
    private bool isOpen = false;
    
    void Awake()
    {
        // 獲取效果控制器依賴
        effectController = GetComponent<BlackCardEffectController>();
        if (effectController == null)
        {
            effectController = gameObject.AddComponent<BlackCardEffectController>();
        }
        
        // 設置玩家隊伍
        effectController.playerTeam = playerTeam;
    }
    public void Open()
    {
        isOpen = true;
        
        Debug.Log("[BlackMarket] 開始執行 Open()");
        
        // 檢查資料是否存在
        if (CardLoader.instance == null || CardLoader.instance.blackCardData == null)
        {
            Debug.LogError("[BlackMarket] CardLoader 或 blackCardData 未初始化");
            return;
        }

        var allCards = CardLoader.instance.blackCardData.cards;
        if (allCards == null || allCards.Count == 0)
        {
            Debug.LogWarning("[BlackMarket] 沒有可用的黑市卡片");
            return;
        }
        
        Debug.Log($"[BlackMarket] 找到 {allCards.Count} 張卡片");

        if (choices == null || choices.Count < 3)
        {
            Debug.LogWarning($"[BlackMarket] choices 列表未正確設定（當前數量: {(choices == null ? "null" : choices.Count.ToString())}）");
            return;
        }
        
        Debug.Log($"[BlackMarket] choices 數量: {choices.Count}");

        // 隨機抽取3張不同的卡片
        var selectedCards = allCards.OrderBy(x => UnityEngine.Random.value).Take(3).ToList();
        currentCards = selectedCards; // 保存當前卡牌列表供後續使用
        
        Debug.Log($"[BlackMarket] 已抽取 {selectedCards.Count} 張卡片");

        // 顯示面板
        if (blackMarketPanel != null)
        {
            blackMarketPanel.SetActive(true);
            Debug.Log("[BlackMarket] 黑市面板已啟用");
        }
        else
        {
            Debug.LogWarning("[BlackMarket] blackMarketPanel 為 null");
        }

        // 設置每個選項的資訊
        for (int i = 0; i < 3 && i < selectedCards.Count; i++)
        {
            var card = selectedCards[i];
            var choice = choices[i];

            Debug.Log($"[BlackMarket] 處理 Choice[{i}]: choice={(choice == null ? "null" : "存在")}, card.name={card.name}");

            if (choice == null)
            {
                Debug.LogWarning($"[BlackMarket] Choice[{i}] 為 null，跳過");
                continue;
            }
            
            if (choice.gameObject == null)
            {
                Debug.LogWarning($"[BlackMarket] Choice[{i}].gameObject 為 null");
                continue;
            }

            Debug.Log($"[BlackMarket] Choice[{i}] GameObject 名稱: {choice.gameObject.name}, 啟用狀態: {choice.gameObject.activeSelf}");

            // 設置卡片資訊
            if (choice.titleText != null)
            {
                choice.titleText.text = card.name;
                Debug.Log($"[BlackMarket] Choice[{i}] 設置標題: {card.name}");
            }
            else
            {
                Debug.LogWarning($"[BlackMarket] Choice[{i}].titleText 為 null");
            }

            if (choice.descriptionText != null)
            {
                choice.descriptionText.text = card.description;
            }
            else
            {
                Debug.LogWarning($"[BlackMarket] Choice[{i}].descriptionText 為 null");
            }

            if (choice.goldCost != null)
                choice.goldCost.text = card.goldCost.ToString();

            if (choice.woodCost != null)
                choice.woodCost.text = card.woodCost.ToString();

            if (choice.meatCost != null)
                choice.meatCost.text = card.meatCost.ToString();

            // 設置卡牌索引
            choice.SetCardIndex(i);

            // 啟用選項物件
            choice.gameObject.SetActive(true);
            Debug.Log($"[BlackMarket] Choice[{i}] 已設為 Active");
        }

        // 隱藏多餘的選項
        for (int i = 3; i < choices.Count; i++)
        {
            if (choices[i] != null && choices[i].gameObject != null)
                choices[i].gameObject.SetActive(false);
        }
        
        Debug.Log("[BlackMarket] Open() 執行完成");
    }
    
    public void Toggle()
    {
        if (!isOpen)
        {
            blackMarketPanel.SetActive(false);
            return;
        }
        blackMarketPanel.SetActive(!blackMarketPanel.activeSelf);
    }

    /// <summary>
    /// 玩家選擇黑市卡牌 - 主要交互邏輯
    /// </summary>
    public void SelectCard(int choiceIndex)
    {
        if (!ValidateSelection(choiceIndex)) return;
        
        var selectedCard = currentCards[choiceIndex];
        
        if (!ProcessCardPurchase(selectedCard)) return;
        
        ExecuteCardEffect(selectedCard);
        
        CloseBlackMarket();
    }
    
    /// <summary>
    /// 驗證選擇是否有效
    /// </summary>
    private bool ValidateSelection(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= currentCards.Count)
        {
            Debug.LogError($"[BlackMarket] 無效的選擇索引: {choiceIndex}");
            return false;
        }
        return true;
    }
    
    /// <summary>
    /// 處理卡牌購買邏輯
    /// </summary>
    private bool ProcessCardPurchase(BlackCardModel card)
    {
        if (!CanAffordCard(card))
        {
            MessageBox.instance.ShowMessage("資源不足！", Color.red);
            return false;
        }
        
        DeductCardCost(card);
        return true;
    }
    
    /// <summary>
    /// 執行卡牌效果 - 委託給效果控制器
    /// </summary>
    private void ExecuteCardEffect(BlackCardModel card)
    {
        bool success = effectController.ExecuteEffect(card);
        
        if (success)
        {
            MessageBox.instance.ShowMessage($"使用了 {card.name}！", Color.green);
        }
        else
        {
            MessageBox.instance.ShowMessage($"卡牌效果執行失敗", Color.red);
        }
    }

    /// <summary>
    /// 檢查是否有足夠資源購買卡牌
    /// </summary>
    private bool CanAffordCard(BlackCardModel card)
    {
        return GameManager.instance.getResource(playerTeam, ResourceType.Gold) >= card.goldCost &&
               GameManager.instance.getResource(playerTeam, ResourceType.Wood) >= card.woodCost &&
               GameManager.instance.getResource(playerTeam, ResourceType.Meat) >= card.meatCost;
    }

    /// <summary>
    /// 扣除卡牌成本
    /// </summary>
    private void DeductCardCost(BlackCardModel card)
    {
        GameManager.instance.costResource(playerTeam, ResourceType.Gold, card.goldCost);
        GameManager.instance.costResource(playerTeam, ResourceType.Wood, card.woodCost);
        GameManager.instance.costResource(playerTeam, ResourceType.Meat, card.meatCost);
    }



    /// <summary>
    /// 關閉黑市面板
    /// </summary>
    private void CloseBlackMarket()
    {
        if (blackMarketPanel != null)
        {
            blackMarketPanel.SetActive(false);
        }
        isOpen = false;
        currentCards.Clear();
    }

}